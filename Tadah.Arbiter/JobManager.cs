using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tadah.Arbiter
{
    public class JobManager
    {
        public static List<Job> OpenJobs = new List<Job>();

        [DllImport("User32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("User32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("User32.dll")]
        static extern int GetWindowTextLength(IntPtr hWnd);

        public static string GetWindowTitle(IntPtr hWnd)
        {
            var length = GetWindowTextLength(hWnd) + 1;
            var title = new StringBuilder(length);
            GetWindowText(hWnd, title, length);
            return title.ToString();
        }

        public static string[] GetCommandLine(int version, string scriptUrl)
        {
            switch (version)
            {
                case 2008:
                    return new string[] { "Gameservers\\2008\\TadahServer.exe", $"-script {scriptUrl}" };

                case 2010:
                case 2011:
                case 2012:
                    return new string[] { "Gameservers\\2013\\TadahServer.exe", $"-a 0 -t 0 -j {scriptUrl}" };

                case 2016:
                    throw new Exception("Attempt to get command line for RccServiceJob");

                default:
                    return new string[] { };
            }
        }

        public static int GetAvailablePort()
        {
            int port = AppSettings.BasePort;

            for (int i = 0; i < AppSettings.MaximumJobs; i++)
            {
                if (OpenJobs.Find(job => job.Port == port) == null)
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

        public static Job OpenJob(string jobId, int placeId, int version)
        {
            Job job;
            int port = GetAvailablePort();

            if (version == 2016)
            {
                job = new RccServiceJob(jobId, placeId, version, port);
            }
            else
            {
                job = new MFCJob(jobId, placeId, version, port);
            }

            OpenJobs.Add(job);
            job.Start();

            return job;
        }

        public static void CloseJob(string jobId, bool forceClose = false)
        {
            Job job = GetJobFromId(jobId);
            if (job == null)
            {
                return;
            }

            job.Close(forceClose);
            OpenJobs.Remove(job);
        }

        public static Job GetJobFromId(string jobId)
        {
            return OpenJobs.Find(job => job.Id == jobId);
        }

        public static bool JobExists(string jobId)
        {
            return GetJobFromId(jobId) != null;
        }

        public static bool IsValidVersion(object version)
        {
            if (!Int32.TryParse(version.ToString(), out int result))
            {
                return false;
            }

            return result == 2009 || result == 2013 || result == 2016;
        }

        public static void MonitorCrashedJobs()
        {
            uint processId;

            while (true)
            {
                IntPtr hWnd = FindWindow(null, "ROBLOX Crash");
                GetWindowThreadProcessId(hWnd, out processId);

                if (processId != 0)
                {
                    Job crashedJob = OpenJobs.Find(Job => Job.Process.Id == processId);
                    if (crashedJob != null)
                    {
                        Log.Write($"[JobManager] '{crashedJob.Id}' has crashed! Closing Job...", LogSeverity.Warning);
                        crashedJob.Status = JobStatus.Crashed;
                        crashedJob.Close();

                        OpenJobs.Remove(crashedJob);
                    }
                }

                Thread.Sleep(5000);
            }
        }

        public static void MonitorUnresponsiveJobs()
        {
            while (true)
            {
                try
                {
                    foreach (Job job in OpenJobs)
                    {
                        if (job is RccServiceJob)
                        {
                            continue;
                        }

                        if (job.Status == JobStatus.Pending || job.Status == JobStatus.Monitored)
                        {
                            continue;
                        }

                        if (job.Version == 2009 && (Unix.From(job.TimeStarted) + 5 < Unix.GetTimestamp()) && !GetWindowTitle(job.Process.MainWindowHandle).Contains("Place1"))
                        {
                            job.IsRunning = false;
                        }

                        if (!job.IsRunning || job.Process.HasExited)
                        {
                            job.Close();
                            OpenJobs.Remove(job);

                            continue;
                        }

                        if (job.Process.Responding)
                        {
                            continue;
                        }

                        Task.Run(() => MonitorUnresponsiveJob(job));
                    }
                }
                catch (InvalidOperationException ex)
                {
#if DEBUG
                    Log.Write($"[JobManager::MonitorUnresponsiveJobs] InvalidOperationException - {ex.Message}", LogSeverity.Debug);
#endif
                }

                Thread.Sleep(5000);
            }
        }

        public static void CloseAllJobs()
        {
            foreach (Job OpenJob in OpenJobs)
            {
                OpenJob.Close();
            }

            OpenJobs.Clear();
            RccServiceProcessManager.CloseAllProcesses();
        }

        public static void MonitorUnresponsiveJob(Job job)
        {
            if (job is RccServiceJob)
            {
                return;
            }

            Log.Write($"[JobManager] '{job.Id}' is not responding! Monitoring...", LogSeverity.Warning);
            job.Status = JobStatus.Monitored;

            for (int i = 0; i <= 30; i++)
            {
                Thread.Sleep(1000);

                if (job.Process.Responding)
                {
                    Log.Write($"[JobManager] '{job.Id}' has recovered from its unresponsive status!", LogSeverity.Information);
                    job.Status = JobStatus.Started;
                    break;
                }
                else if (i == 30)
                {
                    Log.Write($"[JobManager] '{job.Id}' has been unresponsive for over 30 seconds. Closing Job...", LogSeverity.Warning);
                    job.Status = JobStatus.Crashed;
                    job.Close();

                    OpenJobs.Remove(job);
                    break;
                }
            }

            return;
        }
    }
}
