using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tadah.Arbiter
{
    public class RccServiceProcessManager
    {
        public static List<RccServiceProcess> OpenProcesses = new List<RccServiceProcess>();

        private static int GetAvailableRccSoapPort()
        {
            int port = AppSettings.BaseRccSoapPort;

            for (int i = 0; i < AppSettings.MaximumRccProcesses; i++)
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

        public static RccServiceProcess New()
        {
            if (OpenProcesses.Count >= AppSettings.MaximumRccProcesses)
            {
                throw new Exception("Maximum amount of RCC processes reached");
            }

            RccServiceProcess process = new RccServiceProcess(GetAvailableRccSoapPort());
            process.Start();

            OpenProcesses.Add(process);
            return process;
        }

        public static RccServiceProcess Best()
        {
            if (!OpenProcesses.Any())
            {
                return New();
            }

            RccServiceProcess best = OpenProcesses.OrderBy(Process => Process.Jobs.Count).Last();
            if (best.Jobs.Count >= AppSettings.MaximumJobsPerRcc)
            {
                return New();
            }

            return best;
        }

        public static void MonitorUnresponsiveProcesses()
        {
            while (true)
            {
                try
                {
                    foreach (RccServiceProcess process in OpenProcesses)
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
                            foreach (RccServiceJob job in process.Jobs)
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
#if DEBUG
                    Log.Write($"[RccServiceProcessManager::MonitorUnresponsiveProcesses] InvalidOperationException - {ex.ToString()}", LogSeverity.Debug);
#endif
                }

                Thread.Sleep(5000);
            }
        }

        private static void MonitorUnresponsiveProcess(RccServiceProcess process)
        {
            Log.Write($"[RccServiceProcessManager] RccServiceProcess with PID '{process.Process.Id}' is not responding! Monitoring...", LogSeverity.Warning);
            process.Monitored = true;

            for (int i = 0; i <= 30; i++)
            {
                Thread.Sleep(1000);

                if (process.Process.Responding)
                {
                    Log.Write($"[RccServiceProcessManager] RccServiceProcess with PID '{process.Process.Id}' has recovered from its unresponsive status!", LogSeverity.Information);
                    process.Monitored = false;

                    break;
                }
                else if (i == 30)
                {
                    Log.Write($"[RccServiceProcessManager] RccServiceProcess with PID '{process.Process.Id}' has been unresponsive for over 30 seconds. Closing Process...", LogSeverity.Warning);
                    process.Close(true);
                    OpenProcesses.Remove(process);

                    // remove all jobs associated
                    foreach (RccServiceJob job in process.Jobs)
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
