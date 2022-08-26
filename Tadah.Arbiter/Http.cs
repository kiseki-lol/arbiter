using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace Tadah.Arbiter
{
    public class Http
    {
        private static readonly HttpClient WebClient = new();
        private static readonly List<Dictionary<string, string>> LogsToSend = new();

        public static int GetAvailableMemory()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                PerformanceCounter performance = new("Memory", "Available MBytes");

                return (int)performance.NextValue();
            }

            return (int)(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1048576.0);
        }

        public static int GetCpuUsage()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                PerformanceCounter performance = new("Processor", "% Processor Time", "_Total");
                performance.NextValue(); // this is always gonna be zero

                Thread.Sleep(500);

                return (int)Math.Round(performance.NextValue());
            }

            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            
            Thread.Sleep(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime; var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds; var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            
            return (int)(cpuUsageTotal * 100);
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

        public static string ConstructUrl(string path, bool https = true)
        {
            string scheme = "http";

#if (!DEBUG)
            if (https) scheme += "s";
#endif

            return $"{scheme}://{Configuration.AppSettings["BaseUrl"]}{path}";
        }

        public static string GetGameserverScript(string jobId, uint placeId, int port, bool returnData = false)
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

                using HttpResponseMessage response = WebClient.SendAsync(message).Result;
                using HttpContent httpContent = response.Content;

                return httpContent.ReadAsStringAsync().Result;
            }
        }

        public static void Log(LogSeverity severity, int timestamp, string output)
        {
            Dictionary<string, string> data = new()
            {
                { "severity", ((int)severity).ToString() },
                { "timestamp", timestamp.ToString() },
                { "output", output },
                { "blur", (output.Contains(Configuration.AppSettings["AccessKey"]) ? Configuration.AppSettings["AccessKey"] : "") }
            };

            if (Configuration.Uuid == Guid.Empty)
            {
                LogsToSend.Add(data);
                return;
            }

            if (LogsToSend.Count > 0)
            {
                foreach (Dictionary<string, string> log in LogsToSend)
                {
                    Request($"/{Configuration.Uuid}/log", HttpMethod.Post, new FormUrlEncodedContent(log));
                }

                LogsToSend.Clear();
            }

            Request($"/{Configuration.Uuid}/log", HttpMethod.Post, new FormUrlEncodedContent(data));
        }

        public static void UpdateState(GameServerState state)
        {
            Request($"/{Configuration.Uuid}/status?state={(int)state}", HttpMethod.Get);
        }

        public static void Fatal(string exception)
        {
            Dictionary<string, string> data = new()
            {
                { "exception", exception }
            };

            Request($"/{Configuration.Uuid}/fatal", HttpMethod.Post, new FormUrlEncodedContent(data));
        }

        public static void StartResourceReporter()
        {
            while (true)
            {
                string ram = GetAvailableMemory().ToString();
                string cpu = GetCpuUsage().ToString();
                Tuple<int, int> traffic = GetNetworkTraffic();

                Dictionary<string, string> data = new()
                {
                    { "cpu", cpu },
                    { "ram", ram },
                    { "inbound", traffic.Item1.ToString() },
                    { "outbound", traffic.Item2.ToString() }
                };

                FormUrlEncodedContent content = new(data);

                Request($"/{Configuration.Uuid}/resources", HttpMethod.Post, content);

                Thread.Sleep(15000);
            }
        }

        public static void UpdateJob(string jobId, JobStatus status, int port = 0)
        {
            string parameters = $"status={(int)status}";

            if (port != 0)
            {
                parameters += $"&machine_address={Configuration.AppSettings["MachineAddress"]}";
            }

            Request($"/{jobId}/update?{parameters}", HttpMethod.Get);
        }

        public static Dictionary<string, object> Identify()
        {
            string offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).ToString();
            offset = offset[0 .. ^3]; // "-07:00:00" -> "-07:00"

            Dictionary<string, string> data = new()
            {
                { "machine_name", Environment.MachineName },
                { "utc_offset", offset }
            };

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(Request($"/identify", HttpMethod.Post, new FormUrlEncodedContent(data)));
        }
    }
}
