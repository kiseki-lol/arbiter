namespace Kiseki.Arbiter;

public static class LogSeverityEx
{
    public static string GetName(this LogSeverity severity)
    {
        return severity switch
        {
            LogSeverity.Fatal => "fatal",
            LogSeverity.Error => "error",
            LogSeverity.Warning => "warn",
            LogSeverity.Event => "event",
            LogSeverity.Information => "info",
            LogSeverity.Debug => "debug",
            LogSeverity.Boot => "boot",
            _ => "info"
        };
    }

    public static ConsoleColor GetColor(this LogSeverity severity)
    {
        return severity switch
        {
            LogSeverity.Fatal => ConsoleColor.DarkRed,
            LogSeverity.Error => ConsoleColor.Red,
            LogSeverity.Warning => ConsoleColor.Yellow,
            LogSeverity.Event => ConsoleColor.Blue,
            LogSeverity.Information => ConsoleColor.DarkGray,
            LogSeverity.Debug => ConsoleColor.DarkBlue,
            LogSeverity.Boot => ConsoleColor.Green,
            _ => ConsoleColor.Gray
        };
    }
}