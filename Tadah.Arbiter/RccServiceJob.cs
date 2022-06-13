using System;
using System.Diagnostics;
using Tadah.Arbiter.RccServiceSoap;

namespace Tadah.Arbiter
{
    public class RccServiceJob : Job
    {
        public int ExpirationInSeconds { get; private set; }
        public int Cores { get; private set; }
        public new Process Process
        {
            get
            {
                return _process.Process;
            }
        }

        private RccServiceProcess _process;

        public RccServiceJob(string Id, int PlaceId, int Version, int Port) : base(Id, PlaceId, Version, Port)
        {
            this.ExpirationInSeconds = 86400;
            _process = RccServiceProcessManager.Best();
        }

        protected override void InternalStart()
        {
            RccServiceSoap.Job job = new RccServiceSoap.Job
            {
                id = Id,
                expirationInSeconds = ExpirationInSeconds,
                category = 1,
                cores = 1
            };

            ScriptExecution script = new ScriptExecution
            {
                name = "Start Server",
                script = Http.GetGameserverScript(Id, PlaceId, Port, true)
            };

            _process.Client.OpenJob(job, script);
            this.IsRunning = true;
        }

        protected override string InternalClose(bool forceClose)
        {
            if (!forceClose)
            {
                _process.Client.CloseJob(Id);
            }

            this.IsRunning = false;
            return "Closed";
        }

        public override void ExecuteScript(string script)
        {
            if (!TadahSignature.VerifyData(script, out string lua))
            {
                return;
            }

            ScriptExecution execution = new ScriptExecution
            {
                name = "Tadah.Arbiter." + Guid.NewGuid(),
                script = lua
            };

            _process.Client.ExecuteEx(Id, execution);
        }

        public void RenewLease(int seconds)
        {
            _process.Client.RenewLease(Id, seconds);
        }
    }
}
