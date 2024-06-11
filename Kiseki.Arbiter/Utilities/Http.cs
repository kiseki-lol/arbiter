namespace Kiseki.Arbiter.Utilities;

public static class Http
{
    public static T? GetJson<T>(string url)
    {
        try
        {
            string json = Web.HttpClient.GetStringAsync(url).GetAwaiter().GetResult();

            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public static T? PostJson<T>(string url, Dictionary<string, string> data)
    {
        try
        {
            var result = Web.HttpClient.PostAsync(url, new FormUrlEncodedContent(data)).GetAwaiter().GetResult();
            string json = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public static T? PostJson<T>(string url, string data)
    {
        try
        {
            Web.HttpClient.DefaultRequestHeaders.Remove("Authorization");

            byte[] bytes = Compression.Compress(Encoding.ASCII.GetBytes(data));
            string compress = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            Logger.Write(compress.Length.ToString(), LogSeverity.Warning);
            Logger.Write(data.Length.ToString(), LogSeverity.Warning);

            var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url),
                    Headers = { 
                        { "Authorization", "Hi"},
                    },
                    Content = new StringContent(data, Encoding.UTF8, "application/json")
                };

            var response = Web.HttpClient.SendAsync(httpRequestMessage).Result;
            return default;
        }
        catch (Exception e)
        {   
            Logger.Write($"{e.ToString()}", LogSeverity.Error);
            return default;
        }
    }
}