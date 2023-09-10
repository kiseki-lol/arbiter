namespace Kiseki.Arbiter;

public class Job
{
    private DateTime _created;
    private DateTime _started;
    private DateTime _closed;
    private JobStatus _status;

    public string Id { get; private set; }
    public int Version { get; private set; }
    public int Port { get; private set; }
    public uint PlaceId { get; private set; }
    public Process? Process { get; private set; } = null;
    public bool IsRunning { get; private set; } = false;

    public DateTime Created {
        get => _created;
        private set {
            _created = value;
            Web.UpdateJobTimestamp(Id, "created_at", _created);
        }
    }

    public DateTime Started {
        get => _started;
        private set {
            _started = value;
            Web.UpdateJobTimestamp(Id, "started_at", _started);
        }
    }

    public DateTime Closed {
        get => _closed;
        private set {
            _closed = value;
            Web.UpdateJobTimestamp(Id, "closed_at", _closed);
        }
    }

    public JobStatus Status { 
        get => _status;
        private set {
            _status = value;
            Web.UpdateJob(Id, _status, Port);
        }
    }

    public Job(string id, int version, int port, uint placeId)
    {
        Id = id;
        Version = version;
        Port = port;
        PlaceId = placeId;

        Created = DateTime.UtcNow;
    }

    public void Start()
    {
        Logger.Write(Id, $"Starting...", LogSeverity.Event);
        Status = JobStatus.Waiting;

        string script = Web.FormatServerScriptUrl(Id, PlaceId, Port);
        string[] args = GetCommandLine(script);

        Process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = args[0],
                Arguments = args[1],
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                // RedirectStandardError = true,
                // RedirectStandardOutput = true
            }
        };

        Process.Start();
        Process.WaitForInputIdle();

        Status = JobStatus.Running;
        IsRunning = true;
        Started = DateTime.UtcNow;

        Logger.Write(Id, $"Started Kiseki.Server {Version} on port UDP/{Port}!", LogSeverity.Event);
    }

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

    private string[] GetCommandLine(string script)
    {
        return new string[] { $"Versions\\{Version}\\Kiseki.Server.exe", $"-a {Web.FormatUrl("/login/negotiate.ashx")} -t 0 -j {script} -no3d" };
    }
}