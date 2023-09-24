namespace Kiseki.Arbiter;

public static class Web
{
    public static Guid? GameServerUuid { get; private set; } = null;
    public static string? CurrentUrl { get; private set; } = null;
    public static bool IsConnected { get; private set; } = false;
    public static bool IsInMaintenance { get; private set; } = false;

    private static readonly List<Dictionary<string, string>> LogQueue = new();
    public static readonly HttpClient HttpClient = new();

    public static void Initialize(bool setAccessKey = true)
    {
        const string LOG_IDENT = "Web::Initialize";

        if (setAccessKey)
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.GetAccessKey());
        }

        CurrentUrl = IsInMaintenance ? $"{Constants.MAINTENANCE_DOMAIN}.{Constants.BASE_URL}" : Constants.BASE_URL;
        
        HealthCheckStatus status = GetHealthStatus();
        if (status != HealthCheckStatus.Success)
        {
            if (status == HealthCheckStatus.Maintenance)
            {
                IsInMaintenance = true;
            }

            return;
        }

        Logger.Write(LOG_IDENT, $"Connected to {Constants.PROJECT_NAME}!", LogSeverity.Debug);

        bool identified = Identify();

        Logger.Write(LOG_IDENT, $"{(identified ? "Successfully identified!" : "Failed identification check.")}", LogSeverity.Debug);

        if (!identified)
        {
            return;
        }

        IsConnected = true;

        if (identified && LogQueue.Count > 0)
        {
            foreach (var log in LogQueue)
            {
                Http.PostJson<object>(FormatUrl($"/arbiter/report/log"), log);
            }

            Logger.Write(LOG_IDENT, $"Pushed {LogQueue.Count} queued log(s)!", LogSeverity.Debug);

            LogQueue.Clear();
        }

        Logger.Write(LOG_IDENT, $"OK!", LogSeverity.Debug);
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
            var response = Http.PostJson<Identification>(FormatUrl("/arbiter/identify"), data);
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
        Http.GetJson<object>(FormatUrl($"/arbiter/report/ping"));
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

    public static HealthCheckStatus GetHealthStatus()
    {
        var response = Http.GetJson<HealthCheck>(FormatUrl("/health-check"));
        
        return response?.Status ?? HealthCheckStatus.Failure;
    }

    public static void ReportFatal(DateTime timestamp, string exception)
    {
        string url = FormatUrl($"/arbiter/report/fatal");

        Dictionary<string, string> data = new()
        {
            { "timestamp", timestamp.ToUnixTime().ToString() },
            { "exception", exception }
        };

        Http.PostJson<object>(url, data);

        UpdateGameServerStatus(GameServerStatus.Offline);
    }

    public static void ReportLog(DateTime timestamp, LogSeverity severity, string message)
    {
        string url = FormatUrl($"/arbiter/report/log");

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

        Http.PostJson<object>(url, data);
    }

    public static void ReportResources(int timestamp, int ram, int cpu)
    {
        string url = FormatUrl($"/arbiter/report/resources");

        Dictionary<string, string> data = new()
        {
            { "timestamp", timestamp.ToString() },
            { "ram", ram.ToString() },
            { "cpu", cpu.ToString() }
        };

        Http.PostJson<object>(url, data);
    }

    public static void UpdateGameServerStatus(GameServerStatus state)
    {
        string url = FormatUrl($"/arbiter/report/status");

        Dictionary<string, string> data = new()
        {
            { "status", ((int)state).ToString() }
        };

        Http.PostJson<object>(url, data);
    }

    public static void UpdateJob(string jobId, JobStatus status, int port)
    {
        string url = FormatUrl($"/arbiter/job/{jobId}/status");

        Dictionary<string, string> data = new()
        {
            { "status", ((int)status).ToString() },
            { "machine_address", Settings.GetMachineAddress() },
            { "port", port.ToString() }
        };

        Http.PostJson<object>(url, data);
    }

    public static void UpdateJobTimestamp(string jobId, string key, DateTime timestamp)
    {
        string url = FormatUrl($"/arbiter/job/{jobId}/timestamp");

        Dictionary<string, string> data = new()
        {
            { "key", key },
            { "timestamp", timestamp.ToUnixTime().ToString() }
        };

        Http.PostJson<object>(url, data);
    }
}