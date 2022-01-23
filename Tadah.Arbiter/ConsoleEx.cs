using System;

namespace Tadah.Arbiter
{
    public static class ConsoleEx
    {
		// would race conditions be an issue here? 

		public static void Write(string Message, ConsoleColor Color = ConsoleColor.Gray)
		{
			Console.ForegroundColor = Color;
			Console.Write(Message);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		public static void WriteLine(string Message, ConsoleColor Color = ConsoleColor.Gray)
		{
			Console.ForegroundColor = Color;
			Console.WriteLine(Message);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		public static void Error(string Message)
		{
			ConsoleEx.WriteLine(Message, ConsoleColor.Red);
			Console.ReadLine();
			Environment.Exit(0);
		}
	}
}
