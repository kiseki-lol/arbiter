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

            return $"{host}{path}";
        }

        public static string GetGameserverScript(string JobID, int PlaceID, int Port)
        {
            // Since this is called on Roblox, we must pass our key
            return ConstructUrl($"/{JobID}/script?placeId={PlaceID}&port={Port}&maxPlayers=10&{AppSettings.AccessKey}", false);
        }

        public static Task<HttpResponseMessage> Request(string uri, HttpMethod method, HttpContent content = null)
        {
            using (var message = new HttpRequestMessage(method, ConstructUrl(uri)))
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AppSettings.AccessKey);

                if (content != null && method == HttpMethod.Post)
                {
                    message.Content = content;
                }

                return WebClient.SendAsync(message);
            }
        }

        public static void SetMarker(bool Online)
        {
            int OnlineInt = Online ? 1 : 0;
            Request($"/{AppSettings.GameserverID}/marker?status={OnlineInt}", HttpMethod.Get);
        }

        public static void StartResourceReporter()
        {
            while (true)
            {
                string AvailableMemory = GetAvailableMemory().ToString();
                string CpuUsage = GetCpuUsage().ToString();

                Dictionary<string, string> FormData = new Dictionary<string, string>
                {
                    { "cpuUsage", CpuUsage },
                    { "availableMemory", AvailableMemory },
                };

                FormUrlEncodedContent FormContent = new FormUrlEncodedContent(FormData);

                Request($"/{AppSettings.GameserverID}/report-resources", HttpMethod.Post, FormContent);

                Thread.Sleep(30000);
            }
        }

        public static void UpdateJob(string JobID, string Status, int Port = 0)
        {
            string parameters = $"status={Status}";

            if (Port != 0)
            {
                parameters += $"&machineAddress={AppSettings.MachineAddress}";
            }

            Request($"/{JobID}/update?{parameters}", HttpMethod.Get);
        }
    }
}
