using System.Net.Http.Headers;

namespace Kiseki.Arbiter
{
    internal static class Utility
    {
        public static string Request(string url, HttpMethod method, HttpContent? content = null)
        {
            using var message = new HttpRequestMessage(method, url);
            
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Settings.GetAccessKey());

            if (method == HttpMethod.Post)
            {
                message.Content = content;
            }

            using HttpResponseMessage response = Program.HttpClient.SendAsync(message).Result;
            using HttpContent httpContent = response.Content;

            return httpContent.ReadAsStringAsync().Result;
        }
    }
}