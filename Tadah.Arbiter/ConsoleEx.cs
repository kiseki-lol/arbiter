using System;

namespace Tadah.Arbiter
{
    public static class ConsoleEx
    {
		// would race conditions be an issue here? 

		public static void Write(string message, ConsoleColor color = ConsoleColor.Gray)
		{
			Console.ForegroundColor = color;
			Console.Write(message);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		public static void WriteLine(string message, ConsoleColor color = ConsoleColor.Gray)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(message);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		public static void Error(string message)
		{
			ConsoleEx.WriteLine(message, ConsoleColor.Red);
			Console.ReadLine();
			Environment.Exit(0);
		}
	}
}
