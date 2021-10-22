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

		private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);

			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
			}

			var dirs = dir.GetDirectories();

			Directory.CreateDirectory(destDirName);

			var files = dir.GetFiles();
			foreach (var file in files)
			{
				string tempPath = Path.Combine(destDirName, file.Name);
				file.CopyTo(tempPath, false);
			}

			if (copySubDirs)
			{
				foreach (var subdir in dirs)
				{
					string tempPath = Path.Combine(destDirName, subdir.Name);
					DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
				}
			}
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

			if (!Directory.Exists("Vanilla_Unpacked"))
			{
				Log("Please name the vanilla Unpacked directory to Vanilla_Unpacked");
				return;
			}

			if (!Directory.Exists("Unpacked"))
			{
				DirectoryCopy("Vanilla_Unpacked", "Unpacked", true);
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

			Directory.Delete("Unpacked");
			foreach (var dir in Directory.GetDirectories(Directory.GetCurrentDirectory(), "Unpacked*"))
			{
				Directory.Delete(dir, true);
			}

			DirectoryCopy("Vanilla_Unpacked", "Unpacked", true);


			Log("Done!");
		}
	}
}
