using System;

namespace Kiseki.Arbiter;

public class Program
{
    public static void Main(string[] args)
    {
        Paths.Initialize(AppContext.BaseDirectory);
        if (!Settings.Initialize())
        {
            Console.WriteLine("Failed to initialize settings.");
            return;
        }

        bool isConnected = Web.Initialize();
        if (!isConnected && Web.IsInMaintenance)
        {
            // Try licensing this launcher and attempt to connect again
            Web.LoadLicense(File.ReadAllText(Settings.GetPublicKeyPath()));
            isConnected = Web.Initialize(false);
        }

        if (!isConnected)
        {
            Console.WriteLine($"Failed to connect to {Constants.PROJECT_NAME}.");
            return;
        }

        Verifier.Initialize();
        
        Console.WriteLine("Hello World!");
    }
}