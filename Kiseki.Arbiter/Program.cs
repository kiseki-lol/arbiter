namespace Kiseki.Arbiter;

using System.Reflection;

public class Program
{
    public readonly static string Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString()[..^2];

    public static void Main()
    {
        Paths.Initialize(AppContext.BaseDirectory);

        Logger.Write($"Initializing {Constants.PROJECT_NAME}.Arbiter {Version}...", LogSeverity.Boot);

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
        
        // We're up!
        Web.UpdateGameServerStatus(GameServerStatus.Online);
        ResourceReporter.Start();

        Console.CancelKeyPress += delegate
        {
            Logger.Write("Received shutdown signal. Shutting down...", LogSeverity.Event);

            TcpServer.Stop();

            Web.UpdateGameServerStatus(GameServerStatus.Offline);
        };

        Task.Run(async () => {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

            while (await timer.WaitForNextTickAsync())
            {
                Web.Ping();
            }
        });
    }
}