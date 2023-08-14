namespace Kiseki.Arbiter;

using System.Text.Json;

public static class Settings
{
    private static Models.AppSettings? AppSettings;
    
    public static bool Initialize()
    {
        try
        {
            AppSettings = JsonSerializer.Deserialize<Models.AppSettings>(File.ReadAllText(Path.Combine(Paths.Base, "AppSettings.json")))!;
        }
        catch
        {
            return false;
        }

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