using System;

using Attribulatorulator.Utils;

using static System.Diagnostics.Process;

namespace Attribulatorulator
{
	public static class Process
	{
		public static bool Create(string path, string arguments)
		{
			try
			{
				var process = Start(path, arguments);

				process.WaitForExit();

				var exitCode = process.ExitCode;
				var result = exitCode == 0;

				if (!result)
				{
					Logging.Fatal($"Process {path} exited with code {exitCode}.");
				}

				return result;
			}
			catch (Exception e)
			{
				Logging.Fatal($"An exception occurred while attempting to create process {path}.");
				Logging.Exception(e);
			}

			return false;
		}
	}
}
