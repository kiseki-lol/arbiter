namespace Kiseki.Arbiter;

public static class ResourceReporter
{
    private static readonly CancellationTokenSource TokenSource = new();

    public static void Start()
    {
        const string LOG_IDENT = "ResourceReporter::Start";

        Task.Run(async () => {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
            while (await timer.WaitForNextTickAsync())
            {
                Report();
            }
        }, TokenSource.Token);

        Logger.Write(LOG_IDENT, "OK!", LogSeverity.Debug);
    }

    public static void Stop()
    {
        TokenSource.Cancel();
    }

    public static void Report()
    {
        int timestamp = DateTime.UtcNow.ToUnixTime();
        int ram = GetAvailableMemory();
        int cpu = GetCpuUsage();

        Web.ReportResources(timestamp, ram, cpu);
    }

    private static int GetAvailableMemory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            PerformanceCounter performance = new("Memory", "Available MBytes");

            return (int)performance.NextValue();
        }

        return -1;
    }

    private static int GetCpuUsage()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            PerformanceCounter performance = new("Processor Information", "% Processor Utility", "_Total");
            performance.NextValue();

            Thread.Sleep(1000);

            return (int)performance.NextValue();
        }

        return -1;
    }
}