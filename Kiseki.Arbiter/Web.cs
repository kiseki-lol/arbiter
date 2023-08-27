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
    public static bool IsConnected { get; private set; } = false;
    public static bool IsInMaintenance { get; private set; } = false;

    private static readonly List<Dictionary<string, string>> LogQueue = new();
    public static readonly HttpClient HttpClient = new();

    public static void Initialize(bool setAccessKey = true)
    {
        if (setAccessKey)
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.GetAccessKey());
        }

        CurrentUrl = IsInMaintenance ? $"{Constants.MAINTENANCE_DOMAIN}.{Constants.BASE_URL}" : Constants.BASE_URL;
        
        int health = GetHealth();

        if (health != RESPONSE_SUCCESS)
        {
            if (health == RESPONSE_MAINTENANCE)
            {
                IsInMaintenance = true;
            }

            return;
        }

#if DEBUG
        Log.Write($"Web::Initialize - Connected!", LogSeverity.Debug);
#endif

        bool identified = Identify();

#if DEBUG
        string state = identified ? "YES" : "NO";
        Log.Write($"Web::Initialize - Identified: {state}!", LogSeverity.Debug);
#endif

        if (identified && LogQueue.Count > 0)
        {
#if DEBUG
            int count = LogQueue.Count + 1; // +1 to account for the log below which will be queued :P
            Log.Write($"Web::Initialize - Pushing {count} queued logs...", LogSeverity.Debug);
#endif

            foreach (var log in LogQueue)
            {
                Helpers.Http.PostJson<object>(FormatUrl($"/arbiter/log"), log);
            }

            LogQueue.Clear();

#if DEBUG
            Log.Write($"Web::Initialize - Pushed {count} logs!", LogSeverity.Debug);
#endif
        }

        IsConnected = true;
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

    private static bool Identify()
    {        
        Dictionary<string, string> data = new()
        {
            { "machine_name", Environment.MachineName }
        };

        try
        {
            var response = Helpers.Http.PostJson<Models.Identification>(FormatUrl("/arbiter/identify"), data);
            GameServerUuid = response?.GameServerUuid ?? null;
        }
        catch
        {
            return false;
        }

        return GameServerUuid != null;
    }

    public static void Ping()
    {
        Helpers.Http.GetJson<object>(FormatUrl($"/arbiter/ping"));
    }

    public static string FormatUrl(string path, string? subdomain = null)
    {
        string scheme = "https";
        string url = subdomain == null ? CurrentUrl! : $"{subdomain!}.{CurrentUrl!}";

#if DEBUG
        scheme = "http";
#endif

        return $"{scheme}://{url}{path}";
    }

    public static string FormatServerScriptUrl(string jobId, uint placeId, int port)
    {
        return FormatUrl($"/arbiter/job/{jobId}/script?placeId={placeId}&port={port}&key={Settings.GetAccessKey()}");
    }

    public static int GetHealth()
    {
        var response = Helpers.Http.GetJson<Models.Health>(FormatUrl("/health"));
        
        return response?.Status ?? RESPONSE_FAILURE;
    }

    public static void ReportFatal(DateTime timestamp, string exception)
    {
        string url = FormatUrl($"/arbiter/fatal");

        Dictionary<string, string> data = new()
        {
            { "timestamp", timestamp.ToUnixTime().ToString() },
            { "exception", exception }
        };

        Helpers.Http.PostJson<object>(url, data);
    }

    public static void ReportLog(DateTime timestamp, LogSeverity severity, string message)
    {
        string url = FormatUrl($"/arbiter/log");

        Dictionary<string, string> data = new()
        {
            { "timestamp", timestamp.ToUnixTime().ToString() },
            { "severity", ((int)severity).ToString() },
            { "message", message }
        };

        if (!IsConnected)
        {
            LogQueue.Add(data);
            return;
        }

        Helpers.Http.PostJson<object>(url, data);
    }

    public static void UpdateGameServerStatus(GameServerStatus state)
    {
        string url = FormatUrl($"/arbiter/status");

        Dictionary<string, string> data = new()
        {
            { "status", ((int)state).ToString() }
        };

        Helpers.Http.PostJson<object>(url, data);
    }

    public static void UpdateJob(string jobId, JobStatus status, int port = -1)
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

        Helpers.Http.PostJson<object>(url, data);
    }
}