using System;

namespace Attribulatorulator.Utils
{
	public static class Logging
	{
		private struct Channel
		{
			public ConsoleColor Color;
			public string Name;
		}

		private static readonly Channel[] _channels = new[]
		{
			new Channel { Color = ConsoleColor.Gray, Name = "   TRACE" },
			new Channel { Color = ConsoleColor.Green, Name = "   DEBUG" },
			new Channel { Color = ConsoleColor.Yellow, Name = " WARNING" },
			new Channel { Color = ConsoleColor.Cyan, Name = "    INFO" },
			new Channel { Color = ConsoleColor.Red, Name = "   FATAL" },
		};

		public static void Exception(Exception e)
		{
			Fatal(e.Message);

			var stackTrace = e.StackTrace;

			if (stackTrace is not null)
			{
				Fatal(stackTrace);
			}
		}

		public static void Trace(string message) => Message(0, message);
		public static void Debug(string message) => Message(1, message);
		public static void Warning(string message) => Message(2, message);
		public static void Info(string message) => Message(3, message);
		public static void Fatal(string message) => Message(4, message);

		public static void Message(int channelIndex, string message)
		{
			Console.Write('[');

			var channel = _channels[channelIndex];
			var color = channel.Color;

			Console.ForegroundColor = color;

			Console.Write(channel.Name);
			Console.ResetColor();
			Console.Write($"] {message}{Environment.NewLine}");
		}
	}
}
