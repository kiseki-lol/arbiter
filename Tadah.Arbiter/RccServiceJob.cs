using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Tadah.Arbiter.RccServiceSoap;

namespace Tadah.Arbiter
{
    public class RccServiceJob : Job
    {
        public int ExpirationInSeconds { get; private set; }
        public int Category { get; private set; }
        public int Cores { get; private set; }
        public new Process Process
        {
            get
            {
                return _process.Process;
            }
        }

        private RccServiceProcess _process;

        public RccServiceJob(string Id, int PlaceId, int Version, int Port, int ExpirationInSeconds, int Category) : base(Id, PlaceId, Version, Port)
        {
            this.ExpirationInSeconds = ExpirationInSeconds;
            this.Category = Category;

            _process = RccServiceProcessManager.Best();
        }

        protected override void InternalStart()
        {
            RccServiceSoap.Job job = new RccServiceSoap.Job
            {
                id = Id,
                expirationInSeconds = ExpirationInSeconds,
                category = Category
            };

            ScriptExecution script = new ScriptExecution
            {
                name = "Start Server",
                script = WebManager.GetGameserverScript(Id, PlaceId, Port, true)
            };

            _process.Client.OpenJob(job, script);
        }

        protected override string InternalClose()
        {
            _process.Client.CloseJob(Id);
            return "Closed";
        }

        public override object ExecuteScript(string script)
        {
            if (!TadahSignature.VerifyData(script))
            {
                return null;
            }

            ScriptExecution execution = new ScriptExecution
            {
                name = "Tadah.Arbiter." + Guid.NewGuid(),
                script = script
            };

            return _process.Client.ExecuteEx(Id, execution);
        }

        public void RenewLease(int seconds)
        {
            _process.Client.RenewLease(Id, seconds);
        }
    }
}
