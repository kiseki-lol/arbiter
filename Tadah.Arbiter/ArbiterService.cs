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
        public int Port { get; private set;  }
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
        private static ManualResetEvent Finished = new ManualResetEvent(false);
        private static Socket Listener;

        public static int Start()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, AppSettings.ServicePort);

            Listener = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Listener.Bind(localEndPoint);
                Listener.Listen(100);
                Task.Run(() => ListenForConnections());
            }
            catch
            {
                Log.Error($"Failed to initialize ArbiterService on port {AppSettings.ServicePort}");
            }
            
            return AppSettings.ServicePort;
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
                Listener.BeginAccept(new AsyncCallback(AcceptCallback), Listener);
                Finished.WaitOne();
            }
        }

        private static void AcceptCallback(IAsyncResult result)
        {
            Finished.Reset();

            Socket listener = (Socket)result.AsyncState;
            Socket handler = listener.EndAccept(result);
            Client client = new Client(handler);

            Log.Write($"[ArbiterService::{client.IpAddress}] Connected on port {client.Port}", LogSeverity.Event);

            StateObject state = new StateObject();
            state.WorkSocket = handler;
            state.Client = client;

            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        private static void ReadCallback(IAsyncResult result)
        {
            String content = String.Empty;

            StateObject state = (StateObject)result.AsyncState;
            Socket handler = state.WorkSocket;

            int read = handler.EndReceive(result);

            if (read > 0)
            {
                state.Builder.Append(Encoding.ASCII.GetString(state.Buffer, 0, read));
                content = state.Builder.ToString();

                if (content.IndexOf("<EOF>") > -1)
                {
#if DEBUG
                    Log.Write($"[ArbiterService::{state.Client.IpAddress}] Read all data, sending response", LogSeverity.Debug);
#endif

                    content = content.Replace("<EOF>", "");
                    string response = ProcessData(content, state.Client);
                    SendData(handler, response);
                }
                else
                {
                    handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
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
#if DEBUG
                Log.Write($"[ArbiterService::SendCallback] {ex.ToString()}", LogSeverity.Error);
#endif
            }
        }

        private static void SendData(Socket handler, string data)
        {
            byte[] dataBytes = Encoding.ASCII.GetBytes(data);
            handler.BeginSend(dataBytes, 0, dataBytes.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private static string ProcessData(string data, Client client)
        {
            string message;

            if (!TadahSignature.VerifyData(data, out message))
            {
#if DEBUG
                Log.Write($"[ArbiterService::{client.IpAddress}] Bad or invalid signature", LogSeverity.Debug);
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
                Log.Write($"[ArbiterService::{client.IpAddress}] Bad or invalid data", LogSeverity.Warning);
                return "";
            }

            switch (request.Operation)
            {
                case "OpenJob":
                    if (JobManager.GetJobFromId(request.JobId) != null)
                    {
                        Log.Write($"[ArbiterService::{client.IpAddress}] Tried opening job '{request.JobId}' - it already exists", LogSeverity.Warning);
                        return "{\"Operation\":\"CloseJob\", \"Status\":\"Error\", \"Message\":\"Job already exists\"}";
                    }
                    else
                    {
                        Task.Run(() => JobManager.OpenJob(request.JobId, request.PlaceId, request.Version));
                        return "{\"Operation\":\"OpenJob\", \"Status\":\"OK\"}";
                    }

                    break;

                case "CloseJob":
                    if (JobManager.GetJobFromId(request.JobId) == null)
                    {
                        Log.Write($"[ArbiterService::{client.IpAddress}] Tried closing job '{request.JobId}' - it doesn't exist", LogSeverity.Warning);
                        return "{\"Operation\":\"CloseJob\", \"Status\":\"Error\", \"Message\":\"Job does not exist\"}";
                    }
                    else
                    {
                        Task.Run(() => JobManager.CloseJob(request.JobId));
                        return "{\"Operation\":\"CloseJob\", \"Status\":\"OK\"}";
                    }

                    break;

                case "ExecuteScript":
                    if (JobManager.GetJobFromId(request.JobId) == null)
                    {
                        Log.Write($"[ArbiterService::{client.IpAddress}] Tried executing script on job '{request.JobId}' - it doesn't exists", LogSeverity.Warning);
                        return "{\"Operation\":\"CloseJob\", \"Status\":\"Error\", \"Message\":\"Job does not exist\"}";
                    }
                    else
                    {
                        Task.Run(() => JobManager.ExecuteScript(request.JobId, request.Script));
                        return "{\"Operation\":\"CloseJob\", \"Status\":\"OK\"}";
                    }

                    break;

                default:
                    Log.Write($"[ArbiterService::{client.IpAddress}] Invalid operation '{request.Operation}'", LogSeverity.Warning);
                    return "{\"Status\": \"Error\", \"Message\": \"Invalid operation\"}";

                    break;
            }
        }
    }
}
