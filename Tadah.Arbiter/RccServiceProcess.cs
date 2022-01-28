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
        public RCCServiceSoapClient Client { get; }
        public Process Process { get; set; }
        public List<RccServiceJob> Jobs { get; set; }

        public RccServiceProcess(int SoapPort)
        {
            this.SoapPort = SoapPort;
            this.Client = new RCCServiceSoapClient("http://tadah.rocks/", $"http://127.0.0.1:${this.SoapPort}");
        }

        internal void Start()
        {
            Process = new Process();
            Process.StartInfo.FileName = "Gameservers\\2016\\RCCService.exe";
            Process.StartInfo.Arguments = $"-Start ${this.SoapPort}";
            Process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            Process.Start();
        }

        internal void Close()
        {
            this.Client.CloseAllJobs();
            Process.Close();
        }
    }
}
