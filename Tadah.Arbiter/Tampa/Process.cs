using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using Tadah;

namespace Tadah.Tampa
{
    public class Process
    {
        public int SoapPort { get; }
        public Tampa.Server.Client Client { get; private set; }
        public bool Monitored { get; set; }
        public System.Diagnostics.Process Handle { get; set; }
        public List<Job> Jobs { get; set; }

        public Process(int SoapPort)
        {
            this.SoapPort = SoapPort;
        }

        internal void Start()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                this.Handle = new System.Diagnostics.Process();
                this.Handle.StartInfo.FileName = "Versions\\Tampa\\TadahServer.exe";
                this.Handle.StartInfo.Arguments = $"-Start ${this.SoapPort}";
                this.Handle.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                this.Handle.Start();
            }
            else
            {
                this.Handle = new System.Diagnostics.Process();
                this.Handle.StartInfo.FileName = "wine";
                this.Handle.StartInfo.Arguments = $"{Directory.GetCurrentDirectory()}/Versions/2016/TadahServer.exe -Start ${this.SoapPort}";
                this.Handle.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                this.Handle.Start();
            }

            this.Client = new Tampa.Server.Client("http://tadah.rocks/", $"http://127.0.0.1:${this.SoapPort}");
        }

        internal void Close(bool forceKill = false)
        {
            if (forceKill)
            {
                Handle.Kill();
            }
            else
            {
                foreach (Job job in Jobs)
                {
                    if (!job.IsRunning)
                    {
                        continue;
                    }

                    job.Close();
                }

                Handle.Close();
            }
        }
    }
}
