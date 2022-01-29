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
        public int PlaceId { get; set; }
        public int Version { get; set; }
        public int Port { get; set; }
        public bool IsRunning { get; set; }
        public JobStatus Status { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeStarted { get; set; }
        public DateTime TimeClosed { get; set; }
        public Process Process;

        protected abstract void InternalStart();
        protected abstract string InternalClose();
        public abstract object ExecuteScript(string script);

        protected void Log(string message, ConsoleColor color = ConsoleColor.Gray)
        {
            ConsoleEx.WriteLine($"[{this.Id}] {message}", color);
        }

        public void Start()
        {
            this.Log($"Starting on port {Port}", ConsoleColor.Blue);
            WebManager.UpdateJob(Id, "Loading", Port);

            this.InternalStart();

            this.IsRunning = true;
            this.Status = JobStatus.Started;
            this.TimeStarted = DateTime.UtcNow;
            WebManager.UpdateJob(Id, "Started");

            this.Log($"Started!", ConsoleColor.Green);
        }

        public void Close()
        {
            string result = this.InternalClose();

            this.Status = (JobStatus) Enum.Parse(typeof(JobStatus), result);
            this.TimeClosed = DateTime.UtcNow;
            WebManager.UpdateJob(Id, result);

            if (this.Status == JobStatus.Crashed)
            {
                this.Log($"Crashed!", ConsoleColor.Red);
            }

            this.Log($"Closed with result 'JobStatus.{result}'", ConsoleColor.DarkBlue);
        }

        public Job(string Id, int PlaceId, int Version, int Port)
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
