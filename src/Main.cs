using System.IO;

using Attribulatorulator.Utils;

namespace Attribulatorulator
{
	public class Program
	{
		private static readonly string ms_VanillaUnpackedDirectoryName = "VanillaUnpacked";
		private static readonly string ms_AttribulatorExecutableName = "Attribulator.CLI.exe";

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

			var scriptsDirectory = Path.Combine(rootDirectory, "src");

			if (!BuildScriptsLua(rootDirectory, Path.Combine(scriptsDirectory, "lua")))
			{
				return false;
			}

			if (!BuildScriptsNFSMS(Path.Combine(scriptsDirectory, "nfsms")))
			{
				return false;
			}

			var mainDirectory = Path.Combine("Packed", "main");

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
				Logging.Info("Skipping directory clean-up, directory Packed does not exist.");
			}

			return false;
		}

		private static bool BuildScriptsLua(string rootDirectory, string scriptsDirectory)
		{
			if (FileSystem.DirectoryExists(scriptsDirectory))
			{
				var compilerName = "luac.exe";
				var compilerPath = Path.Combine(rootDirectory, "tools", compilerName);

				if (File.Exists(compilerPath))
				{
					// compile each script in place.
					foreach (var script in Directory.GetFiles(scriptsDirectory, "*.lua", SearchOption.AllDirectories))
					{
						var dstFile = Path.Combine(Path.GetDirectoryName(script), Path.ChangeExtension(Path.GetFileName(script), ".bin"));

						if (!Process.Create(compilerPath, $"-s -o {dstFile} {script}"))
						{
							Logging.Fatal($"Script {script} could not compile.");

							return false;
						}
					}

					return FileSystem.CopyDirectory(scriptsDirectory, "Unpacked/main/gameplay", true);
				}
				else
				{
					Logging.Warning($"File {compilerName} does not exist.");
				}
			}
			else
			{
				Logging.Warning($"Directory {scriptsDirectory} does not exist.");
			}

			return false;
		}

		private static bool BuildScriptsNFSMS(string scriptsDirectory)
		{
			var directories = string.Empty;

			if (FileSystem.DirectoryExists(scriptsDirectory))
			{
				foreach (var subDirectory in new[]
				{
					"shared",
					"attribulator",
				})
				{
					directories += $"{Path.Combine(scriptsDirectory, subDirectory)} ";
				}
			}
			else
			{
				Logging.Warning($"Directory {scriptsDirectory} does not exist.");
			}

			return Process.Create(ms_AttribulatorExecutableName, $"apply-script -i Unpacked -o Packed -p CARBON -s {directories}");
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
						if (File.Exists(ms_AttribulatorExecutableName))
						{
							return Build(args[1], dstDirectory);
						}
						else
						{
							Logging.Fatal($"File {ms_AttribulatorExecutableName} does not exist.");
						}
					}
					else
					{
						Logging.Fatal($"Directory {ms_VanillaUnpackedDirectoryName} does not exist.");
					}
				}
				else
				{
					Logging.Fatal($"Directory {attribulatorDirectory} does not exist.");
				}
			}
			else
			{
				var currentProcess = System.Diagnostics.Process.GetCurrentProcess();

				Logging.Info($"Usage: {currentProcess.ProcessName} path/to/attribulator path/to/repository [path/to/copy/post/build].");
			}

			return false;
		}

		public static void Main(string[] args)
		{
			var result = Build(args);
			var @string = "successful";
			var channelIndex = 3;

			if (!result)
			{
				@string = "failed";
				++channelIndex;
			}

			Logging.Message(channelIndex, $"Build {@string}.");
		}
	}
}
