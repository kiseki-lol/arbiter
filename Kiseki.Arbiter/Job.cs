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

    public void Close()
    {
        Logger.Write(Id, $"Closing...", LogSeverity.Event);
        
        Process!.Kill();
        
        Status = JobStatus.Closed;
        IsRunning = false;
        Closed = DateTime.UtcNow;

        Logger.Write(Id, $"Closed!", LogSeverity.Event);
    }
}