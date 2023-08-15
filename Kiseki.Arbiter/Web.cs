namespace Kiseki.Arbiter;

using System.Net.Http.Headers;
using System.Text.Json;

public static class Web
{
    public const int RESPONSE_FAILURE = -1;
    public const int RESPONSE_SUCCESS = 0;
    public const int RESPONSE_MAINTENANCE = 1;

    public static Guid? GameServerUuid { get; private set; } = null;
    public static string? CurrentUrl { get; private set; } = null;
    public static bool IsInMaintenance { get; private set; } = false;

    private static readonly List<Dictionary<string, string>> LogQueue = new();
    public static readonly HttpClient HttpClient = new();

    public static bool Initialize(bool setAccessKey = true)
    {
        // All synchronous blocks here, since we're an initializing function!

        if (setAccessKey)
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.GetAccessKey());
        }

        CurrentUrl = IsInMaintenance ? $"{Constants.MAINTENANCE_DOMAIN}.{Constants.BASE_URL}" : Constants.BASE_URL;
        
        int health = GetHealth().GetAwaiter().GetResult();

        if (health != RESPONSE_SUCCESS)
        {
            if (health == RESPONSE_MAINTENANCE)
            {
                IsInMaintenance = true;
            }

            return false;
        }

        // If we've initialized, we certainly may identify ourselves!
        bool identified = Identify().GetAwaiter().GetResult();

        return identified;
    }

    public static bool License(string license)
    {
        Dictionary<string, string> headers;

        try
        {
            headers = JsonSerializer.Deserialize<Dictionary<string, string>>(license)!;
        }
        catch
        {
            return false;
        }
        
        for (int i = 0; i < headers.Count; i++)
        {
            HttpClient.DefaultRequestHeaders.Add(headers.ElementAt(i).Key, headers.ElementAt(i).Value);
        }

        return true;
    }

    private static async Task<bool> Identify()
    {
        string offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).ToString();
        offset = offset[..^3]; // "-07:00:00" -> "-07:00"
        
        Dictionary<string, string> data = new()
        {
            { "machine_name", Environment.MachineName },
            { "utc_offset", offset }
        };

        try
        {
            var response = await Helpers.Http.PostJson<Models.Identification>(FormatUrl("/arbiter/identify"), data);
            GameServerUuid = response?.GameServerUuid ?? null;
        }
        catch
        {
            return false;
        }

        return true;
    }

    public static string FormatUrl(string path)
    {
        string scheme = "https";

#if DEBUG
        scheme = "http";
#endif

        return $"{scheme}://{CurrentUrl}{path}";
    }

    public static string FormatServerScriptUrl(string jobId, uint placeId, int port)
    {
        return FormatUrl($"/arbiter/job/{jobId}/script?placeId={placeId}&port={port}&key={Settings.GetAccessKey()}");
    }

    public static async Task<int> GetHealth()
    {
        var response = await Helpers.Http.GetJson<Models.Health>(FormatUrl("/health"));
        
        return response?.Status ?? RESPONSE_FAILURE;
    }

    public static async Task ReportFatal(DateTime timestamp, string exception)
    {
        string url = FormatUrl($"/arbiter/{GameServerUuid}/fatal");

        Dictionary<string, string> data = new()
        {
            { "timestamp", timestamp.ToUnixTime().ToString() },
            { "exception", exception }
        };

        await Helpers.Http.PostJson<object>(url, data);
    }

    public static async Task ReportLog(DateTime timestamp, LogSeverity severity, string message)
    {
        string url = FormatUrl($"/arbiter/{GameServerUuid}/log");

        Dictionary<string, string> data = new()
        {
            { "timestamp", timestamp.ToUnixTime().ToString() },
            { "severity", ((int)severity).ToString() },
            { "message", message }
        };

        if (GameServerUuid == null)
        {
            LogQueue.Add(data);
            return;
        }

        if (LogQueue.Count > 0)
        {
            foreach (var log in LogQueue)
            {
                await Helpers.Http.PostJson<object>(url, log);
            }

            LogQueue.Clear();
        }

        await Helpers.Http.PostJson<object>(url, data);
    }

    public static async Task UpdateGameServerStatus(GameServerStatus state)
    {
        string url = FormatUrl($"/arbiter/{GameServerUuid}/status");

        Dictionary<string, string> data = new()
        {
            { "status", ((int)state).ToString() }
        };

        await Helpers.Http.PostJson<object>(url, data);
    }

    public static async Task UpdateJob(string jobId, JobStatus status, int port = -1)
    {
        string url = FormatUrl($"/arbiter/job/{jobId}/status");

        Dictionary<string, string> data = new()
        {
            { "status", ((int)status).ToString() }
        };

        if (port != -1)
        {
            // This means the game is actually up.
            data.Add("machine_address", Settings.GetMachineAddress());
            data.Add("port", port.ToString());
        }

        await Helpers.Http.PostJson<object>(url, data);
    }
}