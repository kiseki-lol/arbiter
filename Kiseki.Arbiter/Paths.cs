namespace Kiseki.Arbiter;

public static class Paths
{
    public static string Base { get; private set; } = "";
    public static string Logs { get; private set; } = "";
    public static string Versions { get; private set; } = "";
    public static string Scripts { get; private set; } = "";

    public static void Initialize(string baseDirectory)
    {
        Base = baseDirectory;

        Logs = Path.Combine(Base, "Logs");
        Versions = Path.Combine(Base, "Versions");
        Scripts = Path.Combine(Base, "Scripts");
    }
}