namespace Kiseki.Arbiter;

using System.Net.NetworkInformation;

public class JobManager
{
    public static List<Job> OpenJobs = new();

    public static int GetAvailablePort()
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

    public static void OpenJob(Job job)
    {
        OpenJobs.Add(job);
        job.Start();
    }

    public static void CloseJob(string jobId)
    {
        Job? job = OpenJobs.Find(job => job.Id == jobId);

        if (job == null)
        {
            return;
        }

        job.Close();
        OpenJobs.Remove(job);
    }

    public static void CloseJob(Job job) => CloseJob(job.Id);

    public static bool IsJobOpen(string jobId)
    {
        return OpenJobs.Find(job => job.Id == jobId) != null;
    }

    public static bool IsJobOpen(Job job) => IsJobOpen(job.Id);
}