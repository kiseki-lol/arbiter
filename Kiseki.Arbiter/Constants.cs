namespace Kiseki.Arbiter;

public static class Constants
{
    public const string PROJECT_NAME = "Kiseki";

#if DEBUG
    public const string BASE_URL = "kiseki.loc";
#else
    public const string BASE_URL = "kiseki.lol";
#endif

    public const string MAINTENANCE_DOMAIN = "test";
}