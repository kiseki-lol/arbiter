using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using Tadah.Arbiter.TampaServerSoap;

namespace Tadah.Arbiter
{
    public class TampaServerProcess
    {
        public int SoapPort { get; }
        public TampaServerSoapClient Client { get; private set; }
        public bool Monitored { get; set; }
        public Process Process { get; set; }
        public List<TampaServerJob> Jobs { get; set; }

        public TampaServerProcess(int SoapPort)
        {
            this.SoapPort = SoapPort;
        }

        internal void Start()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                this.Process = new Process();
                this.Process.StartInfo.FileName = "Gameservers\\2016\\TampaServer.exe";
                this.Process.StartInfo.Arguments = $"-Start ${this.SoapPort}";
                this.Process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                this.Process.Start();
            }
            else
            {
                this.Process = new Process();
                this.Process.StartInfo.FileName = "wine";
                this.Process.StartInfo.Arguments = $"{Directory.GetCurrentDirectory()}/Gameservers/2016/TampaServer.exe -Start ${this.SoapPort}";
                this.Process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                this.Process.Start();
            }

            this.Client = new TampaServerSoapClient("http://tadah.rocks/", $"http://127.0.0.1:${this.SoapPort}");
        }

        internal void Close(bool forceKill = false)
        {
            if (forceKill)
            {
                Process.Kill();
            }
            else
            {
                foreach (TampaServerJob job in Jobs)
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
