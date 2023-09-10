namespace Kiseki.Arbiter;

public class PlaceJob : Job
{
    public int Version { get; private set; }
    public uint PlaceId { get; private set; }

    public PlaceJob(string id, int version, int port, uint placeId) : base(id, port)
    {
        Version = version;
        PlaceId = placeId;
    }

    public override void Start()
    {
        Logger.Write(Id, $"Starting...", LogSeverity.Event);
        Status = JobStatus.Waiting;

        string script = Web.FormatServerScriptUrl(Id, PlaceId, Port);
        string[] args = new string[] { $"Versions\\{Version}\\Kiseki.Server.exe", $"-a {Web.FormatUrl("/login/negotiate.ashx")} -t 0 -j {script} -no3d" };

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
}