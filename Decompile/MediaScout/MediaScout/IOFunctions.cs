using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MediaScout
{
	public static class IOFunctions
	{
		public static string GetDirectoryName(string path)
		{
			return new System.IO.DirectoryInfo(path).Name;
		}

		public static string GetValidName(string name, string ReplaceChar)
		{
			return Regex.Replace(name, "[\\\\|\\/|\\:|\\*|\\?|\\\"|\\<|\\>|\\|]", ReplaceChar);
		}

		public static string GetValidName1(string name)
		{
			char[] invalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();
			for (int i = 0; i < invalidFileNameChars.Length; i++)
			{
				char c = invalidFileNameChars[i];
				name = Regex.Replace(name, c.ToString(), string.Empty);
			}
			return name;
		}

		public static bool MergeDirectories(string src, string dest, bool overwrite)
		{
			bool result = false;
			if (System.IO.Directory.Exists(src))
			{
				if (!System.IO.Directory.Exists(dest))
				{
					System.IO.Directory.Move(src, dest);
				}
				else
				{
					System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(src);
					System.IO.DirectoryInfo directoryInfo2 = new System.IO.DirectoryInfo(dest);
					if (src.ToLower() != dest.ToLower())
					{
						result = IOFunctions.CopyDirectory(directoryInfo, directoryInfo2, false);
					}
					else if (overwrite || MessageBox.Show("Do you want to rename folder from " + directoryInfo.Name + " to " + directoryInfo2.Name, "MediaScout", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						System.IO.Directory.Move(src, dest + "temp");
						System.IO.Directory.Move(dest + "temp", dest);
						result = true;
					}
				}
			}
			return result;
		}

		public static bool CopyDirectory(System.IO.DirectoryInfo source, System.IO.DirectoryInfo target, bool overwrite)
		{
			System.IO.DirectoryInfo[] directories = source.GetDirectories();
			for (int i = 0; i < directories.Length; i++)
			{
				System.IO.DirectoryInfo directoryInfo = directories[i];
				if (System.IO.Directory.Exists(System.IO.Path.Combine(target.FullName, directoryInfo.Name)))
				{
					IOFunctions.CopyDirectory(directoryInfo, new System.IO.DirectoryInfo(System.IO.Path.Combine(target.FullName, directoryInfo.Name)), overwrite);
				}
				else
				{
					IOFunctions.CopyDirectory(directoryInfo, target.CreateSubdirectory(directoryInfo.Name), overwrite);
				}
			}
			System.IO.FileInfo[] files = source.GetFiles();
			for (int j = 0; j < files.Length; j++)
			{
				System.IO.FileInfo fileInfo = files[j];
				string text = System.IO.Path.Combine(target.FullName, fileInfo.Name);
				if (!System.IO.File.Exists(text))
				{
					fileInfo.MoveTo(text);
				}
				else if (overwrite || MessageBox.Show("Do you want to overwrite " + fileInfo.FullName + " with " + text, "MediaScout", MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					IOFunctions.DeleteFile(text, true);
					fileInfo.MoveTo(text);
				}
			}
			if (source.GetFiles().Length == 0 && source.GetDirectories().Length == 0)
			{
				IOFunctions.DeleteDirectory(source.FullName, true);
				return true;
			}
			return false;
		}

		public static void CreateHiddenFolder(string FolderPath)
		{
			System.IO.Directory.CreateDirectory(FolderPath);
			System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(FolderPath);
			if ((directoryInfo.Attributes & System.IO.FileAttributes.Hidden) != System.IO.FileAttributes.Hidden)
			{
				directoryInfo.Attributes |= System.IO.FileAttributes.Hidden;
			}
		}

		public static System.IO.FileInfo MoveFile(System.IO.FileInfo src, string dest, bool overwrite)
		{
			try
			{
				if (!System.IO.File.Exists(dest))
				{
					src.MoveTo(dest);
				}
				else if (overwrite || MessageBox.Show(string.Concat(new object[]
				{
					"Do you want to overwrite ",
					src,
					" with ",
					dest
				}), "MediaScout", MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					if (src.FullName != dest)
					{
						IOFunctions.DeleteFile(dest, true);
						src.MoveTo(dest);
					}
					else
					{
						System.IO.File.Move(src.FullName, dest + "temp");
						System.IO.File.Move(dest + "temp", dest);
					}
				}
			}
			catch (System.Exception ex)
			{
				throw new System.Exception(ex.Message, ex);
			}
			return src;
		}

		public static bool MoveFile(string src, string dest, bool overwrite)
		{
			bool result = false;
			System.IO.FileInfo fileInfo = new System.IO.FileInfo(src);
			fileInfo = IOFunctions.MoveFile(fileInfo, dest, overwrite);
			if (fileInfo.FullName != src)
			{
				result = true;
			}
			return result;
		}

		public static void DeleteDirectory(string path, bool SendtoRecycleBin)
		{
			if (SendtoRecycleBin)
			{
				FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
				return;
			}
			FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
		}

		public static void DeleteFile(string path, bool SendtoRecycleBin)
		{
			if (SendtoRecycleBin)
			{
				FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
				return;
			}
			FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
		}
	}
}
