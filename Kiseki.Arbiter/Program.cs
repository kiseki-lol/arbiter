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

        Logger.Write($"Assigned game server UUID is '{Web.GameServerUuid}'.", LogSeverity.Boot);
        Logger.Write("Starting service...", LogSeverity.Boot);
        
        int port = Service.Start();

        if (port == -1)
        {
            Logger.Fatal("Failed to start service.");
            return;
        }

        Logger.Write($"Started service on port {port}.", LogSeverity.Boot);
        
        // We're up!
        Web.UpdateGameServerStatus(GameServerStatus.Online);
        Monitor.Start();

        Console.CancelKeyPress += delegate
        {
            Logger.Write("Received shutdown signal. Shutting down...", LogSeverity.Event);

            Service.Stop();

            Web.UpdateGameServerStatus(GameServerStatus.Offline);
        };

        while (true)
        {
            Thread.Sleep(30000);
            Web.Ping();
        }
    }
}