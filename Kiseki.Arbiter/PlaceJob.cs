using ServiceReference;

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

    public PlaceJob(string uuid, uint placeId, int version, string placeToken) : base(uuid, JobManager.GetAvailableGameserverPort(), JobManager.GetAvailableSoapPort())
    {
        PlaceId = placeId;
        PlaceToken = placeToken;
        Version = version;
    }

    public override void Start()
    {
        Logger.Write($"PlaceJob:{Uuid}", $"Starting...", LogSeverity.Event);
        Status = JobStatus.Waiting;

        // read Place script
        JobScript.LoadFromPath("gameserver.lua");

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
            Logger.Write($"PlaceJob:{Uuid}", $"Exited with code {Process.ExitCode}!", LogSeverity.Event);
            Status = JobStatus.Closed;
            IsRunning = false;
            Closed = DateTime.UtcNow;

            JobManager.CloseJob(Uuid);
        };

        Process.OutputDataReceived += async (sender, e) => {
            Logger.Write($"PlaceJob:Output:{Uuid}", $"{e.Data}", LogSeverity.Debug);

            // check if SOAP started...
            try
            {
                // yay!
                if(!SoapReady && e.Data!.ToString().StartsWith("Now listening for incoming"))
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
                    
                    ScriptExecution scriptExecution = new ScriptExecution
                    {
                        script    = JobScript.Script,
                        name      = "Gameserver",
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
                                value = "Gameserver"
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
                                value = PlaceId.ToString() // why?
                            },
                            new LuaValue
                            {
                                type = LuaType.LUA_TNUMBER,
                                value = Port.ToString() // why?
                            },
                            new LuaValue
                            {
                                type = LuaType.LUA_TSTRING,
                                value = PlaceToken
                            },
                        ]
                    };

                    LuaValue[] result = await SoapClient.OpenJobExAsync(job, scriptExecution);

                    SoapReady = true;
                    IsRunning = true;
                    Started = DateTime.UtcNow;
                    Status = JobStatus.Running;

                    Logger.Write($"PlaceJob:{Uuid}", $"Started Kiseki.Server {Version} on port UDP/{Port}!", LogSeverity.Event);
                }
            }
            catch (Exception ex)
            {
                // bail!
                Logger.Write($"PlaceJob:Soap:{Uuid}", $"Place run failed: {ex.Message}", LogSeverity.Error); 
                Logger.Write($"PlaceJob:Soap:{Uuid}", $"Could not start gameserver, closing job (most likely port conflicts! Check config json)", LogSeverity.Event);                
            
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
            Logger.Write($"PlaceJob:{Uuid}", $"Error starting process: {ex}", LogSeverity.Error);
            Status = JobStatus.Closed;
            IsRunning = false;
            Closed = DateTime.UtcNow;

            JobManager.CloseJob(Uuid);
        }
    }
}