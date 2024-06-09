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
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Headers = { 
                    { HttpRequestHeader.Authorization.ToString(), data },
                    { HttpRequestHeader.Accept.ToString(), "application/json" },
                },
            };

            var response = Web.HttpClient.SendAsync(httpRequestMessage).Result;

            // return JsonSerializer.Deserialize<T>(response);
            return default;
        }
        catch
        {
            return default;
        }
    }
}