using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tadah.Arbiter.RccServiceSoap;

namespace Tadah.Arbiter
{
    public class RccServiceProcess
    {
        public int SoapPort { get; }
        public RCCServiceSoapClient Client { get; private set; }
        public bool Monitored { get; set; }
        public Process Process { get; set; }
        public List<RccServiceJob> Jobs { get; set; }

        public RccServiceProcess(int SoapPort)
        {
            this.SoapPort = SoapPort;
        }

        internal void Start()
        {
            Process = new Process();
            Process.StartInfo.FileName = "Gameservers\\2016\\RCCService.exe";
            Process.StartInfo.Arguments = $"-Start ${this.SoapPort}";
            Process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            Process.Start();

            this.Client = new RCCServiceSoapClient("http://tadah.rocks/", $"http://127.0.0.1:${this.SoapPort}");
        }

        internal void Close(bool forceKill = false)
        {
            if (forceKill)
            {
                Process.Kill();
            }
            else
            {
                foreach (RccServiceJob job in Jobs)
                {
                    if (!job.IsRunning)
                    {
                        continue;
                    }

                    job.Close();
                }

                Process.Close();
            }
        }
    }
}
