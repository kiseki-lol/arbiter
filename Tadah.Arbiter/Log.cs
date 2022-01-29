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
		Debug = 4 // Dark Blue
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

				File.Move(LogFile, Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"{DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss")}.log"));
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

			return "info";
		}

		static internal void Write(string message, LogSeverity severity = LogSeverity.Information, string _event = "")
		{
			if (Writer != null)
            {
				lock (Writer)
                {
                    Console.Write($"[{DateTime.Now.ToString("G")}] ");

					Console.ForegroundColor = SeverityToColor(severity);
					if (_event == "")
					{
						_event = SeverityToEvent(severity);
					}
					Console.Write($"[{_event}]");

					Console.ForegroundColor = ConsoleColor.Gray;
					Console.WriteLine($" {message}");

					Writer.WriteLine($"[{DateTime.Now.ToString("G")}] [{_event}] {message}");
					Writer.Flush();
				}
			}
		}

		public static void Error(string message)
		{
			if (Writer != null)
			{
				lock (Writer)
				{
					Writer.WriteLine($"[{DateTime.Now.ToString("G")}] [FATAL] {message}");
					Writer.Flush();
				}

				Console.WriteLine(message, ConsoleColor.Red);
				Console.ReadLine();
				Environment.Exit(0);
			}
		}
	}
}
