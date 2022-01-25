using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Tadah.Arbiter
{
    internal class ArbiterService
    {
        private static TcpListener Service = new TcpListener(IPAddress.Any, AppSettings.ServicePort);

        public static int Start()
        {
            Service.Start();
            Task.Run(() => ListenForConnections());
            return AppSettings.ServicePort;
        }

        public static void Stop()
        {
            Service.Stop();
        }

        private static void ListenForConnections()
        {
            while (true)
            {
                TcpClient Client = Service.AcceptTcpClient();
                string ClientAddress = GetClientAddress(Client);
#if DEBUG
                ConsoleEx.WriteLine($"[Tadah.Arbiter] Service received a new connection from '{ClientAddress}'", ConsoleColor.Blue);
#endif
                Task.Run(() => HandleConnection(Client, ClientAddress));
            }
        }

        private static void HandleConnection(TcpClient Client, string ClientAddress)
        {
            NetworkStream Stream = Client.GetStream();
            StreamReader Reader = new StreamReader(Stream);

            try
            {
                while (Client.Connected)
                {
                    string IncomingCommand = Reader.ReadLine();

                    if (IncomingCommand == null)
                    {
#if DEBUG
                        ConsoleEx.WriteLine($"[{ClientAddress}] Client disconnected", ConsoleColor.Blue);
#endif
                        Stream.Close();
                        Client.Close();
                    }
                    else
                    {
#if DEBUG
                        ConsoleEx.WriteLine($"[{ClientAddress}] Received command '{IncomingCommand}'", ConsoleColor.Blue);
#endif
                        ProcessCommand(Stream, ClientAddress, IncomingCommand);
                    }
                }
            }
            catch (IOException)
            {
#if DEBUG
                ConsoleEx.WriteLine($"$[{ClientAddress}] Client disconnected", ConsoleColor.Blue);
#endif
            }
        }

        private static void WriteToClient(NetworkStream Stream, string ClientAddress, string Response)
        {
#if DEBUG
            ConsoleEx.WriteLine($"[{ClientAddress}] Writing to client with '{Response}'", ConsoleColor.Blue);
#endif
            StreamWriter Writer = new StreamWriter(Stream);
            Writer.WriteLine(Response);
            Writer.Flush();
        }

        private static void ProcessCommand(NetworkStream Stream, string ClientAddress, string Data)
        {
            if (!Data.StartsWith("%"))
            {
#if DEBUG
                ConsoleEx.WriteLine($"[{ClientAddress}] Bad data received", ConsoleColor.Red);
#endif
                return;
            }

            // get signature
            string message = null;
            string signature = null;

            try
            {
                signature = Data.Substring(1); // remove first %
                signature = signature.Substring(0, signature.IndexOf("%", StringComparison.Ordinal)); // get all data before the next %; essentially extracts the base64 signature data

                message = Data.Substring(signature.Length + 2); // remove the signature by starting to read the data at the signatures length plus two (two %s ; the signature delimiter)
            }
            catch
            {
#if DEBUG
                ConsoleEx.WriteLine($"[{ClientAddress}] Bad signature", ConsoleColor.Red);
#endif

                return;
            }
            

            if (!TadahSignature.Verify(message, signature))
            {
#if DEBUG
                ConsoleEx.WriteLine($"[{ClientAddress}] Bad signature", ConsoleColor.Red);
#endif
                return;
            }

            TadahMessage Request = JsonConvert.DeserializeObject<TadahMessage>(message);
#if DEBUG
            ConsoleEx.WriteLine($"[{ClientAddress}] Successfully verified message!", ConsoleColor.Green);
#endif

            switch (Request.Operation)
            {
                case "OpenJob":
                    if (JobManager.GetJobFromID(Request.JobID) != null)
                    {
                        WriteToClient(Stream, ClientAddress, "{\"Operation\":\"CloseJob\", \"Status\":\"Error\", \"Message\":\"Job already exists\"}");
                    }
                    else
                    {
                        Task.Run(() => JobManager.OpenJob(Request.JobID, Request.Version, Request.PlaceID));
                        WriteToClient(Stream, ClientAddress, "{\"Operation\":\"OpenJob\", \"Status\":\"OK\"}");
                    }

                    break;

                case "CloseJob":
                    if (JobManager.GetJobFromID(Request.JobID) == null)
                    {
                        WriteToClient(Stream, ClientAddress, "{\"Operation\":\"CloseJob\", \"Status\":\"Error\", \"Message\":\"Job does not exist\"}");
                    }
                    else
                    {
                        Task.Run(() => JobManager.CloseJob(Request.JobID));
                        WriteToClient(Stream, ClientAddress, "{\"Operation\":\"CloseJob\", \"Status\":\"OK\"}");
                    }

                    break;

                default:
                    ConsoleEx.WriteLine($"[ArbiterService/{ClientAddress}] Invalid command received", ConsoleColor.Blue);
                    break;
            }
        }

        private static string GetClientAddress(TcpClient Client)
        {
            return Client.Client.RemoteEndPoint.ToString();
        }
    }
}
