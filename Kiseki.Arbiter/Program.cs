namespace Kiseki.Arbiter;

using System.Reflection;

public class Program
{
    public readonly static string Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString()[..^2];

    public static void Main()
    {        
        Paths.Initialize(AppContext.BaseDirectory);

        Log.Write($"Initializing {Constants.PROJECT_NAME}.Arbiter {Version}...", LogSeverity.Boot);

        if (!Settings.Initialize())
        {
            Log.Fatal("Failed to initialize settings. Does AppSettings.json exist?");
            return;
        }

        Log.Write("Settings::Initialize - OK", LogSeverity.Debug);

        bool isConnected = Web.Initialize();
        if (!isConnected && Web.IsInMaintenance)
        {
            // Try licensing this arbiter and attempt to connect again

            try
            {
                Web.License(File.ReadAllText(Settings.GetLicensePath()!));
                isConnected = Web.Initialize(false);
            }
            catch
            {
                Log.Fatal("Failed to load license. Have you set the license path?");
                return;
            }
        }

        if (!isConnected)
        {
            Log.Write($"Failed to connect to {Constants.PROJECT_NAME}.", LogSeverity.Error);
            return;
        }

        Log.Write("Web::Initialize - OK", LogSeverity.Debug);
        Log.Write($"Assigned game server UUID is '{Web.GameServerUuid}'", LogSeverity.Boot);

        Log.Write("Starting service...", LogSeverity.Boot);
        
        int port = Service.Start();

        if (port == -1)
        {
            Log.Fatal("Failed to start arbiter service.");
            return;
        }

        Log.Write($"Started service on port {port}.", LogSeverity.Boot);

        Console.CancelKeyPress += async delegate
        {
            Log.Write("Received shutdown signal. Shutting down...", LogSeverity.Event);

            await Service.Stop();

            await Web.UpdateGameServerStatus(GameServerStatus.Offline);
        };

        while (true)
        {
            Thread.Sleep(1000);
        }
    }
}