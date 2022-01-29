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
                case 2009:
                    return new string[] { "Gameservers\\2009\\TadahServer.exe", $"-a {WebManager.ConstructUrl("/")} -t 0 -j {scriptUrl}" };

                case 2013:
                    return new string[] { "Gameservers\\2013\\TadahServer.exe", $"-a {WebManager.ConstructUrl("/")} -t 0 -j {scriptUrl}" };

                default:
                    return new string[] { };
            }
        }

        public static int GetAvailablePort()
        {
            int Port = AppSettings.BasePort;

            for (int i = 0; i < AppSettings.MaximumJobs; i++)
            {
                if (OpenJobs.Find(Job => Job.Port == Port) == null)
                    break;
                else
                    Port++;
            }

            return Port;
        }

        public static Job OpenJob(string jobId, int placeId, int version)
        {
            Job job;
            int port = GetAvailablePort();

            if (version == 2016)
            {
                job = new RccServiceJob(jobId, placeId, version, port, 86400, 0);
            }
            else
            {
                job = new MFCJob(jobId, placeId, version, port);
            }

            OpenJobs.Add(job);
            job.Start();

            return job;
        }

        public static void CloseJob(string jobId)
        {
            Job JobToClose = GetJobFromId(jobId);
            if (JobToClose == null) return;

            JobToClose.Close();
            OpenJobs.Remove(JobToClose);
        }

        public static void ExecuteScript(string jobId, string script)
        {
            Job JobToExecute = GetJobFromId(jobId);
            if (JobToExecute == null) return;

            JobToExecute.ExecuteScript(script);
        }

        public static Job GetJobFromId(string jobId)
        {
            return OpenJobs.Find(job => job.Id == jobId);
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
                        if (crashedJob is RccServiceJob)
                        {
                            crashedJob.Process.Kill();
                        }
                        else
                        {
                            crashedJob.Status = JobStatus.Crashed;
                            crashedJob.Close();
                        }

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
                    foreach (Job OpenJob in OpenJobs)
                    {
                        if (OpenJob.Status == JobStatus.Pending || OpenJob.Status == JobStatus.Monitored) continue;

                        if (OpenJob.Version == 2009 && (Unix.From(OpenJob.TimeStarted) + 5 < Unix.GetTimestamp()) && !GetWindowTitle(OpenJob.Process.MainWindowHandle).Contains("Place1"))
                        {
                            OpenJob.IsRunning = false;
                        }

                        if (OpenJob.IsRunning || OpenJob.Process.HasExited)
                        {
                            OpenJob.Close();
                            OpenJobs.Remove(OpenJob);
                            continue;
                        }

                        if (OpenJob.Process.Responding) continue;

                        Task.Run(() => MonitorUnresponsiveJob(OpenJob));
                    }
                }
                catch (InvalidOperationException)
                {

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
        }

        public static void MonitorUnresponsiveJob(Job UnresponsiveJob)
        {
            Log.Write($"[JobManager] '{UnresponsiveJob.Id}' is not responding! Monitoring...", LogSeverity.Warning);
            UnresponsiveJob.Status = JobStatus.Monitored;

            for (int i = 1; i <= 30; i++)
            {
                Thread.Sleep(1000);

                if (UnresponsiveJob.Process.Responding)
                {
                    Log.Write($"[JobManager] '{UnresponsiveJob.Id}' has recovered from its unresponsive status!", LogSeverity.Information);
                    UnresponsiveJob.Status = JobStatus.Started;
                    break;
                }
                else if (i == 30)
                {
                    Log.Write($"[JobManager] '{UnresponsiveJob.Id}' has been unresponsive for over 30 seconds. Closing Job...", LogSeverity.Warning);
                    UnresponsiveJob.Status = JobStatus.Crashed;
                    UnresponsiveJob.Close();

                    OpenJobs.Remove(UnresponsiveJob);
                    break;
                }
            }

            return;
        }
    }
}
