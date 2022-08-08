using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tadah.Arbiter
{
    internal struct Client
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
                throw new Exception("Failed to resolve information from socket");
            }
        }
    }

    internal class StateObject
    {
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];
        public StringBuilder Builder = new StringBuilder();
        public Socket WorkSocket = null;
        public Client Client;
    }

    public class ArbiterService
    {
        private static string EOFDelimiter = "<<<EOF>>>";
        private static ManualResetEvent Finished = new ManualResetEvent(false);
        private static Socket Listener;

        public static int Start()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, int.Parse(Configuration.AppSettings["ServicePort"]));

            Listener = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Listener.Bind(localEndPoint);
                Listener.Listen(100);
                Task.Run(() => ListenForConnections());
            }
            catch
            {
                Log.Error($"Failed to initialize ArbiterService on port {Configuration.AppSettings["ServicePort"]}");
            }

            return int.Parse(Configuration.AppSettings["ServicePort"]);
        }

        public static void Stop()
        {
            Listener.Close();
        }

        private static void ListenForConnections()
        {
            while (true)
            {
                try
                {
                    Finished.Reset();
                    Listener.BeginAccept(new AsyncCallback(AcceptCallback), Listener);
                    Finished.WaitOne();
                }
                catch
                {
                    // Nah
                }
            }
        }

        private static void AcceptCallback(IAsyncResult result)
        {
            try
            {
                Finished.Set();

                Socket listener = (Socket)result.AsyncState;
                Socket handler = listener.EndAccept(result);
                Client client = new Client(handler);

                Log.Write($"[ArbiterService] '{client.IpAddress}' Connected on port {client.Port}", LogSeverity.Event);

                try
                {
                    StateObject state = new StateObject();
                    state.WorkSocket = handler;
                    state.Client = client;

                    handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
                catch (Exception)
                {
                    // idc
                }
            }
            catch (Exception)
            {
                // idc
            }
        }

        private static void ReadCallback(IAsyncResult result)
        {
            String content = String.Empty;

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
                        // YAAAAAAAAAAAAAAAAAAAAY.
                        Log.Write($"[ArbiterService::ReadCallback] '{state.Client.IpAddress}' - Did not include a signature", LogSeverity.Warning);
                        SendData(handler, "");
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
            catch (Exception ex)
            {
                Log.Write($"[ArbiterService::ReadCallback] - {ex.Message}", LogSeverity.Error);
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

            TadahMessage request = null;

            try
            {
                request = JsonConvert.DeserializeObject<TadahMessage>(message);
            }
            catch
            {
                Log.Write($"[ArbiterService::ProcessData] '{client.IpAddress}' - Bad or invalid data", LogSeverity.Warning);
                return "";
            }

            try
            {
                switch (request.Operation)
                {
                    case "OpenJob":
                        {
                            if (JobManager.JobExists(request.JobId))
                            {
                                Log.Write($"[ArbiterService::{client.IpAddress}] Tried 'OpenJob' with '{request.JobId}' - it already exists", LogSeverity.Warning);
                                return JsonConvert.SerializeObject(new TadahResponse
                                {
                                    Operation = "OpenJob",
                                    Success = false,
                                    Message = "Job already exists"
                                });
                            }

                            if (!JobManager.IsValidVersion(request.Version))
                            {
                                Log.Write($"[ArbiterService::{client.IpAddress}] Tried 'OpenJob' with '{request.JobId}' and version '{request.Version}' - is not a valid version", LogSeverity.Warning);
                                return JsonConvert.SerializeObject(new TadahResponse
                                {
                                    Operation = "OpenJob",
                                    Success = false,
                                    Message = "Invalid version"
                                });
                            }

                            if (JobManager.OpenJobs.Count >= int.Parse(Configuration.AppSettings["MaximumJobs"]))
                            {
                                Log.Write($"[ArbiterService::{client.IpAddress}] Tried 'OpenJob' - maximum amount of jobs reached", LogSeverity.Warning);
                                return JsonConvert.SerializeObject(new TadahResponse
                                {
                                    Operation = "OpenJob",
                                    Success = false,
                                    Message = "Maximum amount of jobs reached"
                                });
                            }

                            Task.Run(() => JobManager.OpenJob(request.JobId, request.PlaceId, request.Version));
                            return JsonConvert.SerializeObject(new TadahResponse
                            {
                                Operation = "OpenJob",
                                Success = true
                            });
                        }

                    case "CloseJob":
                        {
                            if (JobManager.JobExists(request.JobId))
                            {
                                Log.Write($"[ArbiterService::{client.IpAddress}] Tried 'CloseJob' on job '{request.JobId}' - it doesn't exist", LogSeverity.Warning);
                                return JsonConvert.SerializeObject(new TadahResponse
                                {
                                    Operation = "CloseJob",
                                    Success = false,
                                    Message = "Job doesn't exist"
                                });
                            }

                            Task.Run(() => JobManager.CloseJob(request.JobId));
                            return JsonConvert.SerializeObject(new TadahResponse
                            {
                                Operation = "CloseJob",
                                Success = true
                            });
                        }

                    /*
                    
                    case "ExecuteScript":
                        {
                            Job job = JobManager.GetJobFromId(request.JobId);

                            if (job == null)
                            {
                                Log.Write($"[ArbiterService::{client.IpAddress}] Tried 'ExecuteScript' on job '{request.JobId}' - it doesn't exist", LogSeverity.Warning);
                                return JsonConvert.SerializeObject(new TadahResponse
                                {
                                    Operation = "ExecuteScript",
                                    Success = false,
                                    Message = "Job doesn't exist"
                                });
                            }

                            Task.Run(() => { job.ExecuteScript(request.Script); });
                            return JsonConvert.SerializeObject(new TadahResponse
                            {
                                Operation = "ExecuteScript",
                                Success = true
                            });
                        }

                    */

                    case "RenewTampaServerJobLease":
                        {
                            Job job = JobManager.GetJobFromId(request.JobId);

                            if (job == null)
                            {
                                Log.Write($"[ArbiterService::{client.IpAddress}] Tried 'RenewLease' on job '{request.JobId}' - it doesn't exist", LogSeverity.Warning);
                                return JsonConvert.SerializeObject(new TadahResponse
                                {
                                    Operation = "RenewTampaServerJobLease",
                                    Success = false,
                                    Message = "Job doesn't exist"
                                });
                            }

                            if (job is not TampaServerJob)
                            {
                                Log.Write($"[ArbiterService::{client.IpAddress}] Tried 'RenewLease' on job '{request.JobId}' - is not a TampaServerJob", LogSeverity.Warning);
                                return JsonConvert.SerializeObject(new TadahResponse
                                {
                                    Operation = "RenewTampaServerJobLease",
                                    Success = false,
                                    Message = "Job is not TampaServerJob"
                                });
                            }

                            TampaServerJob tsJob = (TampaServerJob)job;
                            Task.Run(() => { tsJob.RenewLease(request.ExpirationInSeconds); });

                            return JsonConvert.SerializeObject(new TadahResponse
                            {
                                Operation = "RenewLease",
                                Success = true
                            });
                        }

                    case "CloseAllJobs":
                        {
                            Task.Run(() => { JobManager.CloseAllJobs(); });
                            return JsonConvert.SerializeObject(new TadahResponse
                            {
                                Operation = "CloseAllJobs",
                                Success = true
                            });
                        }

                    case "CloseAllTampaServerProcesses":
                        {
                            Task.Run(() => { TampaServerProcessManager.CloseAllProcesses(); });
                            return JsonConvert.SerializeObject(new TadahResponse
                            {
                                Operation = "CloseAllTampaServerProcesses",
                                Success = true
                            });
                        }

                    default:
                        {
                            Log.Write($"[ArbiterService::{client.IpAddress}] Invalid operation '{request.Operation}'", LogSeverity.Warning);
                            return JsonConvert.SerializeObject(new TadahResponse
                            {
                                Operation = request.Operation,
                                Success = false,
                                Message = "Invalid operation"
                            });
                        }
                }
            }
            catch (Exception ex)
            {
                Log.Write($"[ArbiterService::ProcessData] '{client.IpAddress}' - {ex.Message}", LogSeverity.Error);

                return JsonConvert.SerializeObject(new TadahResponse
                {
                    Operation = request.Operation,
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
