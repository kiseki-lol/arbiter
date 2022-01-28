using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tadah.Arbiter.RccServiceSoap;

namespace Tadah.Arbiter
{
    public enum RccServiceJobStatus
    {
        Pending = 0,
        Started = 1,
        Closed = 2
    };

    public class RccServiceJob
    {
        public string ID { get; set; }
        public int ExpirationInSeconds { get; set; }
        public int Category { get; set; }
        public int Cores { get; set; }
        public int TimeCreated { get; set; }
        public int TimeStarted { get; set; }
        public bool HasShutdown { get; set; }
        public int Port { get; set; }
        public RccServiceJobStatus Status { get; set; }
        private RccServiceProcess Process { get; set; }
        public string LuaScript { get; set; }

        public RccServiceJob(RccServiceProcess Process, string ID, int ExpirationInSeconds, int Category, int Port, string LuaScript)
        {
            this.Process = Process;
            this.ID = ID;
            this.ExpirationInSeconds = ExpirationInSeconds;
            this.Category = Category;
            this.Status = RccServiceJobStatus.Pending;
            this.HasShutdown = false;
            this.Port = Port;
            this.LuaScript = LuaScript;
            this.TimeCreated = UnixTime.GetTimestamp();
        }

        internal void Start()
        {
            RccServiceSoap.Job job = new RccServiceSoap.Job();
            job.id = ID;
            job.expirationInSeconds = ExpirationInSeconds;
            job.category = Category;

            ScriptExecution script = new ScriptExecution();
            script.name = "Start Server";
            script.script = LuaScript;

            Process.Client.OpenJob(job, script);

            this.Status = RccServiceJobStatus.Started;
        }

        internal void Close()
        {
            Process.Client.CloseJob(ID);
            this.Status = RccServiceJobStatus.Closed;
        }

        internal LuaValue[] ExecuteScript(string script)
        {
            ScriptExecution execution = new ScriptExecution();
            execution.name = "Tadah.Arbiter." + Guid.NewGuid();
            execution.script = script;

            return Process.Client.ExecuteEx(ID, execution);
        }

        internal void RenewLease(int seconds)
        {
            Process.Client.RenewLease(ID, seconds);
        }
    }
}
