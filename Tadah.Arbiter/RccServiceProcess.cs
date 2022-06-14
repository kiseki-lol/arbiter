using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                this.Process = new Process();
                this.Process.StartInfo.FileName = "Gameservers\\2016\\RCCService.exe";
                this.Process.StartInfo.Arguments = $"-Start ${this.SoapPort}";
                this.Process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                this.Process.Start();
            }
            else
            {
                this.Process = new Process();
                this.Process.StartInfo.FileName = "wine";
                this.Process.StartInfo.Arguments = $"{Directory.GetCurrentDirectory()}/Gameservers/2016/RCCService.exe -Start ${this.SoapPort}";
                this.Process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                this.Process.Start();
            }

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
