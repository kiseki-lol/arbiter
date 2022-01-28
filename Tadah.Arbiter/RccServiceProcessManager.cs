using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tadah.Arbiter
{
    public class RccServiceProcessManager
    {
        public static List<RccServiceProcess> OpenProcesses = new List<RccServiceProcess>();

        private static int GetAvailableRccSoapPort()
        {
            int Port = AppSettings.BaseRccSoapPort;

            for (int i = 0; i < AppSettings.MaximumRccProcesses; i++)
            {
                if (OpenProcesses.Find(Process => Process.SoapPort == Port) == null)
                    break;
                else
                    Port++;
            }

            return Port;
        }

        public static RccServiceProcess New()
        {
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
            return best;
        }
    }
}
