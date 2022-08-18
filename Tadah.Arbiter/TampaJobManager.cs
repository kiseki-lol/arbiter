using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tadah.Arbiter
{
    public class TampaProcessManager
    {
        public static List<TampaProcess> OpenProcesses = new();

        private static int GetAvailableRccSoapPort()
        {
            int port = Configuration.BaseTampaSoapPort;

            for (int i = 0; i < Configuration.MaximumTampaProcesses; i++)
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

        public static TampaProcess New()
        {
            if (OpenProcesses.Count >= Configuration.MaximumTampaProcesses)
            {
                throw new Exception("Maximum amount of Tampa processes reached");
            }

            TampaProcess process = new(GetAvailableRccSoapPort());
            process.Start();

            OpenProcesses.Add(process);
            return process;
        }

        public static TampaProcess Best()
        {
            if (!OpenProcesses.Any())
            {
                return New();
            }

            TampaProcess best = OpenProcesses.OrderBy(Process => Process.Jobs.Count).Last();
            if (best.Jobs.Count >= Configuration.MaximumJobsPerTampaProcess)
            {
                return New();
            }

            return best;
        }

        public static void CloseAllProcesses()
        {
            foreach (TampaProcess process in OpenProcesses)
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
                    foreach (TampaProcess process in OpenProcesses)
                    {
                        if (process.Monitored)
                        {
                            continue;
                        }

                        if (process.Process.HasExited)
                        {
                            process.Close(true);
                            OpenProcesses.Remove(process);

                            // remove all jobs associated
                            foreach (TampaJob job in process.Jobs)
                            {
                                JobManager.CloseJob(job.Id, true);
                            }

                            continue;
                        }

                        if (process.Process.Responding)
                        {
                            continue;
                        }

                        Task.Run(() => MonitorUnresponsiveProcess(process));
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Log.Write($"[TampaProcessManager::MonitorUnresponsiveProcesses] InvalidOperationException - {ex.Message}", LogSeverity.Debug);
                }

                Thread.Sleep(5000);
            }
        }

        private static void MonitorUnresponsiveProcess(TampaProcess process)
        {
            Log.Write($"[TampaProcessManager] TampaProcess with PID '{process.Process.Id}' is not responding! Monitoring...", LogSeverity.Warning);
            process.Monitored = true;

            for (int i = 0; i <= 30; i++)
            {
                Thread.Sleep(1000);

                if (process.Process.Responding)
                {
                    Log.Write($"[TampaProcessManager] TampaProcess with PID '{process.Process.Id}' has recovered from its unresponsive status!", LogSeverity.Information);
                    process.Monitored = false;

                    break;
                }
                else if (i == 30)
                {
                    Log.Write($"[TampaProcessManager] TampaProcess with PID '{process.Process.Id}' has been unresponsive for over 30 seconds. Closing Process...", LogSeverity.Warning);
                    process.Close(true);
                    OpenProcesses.Remove(process);

                    // remove all jobs associated
                    foreach (TampaJob job in process.Jobs)
                    {
                        JobManager.CloseJob(job.Id, true);
                    }

                    break;
                }
            }

            return;
        }
    }
}
