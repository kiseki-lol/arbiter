namespace Kiseki.Arbiter;

public static class Logger
{
    private static readonly StreamWriter? Writer;

    static Logger()
    {
        string filename = Path.Combine(Paths.Base, "latest.log");

        if (File.Exists(filename))
        {
            if (!Directory.Exists(Paths.Logs))
            {
                Directory.CreateDirectory(Paths.Logs);
            }

            File.Move(filename, Path.Combine(Paths.Logs, $"{File.GetCreationTimeUtc(filename).ToUniversalIso8601().Replace(":", "_")}.log"));
        }
        else
        {
            File.Create(filename).Close();
        }

        Writer = new StreamWriter(filename);
    }

    public static void Write(string message, LogSeverity severity = LogSeverity.Information)
    {
#if DEBUG
        if (severity == LogSeverity.Debug && !Program.EnableVerboseLogging)
        {
            return;
        }
#endif

        DateTime timestamp = DateTime.UtcNow;

        // Spit to web
        Task.Run(() => Web.ReportLog(timestamp.ToUniversalTime(), severity, message));

        // Spit to file
        if (Writer != null)
        {
            lock (Writer)
            {
                // Spit to file
                Writer.WriteLine($"[{timestamp.ToUniversalIso8601()}] [{severity.GetName()}] {message}");
                Writer.Flush();
            }
        }
        
        // Spit to user
        Print(timestamp, message, severity);
    }

    public static void Write(string identification, string message, LogSeverity severity = LogSeverity.Information) => Write($"[{identification}] {message}", severity);

    public static void Fatal(string exception)
    {
        DateTime timestamp = DateTime.UtcNow;

        // Spit to web (synchronously, since web needs to know first)
        Web.ReportFatal(timestamp.ToUniversalTime(), exception);
        
        // Spit to file
        if (Writer != null)
        {
            lock (Writer)
            {
                // Spit to file
                Writer.WriteLine($"[{timestamp.ToUniversalIso8601()}] [fatal] {exception}");
                Writer.Flush();
            }
        }

        // Spit to user
        Print(timestamp, exception, LogSeverity.Fatal);

        Environment.Exit(1);
    }

    private static void Print(DateTime timestamp, string message, LogSeverity severity)
    {
        ConsoleColor saved = Console.ForegroundColor;
        string name = severity.GetName();

        ConsoleColor color = severity.GetColor();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"[{timestamp.ToLocalTime():G}] ");

        Console.ForegroundColor = color;
        Console.Write($"[{name}] ");
        Console.ForegroundColor = saved;

        Console.WriteLine(message);
    }
}