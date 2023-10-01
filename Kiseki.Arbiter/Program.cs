namespace Kiseki.Arbiter;

using System.Linq;
using System.Reflection;

public class Program
{
    public readonly static string Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString()[..^2];
    public static bool EnableVerboseLogging { get; private set; } = false;

    private static bool IsOnline = false;
    
    public static void Main(string[] arguments)
    {
        EnableVerboseLogging = arguments.Contains("--verbose") || arguments.Contains("-v");
        Paths.Initialize(AppContext.BaseDirectory);

        Logger.Write($"Initializing {Constants.PROJECT_NAME}.Arbiter v{Version}...", LogSeverity.Boot);

        if (!Settings.Initialize())
        {
            Logger.Fatal("Failed to initialize settings. Does 'AppSettings.json' exist in the arbiter's directory?");
            return;
        }

        Web.Initialize();

        if (!Web.IsConnected && Web.IsInMaintenance)
        {
            // Try licensing this arbiter and attempt to connect again
            try
            {
                Web.License(File.ReadAllText(Settings.GetLicensePath()!));
                Web.Initialize(false);
            }
            catch
            {
                Logger.Fatal("Failed to load license. Have you set the license path?");
                return;
            }
        }

        if (!Web.IsConnected)
        {
            Logger.Write($"Failed to connect to {Constants.PROJECT_NAME}.", LogSeverity.Error);
            return;
        }

        Verifier.Initialize();

        Logger.Write($"Assigned game server UUID is '{Web.GameServerUuid}'.", LogSeverity.Boot);
        Logger.Write("Starting TCP server...", LogSeverity.Boot);
        
        int port = TcpServer.Start();

        if (port == -1)
        {
            Logger.Fatal($"Failed to start TCP server. Is another instance of {Constants.PROJECT_NAME}.Arbiter running?");
            return;
        }

        Logger.Write($"Started TCP server on port {port}.", LogSeverity.Boot);
        
        ResourceReporter.Start();
        Logger.Write($"Started resource reporter.", LogSeverity.Boot);

        IsOnline = true;
        Web.UpdateGameServerStatus(GameServerStatus.Online);

        Logger.Write($"Successfully started {Constants.PROJECT_NAME}.Arbiter v{Version}!", LogSeverity.Boot);
        ResourceReporter.Report();
        
        Console.CancelKeyPress += delegate
        {
            Shutdown();
        };

        while (IsOnline)
        {
            Thread.Sleep(30000);
            Web.Ping();
        }
    }

    public static void Shutdown()
    {
        IsOnline = false;

        Logger.Write("Received shutdown signal. Shutting down...", LogSeverity.Event);
        IsOnline = false;

        JobManager.CloseAllJobs();
        
        ResourceReporter.Stop();
        TcpServer.Stop();

        Web.UpdateGameServerStatus(GameServerStatus.Offline);
        
        Environment.Exit(0);
    }
}