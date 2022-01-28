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

        public static string[] GetCommandLine(int Version, string ScriptUrl)
        {
            switch (Version)
            {
                case 2009:
                    return new string[] { "Gameservers\\2009\\TadahServer.exe", $"-a {WebManager.ConstructUrl("/")} -t 0 -j {ScriptUrl}" };

                case 2013:
                    return new string[] { "Gameservers\\2013\\TadahServer.exe", $"-a {WebManager.ConstructUrl("/")} -t 0 -j {ScriptUrl}" };

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

        public static object OpenJob(string JobID, int Version, int PlaceID)
        {
            ConsoleEx.WriteLine($"[JobManager] Opening Job for '{JobID}' ({Version})", ConsoleColor.Blue);

            if (Version != 2016)
            {
                Job NewJob = new Job(JobID, Version, PlaceID, GetAvailablePort());
                OpenJobs.Add(NewJob);
                NewJob.Start();

                return NewJob;
            }

            int port = GetAvailablePort();
            RccServiceProcess process = RccServiceProcessManager.Best();
            RccServiceJob job = new RccServiceJob(process, JobID, 86400, 0, port, WebManager.GetGameserverScript(JobID, PlaceID, port, true));
            process.Jobs.Add(job);
            job.Start();

            return job;
        }

        public static void CloseJob(string JobID)
        {
            Job JobToClose = GetJobFromID(JobID);
            if (JobToClose == null) return;

            JobToClose.Close();
            OpenJobs.Remove(JobToClose);
        }

        public static void ExecuteScript(string JobID, string Script)
        {
            Job JobToExecute = GetJobFromID(JobID);
            if (JobToExecute == null) return;

            JobToExecute.ExecuteScript(Script);
        }

        public static Job GetJobFromID(string JobID)
        {
            return OpenJobs.Find(Job => Job.ID == JobID);
        }

        public static bool CheckIfJobExists(string JobID)
        {
            foreach (Job OpenJob in OpenJobs)
            {
                if (OpenJob.ID == JobID) return true;
            }

            return false;
        }

        public static void MonitorCrashedJobs()
        {
            uint ProcessID;

            while (true)
            {
                IntPtr hWnd = FindWindow(null, "ROBLOX Crash");
                GetWindowThreadProcessId(hWnd, out ProcessID);

                if (ProcessID != 0)
                {
                    Job CrashedJob = JobManager.OpenJobs.Find(Job => Job.Process.Id == ProcessID);
                    if (CrashedJob != null)
                    {
                        ConsoleEx.WriteLine($"[JobManager] '{CrashedJob.ID}' has crashed! Closing Job...", ConsoleColor.Yellow);
                        CrashedJob.Status = JobStatus.Crashed;
                        CrashedJob.Close();

                        OpenJobs.Remove(CrashedJob);
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

                        if (OpenJob.Version == 2010 && (OpenJob.TimeStarted + 5) < UnixTime.GetTimestamp() && !GetWindowTitle(OpenJob.Process.MainWindowHandle).Contains("Place1"))
                        {
                            OpenJob.HasShutdown = true;
                        }

                        if (OpenJob.HasShutdown || OpenJob.Process.HasExited)
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
            ConsoleEx.WriteLine($"[JobManager] '{UnresponsiveJob.ID}' is not responding! Monitoring...", ConsoleColor.Yellow);
            UnresponsiveJob.Status = JobStatus.Monitored;

            for (int i = 1; i <= 30; i++)
            {
                Thread.Sleep(1000);

                if (UnresponsiveJob.Process.Responding)
                {
                    ConsoleEx.WriteLine($"[JobManager] '{UnresponsiveJob.ID}' has recovered from its unresponsive status!", ConsoleColor.Green);
                    UnresponsiveJob.Status = JobStatus.Started;
                    break;
                }
                else if (i == 30)
                {
                    ConsoleEx.WriteLine($"[JobManager] '{UnresponsiveJob.ID}' has been unresponsive for over 30 seconds. Closing Job...", ConsoleColor.Yellow);
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
