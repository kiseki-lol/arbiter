using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;

namespace Tadah.Arbiter
{
    internal class Client
    {
        public string IpAddress { get; private set; }
        public int Port { get; private set; }
        public Socket Socket { get; set; }

        public Client(Socket socket)
        {
            this.IpAddress = null;
            this.Port = 0;
            this.Socket = socket;

            IPEndPoint remote = socket.RemoteEndPoint as IPEndPoint;
            IPEndPoint local = socket.LocalEndPoint as IPEndPoint;

            if (remote != null)
            {
                this.IpAddress = remote.Address.ToString();
                this.Port = remote.Port;
            }

            if (local != null)
            {
                this.IpAddress = local.Address.ToString();
                this.Port = local.Port;
            }

            if (local == null && remote == null)
            {
                throw new("Failed to resolve information from socket");
            }
        }
    }

    internal class StateObject
    {
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];
        public StringBuilder Builder = new();
        public Socket WorkSocket = null;
        public Client Client;
    }

    public class ArbiterService
    {
        private static readonly string EOFDelimiter = "<<<EOF>>>";
        private static readonly ManualResetEvent Finished = new(false);
        private static Socket Listener;

        public static int Start()
        {
            IPEndPoint localEndPoint = new(IPAddress.Any, Configuration.ServicePort);

            Listener = new(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Listener.Bind(localEndPoint);
                Listener.Listen(100);
            }
            finally
            {
                try
                {
                    Task.Run(() => ListenForConnections());
                }
                catch (Exception exception)
                {
                    Log.Error($"ListenForConnections failed: {exception}");
                }
            }

            return Configuration.ServicePort;
        }

        public static void Stop()
        {
            Listener.Close();
        }

        private static void ListenForConnections()
        {
            while (true)
            {
                Finished.Reset();
                Listener.BeginAccept(new(AcceptCallback), Listener);
                Finished.WaitOne();
            }
        }

        private static void AcceptCallback(IAsyncResult result)
        {
            Client client = null;
            Socket handler = null;

            try
            {
                Finished.Set();

                Socket listener = (Socket)result.AsyncState;
                handler = listener.EndAccept(result);
                client = new(handler);

                Log.Write($"[ArbiterService] '{client.IpAddress}' Connected on port {client.Port}", LogSeverity.Event);
            }
            finally
            {
                StateObject state = new();
                state.WorkSocket = handler;
                state.Client = client;

                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new(ReadCallback), state);
            }
        }

        private static void ReadCallback(IAsyncResult result)
        {
            string content = string.Empty;

            StateObject state = (StateObject)result.AsyncState;
            Socket handler = state.WorkSocket;

            try
            {
                int read = handler.EndReceive(result);

                if (read > 0)
                {
                    state.Builder.Append(Encoding.ASCII.GetString(state.Buffer, 0, read));
                    content = state.Builder.ToString();

                    if (content.Length > 0 && !content.StartsWith("%"))
                    {
                        Log.Write($"[ArbiterService::ReadCallback] '{state.Client.IpAddress}' - Did not include a signature", LogSeverity.Warning);
                        handler.Close();
                    }

                    if (content.IndexOf(EOFDelimiter) > -1)
                    {
                        content = content.Replace(EOFDelimiter, "");
#if DEBUG
                        Log.Write($"[ArbiterService::ReadCallback] '{state.Client.IpAddress}' - Read all data, sending response", LogSeverity.Debug);
#endif
                        string response = Encoding.Default.GetString(ProcessData(content, state.Client).ToArray());
                        SendData(handler, response);
                    }
                    else
                    {
                        handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Write($"[ArbiterService::ReadCallback] - {exception}", LogSeverity.Error);
            }
        }

        private static void SendCallback(IAsyncResult result)
        {
            try
            {
                Socket handler = (Socket)result.AsyncState;

                int bytesSent = handler.EndSend(result);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception ex)
            {
                Log.Write($"[ArbiterService::SendCallback] {ex.Message}", LogSeverity.Error);
            }
        }

        private static void SendData(Socket handler, string data)
        {
            try
            {
                byte[] dataBytes = Encoding.ASCII.GetBytes(data);
                handler.BeginSend(dataBytes, 0, dataBytes.Length, 0, new AsyncCallback(SendCallback), handler);
            }
            catch (Exception ex)
            {
                Log.Write($"[ArbiterService::SendData] {ex.Message}", LogSeverity.Error);
            }
        }

        private static MemoryStream SignalError(Proto.Signal signal, Client client, string reason)
        {
            MemoryStream stream = new();

            try
            {
                Log.Write($"[ArbiterService::{client.IpAddress}] Tried '{Enum.GetName(signal.Operation)}' with '{signal.JobId}' - {reason}", LogSeverity.Warning);
            }
            finally
            {
                new Proto.Response
                {
                    Operation = signal.Operation,
                    Success = false,
                    Message = reason
                }.WriteTo(stream);
            }

            return stream;
        }

        private static MemoryStream ProcessData(string data, Client client)
        {

            if (!TadahSignature.VerifyData(data, out string message))
            {
#if DEBUG
                Log.Write($"[ArbiterService::ProcessData] '{client.IpAddress}' - Bad or invalid signature", LogSeverity.Debug);
#endif
                return new();
            }

            Proto.Signal signal = null;

            try
            {
                signal = Proto.Signal.Parser.ParseFrom(Encoding.Default.GetBytes(message));
            }
            catch
            {
                Log.Write($"[ArbiterService::ProcessData] '{client.IpAddress}' - Bad or invalid data", LogSeverity.Warning);
                return new();
            }

            // Last security check (see if nonce >= 10 seconds old)
            // This is so that people can't re-send signed data with destructive commands (i.e. "CloseAllJobs")
            if (Unix.GetTimestamp() + 10 > Unix.From(signal.Nonce.ToDateTime()))
            {
                Log.Write($"[ArbiterService::ProcessData] '{client.IpAddress}' - Old data", LogSeverity.Warning);
                return new();
            }

            MemoryStream stream = new();
            bool success = false;
            object responseData = null;

            try
            {
                switch (signal.Operation)
                {
                    case Proto.Operation.OpenJob:
                        {
                            if (signal.Place.Count == 0)
                            {
                                return SignalError(signal, client, "No place specified");
                            }

                            Proto.Signal.Types.Place place = signal.Place[0];

                            if (JobManager.JobExists(signal.JobId))
                            {
                                return SignalError(signal, client, "Job already exists");
                            }

                            if (!JobManager.IsValidVersion(signal.Version))
                            {
                                return SignalError(signal, client, "Invalid ClientVersion");
                            }

                            if (JobManager.OpenJobs.Count >= Configuration.MaximumPlaceJobs)
                            {
                                return SignalError(signal, client, "Maximum amount of jobs reached on this arbiter -- please try another");
                            }

                            Task.Run(() => JobManager.OpenJob(signal.JobId, place.PlaceId, signal.Version));
                            success = true;

                            break;
                        }

                    case Proto.Operation.CloseJob:
                        {
                            if (JobManager.JobExists(signal.JobId))
                            {
                                return SignalError(signal, client, "Job doesn't exist");
                            }

                            Task.Run(() => JobManager.CloseJob(signal.JobId));
                            success = true;

                            break;
                        }

                    case Proto.Operation.ExecuteScript:
                        {
                            Job job = JobManager.GetJobFromId(signal.JobId);

                            if (job == null)
                            {
                                return SignalError(signal, client, "Job does not exist");
                            }

                            if (job is TaipeiJob)
                            {
                                return SignalError(signal, client, "Job does not support such functionality (is TaipeiJob)");
                            }

                            if (signal.Place.Count == 0)
                            {
                                return SignalError(signal, client, "No place specified");
                            }

                            Proto.Signal.Types.Place place = signal.Place[0];
                            Task.Run(() => { job.ExecuteScript(place.Script); });

                            success = true;

                            break;
                        }

                    case Proto.Operation.RenewTampaJobLease:
                        {
                            Job job = JobManager.GetJobFromId(signal.JobId);

                            if (job == null)
                            {
                                return SignalError(signal, client, "Job doesn't exist");
                            }

                            if (job is not TampaJob)
                            {
                                return SignalError(signal, client, "Job does not support such functionality (is TampaJob)");
                            }

                            if (signal.Place.Count == 0)
                            {
                                return SignalError(signal, client, "No place specified");
                            }

                            TampaJob taJob = (TampaJob)job;
                            Task.Run(() => { taJob.RenewLease(signal.Place[0].ExpirationInSeconds); });

                            success = true;

                            break;
                        }

                    case Proto.Operation.CloseAllJobs:
                        {
                            int jobs = JobManager.OpenJobs.Count;

                            Task.Run(() => { JobManager.CloseAllJobs(); });

                            success = true;
                            responseData = jobs;

                            break;
                        }

                    case Proto.Operation.CloseAllTampaProcesses:
                        {
                            int processes = TampaProcessManager.OpenProcesses.Count;

                            Task.Run(() => { TampaProcessManager.CloseAllProcesses(); });

                            success = true;
                            responseData = processes;

                            break;
                        }

                    default:
                        {
                            new Proto.Response
                            {
                                Success = false,
                                Message = "No such operation exists"
                            }.WriteTo(stream);

                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Log.Write($"[ArbiterService::ProcessData] '{client.IpAddress}' - {exception.Message}", LogSeverity.Error);

#if DEBUG
                new Proto.Response
                {
                    Operation = signal.Operation,
                    Success = false,
                    Message = exception.Message,
                }.WriteTo(stream);
#else
                // Will just give a blank error
#endif
            }

            if (success)
            {
                Proto.Response response = new()
                {
                    Operation = signal.Operation,
                    Success = true
                };

                if (responseData != null)
                {
                    response.Data = (string)responseData;
                }

                response.WriteTo(stream);
            }
            else
            {
                // If it wasn't successful, the stream has already been written to
                // In the case of an exception or the "default" fallthrough in the switch statement.
            }


            return stream;
        }
    }
}
