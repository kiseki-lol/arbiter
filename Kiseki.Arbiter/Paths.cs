namespace Kiseki.Arbiter;

public static class Paths
{
    public static string Base { get; private set; } = "";
    public static string Logs { get; private set; } = "";
    public static string Versions { get; private set; } = "";
    public static string Application { get; private set; } = "";

    public static void Initialize(string baseDirectory)
    {
        Base = baseDirectory;

        Logs = Path.Combine(Base, "Logs");
        Versions = Path.Combine(Base, "Versions");

        Application = Path.Combine(Base, $"{Constants.PROJECT_NAME}.Arbiter.exe");
    }
}