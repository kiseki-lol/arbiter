namespace Kiseki.Arbiter;

using System.Net.NetworkInformation;

public class JobManager
{
    public static List<Job> OpenJobs = new();

    public static int GetAvailableGameserverPort()
    {
        int port = Settings.GetBaseJobPort();

        for (int i = 0; i < ushort.MaxValue - Settings.GetBaseJobPort(); i++)
        {
            if (OpenJobs.Find(job => job.Port == port) == null && !IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners().Any(listener => listener.Port == port))
            {
                break;
            }
            else
            {
                port++;
            }
        }

        return port;
    }

    public static int GetAvailableHttpPort()
    {
        int httpPort = Settings.GetBaseHttpPort();

        for (int i = 0; i < ushort.MaxValue - Settings.GetBaseHttpPort(); i++)
        {
            if (OpenJobs.Find(job => job.HttpPort == httpPort) == null && !IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners().Any(listener => listener.Port == httpPort))
            {
                break;
            }
            else
            {
                httpPort++;
            }
        }

        return httpPort;
    }

    public static void OpenJob(Job job)
    {
        OpenJobs.Add(job);
        job.Start();
    }

    public static void CloseJob(string jobUuid)
    {
        Job? job = OpenJobs.Find(job => job.Uuid == jobUuid);

        if (job == null)
        {
            return;
        }

        job.Close();
        OpenJobs.Remove(job);
    }

    public static void CloseJob(Job job) => CloseJob(job.Uuid);

    public static bool IsJobOpen(string jobUuid)
    {
        return OpenJobs.Find(job => job.Uuid == jobUuid) != null;
    }

    public static bool IsJobOpen(Job job) => IsJobOpen(job.Uuid);

    public static void CloseAllJobs()
    {
        foreach (Job job in OpenJobs)
        {
            job.Close();
        }

        OpenJobs.Clear();
    }
}