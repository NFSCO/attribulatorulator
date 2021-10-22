using System.Diagnostics;
using System.IO;
using System;

namespace Attribulatorulator
{
	public class Program
	{
		private static void Log(string message)
		{
			Console.WriteLine($"ATTRIBULATORULATOR: {message}");
		}

		public static void Main()
		{
			if (!File.Exists("Attribulator.CLI.exe"))
			{
				Log("Attribulator.CLI.exe not found.");

				return;
			}

			var rootDirectory = "D:\\Dev\\nfsco-bin\\scripts\\nfsms";

			var subDirectories = new[]
			{
				"shared",
				"attribulator",
			};

			var scripts = "";

			foreach (var subDirectory in subDirectories)
			{
				foreach (var file in Directory.GetFiles($"{rootDirectory}\\{subDirectory}", "*.nfsms"))
				{
					Log($"Including script {file}.");

					scripts += $"{file} ";
				}
			}

			Log("Starting Attribulator...");

			var attribulatorProcess = Process.Start("Attribulator.CLI.exe", $"apply-script -i Unpacked -o Packed -p CARBON -s {scripts}");

			attribulatorProcess.WaitForExit();

			Log("Deleting residue folders...");

			Directory.Delete("Unpacked", true);

			var dirs = Directory.GetDirectories(Directory.GetCurrentDirectory(), "Unpacked_*");

			if (dirs.Length > 0)
			{
				Directory.Move(dirs[0], "Unpacked");
			}
		}
	}
}
