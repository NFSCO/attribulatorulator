using System;

namespace Attribulatorulator.Utils
{
	public static class Logging
	{
		public static void Message(string message) => Console.WriteLine(message);

		public static void Exception(Exception e)
		{
			Message(e.Message);

			var stackTrace = e.StackTrace;

			if (stackTrace is not null)
			{
				Message(stackTrace);
			}
		}
	}
}
