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
                Http.PostJson<object>(FormatUrl($"/api/arbiter/report/log"), log);
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
            var response = Http.PostJson<Identification>(FormatUrl("/api/arbiter/identify"), data);
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
        Http.GetJson<object>(FormatUrl($"/api/arbiter/report/ping"));
    }

    public static string FormatUrl(string path, string? subdomain = null, bool forceHttp = false)
    {
        string scheme = "https";
        string url = subdomain == null ? CurrentUrl! : $"{subdomain!}.{CurrentUrl!}";

#if DEBUG
        scheme = "http";
#endif

        if (forceHttp)
        {
            scheme = "http";
        }

        return $"{scheme}://{url}{path}";
    }

    public static string FormatServerUrl(string path, int port, string jwt)
    {
        string scheme = "http"; // RCC schema will ALWAYS be http
        string url = "127.0.0.1"; // localhost, assuming rcc runs alongside & on same network adap. as arbit

        return $"{scheme}://{url}:{port.ToString()}{path}&t=" + jwt;
    }

    public static HealthCheckStatus GetHealthStatus()
    {
        var response = Http.GetJson<HealthCheck>(FormatUrl("/api/health"));
        
        return response?.Status ?? HealthCheckStatus.Failure;
    }

    public static void ReportFatal(DateTime timestamp, string exception)
    {
        string url = FormatUrl($"/api/arbiter/report/fatal");

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
        string url = FormatUrl($"/api/arbiter/report/log");

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
        string url = FormatUrl($"/api/arbiter/report/resources");

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
        string url = FormatUrl($"/api/arbiter/report/status");

        Dictionary<string, string> data = new()
        {
            { "status", ((int)state).ToString() }
        };

        Http.PostJson<object>(url, data);
    }

    public static void UpdatePlaceJob(string jobUuid, JobStatus status, int? port = null)
    {
        string url = FormatUrl($"/api/arbiter/place-job/{jobUuid}/status");

        Dictionary<string, string> data = new()
        {
            { "status", ((int)status).ToString() }
        };

        if (status == JobStatus.Running)
        {
            data.Add("port", port.ToString()!);
        }

        Http.PostJson<object>(url, data);
    }

    public static void UpdateAssetThumbnail(string jobUuid, uint assetId, string base64Result)
    {
        // no auth, we're just praying that nobody finds this endpoint and knows how requests are
        // formed
        string url = FormatUrl($"/api/arbiter/update-render/asset");

        Dictionary<string, string> data = new()
        {
            { "jobUUId", jobUuid },
            { "assetId", assetId.ToString() },
            { "base64",  base64Result },
        };

        Http.PostJson<object>(url, data);
    }

    public static void UpdateUserThumbnail(string jobUuid, uint assetId, string base64Result, bool isHeadshot)
    {
        // no auth, we're just praying that nobody finds this endpoint and knows how requests are
        // formed
        string url;

        if (isHeadshot)
            url = FormatUrl($"/api/arbiter/update-render/user/head");
        else
            url = FormatUrl($"/api/arbiter/update-render/user/body");

        Dictionary<string, string> data = new()
        {
            { "jobUUId", jobUuid },
            { "userId", assetId.ToString() },
            { "base64",  base64Result },
        };

        Http.PostJson<object>(url, data);
    }

    public static void UpdatePlaceJobTimestamp(string jobUuid, string key, DateTime timestamp)
    {
        string url = FormatUrl($"/api/arbiter/place-job/{jobUuid}/timestamp");

        Dictionary<string, string> data = new()
        {
            { "key", key },
            { "timestamp", timestamp.ToUnixTime().ToString() }
        };

        Http.PostJson<object>(url, data);
    }
}