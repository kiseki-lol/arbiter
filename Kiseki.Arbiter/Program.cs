namespace Kiseki.Arbiter;

public class Program
{
    public static void Main()
    {
        Paths.Initialize(AppContext.BaseDirectory);

        if (!Settings.Initialize())
        {
            Log.Write("Failed to initialize settings.", LogSeverity.Error);
            return;
        }

        bool isConnected = Web.Initialize();
        if (!isConnected && Web.IsInMaintenance)
        {
            // Try licensing this arbiter and attempt to connect again

            try
            {
                Web.LoadLicense(File.ReadAllText(Settings.GetPublicKeyPath()));
                isConnected = Web.Initialize(false);
            }
            catch
            {
                Log.Write("Failed to load license.", LogSeverity.Error);
                return;
            }
        }

        if (!isConnected)
        {
            Log.Write($"Failed to connect to {Constants.PROJECT_NAME}.", LogSeverity.Error);
            return;
        }

        if (!Verifier.Initialize())
        {
            Log.Write("Failed to initialize verifier.", LogSeverity.Error);
        }
        
        Console.WriteLine("Hello World!");
    }
}