using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Tadah.Arbiter
{
    public class WebManager
    {
        private static readonly HttpClient WebClient = new HttpClient();

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

        public static string ConstructUrl(string path, bool https = true)
        {
            string host = "http";
            if (https) host += "s";

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

        public static void SetMarker(bool online)
        {
            Request($"/{AppSettings.GameserverID}/marker?status={(online ? 1 : 0)}", HttpMethod.Get);
        }

        public static void StartResourceReporter()
        {
            while (true)
            {
                string availableRAM = GetAvailableMemory().ToString();
                string cpuUsage = GetCpuUsage().ToString();

                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "cpuUsage", cpuUsage },
                    { "availableMemory", availableRAM },
                };

                FormUrlEncodedContent content = new FormUrlEncodedContent(data);

                Request($"/{AppSettings.GameserverID}/report-resources", HttpMethod.Post, content);

                Thread.Sleep(30000);
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
    }
}
