namespace Kiseki.Arbiter;

public abstract class Job
{
    private DateTime _created;
    private DateTime _started;
    private DateTime _closed;
    private JobStatus _status;

    public string Id { get; protected set; }
    public int Port { get; protected set; }
    public Process? Process { get; protected set; } = null;
    public bool IsRunning { get; protected set; } = false;

    public DateTime Created {
        get => _created;
        protected set {
            _created = value;
            Web.UpdateJobTimestamp(Id, "created_at", _created);
        }
    }

    public DateTime Started {
        get => _started;
        protected set {
            _started = value;
            Web.UpdateJobTimestamp(Id, "started_at", _started);
        }
    }

    public DateTime Closed {
        get => _closed;
        protected set {
            _closed = value;
            Web.UpdateJobTimestamp(Id, "closed_at", _closed);
        }
    }

    public JobStatus Status { 
        get => _status;
        protected set {
            _status = value;
            Web.UpdateJob(Id, _status, Port);
        }
    }

    public Job(string id, int port)
    {
        Id = id;
        Port = port;

        Created = DateTime.UtcNow;
    }

    public abstract void Start();

    public void Close(bool forceClose = false)
    {
        Logger.Write(Id, $"Closing...", LogSeverity.Event);

        if (forceClose || Process == null)
        {
            Status = JobStatus.Crashed;
        }
        else
        {
            try
            {
                if (!IsRunning || Process.HasExited)
                {
                    if (!IsRunning)
                    {
                        Logger.Write(Id, $"Process is not running!", LogSeverity.Warning);
                    }
                    else if (Process.HasExited)
                    {
                        Logger.Write(Id, $"Process has exited!", LogSeverity.Warning);
                    }

                    if (!IsRunning)
                    {
                        Process.CloseMainWindow();
                    }

                    Process.Close();

                    Status = JobStatus.Closed;
                }
                else if (Status == JobStatus.Crashed || !Process.Responding)
                {
                    Process.Kill();
                    Process.Close();

                    Status = JobStatus.Crashed;
                }
                else
                {
                    Process.CloseMainWindow();
                    Process.Close();

                    Status = JobStatus.Closed;
                }
            }
            catch (InvalidOperationException)
            {
                Status = JobStatus.Crashed;
            }
        }

        IsRunning = false;
        Closed = DateTime.UtcNow;

        if (Status == JobStatus.Crashed)
        {
            Logger.Write(Id, $"Crashed!", LogSeverity.Error);
        }
        else
        {
            Logger.Write(Id, $"Closed!", LogSeverity.Event);
        }
    }
}