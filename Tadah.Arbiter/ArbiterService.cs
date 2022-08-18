using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
                        string response = ProcessData(content, state.Client);
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

        private static string ProcessData(string data, Client client)
        {

            if (!TadahSignature.VerifyData(data, out string message))
            {
#if DEBUG
                Log.Write($"[ArbiterService::ProcessData] '{client.IpAddress}' - Bad or invalid signature", LogSeverity.Debug);
#endif
                return "";
            }

            Dictionary<string, string> request = null;

            try
            {
                request = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            }
            catch
            {
                Log.Write($"[ArbiterService::ProcessData] '{client.IpAddress}' - Bad or invalid data", LogSeverity.Warning);
                return "";
            }

            try
            {
                switch (request["Operation"])
                {
                    case "OpenJob":
                        {
                            if (JobManager.JobExists(request["JobId"]))
                            {
                                Log.Write($"[ArbiterService::{client.IpAddress}] Tried 'OpenJob' with '{request["JobId"]}' - it already exists", LogSeverity.Warning);
                                return JsonConvert.SerializeObject(new Dictionary<string, object>()
                                {
                                    { "Operation", "OpenJob" },
                                    { "Success", false },
                                    { "Message", "Job already exists" }
                                });
                            }

                            if (!JobManager.IsValidVersion(request["Version"]))
                            {
                                Log.Write($"[ArbiterService::{client.IpAddress}] Tried 'OpenJob' with '{request["JobId"]}' and version '{request["Version"]}' - is not a valid version", LogSeverity.Warning);
                                return JsonConvert.SerializeObject(new Dictionary<string, object>()
                                {
                                    { "Operation", "OpenJob" },
                                    { "Success", false },
                                    { "Message", "Invalid ClientVersion" }
                                });
                            }

                            if (JobManager.OpenJobs.Count >= Configuration.MaximumPlaceJobs)
                            {
                                Log.Write($"[ArbiterService::{client.IpAddress}] Tried 'OpenJob' - maximum amount of jobs reached", LogSeverity.Warning);
                                return JsonConvert.SerializeObject(new Dictionary<string, object>()
                                {
                                    { "Operation", "OpenJob" },
                                    { "Success", false },
                                    { "Message", "Maximum amount of jobs reached" }
                                });
                            }

                            Task.Run(() => JobManager.OpenJob(request["JobId"], int.Parse(request["PlaceId"]), (ClientVersion)int.Parse(request["Version"])));
                            return JsonConvert.SerializeObject(new Dictionary<string, object>()
                            {
                                { "Operation", "OpenJob" },
                                { "Success", true }
                            });
                        }

                    case "CloseJob":
                        {
                            if (JobManager.JobExists(request["JobId"]))
                            {
                                Log.Write($"[ArbiterService::{client.IpAddress}] Tried 'CloseJob' on job '{request["JobId"]}' - it doesn't exist", LogSeverity.Warning);
                                return JsonConvert.SerializeObject(new Dictionary<string, object>()
                                {
                                    { "Operation", "CloseJob" },
                                    { "Success", false },
                                    { "Message", "Job doesn't exist" }
                                });
                            }

                            Task.Run(() => JobManager.CloseJob(request["JobId"]));
                            return JsonConvert.SerializeObject(new Dictionary<string, object>()
                            {
                                { "Operation", "CloseJob" },
                                { "Success", true }
                            });
                        }

                    case "ExecuteScript":
                        {
                            Job job = JobManager.GetJobFromId(request["JobId"]);

                            if (job == null)
                            {
                                Log.Write($"[ArbiterService::{client.IpAddress}] Tried 'ExecuteScript' on job '{request["JobId"]}' - it doesn't exist", LogSeverity.Warning);
                                return JsonConvert.SerializeObject(new Dictionary<string, object>()
                                {
                                    { "Operation", "ExecuteScript" },
                                    { "Success", false },
                                    { "Message", "Job doesn't exist" }
                                });
                            }

                            if (job is TaipeiJob)
                            {
                                Log.Write($"[ArbiterService::{client.IpAddress}] Tried 'ExecuteScript' on job '{request["JobId"]}' - it does not support such capability (is TaipeiJob)", LogSeverity.Warning);
                                return JsonConvert.SerializeObject(new Dictionary<string, object>()
                                {
                                    { "Operation", "ExecuteScript" },
                                    { "Success", false },
                                    { "Message", "Cannot run ExecuteScript on TaipeiJob" }
                                });
                            }

                            Task.Run(() => { job.ExecuteScript(request["Script"]); });
                            return JsonConvert.SerializeObject(new Dictionary<string, object>()
                            {
                                { "Operation", "ExecuteScript" },
                                { "Success", true }
                            });
                        }

                    case "RenewTampaJobLease":
                        {
                            Job job = JobManager.GetJobFromId(request["JobId"]);

                            if (job == null)
                            {
                                Log.Write($"[ArbiterService::{client.IpAddress}] Tried 'RenewLease' on job '{request["JobId"]}' - it doesn't exist", LogSeverity.Warning);
                                return JsonConvert.SerializeObject(new Dictionary<string, object>()
                                {
                                    { "Operation", "RenewTampaJobLease" },
                                    { "Success", false },
                                    { "Message", "Job doesn't exist" }
                                });
                            }

                            if (job is not TampaJob)
                            {
                                Log.Write($"[ArbiterService::{client.IpAddress}] Tried 'RenewLease' on job '{request["JobId"]}' - is not a TampaJob", LogSeverity.Warning);
                                return JsonConvert.SerializeObject(new Dictionary<string, object>()
                                {
                                    { "Operation", "RenewTampaJobLease" },
                                    { "Success", false },
                                    { "Message", "Job is not a TampaJob" }
                                });
                            }

                            TampaJob taJob = (TampaJob)job;
                            Task.Run(() => { taJob.RenewLease(int.Parse(request["ExpirationInSeconds"])); });

                            return JsonConvert.SerializeObject(new Dictionary<string, object>()
                            {
                                { "Operation", "RenewTampaJobLease" },
                                { "Success", true }
                            });
                        }

                    case "CloseAllJobs":
                        {
                            int jobs = JobManager.OpenJobs.Count;

                            Task.Run(() => { JobManager.CloseAllJobs(); });
                            return JsonConvert.SerializeObject(new Dictionary<string, object>()
                            {
                                { "Operation", "CloseAllJobs" },
                                { "Success", true },
                                { "Data", jobs }
                            });
                        }

                    case "CloseAllTampaProcesses":
                        {
                            int processes = TampaProcessManager.OpenProcesses.Count;

                            Task.Run(() => { TampaProcessManager.CloseAllProcesses(); });
                            return JsonConvert.SerializeObject(new Dictionary<string, object>()
                            {
                                { "Operation", "CloseAllTampaProcesses" },
                                { "Success", true },
                                { "Data", processes }
                            });
                        }

                    default:
                        {
                            Log.Write($"[ArbiterService::{client.IpAddress}] Invalid operation '{request["Operation"]}'", LogSeverity.Warning);
                            return JsonConvert.SerializeObject(new Dictionary<string, object>()
                            {
                                { "Operation", request["Operation"] },
                                { "Success", false },
                                { "Message", $"Invalid operation '{request["Operation"]}'" }
                            });
                        }
                }
            }
            catch (Exception exception)
            {
                Log.Write($"[ArbiterService::ProcessData] '{client.IpAddress}' - {exception.Message}", LogSeverity.Error);

                return JsonConvert.SerializeObject(new Dictionary<string, object>()
                {
                    { "Operation", request["Operation"] },
                    { "Success", false },
                    { "Message", exception.Message }
                });
            }
        }
    }
}
