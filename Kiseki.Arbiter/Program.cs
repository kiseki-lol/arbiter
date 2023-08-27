﻿namespace Kiseki.Arbiter;

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
            Log.Fatal("Failed to initialize settings. Does 'AppSettings.json' exist in the arbiter's directory?");
            return;
        }

#if DEBUG
        Log.Write("Settings::Initialize - OK!", LogSeverity.Debug);
#endif

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
                Log.Fatal("Failed to load license. Have you set the license path?");
                return;
            }
        }

        if (!Web.IsConnected)
        {
            Log.Write($"Failed to connect to {Constants.PROJECT_NAME}.", LogSeverity.Error);
            return;
        }

#if DEBUG
        Log.Write("Web::Initialize - OK!", LogSeverity.Debug);
#endif

        Log.Write($"Assigned game server UUID is '{Web.GameServerUuid}'.", LogSeverity.Boot);

        Log.Write("Starting service...", LogSeverity.Boot);
        
        int port = Service.Start();

        if (port == -1)
        {
            Log.Fatal("Failed to start service.");
            return;
        }

        Log.Write($"Started service on port {port}.", LogSeverity.Boot);
        
        // And now we're up!
        Web.UpdateGameServerStatus(GameServerStatus.Online);
        ResourceReporter.Start();

        Console.CancelKeyPress += delegate
        {
            Log.Write("Received shutdown signal. Shutting down...", LogSeverity.Event);

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