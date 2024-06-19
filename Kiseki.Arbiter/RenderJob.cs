using ServiceReference;

namespace Kiseki.Arbiter;

public class RenderJob : Job
{
    public uint AssetId { get; private set; }
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

    public void GetScriptFromRenderType()
    {
        switch (RenderType)
        {
            case RenderJobType.Headshot:
                JobScript.LoadFromPath("headshot.lua");
                break;
            case RenderJobType.Bodyshot:
                JobScript.LoadFromPath("bodyshot.lua");
                break;
            case RenderJobType.Place:
                JobScript.LoadFromPath("place.lua");
                break;
            case RenderJobType.Asset:
                JobScript.LoadFromPath("bodyasset.lua");
                break;
            case RenderJobType.XML:
                JobScript.LoadFromPath("xml.lua");
                break;
            default:
                // ??? don't think this will EVER happen
                throw new Exception("RenderType was not set!");
        }
    }

    public ScriptExecution GetScriptExecutionFromRenderType()
    {
        Logger.Write($"{RenderJobType.Asset}", LogSeverity.Information);
        Logger.Write($"{RenderType}", LogSeverity.Information);

        if (RenderType == RenderJobType.Headshot || RenderType == RenderJobType.Bodyshot)
        {
            return new ScriptExecution
            {
                script    = JobScript.Script,
                name      = "RenderJob",
                arguments = 
                [
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = Uuid
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = "Render"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = "PNG" // maybe we should make RCC output webp?
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = "420"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = "420"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
// kiseki.local - no tls
#if DEBUG
                        value = "http://" + Constants.BASE_URL
#else
                        value = "https://" + Constants.BASE_URL
#endif
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = AssetId.ToString() // why?
                    },
                ]
            };
        }

        if (RenderType == RenderJobType.Place)
        {
            return new ScriptExecution
            {
                script    = JobScript.Script,
                name      = "RenderJob",
                arguments = 
                [
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = Uuid
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = "Render"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = "PNG" // maybe we should make RCC output webp?
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = "420"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = "420"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
// kiseki.local - no tls
#if DEBUG
                        value = "http://" + Constants.BASE_URL
#else
                        value = "https://" + Constants.BASE_URL
#endif
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = AssetId.ToString() // why?
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = PlaceToken
                    },
                ]
            };
        }

        if (RenderType == RenderJobType.Asset)
        {
            return new ScriptExecution
            {
                script    = JobScript.Script,
                name      = "RenderJob",
                arguments = 
                [
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = Uuid
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = "Render"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = "PNG" // maybe we should make RCC output webp?
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = "420"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = "420"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
// kiseki.local - no tls
#if DEBUG
                        value = "http://" + Constants.BASE_URL
#else
                        value = "https://" + Constants.BASE_URL
#endif
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = AssetId.ToString() // why?
                    },
                ]
            };
        }

        throw new Exception("not implemented for render type");
    }

    public override void Start()
    {
        Logger.Write($"RenderJob:{Uuid}", $"Starting...", LogSeverity.Event);
        Logger.Write($"RenderJob:{Uuid}", $"Fart", LogSeverity.Event);
        Status = JobStatus.Waiting;
        Logger.Write($"RenderJob:{Uuid}", $"Fart", LogSeverity.Event);

        // read Place script
        GetScriptFromRenderType();
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

        Process.Exited += (sender, e) => {
            Logger.Write($"RenderJob:{Uuid}", $"Exited with code {Process.ExitCode}!", LogSeverity.Event);
            Status = JobStatus.Closed;
            IsRunning = false;
            Closed = DateTime.UtcNow;
        };

        Process.OutputDataReceived += async (sender, e) => {
            Logger.Write($"RenderJob:Output:{Uuid}", $"{e.Data}", LogSeverity.Debug);

            // check if SOAP started...
            // Object reference not set to an instance of an object.
            try
            {
                // yay!
                if(!SoapReady && e.Data!.ToString().StartsWith("Now listening for incoming"))
                {
                    // should we just do all of this stuff in Job?
                    // setup job

                    Logger.Write($"RenderJob:{Uuid}", $"placetoken: {PlaceToken}", LogSeverity.Event);
                    
                    ServiceReference.Job job = new ServiceReference.Job
                    {
                        cores               = 1,
                        category            = 0,
                        expirationInSeconds = 120,
                        id                  = Uuid
                    };

                    LuaValue[] result = await SoapClient.OpenJobExAsync(job, GetScriptExecutionFromRenderType());

                    SoapReady = true;
                    IsRunning = true;
                    Started = DateTime.UtcNow;
                    Status = JobStatus.Running;

                    Logger.Write($"RenderJob:{Uuid}", $"Started Kiseki.Server {Version} on port UDP/{Port}!", LogSeverity.Event);
                }
            }
            catch (Exception ex)
            {
                // bail!
                Logger.Write($"RenderJob:Soap:{Uuid}", $"Render run failed: {ex.Message}", LogSeverity.Error); 
                Logger.Write($"RenderJob:Soap:{Uuid}", $"Could not start gameserver, closing job", LogSeverity.Event);                
            
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
        }
    }
}