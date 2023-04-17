using System;

namespace StockTrackerApp
{
	public static class PrintWithColorHelper
	{
	}
	public static class PrintWithColor
	{
		public static void WriteLine(Transaction log, string prefix = "")
		{
			PrintWithColor.WriteLine(prefix + log.AsReadably());
			// PrintWithColor.WriteLine(" time: " + log.GetTimeSpan().ToTotalHours(), background: log.Action == Action.Start ? ConsoleColor.Blue : null);
		}
		public static void WriteLine(string text, ConsoleColor? background = default, ConsoleColor? foreground = default)
		{
			Console.BackgroundColor = background ?? Console.BackgroundColor;
			Console.ForegroundColor = foreground ?? Console.ForegroundColor;
			Console.WriteLine(text);
			Console.ResetColor();
		}
		public static void Write(string text, ConsoleColor? background = default, ConsoleColor? foreground = default)
		{
			Console.BackgroundColor = background ?? Console.BackgroundColor;
			Console.ForegroundColor = foreground ?? Console.ForegroundColor;
			Console.Write(text);
			Console.ResetColor();
		}
	}
}