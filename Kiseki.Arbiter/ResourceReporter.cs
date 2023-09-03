namespace Kiseki.Arbiter;

public static class ResourceReporter
{
    public static void Start()
    {
        const string LOG_IDENT = "ResourceReporter::Start";

        Task.Run(async () => {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            while (await timer.WaitForNextTickAsync())
            {
                Report();
            }
        });

        Logger.Write(LOG_IDENT, "OK!", LogSeverity.Debug);
    }

    public static void Report()
    {
        string ram = GetAvailableMemory().ToString();
        string cpu = GetCpuUsage().ToString();

        Web.ReportResources(ram, cpu);
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