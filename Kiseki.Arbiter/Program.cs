namespace Kiseki.Arbiter;

using System.Reflection;
using Kiseki.Arbiter.Helpers;

public class Program
{
    public readonly static string Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString()[..^2];

    public static async void Main()
    {        
        Paths.Initialize(AppContext.BaseDirectory);

        Log.Write($"Initializing {Constants.PROJECT_NAME}.Arbiter {Version}...", LogSeverity.Boot);

        if (!Settings.Initialize())
        {
            Log.Fatal("Failed to initialize settings. Does AppSettings.json exist?");
            return;
        }

        Log.Write("Settings::Initialize - OK", LogSeverity.Debug);

        bool isConnected = await Web.Initialize();
        if (!isConnected && Web.IsInMaintenance)
        {
            // Try licensing this arbiter and attempt to connect again

            try
            {
                Web.License(File.ReadAllText(Settings.GetLicensePath()!));
                isConnected = await Web.Initialize(false);
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
        Log.Write($"Assigned game server ID is '{Web.GameServerId}'", LogSeverity.Boot);

        if (!Verifier.Initialize())
        {
            Log.Fatal("Failed to initialize verifier. Have you set the public key path?");
            return;
        }

        Log.Write("Verifier::Initialize - OK", LogSeverity.Debug);

        Log.Write("Service starting...", LogSeverity.Boot);
    }
}