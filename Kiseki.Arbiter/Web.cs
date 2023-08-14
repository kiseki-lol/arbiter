namespace Kiseki.Arbiter;

using System.Net.Http.Headers;
using System.Text.Json;

public static class Web
{
    public const int RESPONSE_FAILURE = -1;
    public const int RESPONSE_SUCCESS = 0;
    public const int RESPONSE_MAINTENANCE = 1;

    public static Guid? GameServerId { get; private set; } = null;
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
        
        Task<int> health = GetHealth();

        if (health.Result != RESPONSE_SUCCESS)
        {
            if (health.Result == RESPONSE_MAINTENANCE)
            {
                IsInMaintenance = true;
            }

            return false;
        }

        // If we've initialized, we certainly may identify ourselves!
        Task<bool> identification = Identify();

        return identification.Result;
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
            var response = await Helpers.Http.PostJson<Models.Identification>(FormatUrl("/api/arbiter/identify"), data);
            GameServerId = response?.GameServerId ?? null;
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
        return FormatUrl($"/api/arbiter/jobs/{jobId}/script?placeId={placeId}&port={port}&key={Settings.GetAccessKey()}");
    }

    public static async Task<int> GetHealth()
    {
        var response = await Helpers.Http.GetJson<Models.Health>(FormatUrl("/api/health"));
        
        return response?.Status ?? RESPONSE_FAILURE;
    }

    public static async Task ReportFatal(DateTime timestamp, string exception)
    {
        string url = FormatUrl($"/api/arbiter/{GameServerId}/fatal");

        Dictionary<string, string> data = new()
        {
            { "timestamp", timestamp.ToUniversalIso8601() },
            { "exception", exception }
        };

        await Helpers.Http.PostJson<object>(url, data);
    }

    public static async Task ReportLog(DateTime timestamp, LogSeverity severity, string message)
    {
        string url = FormatUrl($"/api/arbiter/{GameServerId}/log");

        Dictionary<string, string> data = new()
        {
            { "timestamp", timestamp.ToUniversalIso8601() },
            { "severity", ((int)severity).ToString() },
            { "message", message }
        };

        if (GameServerId == null)
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
        string url = FormatUrl($"/api/arbiter/{GameServerId}/status");

        Dictionary<string, string> data = new()
        {
            { "status", ((int)state).ToString() }
        };

        await Helpers.Http.PostJson<object>(url, data);
    }

    public static async Task UpdateJob(string jobId, JobStatus status, int port = -1)
    {
        string url = FormatUrl($"/api/arbiter/jobs/{jobId}");

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