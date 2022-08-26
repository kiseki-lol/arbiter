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
        public static List<Job> OpenJobs = new();

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        public static string GetWindowTitle(IntPtr hWnd)
        {
            var length = GetWindowTextLength(hWnd) + 1;
            var title = new StringBuilder(length);
            GetWindowText(hWnd, title, length);

            return title.ToString();
        }

        public static string[] GetCommandLine(Proto.ClientVersion version, string scriptUrl, string jobId)
        {
            switch (version)
            {
                case Proto.ClientVersion.Taipei:
                    return new string[] { "Versions\\Taipei\\TadahServer.exe", $"-a https://tadah.rocks/Login/Negotiate.ashx -t 0 -j {scriptUrl} -jobId {jobId}" };

                case Proto.ClientVersion.Tampa:
                    throw new Exception("Attempt to get command line for TampaJob");

                default:
                    throw new Exception("Attempt to get command line for invalid version");
            }
        }

        public static int GetAvailablePort()
        {
            int port = int.Parse(Configuration.AppSettings["BasePort"]);

            for (int i = 0; i < Configuration.BasePlaceJobPort; i++)
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

        public static Job OpenJob(string jobId, uint placeId, Proto.ClientVersion version)
        {
            Job job;
            int port = GetAvailablePort();

            if (version == Proto.ClientVersion.Tampa)
            {
                job = new Tampa.Job(jobId, placeId, version, port);
            }
            else if (version == Proto.ClientVersion.Taipei)
            {
                job = new Taipei.Job(jobId, placeId, version, port);
            }
            else
            {
                throw new Exception("Invalid ClientVersion");
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
            if (!int.TryParse(version.ToString(), out int result))
            {
                return false;
            }

            return Enum.IsDefined(typeof(Proto.ClientVersion), result);
        }

        public static void MonitorCrashedJobs()
        {
            while (true)
            {
                IntPtr hWnd = FindWindow(null, "Tadah Crash");
                GetWindowThreadProcessId(hWnd, out uint processId);

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
                        if (job is Tampa.Job)
                        {
                            continue;
                        }

                        if (job.Status == JobStatus.Pending || job.Status == JobStatus.Monitored)
                        {
                            continue;
                        }

                        if (job.Version == Proto.ClientVersion.Taipei && (Unix.From(job.TimeStarted) + 5 < Unix.Now()) && !GetWindowTitle(job.Process.MainWindowHandle).Contains("Place1"))
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
                    Log.Write($"[JobManager::MonitorUnresponsiveJobs] InvalidOperationException - {ex.Message}", LogSeverity.Debug);
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
            Tampa.ProcessManager.CloseAllProcesses();
        }

        public static void MonitorUnresponsiveJob(Job job)
        {
            if (job is Job)
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
