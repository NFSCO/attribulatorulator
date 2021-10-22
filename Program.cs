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

		public static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Log("Not enough arguments provided.");
				Log("Usage: Attribulatorulator.exe path\\to\\attribulator path\\to\\scripts");

				return;
			}

			Directory.SetCurrentDirectory(args[0]);

			if (!File.Exists("Attribulator.CLI.exe"))
			{
				Log("Attribulator.CLI.exe not found.");

				return;
			}

			var rootDirectory = args[1];
			var scripts = "";

			foreach (var subDirectory in new[]
			{
				"shared",
				"attribulator",
			})
			{
				foreach (var file in Directory.GetFiles($"{rootDirectory}\\{subDirectory}", "*.nfsms", SearchOption.AllDirectories))
				{
					Log($"Including script {file}.");

					scripts += $"{file} ";
				}
			}

			Log("Starting Attribulator...");

			Process.Start("Attribulator.CLI.exe", $"apply-script -i Unpacked -o Packed -p CARBON -s {scripts}").WaitForExit();

			Log("Deleting residue folders...");

			Directory.Delete("Unpacked", true);

			var dirs = Directory.GetDirectories(Directory.GetCurrentDirectory(), "Unpacked_*");

			if (dirs.Length > 0)
			{
				Directory.Move(dirs[0], "Unpacked");
			}

			else
			{
				Log("Could not find Unpacked_*.");
			}

			Log("Done!");
		}
	}
}
