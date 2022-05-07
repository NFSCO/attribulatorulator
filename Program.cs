using System.Diagnostics;
using System.IO;
using System;

namespace Attribulatorulator
{
	public class Program
	{
		private static string ms_VanillaUnpackedFolderName = "VanillaUnpacked";

		private static void Log(string message) => Console.WriteLine(message);

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
				Log("Usage: Attribulatorulator.exe path/to/attribulator path/to/repository [destination/path].");

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

			// delete folders from past compilation.
			foreach (var directory in new[]
			{
				"Packed",
				"Unpacked",
			})
			{
				if (Directory.Exists(directory))
				{
					Directory.Delete(directory, true);
				}
			}

			CopyDirectory(ms_VanillaUnpackedFolderName, "Unpacked", true);

			var rootDirectory = args[1];
			var scripts = "";

			foreach (var script in Directory.GetFiles($"{rootDirectory}/scripts/lua", "*.lua", SearchOption.AllDirectories))
			{
				var scriptLua = Path.GetFileName(script);
				var dstHandler = Path.ChangeExtension(scriptLua, ".bin");

				Log($"Compiling {scriptLua}...");

				foreach (var srcHandler in Directory.GetFiles("Unpacked", "*.bin", SearchOption.AllDirectories))
				{
					if (Path.GetFileName(srcHandler) == dstHandler)
					{
						Process.Start($"{rootDirectory}/tools/luac.exe", $"-o {srcHandler} {script}").WaitForExit();

						break;
					}
				}
			}

			foreach (var subDirectory in new[]
			{
				"shared",
				"attribulator",
			})
			{
				foreach (var file in Directory.GetFiles($"{rootDirectory}/scripts/nfsms/{subDirectory}", "*.nfsms", SearchOption.AllDirectories))
				{
					scripts += $"{file} ";
				}
			}

			Process.Start("Attribulator.CLI.exe", $"apply-script -i Unpacked -o Packed -p CARBON -s {scripts}").WaitForExit();

			if (Directory.Exists("Packed/main"))
			{
				if (!string.IsNullOrEmpty(dstDirectory) && Directory.Exists(dstDirectory))
				{
					CopyDirectory("Packed/main", dstDirectory, false);

					// if we have a post-build copy directory, also delete the Packed folder.
					Directory.Delete("Packed", true);
				}

				Directory.Delete("Unpacked", true);
			}

			else
			{
				Log("Skipping deletion of Unpacked, Packed folder not found.");
			}

			Log("Done!");
		}
	}
}
