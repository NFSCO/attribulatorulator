using System.Diagnostics;
using System.IO;
using System;

namespace Attribulatorulator
{
	public class Program
	{
		private static string ms_VanillaUnpackedFolderName = "VanillaUnpacked";

		private static void Log(string message)
		{
			Console.WriteLine(message);
		}

		private static void CopyDirectory(string srcDirectory, string dstDirectory, bool copySubDirectories)
		{
			var directory = new DirectoryInfo(srcDirectory);

			if (directory.Exists)
			{
				Directory.CreateDirectory(dstDirectory);

				foreach (var file in directory.GetFiles())
				{
					file.CopyTo(Path.Combine(dstDirectory, file.Name), true);
				}

				if (copySubDirectories)
				{
					foreach (var subDirectory in directory.GetDirectories())
					{
						CopyDirectory(subDirectory.FullName, Path.Combine(dstDirectory, subDirectory.Name), copySubDirectories);
					}
				}
			}

			else
			{
				throw new DirectoryNotFoundException($"Source directory {srcDirectory} does not exist or could not be found.");
			}
		}

		public static void Main(string[] args)
		{
			var dstDirectory = "";

			if (args.Length > 1)
			{
				if (args.Length > 2)
				{
					dstDirectory = args[2];
				}
			}

			else
			{
				Log("Not enough arguments provided.");
				Log("Usage: Attribulatorulator.exe path\\to\\attribulator path\\to\\nfsms\\scripts [destination\\path].");

				return;
			}

			Directory.SetCurrentDirectory(args[0]);

			if (!File.Exists("Attribulator.CLI.exe"))
			{
				Log("Attribulator.CLI.exe not found.");

				return;
			}

			if (!Directory.Exists(ms_VanillaUnpackedFolderName))
			{
				Log($"{ms_VanillaUnpackedFolderName} not found.");

				return;
			}

			if (!Directory.Exists("Unpacked"))
			{
				CopyDirectory(ms_VanillaUnpackedFolderName, "Unpacked", true);
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

			Process.Start("Attribulator.CLI.exe", $"apply-script -i Unpacked -o Packed -p CARBON -s {scripts}").WaitForExit();

			if (Directory.Exists("Packed"))
			{
				if (!string.IsNullOrEmpty(dstDirectory) && Directory.Exists(dstDirectory))
				{
					CopyDirectory("Packed\\main", dstDirectory, false);
				}

				Directory.Delete("Unpacked", true);
			}

			else
			{
				Log("Skipping deletion of Unpacked, Packed folder not found.");
			}

			foreach (var dir in Directory.GetDirectories(Directory.GetCurrentDirectory(), "Unpacked*"))
			{
				Directory.Delete(dir, true);
			}

			CopyDirectory(ms_VanillaUnpackedFolderName, "Unpacked", true);

			Log("Done!");
		}
	}
}
