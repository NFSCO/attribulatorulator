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

				Logging.Message($"{path} exited with code {exitCode}.");

				return exitCode == 0;
			}
			catch (Exception e)
			{
				Logging.Message($"An exception occurred while attempting to create process {path}.");
				Logging.Exception(e);
			}

			return false;
		}
	}
}
