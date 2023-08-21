namespace Kiseki.Arbiter;

public class Service
{
    public static int Start()
    {
        return Settings.GetServicePort();
    }

    public static void Stop()
    {
        return;
    }
}