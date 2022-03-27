using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

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

        public static int GetOutboundTraffic()
        {
            return 0;
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

            return $"{host}://{AppSettings.BaseUrl}{path}";
        }

        public static string GetGameserverScript(string jobId, int placeId, int port, bool returnData = false)
        {
            string url = $"/{jobId}/script?placeId={placeId}&port={port}&maxPlayers=10";

            if (!returnData)
            {
                // Since this is called on Roblox, we must pass our key
                return ConstructUrl(url + $"&{AppSettings.AccessKey}", false);
            }

            return Request(ConstructUrl(url), HttpMethod.Get);
        }

        public static string Request(string uri, HttpMethod method, HttpContent content = null)
        {
            using (var message = new HttpRequestMessage(method, ConstructUrl(uri)))
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AppSettings.AccessKey);

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
                { "output", output }
            };

            if (AppSettings.GameserverId == String.Empty)
            {
                LogsToSend.Add(data);
                return;
            }

            if (LogsToSend.Count > 0)
            {
                foreach (Dictionary<string, string> log in LogsToSend)
                {
                    Request($"/{AppSettings.GameserverId}/log", HttpMethod.Post, new FormUrlEncodedContent(log));
                }
            }

            Request($"/{AppSettings.GameserverId}/log", HttpMethod.Post, new FormUrlEncodedContent(data));
        }

        public static void UpdateState(GameServerState state)
        {
            Request($"/{AppSettings.GameserverId}/status?state={((int) state).ToString()}", HttpMethod.Get);
        }

        public static void Fatal(string exception)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "exception", exception }
            };

            Request($"/{AppSettings.GameserverId}/fatal", HttpMethod.Post, new FormUrlEncodedContent(data));
        }

        public static void StartResourceReporter()
        {
            while (true)
            {
                string ram = GetAvailableMemory().ToString();
                string cpu = GetCpuUsage().ToString();
                string inbound = GetInboundTraffic().ToString();
                string outbound = GetOutboundTraffic().ToString();

                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "cpu", cpu },
                    { "ram", ram },
                    { "inbound", inbound },
                    { "outbound", outbound }
                };

                FormUrlEncodedContent content = new FormUrlEncodedContent(data);

                Request($"/{AppSettings.GameserverId}/resources", HttpMethod.Post, content);

                Thread.Sleep(15000);
            }
        }

        public static void UpdateJob(string jobId, string status, int port = 0)
        {
            string parameters = $"status={status}";

            if (port != 0)
            {
                parameters += $"&machineAddress={AppSettings.MachineAddress}";
            }

            Request($"/{jobId}/update?{parameters}", HttpMethod.Get);
        }

        public static string GetGameserverId()
        {
            return Request($"/identify?name={HttpUtility.UrlEncode(Environment.MachineName)}", HttpMethod.Get);
        }
    }
}
