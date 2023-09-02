namespace Kiseki.Arbiter;

public static class Monitor
{
    public const int REPORT_TIMEOUT = 15 * 1000; // Measured in milliseconds

    public static void Start()
    {
        Task.Run(async () => {
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(REPORT_TIMEOUT));

            while (await timer.WaitForNextTickAsync())
            {
                Report();
            }
        });
    }

    public static void Report()
    {
        string ram = GetAvailableMemory().ToString();
        string cpu = GetCpuUsage().ToString();
        Tuple<int, int> traffic = GetNetworkTraffic();

        Web.ReportResources(ram, cpu, traffic.Item1.ToString(), traffic.Item2.ToString());
    }

    private static int GetAvailableMemory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            PerformanceCounter performance = new("Memory", "Available MBytes");

            return (int)performance.NextValue();
        }

        return (int)(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1048576.0);
    }

    private static int GetCpuUsage()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            PerformanceCounter performance = new("Processor", "% Processor Time", "_Total");
            performance.NextValue(); // always zero

            Thread.Sleep(500);

            return (int)Math.Round(performance.NextValue());
        }

        var startTime = DateTime.UtcNow;
        var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
        
        Thread.Sleep(500);

        var endTime = DateTime.UtcNow;
        var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime; var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds; var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
        
        return (int)(cpuUsageTotal * 100);
    }

    private static Tuple<int, int> GetNetworkTraffic()
    {
        if (!NetworkInterface.GetIsNetworkAvailable())
        {
            return new Tuple<int, int>(0, 0);
        }

        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

        int sent = 0;
        int received = 0;

        foreach (NetworkInterface ni in interfaces)
        {
            sent += (int)ni.GetIPStatistics().BytesSent;
            received += (int)ni.GetIPStatistics().BytesReceived;
        }

        return new Tuple<int, int>(sent, received);
    }
}