using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Net.NetworkInformation;

namespace Tadah.Arbiter
{
    public class Http
    {
        private static readonly HttpClient WebClient = new HttpClient();
        private static List<Dictionary<string, string>> LogsToSend = new List<Dictionary<string, string>>();

        public static int GetAvailableMemory()
        {
            PerformanceCounter performance = new PerformanceCounter("Memory", "Available MBytes");
            return (int)performance.NextValue();
        }

        public static int GetCpuUsage()
        {
            PerformanceCounter performance = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            performance.NextValue(); // this is always gonna be zero
            Thread.Sleep(500);
            return (int)Math.Round(performance.NextValue());
        }

        public static Tuple<int, int> GetNetworkTraffic()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return new Tuple<int, int>(0, 0);
            }

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            int sent = 0;
            int received = 0;

            foreach (NetworkInterface ni in interfaces)
            {
                sent += (int)ni.GetIPStatistics().BytesSent;
                received += (int)ni.GetIPStatistics().BytesReceived;
            }

            return new Tuple<int, int>(sent, received);
        }

        public static int GetInboundTraffic()
        {
            return 0;
        }

        public static string ConstructUrl(string path, bool https = true)
        {
            string host = "http";

#if (!DEBUG)
            if (https) host += "s";
#endif

            return $"{host}://{Configuration.AppSettings["BaseUrl"]}{path}";
        }

        public static string GetGameserverScript(string jobId, int placeId, int port, bool returnData = false)
        {
            string url = $"/{jobId}/script?placeId={placeId}&port={port}&maxPlayers=10";

            if (!returnData)
            {
                // Since this is called on Roblox, we must pass our key
                return ConstructUrl(url + $"&{Configuration.AppSettings["AccessKey"]}", false);
            }

            return Request(ConstructUrl(url), HttpMethod.Get);
        }

        public static string Request(string uri, HttpMethod method, HttpContent content = null)
        {
            using (var message = new HttpRequestMessage(method, ConstructUrl(uri)))
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Configuration.AppSettings["AccessKey"]);

                if (content != null && method == HttpMethod.Post)
                {
                    message.Content = content;
                }

                using (HttpResponseMessage response = WebClient.SendAsync(message).Result)
                {
                    using (HttpContent httpContent = response.Content)
                    {
                        return httpContent.ReadAsStringAsync().Result;
                    }
                }
            }
        }

        public static void Log(LogSeverity severity, int timestamp, string output)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "severity", ((int) severity).ToString() },
                { "timestamp", timestamp.ToString() },
                { "output", output },
                { "blur", (output.Contains(Configuration.AppSettings["AccessKey"]) ? Configuration.AppSettings["AccessKey"] : "") }
            };

            if (Configuration.GameserverId == null)
            {
                LogsToSend.Add(data);
                return;
            }

            if (LogsToSend.Count > 0)
            {
                foreach (Dictionary<string, string> log in LogsToSend)
                {
                    Request($"/{Configuration.GameserverId}/log", HttpMethod.Post, new FormUrlEncodedContent(log));
                }

                LogsToSend.Clear();
            }

            Request($"/{Configuration.GameserverId}/log", HttpMethod.Post, new FormUrlEncodedContent(data));
        }

        public static void UpdateState(GameServerState state)
        {
            Request($"/{Configuration.GameserverId}/status?state={((int)state).ToString()}", HttpMethod.Get);
        }

        public static void Fatal(string exception)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "exception", exception }
            };

            Request($"/{Configuration.GameserverId}/fatal", HttpMethod.Post, new FormUrlEncodedContent(data));
        }

        public static void StartResourceReporter()
        {
            while (true)
            {
                string ram = GetAvailableMemory().ToString();
                string cpu = GetCpuUsage().ToString();
                Tuple<int, int> traffic = GetNetworkTraffic();

                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "cpu", cpu },
                    { "ram", ram },
                    { "inbound", traffic.Item1.ToString() },
                    { "outbound", traffic.Item2.ToString() }
                };

                FormUrlEncodedContent content = new FormUrlEncodedContent(data);

                Request($"/{Configuration.GameserverId}/resources", HttpMethod.Post, content);

                Thread.Sleep(15000);
            }
        }

        public static void UpdateJob(string jobId, string status, int port = 0)
        {
            string parameters = $"status={status}";

            if (port != 0)
            {
                parameters += $"&machineAddress={Configuration.AppSettings["MachineAddress"]}";
            }

            Request($"/{jobId}/update?{parameters}", HttpMethod.Get);
        }

        public static string GetGameserverId()
        {
            string offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).ToString();
            offset = offset.Substring(0, offset.Length - 3); // "-07:00:00" -> "-07:00"

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "name", Environment.MachineName },
                { "utc_offset", offset }
            };

            return Request($"/identify", HttpMethod.Post, new FormUrlEncodedContent(data));
        }
    }
}
