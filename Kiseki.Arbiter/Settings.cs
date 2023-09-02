namespace Kiseki.Arbiter;

public static class Settings
{
    private static AppSettings? AppSettings;
    
    public static bool Initialize()
    {
        const string LOG_IDENT = "Settings::Initialize";

        try
        {
            AppSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(Path.Combine(Paths.Base, "AppSettings.json")))!;
        }
        catch
        {
            return false;
        }

        Logger.Write(LOG_IDENT, "OK!", LogSeverity.Debug);

        return true;
    }

    public static string GetAccessKey()
    {
        return AppSettings!.AccessKey;
    }

    public static string GetMachineAddress()
    {
        return AppSettings!.MachineAddress;
    }

    public static string GetPublicKeyPath()
    {
        return AppSettings!.PublicKeyPath;
    }

    public static string? GetLicensePath()
    {
        return AppSettings!.LicensePath ?? null;
    }

    public static int GetServicePort()
    {
        return AppSettings!.ServicePort;
    }

    public static int GetBasePort()
    {
        return AppSettings!.BasePort;
    }
}