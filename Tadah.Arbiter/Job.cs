using System;
using System.Diagnostics;

namespace Tadah.Arbiter
{
    [Flags]
    public enum JobStatus
    {
        Pending = 0,
        Started = 1,
        Monitored = 2,
        Closed = 3,
        Crashed = 4
    }

    public abstract class Job
    {
        public string Id { get; set; }
        public uint PlaceId { get; set; }
        public Proto.ClientVersion Version { get; set; }
        public int Port { get; set; }
        public bool IsRunning { get; set; }
        public JobStatus Status { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeStarted { get; set; }
        public DateTime TimeClosed { get; set; }
        public Process Process;

        protected abstract void InternalStart();
        protected abstract JobStatus InternalClose(bool forceClose = false);
        public abstract void ExecuteScript(string script);

        protected void Log(string message, LogSeverity severity = LogSeverity.Information)
        {
            if (this is Tampa.Job)
            {
                Arbiter.Log.Write($"[TampaJob-{this.Id}] {message}", severity);
            }
            else if (this is Taipei.Job)
            {
                Arbiter.Log.Write($"[TaipeiJob-{this.Id}] {message}", severity);
            }
        }

        public void Start()
        {
            this.Log($"Starting {Version} on port {Port} ...", LogSeverity.Event);
            Http.UpdateJob(Id, JobStatus.Pending, Port);

            this.InternalStart();

            this.IsRunning = true;
            this.Status = JobStatus.Started;
            this.TimeStarted = DateTime.UtcNow;
            Http.UpdateJob(Id, JobStatus.Started);

            this.Log($"Started!", LogSeverity.Event);
        }

        public void Close(bool forceClose = false)
        {
            JobStatus state = this.InternalClose(forceClose);

            this.Status = state;
            this.TimeClosed = DateTime.UtcNow;

            Http.UpdateJob(Id, state);

            if (this.Status == JobStatus.Crashed)
            {
                this.Log($"Crashed!", LogSeverity.Error);
            }

            this.Log($"Closed with result '{Enum.GetName(state)}'", LogSeverity.Information);
        }

        public Job(string Id, uint PlaceId, Proto.ClientVersion Version, int Port)
        {
            this.Id = Id;
            this.PlaceId = PlaceId;
            this.Version = Version;
            this.Port = Port;

            this.Status = JobStatus.Pending;
            this.TimeCreated = DateTime.UtcNow;
        }
    }
}
