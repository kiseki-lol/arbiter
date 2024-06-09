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

    public PlaceJob(string uuid, uint placeId, int version) : base(uuid, JobManager.GetAvailableGameserverPort(), JobManager.GetAvailableHttpPort())
    {
        PlaceId = placeId;
        Version = version;
    }

    public override void Start()
    {
        Logger.Write($"PlaceJob:{Uuid}", $"Starting...", LogSeverity.Event);
        Status = JobStatus.Waiting;

        string arbiterLocation   = AppDomain.CurrentDomain.BaseDirectory;
        bool isLinux  = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        string script = Web.FormatPlaceJobScriptUrl(Uuid, Port);
        // https://stackoverflow.com/questions/52599105/c-sharp-under-linux-process-start-exception-of-no-such-file-or-directory WHY??? MICROSOFT
        string binary = $"Versions/{Version}/{(isLinux ? "Kiseki.Aya.Server" : "Kiseki.Aya.Server.exe")}";
        string cwd    = $"{arbiterLocation}Versions/{Version}/"; // arbiterLocation already contains /
        string[] args = new string[] { binary, $"--port {HttpPort} --nostdin" };

        Process = new Process
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = args[0],
                Arguments = args[1],
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = cwd,

                RedirectStandardError = false,
                RedirectStandardOutput = true,
            },

            EnableRaisingEvents = true
        };

        Process.Exited += (sender, e) => {
            Logger.Write($"PlaceJob:{Uuid}", $"Exited with code {Process.ExitCode}!", LogSeverity.Event);
            Status = JobStatus.Closed;
            IsRunning = false;
            Closed = DateTime.UtcNow;
        };

        Process.OutputDataReceived += (sender, e) => {
#if DEBUG
            Logger.Write($"PlaceJob:stdout:{Uuid}", $"{e.Data}", LogSeverity.Event);
#endif

            if (e.Data.StartsWith("Starting webserver to listen for POST requests on "))
            {
                Web.SendStartGameRequestJwt(Port, PlaceId, "placeholder");
            }
        };

        try
        {
            Process.Start();
            Process.BeginOutputReadLine();
        }
        catch (Exception ex)
        {
            Logger.Write($"PlaceJob:{Uuid}", $"Error starting process: {ex}", LogSeverity.Error);
            Status = JobStatus.Closed;
            IsRunning = false;
            Closed = DateTime.UtcNow;
        }

        /*
        IsRunning = true;
        Started = DateTime.UtcNow;
        Status = JobStatus.Running;

        Logger.Write($"PlaceJob:{Uuid}", $"Started Kiseki.Server {Version} on port UDP/{Port}!", LogSeverity.Event);
        */
    }
}