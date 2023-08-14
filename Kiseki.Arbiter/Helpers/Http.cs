namespace Kiseki.Arbiter.Helpers;

using System.Text.Json;

public static class Http
{
    public static async Task<T?> GetJson<T>(string url)
    {
        try
        {
            string json = await Web.HttpClient.GetStringAsync(url);

            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public static async Task<T?> PostJson<T>(string url, Dictionary<string, string> data)
    {
        try
        {
            var result = await Web.HttpClient.PostAsync(url, new FormUrlEncodedContent(data));
            string json = await result.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }
}