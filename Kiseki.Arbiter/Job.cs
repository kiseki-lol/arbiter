namespace Kiseki.Arbiter;

public abstract class Job
{
    protected DateTime _started;
    protected DateTime _closed;
    protected JobStatus _status;

    public string Uuid { get; protected set; }
    public int Port { get; protected set; }
    public int HttpPort { get; protected set; }
    public Process? Process { get; protected set; } = null;
    public bool IsRunning { get; protected set; } = false;

    public virtual DateTime Started {
        get => _started;
        protected set {
            _started = value;
        }
    }

    public virtual DateTime Closed {
        get => _closed;
        protected set {
            _closed = value;
        }
    }

    public virtual JobStatus Status { 
        get => _status;
        protected set {
            _status = value;
        }
    }

    public Job(string uuid, int port, int httpPort)
    {
        Uuid = uuid;
        Port = port;
        HttpPort = httpPort;
    }

    public abstract void Start();

    public void Close()
    {
        Logger.Write(Uuid, $"Closing...", LogSeverity.Event);

        // set status closed just in case process doesn't actually exist        
        Status = JobStatus.Closed;
        IsRunning = false;
        Closed = DateTime.UtcNow;

        try
        {
            Process!.Kill();

            Logger.Write(Uuid, $"Closed!", LogSeverity.Event);
        }
        catch (Exception e)
        {
            Logger.Write(Uuid, $"Error while closing job process! { e }", LogSeverity.Error);
        }
    }
}