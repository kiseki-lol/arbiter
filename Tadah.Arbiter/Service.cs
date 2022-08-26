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
        public byte[] Buffer = new byte[1024];
        public ushort Size = 0;
        public Socket WorkSocket = null;
        public Client Client;
    }

    public class Service
    {
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

                handler.BeginReceive(state.Buffer, 0, 1024, 0, new(ReadCallback), state);
            }
        }

        private static void ReadCallback(IAsyncResult result)
        {
            StateObject state = (StateObject)result.AsyncState;
            Socket handler = state.WorkSocket;

            try
            {
                int read = handler.EndReceive(result);

                if (read > 0)
                {
                    /*
                     * TadahMessage format:
                     * 
                     * 0x02      0x00 0x00 0x00 0x00 0x02      0x00 0x00 .. ..     0x02      0x00 0x00 .. ..
                     * (STX)     (UINT16)  (UINT16)  (STX)     (UINT16)  (DATA)    (STX)     (UINT16)  (DATA)
                     * (MSGREAD) (MSGSIZE) (CHKSUM)  (SIGREAD) (SIGSIZE) (SIGDATA) (BUFREAD) (BUFSIZE) (BUFDATA)
                     * 
                     * From this, we can do some sanity checks while getting a complete read *and* fending off potential attackers;
                     * - See if the message begins with our MSGREAD
                     * - If it does, then recieve next 2 bytes; try parsing as a uint8 and use that as message size
                     * - Read message up to the specified message size
                     * - Try parsing the entire message
                     * - Verify buffer with given signature
                     * - Send buffer to ProcessData
                     */

                    if (state.Buffer[0] != 0x02)
                    {
                        Log.Write($"[ArbiterService::ReadCallback] '{state.Client.IpAddress}' - Not a TadahMessage", LogSeverity.Warning);
                        handler.Close();
                    }

                    if (read == 3)
                    {
                        if (state.Size == 0)
                        {
                            ushort parsed = 0;

                            try
                            {
                                parsed = BitConverter.ToUInt16(state.Buffer, 1); // Offset by 1 (MSGREAD[STX])
                            }
                            catch
                            {
                                Log.Write($"[ArbiterService::ReadCallback] '{state.Client.IpAddress}' - Not a TadahMessage", LogSeverity.Warning);
                                handler.Close();
                            }
                            finally
                            {
                                if (parsed == 0)
                                {
                                    Log.Write($"[ArbiterService::ReadCallback] '{state.Client.IpAddress}' - Not a TadahMessage", LogSeverity.Warning);
                                    handler.Close();
                                }

                                state.Size = parsed;
                            }
                        }

                        handler.BeginReceive(state.Buffer, 0, state.Size - 3, 0, new AsyncCallback(ReadCallback), state); // state.Size - 3 for 3 bytes already read (MSGREAD[STX], MSGSIZE[UINT8])
                    }

                    // Parse given data
                    if (!Message.TryParse(state.Buffer, out Message message))
                    {
                        Log.Write($"[ArbiterService::ReadCallback] '{state.Client.IpAddress}' - Not a TadahMessage", LogSeverity.Warning);
                        handler.Close();
                    }

                    // Verify signature
                    if (!Signature.Verify(message.Data, message.Signature))
                    {
                        Log.Write($"[ArbiterService::ReadCallback] '{state.Client.IpAddress}' - Bad or invalid signature", LogSeverity.Warning);
                        handler.Close();
                    }

                    // See if signal is older than ten seconds (so that people can't re-send signed destructive commands, such as "CloseAllJobs")
                    if (Unix.Now() + 10 > Unix.From(message.Signal.Nonce.ToDateTime()))
                    {
                        Log.Write($"[ArbiterService::ReadCallback] '{state.Client.IpAddress}' - Too old message received", LogSeverity.Warning);
                        handler.Close();
                    }

                    // Process data
                    byte[] response = ProcessSignal(message.Signal, state.Client);
                    SendData(handler, response);
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

        private static void SendData(Socket handler, byte[] data)
        {
            try
            {
                handler.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), handler);
            }
            catch (Exception ex)
            {
                Log.Write($"[ArbiterService::SendData] {ex.Message}", LogSeverity.Error);
            }
        }

        private static byte[] SignalError(Proto.Signal signal, Client client, string reason)
        {
            byte[] stream = new byte[1024];

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

        private static byte[] ProcessSignal(Proto.Signal signal, Client client)
        {
            byte[] stream = new byte[1024];
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

                            if (job is Taipei.Job)
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

                            if (job is not Tampa.Job)
                            {
                                return SignalError(signal, client, "Job does not support such functionality (is TampaJob)");
                            }

                            if (signal.Place.Count == 0)
                            {
                                return SignalError(signal, client, "No place specified");
                            }

                            Tampa.Job taJob = (Tampa.Job)job;
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
                            int processes = Tampa.ProcessManager.OpenProcesses.Count;

                            Task.Run(() => { Tampa.ProcessManager.CloseAllProcesses(); });

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
