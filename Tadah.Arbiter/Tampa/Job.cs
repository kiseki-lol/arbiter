using System;
using Tadah.Tampa.Server;

namespace Tadah.Tampa
{
    public class Job : Arbiter.Job
    {
        public int ExpirationInSeconds { get; private set; }
        public int Cores { get; private set; }
        public new System.Diagnostics.Process Process
        {
            get
            {
                return _process.Handle;
            }
        }

        private readonly Process _process;

        public Job(string Id, uint PlaceId, Proto.ClientVersion Version, int Port) : base(Id, PlaceId, Version, Port)
        {
            this.ExpirationInSeconds = 86400;
            _process = ProcessManager.Best();
        }

        protected override void InternalStart()
        {
            Tampa.Server.Job job = new()
            {
                id = Id,
                expirationInSeconds = ExpirationInSeconds,
                category = 1,
                cores = 1
            };

            ScriptExecution script = new()
            {
                name = "Start Server",
                script = Arbiter.Http.GetGameserverScript(Id, PlaceId, Port, true)
            };

            _process.Client.OpenJob(job, script);
            this.IsRunning = true;
        }

        protected override Arbiter.JobStatus InternalClose(bool forceClose)
        {
            if (!forceClose)
            {
                _process.Client.CloseJob(Id);
            }

            this.IsRunning = false;
            return Arbiter.JobStatus.Closed;
        }

        public override void ExecuteScript(string script)
        {
            if (!Signature.VerifyData(script, out string lua))
            {
                return;
            }

            ScriptExecution execution = new()
            {
                name = "Tadah.Arbiter." + Guid.NewGuid(),
                script = lua
            };

            _process.Client.ExecuteEx(Id, execution);
        }

        public void RenewLease(uint seconds)
        {
            _process.Client.RenewLease(Id, seconds);
        }
    }
}
