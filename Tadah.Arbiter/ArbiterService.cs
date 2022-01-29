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
        public string IpAddress;
        public int Port;

        public Client(string ipAddress, int port)
        {
            IpAddress = ipAddress;
            Port = port;
        }
    }

    internal class StateObject
    {
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];
        public StringBuilder Builder = new StringBuilder();
        public Socket WorkSocket = null;
    }

    public class ArbiterService
    {
        private static ManualResetEvent Finished = new ManualResetEvent(false);
        private static Socket Listener;

        public static int Start()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, AppSettings.ServicePort);

            Listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Listener.Bind(localEndPoint);
                Listener.Listen(100);

                Task.Run(() => ListenForConnections());
            }
            catch
            {
                ConsoleEx.Error($"Failed to initialize ArbiterService on port {AppSettings.ServicePort}");
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
            Client client = GetIPFromSocket(handler);

            ConsoleEx.WriteLine($"[{client.IpAddress}] Connected on port {client.Port}");

            StateObject state = new StateObject();
            state.WorkSocket = handler;
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
                    Client client = GetIPFromSocket(handler);

                    content = content.Replace("<EOF>", "");
                    string response = ProcessData(content, client);
#if DEBUG
                    ConsoleEx.WriteLine($"[{client.IpAddress}] Read all data, returning response", ConsoleColor.DarkBlue)
#endif
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
                ConsoleEx.WriteLine($"[Tadah.Arbiter] Error on ArbiterService::SendCallback - {ex.ToString()}", ConsoleColor.Red);
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
            if (!TadahSignature.VerifyData(data))
            {
                return "";
            }

            TadahMessage Request = JsonConvert.DeserializeObject<TadahMessage>(data);

            switch (Request.Operation)
            {
                case "OpenJob":
                    if (JobManager.GetJobFromId(Request.JobId) != null)
                    {
                        ConsoleEx.WriteLine($"[{client.IpAddress}] Tried opening job '{Request.JobId}' - it already exists", ConsoleColor.Yellow);
                        return "{\"Operation\":\"CloseJob\", \"Status\":\"Error\", \"Message\":\"Job already exists\"}";
                    }
                    else
                    {
                        Task.Run(() => JobManager.OpenJob(Request.JobId, Request.PlaceId, Request.Version));
                        return "{\"Operation\":\"OpenJob\", \"Status\":\"OK\"}";
                    }

                case "CloseJob":
                    if (JobManager.GetJobFromId(Request.JobId) == null)
                    {
                        ConsoleEx.WriteLine($"[{client.IpAddress}] Tried closing job '{Request.JobId}' - it doesn't exists", ConsoleColor.Yellow);
                        return "{\"Operation\":\"CloseJob\", \"Status\":\"Error\", \"Message\":\"Job does not exist\"}";
                    }
                    else
                    {
                        Task.Run(() => JobManager.CloseJob(Request.JobId));
                        return "{\"Operation\":\"CloseJob\", \"Status\":\"OK\"}";
                    }

                case "ExecuteScript":
                    if (JobManager.GetJobFromId(Request.JobId) == null)
                    {
                        ConsoleEx.WriteLine($"[{client.IpAddress}] Tried executing script on job '{Request.JobId}' - it doesn't exists", ConsoleColor.Yellow);
                        return "{\"Operation\":\"CloseJob\", \"Status\":\"Error\", \"Message\":\"Job does not exist\"}";
                    }
                    else
                    {
                        Task.Run(() => JobManager.ExecuteScript(Request.JobId, Request.Script));
                        return "{\"Operation\":\"CloseJob\", \"Status\":\"OK\"}";
                    }

                default:
                    ConsoleEx.WriteLine($"[{client.IpAddress}] Invalid operation '{Request.Operation}'", ConsoleColor.Yellow);
                    return "";
            }
        }

        private static Client GetIPFromSocket(Socket socket)
        {
            IPEndPoint remote = socket.RemoteEndPoint as IPEndPoint;
            IPEndPoint local = socket.LocalEndPoint as IPEndPoint;

            if (remote != null)
            {
                return new Client(remote.Address.ToString(), remote.Port);
            }

            if (local != null)
            {
                return new Client(local.Address.ToString(), local.Port);
            }

            throw new Exception("Failed to receive IP info from socket");
        }
    }
}
