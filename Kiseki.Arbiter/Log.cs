
namespace Kiseki.Arbiter;

public static class Log
{
    private static readonly StreamWriter? Writer;

    static Log()
    {
        string filename = Path.Combine(Paths.Base, "latest.log");

        if (File.Exists(filename))
        {
            if (!Directory.Exists(Paths.Logs))
            {
                Directory.CreateDirectory(Paths.Logs);
            }

            File.Move(filename, Path.Combine(Paths.Logs, $"{File.GetCreationTimeUtc(filename).ToFileName()}.log"));
        }
        else
        {
            File.Create(filename);
        }

        Writer = new StreamWriter(filename);
    }

    public static void Write(string message, LogSeverity severity = LogSeverity.Information)
    {
#if RELEASE
        if (severity == LogSeverity.Debug)
        {
            return;
        }
#endif

        DateTime timestamp = DateTime.Now;

        // Spit to web
        Web.Log(timestamp.ToUniversalTime(), severity, message);

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

    public static void Fatal(string message)
    {
        DateTime timestamp = DateTime.Now;

        // Spit to web
        Web.Fatal(timestamp.ToUniversalTime(), message);

        // Spit to file
        if (Writer != null)
        {
            lock (Writer)
            {
                // Spit to file
                Writer.WriteLine($"[{timestamp.ToUniversalIso8601()}] [fatal] {message}");
                Writer.Flush();
            }
        }

        // Spit to user
        Print(timestamp, message, LogSeverity.Fatal);

        Environment.Exit(1);
    }

    private static void Print(DateTime timestamp, string message, LogSeverity severity)
    {
        ConsoleColor saved = Console.ForegroundColor;
        string name = severity.GetName();

        ConsoleColor color = severity.GetColor();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"[{timestamp:G}] ");

        Console.ForegroundColor = color;
        Console.Write($"[{name}] ");
        Console.ForegroundColor = saved;

        Console.WriteLine(message);
    }
}