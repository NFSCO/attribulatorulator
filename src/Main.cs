using System.Diagnostics;
using System.IO;

using Attribulator.Utils;

namespace Attribulatorulator
{
	public class Program
	{
		private static readonly string ms_VanillaUnpackedDirectoryName = "VanillaUnpacked";

		private static bool Build(string rootDirectory, string dstDirectory)
		{
			// delete directories from past compilation, if any.
			foreach (var directory in new[]
			{
				"Packed",
				"Unpacked",
			})
			{
				if (!FileSystem.DeleteDirectory(directory, true))
				{
					return false;
				}
			}

			if (!FileSystem.CopyDirectory(ms_VanillaUnpackedDirectoryName, "Unpacked", true))
			{
				return false;
			}

			var scriptsDirectory = $"{rootDirectory}/scripts/";

			if (!BuildScriptsLua(rootDirectory, $"{scriptsDirectory}lua"))
			{
				return false;
			}

			if (!BuildScriptsNFSMS($"{scriptsDirectory}nfsms"))
			{
				return false;
			}

			var mainDirectory = "Packed/main";

			if (FileSystem.DirectoryExists(mainDirectory))
			{
				// the post-build copy directory is optional.
				if (FileSystem.DirectoryExists(dstDirectory))
				{
					if (!FileSystem.CopyDirectory(mainDirectory, dstDirectory, false))
					{
						return false;
					}

					// if we have a post-build copy directory, also delete the Packed directory.
					if (!FileSystem.DeleteDirectory("Packed", true))
					{
						return false;
					}
				}

				foreach (var directory in Directory.GetDirectories(Directory.GetCurrentDirectory(), "Unpacked*"))
				{
					if (!FileSystem.DeleteDirectory(directory, true))
					{
						return false;
					}
				}

				return true;
			}

			else
			{
				Logging.Message("Skipping directory clean-up, directory Packed does not exist.");
			}

			return false;
		}

		private static bool BuildScriptsLua(string rootDirectory, string scriptsDirectory)
		{
			if (FileSystem.DirectoryExists(scriptsDirectory))
			{
				var compilerName = "luac.exe";
				var compilerPath = $"{rootDirectory}/tools/{compilerName}";

				if (File.Exists(compilerPath))
				{
					// compile each script in place.
					foreach (var script in Directory.GetFiles(scriptsDirectory, "*.lua", SearchOption.AllDirectories))
					{
						var srcFile = Path.GetFileName(script);
						var dstFile = Path.Combine(Path.GetDirectoryName(script), Path.ChangeExtension(srcFile, ".bin"));
						var process = Process.Start(compilerPath, $"-s -o {dstFile} {script}");

						process.WaitForExit();

						Logging.Message($"Compiled Lua script {srcFile}. ({process.ExitCode})");
					}

					return FileSystem.CopyDirectory(scriptsDirectory, "Unpacked/main/gameplay", true);
				}

				else
				{
					Logging.Message($"File {compilerName} does not exist.");
				}
			}

			else
			{
				Logging.Message($"Directory {scriptsDirectory} does not exist.");
			}

			return false;
		}

		private static bool BuildScriptsNFSMS(string scriptsDirectory)
		{
			var scripts = string.Empty;
			var scriptsCount = 0;

			if (FileSystem.DirectoryExists(scriptsDirectory))
			{
				foreach (var subDirectory in new[]
				{
					"shared",
					"attribulator",
				})
				{
					foreach (var file in Directory.GetFiles($"{scriptsDirectory}/{subDirectory}", "*.nfsms", SearchOption.AllDirectories))
					{
						scripts += $"{file} ";
						++scriptsCount;
					}
				}
			}

			else
			{
				Logging.Message($"Directory {scriptsDirectory} does not exist.");
			}

			if (scriptsCount > 0)
			{
				Logging.Message($"Compiling {scriptsCount} NFSMS script(s)...");

				var process = Process.Start("Attribulator.CLI.exe", $"apply-script -i Unpacked -o Packed -p CARBON -s {scripts}");

				process.WaitForExit();

				var exitCode = process.ExitCode;

				Logging.Message($"Attribulator exited with code {exitCode}.");

				return exitCode == 0;
			}

			return true;
		}

		private static bool Build(string[] args)
		{
			var dstDirectory = string.Empty;

			if (args.Length > 1)
			{
				if (args.Length > 2)
				{
					dstDirectory = args[2];
				}

				var attribulatorDirectory = args[0];

				if (FileSystem.DirectoryExists(attribulatorDirectory))
				{
					if (!FileSystem.SetCurrentDirectory(attribulatorDirectory))
					{
						return false;
					}

					if (FileSystem.DirectoryExists(ms_VanillaUnpackedDirectoryName))
					{
						var attribulatorName = "Attribulator.CLI.exe";

						if (File.Exists(attribulatorName))
						{
							return Build(args[1], dstDirectory);
						}

						else
						{
							Logging.Message($"File {attribulatorName} does not exist.");
						}
					}

					else
					{
						Logging.Message($"Directory {ms_VanillaUnpackedDirectoryName} does not exist.");
					}
				}

				else
				{
					Logging.Message($"Directory {attribulatorDirectory} does not exist.");
				}
			}

			else
			{
				Logging.Message($"Usage: {Process.GetCurrentProcess().ProcessName} path/to/attribulator path/to/repository [path/to/copy/post/build].");
			}

			return false;
		}

		public static void Main(string[] args) => Logging.Message($"Build {(Build(args) ? "successful" : "failed")}.");
	}
}
