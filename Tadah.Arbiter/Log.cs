using System;
using System.IO;

namespace Tadah.Arbiter
{
    public enum LogSeverity
    {
        Error = 0, // Red
        Warning = 1, // Yellow
        Event = 2, // Blue
        Information = 3, // Grey
        Debug = 4, // Dark Blue
        Boot = 5 // Green
    };

    internal static class Log
    {
        static readonly StreamWriter Writer;
        static readonly string LogFile = Path.Combine(Directory.GetCurrentDirectory(), "latest.log");

        static Log()
        {
            if (File.Exists(LogFile))
            {
                if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Logs")))
                {
                    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Logs"));
                }

                File.Move(LogFile, Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"{DateTime.UtcNow:s}.log"));
            }
            else
            {
                File.Create(LogFile);
            }

            Writer = new StreamWriter(LogFile);
        }

        static ConsoleColor SeverityToColor(LogSeverity severity)
        {
            if (severity == LogSeverity.Error)
            {
                return ConsoleColor.Red;
            }

            if (severity == LogSeverity.Warning)
            {
                return ConsoleColor.Yellow;
            }

            if (severity == LogSeverity.Event)
            {
                return ConsoleColor.Blue;
            }

            if (severity == LogSeverity.Information)
            {
                return ConsoleColor.DarkGray;
            }

            if (severity == LogSeverity.Debug)
            {
                return ConsoleColor.DarkBlue;
            }

            if (severity == LogSeverity.Boot)
            {
                return ConsoleColor.Green;
            }

            return ConsoleColor.Gray;
        }

        static string SeverityToEvent(LogSeverity severity)
        {
            if (severity == LogSeverity.Error)
            {
                return "error";
            }

            if (severity == LogSeverity.Warning)
            {
                return "warn";
            }

            if (severity == LogSeverity.Event)
            {
                return "event";
            }

            if (severity == LogSeverity.Information)
            {
                return "info";
            }

            if (severity == LogSeverity.Debug)
            {
                return "debug";
            }

            if (severity == LogSeverity.Boot)
            {
                return "boot";
            }

            return "info";
        }

        static internal void Write(string message, LogSeverity severity = LogSeverity.Information)
        {
#if RELEASE
            if (severity == LogSeverity.Debug)
            {
                return;
            }
#endif

            if (Writer != null)
            {
                lock (Writer)
                {
                    ConsoleColor color = SeverityToColor(severity);
                    string _event = SeverityToEvent(severity);
                    int time = Unix.Now();

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"[{time:G}] ");

                    Console.ForegroundColor = color;
                    Console.Write($"[{_event}]");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    Console.WriteLine($" {message.Replace(Configuration.AppSettings["AccessKey"], "")}");

                    Writer.WriteLine($"[{time}] [{_event}] {message.Replace(Configuration.AppSettings["AccessKey"], "*********")}");

                    Http.Log(severity, time, message);

                    Writer.Flush();
                }
            }
        }

        public static void Error(string message)
        {
            if (Writer != null)
            {
                Http.Fatal(message);

                lock (Writer)
                {
                    Writer.WriteLine($"[{DateTime.Now:G}] [FATAL] {message}");
                    Writer.Flush();
                }

                Console.WriteLine(message, ConsoleColor.Red);
                Console.ReadLine();
                Environment.Exit(0);
            }
        }
    }
}
