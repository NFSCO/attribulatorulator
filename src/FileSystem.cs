using System;
using System.IO;

using Attribulatorulator.Utils;

namespace Attribulatorulator
{
	public static class FileSystem
	{
		public static FileInfo CopyFile(FileInfo info, string dstPath)
		{
			try
			{
				return info.CopyTo(dstPath, true);
			}

			catch (Exception e)
			{
				Logging.Message($"An exception occurred while attempting to copy file {info.Name}.");
				Logging.Exception(e);
			}

			return null;
		}

		public static bool DirectoryExists(string path) => Directory.Exists(path);

		public static bool SetCurrentDirectory(string path)
		{
			try
			{
				Directory.SetCurrentDirectory(path);

				return true;
			}

			catch (Exception e)
			{
				Logging.Message($"An exception occurred while attempting to change the current directory to {path}.");
				Logging.Exception(e);
			}

			return false;
		}

		public static DirectoryInfo CreateDirectory(string path)
		{
			try
			{
				return Directory.CreateDirectory(path);
			}

			catch (Exception e)
			{
				Logging.Message($"An exception occurred while attempting to create directory {path}.");
				Logging.Exception(e);
			}

			return null;
		}

		public static bool DeleteDirectory(string path, bool isRecursive)
		{
			if (DirectoryExists(path))
			{
				try
				{
					Directory.Delete(path, isRecursive);

					return true;
				}

				catch (Exception e)
				{
					Logging.Message($"An exception occurred while attempting to delete directory {path}.");
					Logging.Exception(e);
				}
			}

			else
			{
				Logging.Message($"Directory {path} does not exist.");

				return true;
			}

			return false;
		}

		public static bool CopyDirectory(string srcPath, string dstPath, bool copySubDirectories)
		{
			try
			{
				var srcDirectory = new DirectoryInfo(srcPath);

				if (srcDirectory.Exists)
				{
					var dstDirectory = CreateDirectory(dstPath);

					if (dstDirectory is not null)
					{
						foreach (var file in srcDirectory.GetFiles())
						{
							if (CopyFile(file, Path.Combine(dstPath, file.Name)) is null)
							{
								return false;
							}
						}

						if (copySubDirectories)
						{
							foreach (var subDirectory in srcDirectory.GetDirectories())
							{
								if (!CopyDirectory(subDirectory.FullName, Path.Combine(dstPath, subDirectory.Name), copySubDirectories))
								{
									return false;
								}
							}
						}

						return true;
					}
				}

				else
				{
					Logging.Message($"Directory {srcPath} does not exist.");
				}
			}

			catch (Exception e)
			{
				Logging.Message($"An exception occurred while attempting to copy directory {srcPath} to directory {dstPath}.");
				Logging.Exception(e);
			}

			return false;
		}
	}
}
