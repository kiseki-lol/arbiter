using ServiceReference;

namespace Kiseki.Arbiter;

public class RenderJob : Job
{
    public uint AssetId { get; private set; } // interchangeable as userId, RenderJobType changes
    public RenderJobType RenderType { get; private set; }
    public AssetType RenderAssetType { get; private set; }
    public int Version { get; private set; }
    public RenderJob(string uuid, uint assetId, int version, string placeToken, int assetType, int renderType) : base(uuid, JobManager.GetAvailableGameserverPort(), JobManager.GetAvailableSoapPort())
    {
        AssetId = assetId;
        PlaceToken = placeToken;
        Version = version;

        // try to get Enum from string
        RenderType = (RenderJobType)renderType;
        RenderAssetType = (AssetType)assetType;
    }

    public override string Base64Result { 
        get => _result;
        protected set {
            _result = value;
            if (
                RenderType == RenderJobType.Place ||
                RenderType == RenderJobType.XML ||
                RenderType == RenderJobType.Asset
            )
            {
                Web.UpdateAssetThumbnail(Uuid, AssetId, _result);
                return;
            }
            else
            {
                // there's probably a better way to do this w/ a ternary
                if (RenderType == RenderJobType.Headshot)
                {
                    Web.UpdateUserThumbnail(Uuid, AssetId, _result, true);
                    return;
                }
                else
                {
                    Web.UpdateUserThumbnail(Uuid, AssetId, _result, false);
                }
            }
        }
    }

    public override void Start()
    {
        Logger.Write($"RenderJob:{Uuid}", $"Starting...", LogSeverity.Event);
        Logger.Write($"RenderJob:{Uuid}", $"Fart", LogSeverity.Event);
        Status = JobStatus.Waiting;
        Logger.Write($"RenderJob:{Uuid}", $"Fart", LogSeverity.Event);

        // read Place script
        RenderSoap.GetScriptFromRenderType(this);
        Logger.Write($"RenderJob:{Uuid}", $"Fart", LogSeverity.Event);

        // all of the process stuff
        string arbiterLocation   = AppDomain.CurrentDomain.BaseDirectory;
        bool isLinux  = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        string binary = $"Versions/{Version}/{(isLinux ? "Kiseki.Aya.Server" : "Kiseki.Aya.Server.exe")}";
        string cwd    = $"{arbiterLocation}Versions/{Version}/"; // arbiterLocation already contains /
        string[] args = new string[] { binary, $"--port {SoapPort}" };
        
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

        // can't put this stuff in the new ProcessStartInfo
        if (isLinux)
            Process.StartInfo.EnvironmentVariables["DISPLAY"] = ":0";

        Process.Exited += (sender, e) => {
            Logger.Write($"RenderJob:{Uuid}", $"Exited with code {Process.ExitCode}!", LogSeverity.Event);
            
            // todo: ugly! fix. check CloseJob method for more info
            Status = JobStatus.Closed;
            Closed = DateTime.UtcNow;

            JobManager.CloseJob(Uuid);
        };

        Process.OutputDataReceived += async (sender, e) => {
            // DO NOT UNCOMMENT ON PROD!
            // THIS DDOSES THE SITE W/ LOGS
            // Logger.Write($"RenderJob:Output:{Uuid}", $"{e.Data}", LogSeverity.Debug);

            // check if SOAP started...
            // Object reference not set to an instance of an object.
            try
            {
                // yay!
                if (!SoapReady && e.Data!.ToString().StartsWith("Now listening for incoming"))
                {
                    // should we just do all of this stuff in Job?
                    // setup job

                    ServiceReference.Job job = new ServiceReference.Job
                    {
                        cores               = 1,
                        category            = 0,
                        expirationInSeconds = 120,
                        id                  = Uuid
                    };

                    LuaValue[] result = await SoapClient.OpenJobExAsync(job, RenderSoap.GetScriptExecutionFromRenderType(this));

                    Base64Result = result[0].value;
                    Logger.Write($"RenderJob:{Uuid}", $"Successfully rendered image! Shutting down job...", LogSeverity.Event);

                    Status = JobStatus.Closed;
                    IsRunning = false;
                    Closed = DateTime.UtcNow;

                    JobManager.CloseJob(Uuid);
                }
            }
            catch (Exception ex)
            {
                // bail!
                Logger.Write($"RenderJob:Soap:{Uuid}", $"Render run failed: {ex.Message}", LogSeverity.Error); 
                Logger.Write($"RenderJob:Soap:{Uuid}", $"Could not start render job, closing job", LogSeverity.Event);                
            
                Close();
            }
        };

        try
        {
            Process.Start();
            Process.BeginOutputReadLine();
        }
        catch (Exception ex)
        {
            Logger.Write($"RenderJob:{Uuid}", $"Error starting process: {ex}", LogSeverity.Error);
            Status = JobStatus.Closed;
            IsRunning = false;
            Closed = DateTime.UtcNow;

            JobManager.CloseJob(Uuid);
        }
    }
}