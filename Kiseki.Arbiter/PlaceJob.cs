namespace Kiseki.Arbiter;

public class PlaceJob : Job
{
    public uint PlaceId { get; private set; }
    public int Version { get; private set; }

    public override DateTime Started {
        get => _started;
        protected set {
            _started = value;
            Web.UpdatePlaceJobTimestamp(Uuid, "started_at", _started);
        }
    }

    public override DateTime Closed {
        get => _closed;
        protected set {
            _closed = value;
            Web.UpdatePlaceJobTimestamp(Uuid, "closed_at", _closed);
        }
    }

    public override JobStatus Status { 
        get => _status;
        protected set {
            _status = value;
            Web.UpdatePlaceJob(Uuid, _status, Port);
        }
    }

    public PlaceJob(string uuid, uint placeId, int version) : base(uuid, JobManager.GetAvailablePort())
    {
        PlaceId = placeId;
        Version = version;
    }

    public override void Start()
    {
        Logger.Write(Uuid, $"Starting...", LogSeverity.Event);
        Status = JobStatus.Waiting;

        string script = Web.FormatPlaceJobScriptUrl(Uuid, Port);
        string[] args = new string[] { $"Versions\\{Version}\\Kiseki.Server.exe", $"-a {Web.FormatUrl("/Login/Negotiate.ashx")} -t 0 -j {script} -no3d" };

        Process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = args[0],
                Arguments = args[1],
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,

                // TODO: Enable following options once the patcher can hook onto STDOUT and STDERR properly
                //       Once that's possible, we can start to save output logs :-)

                // RedirectStandardError = true,
                // RedirectStandardOutput = true
            }
        };

        Process.Start();
        Process.WaitForInputIdle();

        Status = JobStatus.Running;
        IsRunning = true;
        Started = DateTime.UtcNow;

        Logger.Write(Uuid, $"Started Kiseki.Server {Version} on port UDP/{Port}!", LogSeverity.Event);
    }
}