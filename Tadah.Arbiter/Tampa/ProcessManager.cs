using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tadah.Tampa
{
    public class ProcessManager
    {
        public static List<Process> OpenProcesses = new();

        private static int GetAvailableRccSoapPort()
        {
            int port = Arbiter.Configuration.BaseTampaSoapPort;

            for (int i = 0; i < Arbiter.Configuration.MaximumTampaProcesses; i++)
            {
                if (OpenProcesses.Find(process => process.SoapPort == port) == null)
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

        public static Process New()
        {
            if (OpenProcesses.Count >= Arbiter.Configuration.MaximumTampaProcesses)
            {
                throw new Exception("Maximum amount of Tampa processes reached");
            }

            Process process = new(GetAvailableRccSoapPort());
            process.Start();

            OpenProcesses.Add(process);
            return process;
        }

        public static Process Best()
        {
            if (!OpenProcesses.Any())
            {
                return New();
            }

            Process best = OpenProcesses.OrderBy(Process => Process.Jobs.Count).Last();
            if (best.Jobs.Count >= Arbiter.Configuration.MaximumJobsPerTampaProcess)
            {
                return New();
            }

            return best;
        }

        public static void CloseAllProcesses()
        {
            foreach (Process process in OpenProcesses)
            {
                process.Close();
            }

            OpenProcesses.Clear();
        }

        public static void MonitorUnresponsiveProcesses()
        {
            while (true)
            {
                try
                {
                    foreach (Process process in OpenProcesses)
                    {
                        if (process.Monitored)
                        {
                            continue;
                        }

                        if (process.Handle.HasExited)
                        {
                            process.Close(true);
                            OpenProcesses.Remove(process);

                            // remove all jobs associated
                            foreach (Job job in process.Jobs)
                            {
                                Arbiter.JobManager.CloseJob(job.Id, true);
                            }

                            continue;
                        }

                        if (process.Handle.Responding)
                        {
                            continue;
                        }

                        Task.Run(() => MonitorUnresponsiveProcess(process));
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Arbiter.Log.Write($"[Tampa.ProcessManager::MonitorUnresponsiveProcesses] InvalidOperationException - {ex.Message}", Arbiter.LogSeverity.Debug);
                }

                Thread.Sleep(5000);
            }
        }

        private static void MonitorUnresponsiveProcess(Process process)
        {
            Arbiter.Log.Write($"[TampaProcessManager] TampaProcess with PID '{process.Handle.Id}' is not responding! Monitoring...", Arbiter.LogSeverity.Warning);
            process.Monitored = true;

            for (int i = 0; i <= 30; i++)
            {
                Thread.Sleep(1000);

                if (process.Handle.Responding)
                {
                    Arbiter.Log.Write($"[TampaProcessManager] TampaProcess with PID '{process.Handle.Id}' has recovered from its unresponsive status!", Arbiter.LogSeverity.Information);
                    process.Monitored = false;

                    break;
                }
                else if (i == 30)
                {
                    Arbiter.Log.Write($"[TampaProcessManager] TampaProcess with PID '{process.Handle.Id}' has been unresponsive for over 30 seconds. Closing Process...", Arbiter.LogSeverity.Warning);
                    process.Close(true);
                    OpenProcesses.Remove(process);

                    // remove all jobs associated
                    foreach (Job job in process.Jobs)
                    {
                        Arbiter.JobManager.CloseJob(job.Id, true);
                    }

                    break;
                }
            }

            return;
        }
    }
}
