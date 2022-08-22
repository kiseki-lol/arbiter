﻿using System;
using System.Diagnostics;

namespace Tadah.Arbiter
{
    public class TampaJob : Job
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

        private TampaProcess _process;

        public TampaJob(string Id, int PlaceId, ClientVersion Version, int Port) : base(Id, PlaceId, Version, Port)
        {
            this.ExpirationInSeconds = 86400;
            _process = TampaProcessManager.Best();
        }

        protected override void InternalStart()
        {
            Tadah.Job job = new()
            {
                id = Id,
                expirationInSeconds = ExpirationInSeconds,
                category = 1,
                cores = 1
            };

            ScriptExecution script = new()
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
