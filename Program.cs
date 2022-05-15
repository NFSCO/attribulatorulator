using System.Diagnostics;
using System.IO;
using System;

namespace Attribulatorulator
{
	public class Program
	{
		private static readonly string ms_VanillaUnpackedDirectoryName = "VanillaUnpacked";

		private static void Build(string rootDirectory, string dstDirectory)
		{
			// delete directories from past compilation, if any.
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

			CopyDirectory(ms_VanillaUnpackedDirectoryName, "Unpacked", true);

			BuildScriptsLua(rootDirectory, $"{rootDirectory}/scripts/lua");
			BuildScriptsNFSMS($"{rootDirectory}/scripts/nfsms");

			if (Directory.Exists("Packed/main"))
			{
				if (Directory.Exists(dstDirectory))
				{
					CopyDirectory("Packed/main", dstDirectory, false);

					// if we have a post-build copy directory, also delete the Packed directory.
					Directory.Delete("Packed", true);
				}

				Directory.Delete("Unpacked", true);
			}

			else
			{
				Log("Skipping deletion of directory Unpacked, directory Packed doesn't exist.");
			}

			Log("Done!");
		}

		private static void BuildScriptsLua(string rootDirectory, string scriptsDirectory)
		{
			if (Directory.Exists(scriptsDirectory))
			{
				var compilerPath = $"{rootDirectory}/tools/luac.exe";

				if (File.Exists(compilerPath))
				{
					var compiledScriptFiles = Directory.GetFiles("Unpacked", "*.bin", SearchOption.AllDirectories);

					foreach (var script in Directory.GetFiles(scriptsDirectory, "*.lua", SearchOption.AllDirectories))
					{
						var srcFile = Path.GetFileName(script);
						var dstFile = Path.ChangeExtension(srcFile, ".bin");

						Log($"Compiling Lua script {srcFile}...");

						foreach (var compiledScriptFile in compiledScriptFiles)
						{
							if (Path.GetFileName(compiledScriptFile) == dstFile)
							{
								Process.Start(compilerPath, $"-o {compiledScriptFile} {script}").WaitForExit();

								break;
							}
						}
					}
				}

				else
				{
					Log("File luac.exe doesn't exist.");
				}
			}

			else
			{
				Log($"Directory {scriptsDirectory} doesn't exist.");
			}
		}

		private static void BuildScriptsNFSMS(string scriptsDirectory)
		{
			var scripts = "";

			if (Directory.Exists(scriptsDirectory))
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
					}
				}
			}

			else
			{
				Log($"Directory {scriptsDirectory} doesn't exist.");
			}

			Process.Start("Attribulator.CLI.exe", $"apply-script -i Unpacked -o Packed -p CARBON -s {scripts}").WaitForExit();
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
				throw new DirectoryNotFoundException($"Directory {srcDirectory} doesn't exist.");
			}
		}

		private static void Log(string message) => Console.WriteLine(message);

		public static void Main(string[] args)
		{
			var dstDirectory = string.Empty;

			if (args.Length > 1)
			{
				if (args.Length > 2)
				{
					dstDirectory = args[2];
				}

				var attribulatorDirectory = args[0];

				if (Directory.Exists(attribulatorDirectory))
				{
					Directory.SetCurrentDirectory(attribulatorDirectory);

					if (Directory.Exists(ms_VanillaUnpackedDirectoryName))
					{
						if (File.Exists("Attribulator.CLI.exe"))
						{
							Build(args[1], dstDirectory);
						}

						else
						{
							Log("File Attribulator.CLI.exe doesn't exist.");
						}
					}

					else
					{
						Log($"Directory {ms_VanillaUnpackedDirectoryName} doesn't exist.");
					}
				}

				else
				{
					Log($"Directory {attribulatorDirectory} doesn't exist.");
				}
			}

			else
			{
				Log("Not enough arguments provided.");
				Log($"Usage: {Process.GetCurrentProcess().ProcessName} path/to/attribulator path/to/repository [path/to/copy/post/build].");
			}
		}
	}
}
