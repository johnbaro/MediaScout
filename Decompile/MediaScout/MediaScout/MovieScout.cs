using MediaScout.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MediaScout
{
	public class MovieScout
	{
		private MediaScoutMessage.Message Message;

		private MovieScoutOptions options;

		private TheMovieDBProvider tmdb;

		private string ImagesByNameLocation;

		public MovieXML m;

		private Posters[] posters;

		private Posters[] backdrops;

		private int level = 1;

		public MovieScout(MovieScoutOptions options, MediaScoutMessage.Message Message, string ImagesByNameLocation)
		{
			this.options = options;
			this.Message = Message;
			this.ImagesByNameLocation = ImagesByNameLocation;
			this.tmdb = new TheMovieDBProvider(Message);
		}

		private string GetMovieDirName()
		{
			string name = string.Format(this.options.DirRenameFormat, this.m.Title, this.m.ProductionYear);
			return IOFunctions.GetValidName(name, this.options.FilenameReplaceChar);
		}

		public string ProcessDirectory(string directory)
		{
			string text = null;
			int num = this.level;
			text = IOFunctions.GetDirectoryName(directory);
			if (this.options.RenameFiles)
			{
				string movieDirName = this.GetMovieDirName();
				if (text != movieDirName)
				{
					string text2 = System.IO.Path.GetDirectoryName(directory) + "\\" + movieDirName;
					if (IOFunctions.MergeDirectories(directory, text2, this.options.overwrite))
					{
						text = "d:" + movieDirName;
					}
					else
					{
						text = movieDirName;
					}
					directory = text2;
				}
			}
			this.SaveMeta(directory, num);
			if (this.options.GetMoviePosters)
			{
				this.SaveImage(directory, "folder.jpg", this.posters, 0, MoviePosterType.Poster, num);
			}
			if (this.options.DownloadAllPosters)
			{
				this.Message("Downloading All Posters", MediaScoutMessage.MessageType.Task, num);
				if (this.posters == null)
				{
					this.posters = this.tmdb.GetPosters(this.m.ID, MoviePosterType.Poster);
				}
				if (this.posters != null)
				{
					int num2 = 0;
					Posters[] array = this.posters;
					for (int i = 0; i < array.Length; i++)
					{
						Posters arg_F4_0 = array[i];
						this.SaveImage(directory, "folder" + num2 + ".jpg", this.posters, num2, MoviePosterType.Poster, num + 1);
						num2++;
					}
				}
				else
				{
					this.Message("No Posters Found", MediaScoutMessage.MessageType.TaskError, num);
				}
			}
			if (this.options.GetMoviePosters)
			{
				if (this.options.SaveXBMCMeta)
				{
					this.SaveImage(directory, "fanart.jpg", this.backdrops, 0, MoviePosterType.Backdrop, num);
				}
				if (this.options.SaveMyMoviesMeta)
				{
					this.SaveImage(directory, "backdrop.jpg", this.backdrops, 0, MoviePosterType.Backdrop, num);
				}
			}
			if (this.options.DownloadAllBackdrops)
			{
				this.Message("Downloading All Backdrops", MediaScoutMessage.MessageType.Task, num);
				if (this.backdrops == null)
				{
					this.backdrops = this.tmdb.GetPosters(this.m.ID, MoviePosterType.Backdrop);
				}
				if (this.backdrops != null)
				{
					int num3 = 0;
					Posters[] array2 = this.backdrops;
					for (int j = 0; j < array2.Length; j++)
					{
						Posters arg_1FE_0 = array2[j];
						if (this.options.SaveXBMCMeta)
						{
							this.SaveImage(directory, "fanart" + num3 + ".jpg", this.backdrops, num3, MoviePosterType.Backdrop, num + 1);
						}
						if (this.options.SaveMyMoviesMeta)
						{
							this.SaveImage(directory, "backdrop" + num3 + ".jpg", this.backdrops, num3, MoviePosterType.Backdrop, num + 1);
						}
						num3++;
					}
				}
				else
				{
					this.Message("No Backdrops Found", MediaScoutMessage.MessageType.TaskError, num);
				}
			}
			System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(directory);
			System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>(this.options.AllowedFileTypes);
			System.IO.FileInfo[] files = directoryInfo.GetFiles();
			for (int k = 0; k < files.Length; k++)
			{
				System.IO.FileInfo fileInfo = files[k];
				if (list.Contains(fileInfo.Extension.ToLower()))
				{
					this.ProcessFile(directory, fileInfo, true, num);
				}
			}
			if (this.options.SaveActors)
			{
				if (this.options.SaveXBMCMeta)
				{
					try
					{
						this.Message("Saving Actors Thumb in " + new Person().GetXBMCDirectory() + "\\", MediaScoutMessage.MessageType.Task, num);
						if (this.m.Persons.Count != 0)
						{
							string text3 = directory + "\\" + new Person().GetXBMCDirectory();
							if (!System.IO.Directory.Exists(text3))
							{
								IOFunctions.CreateHiddenFolder(text3);
							}
							foreach (Person current in this.m.Persons)
							{
								if (current.Type == "Actor")
								{
									System.Collections.Generic.List<Posters> personImage = this.tmdb.GetPersonImage(current.ID);
									if (personImage != null && personImage.Count > 0)
									{
										current.Thumb = personImage[0].Poster;
									}
									if (!string.IsNullOrEmpty(current.Thumb))
									{
										string xBMCFilename = current.GetXBMCFilename();
										string text4 = text3 + "\\" + xBMCFilename;
										if (!System.IO.File.Exists(text4) || this.options.ForceUpdate)
										{
											current.SaveThumb(text4);
										}
									}
								}
							}
							this.Message("Done", MediaScoutMessage.MessageType.TaskResult, num);
						}
						else
						{
							this.Message("No Actors Found", MediaScoutMessage.MessageType.TaskError, num);
						}
					}
					catch (System.Exception ex)
					{
						this.Message(ex.Message, MediaScoutMessage.MessageType.TaskError, num);
					}
				}
				if (this.options.SaveMyMoviesMeta)
				{
					try
					{
						this.Message("Saving Actors Thumb in ImagesByName\\", MediaScoutMessage.MessageType.Task, num);
						if (!string.IsNullOrEmpty(this.ImagesByNameLocation))
						{
							if (this.m.Persons.Count != 0)
							{
								foreach (Person current2 in this.m.Persons)
								{
									if (current2.Type == "Actor")
									{
										System.Collections.Generic.List<Posters> personImage2 = this.tmdb.GetPersonImage(current2.ID);
										if (personImage2 != null && personImage2.Count > 0)
										{
											current2.Thumb = personImage2[0].Poster;
										}
										if (!string.IsNullOrEmpty(current2.Thumb))
										{
											string text5 = this.ImagesByNameLocation + "\\" + current2.GetMyMoviesDirectory();
											string text6 = text5 + "\\" + current2.GetMyMoviesFilename();
											if (!System.IO.File.Exists(text6) || this.options.ForceUpdate)
											{
												if (!System.IO.Directory.Exists(text5))
												{
													System.IO.Directory.CreateDirectory(text5);
												}
												current2.SaveThumb(text6);
											}
										}
									}
								}
								this.Message("Done", MediaScoutMessage.MessageType.TaskResult, num);
							}
							else
							{
								this.Message("No Actors Found", MediaScoutMessage.MessageType.TaskError, num);
							}
						}
						else
						{
							this.Message("ImagesByName Location Not Defined", MediaScoutMessage.MessageType.TaskError, num);
						}
					}
					catch (System.Exception ex2)
					{
						this.Message(ex2.Message, MediaScoutMessage.MessageType.TaskError, num);
					}
				}
			}
			return text;
		}

		private MovieType GetMovieFileType(System.IO.FileInfo fi)
		{
			MovieType movieType = MovieType.None;
			Match match = Regex.Match(fi.Name, "trailer", RegexOptions.IgnoreCase);
			if (match.Success)
			{
				movieType = MovieType.Trailer;
			}
			match = Regex.Match(fi.Name, "sample", RegexOptions.IgnoreCase);
			if (movieType == MovieType.None && match.Success)
			{
				movieType = MovieType.Sample;
			}
			match = Regex.Match(fi.Name, "cd([0-9]+)", RegexOptions.IgnoreCase);
			if (movieType == MovieType.None && match.Success)
			{
				movieType = MovieType.Multi_File_Rip;
			}
			if (movieType == MovieType.None)
			{
				movieType = MovieType.Single_File_Rip;
			}
			return movieType;
		}

		public string ProcessFile(string MovieDirectory, System.IO.FileInfo fi, bool IsInsideMovieFolder, int level)
		{
			if (level == -1)
			{
				level = this.level;
			}
			if (this.options.MoveFiles && !IsInsideMovieFolder)
			{
				string text = MovieDirectory + "\\" + this.GetMovieDirName();
				if (!System.IO.Directory.Exists(text))
				{
					System.IO.Directory.CreateDirectory(text);
				}
				MovieDirectory = text;
				System.IO.FileInfo fi2 = new System.IO.FileInfo(fi.FullName);
				string text2 = MovieDirectory + "\\" + fi.Name;
				fi = this.MoveFile(fi.FullName, text2, level, false);
				if (fi.FullName == text2)
				{
					this.MoveRelatedFiles(fi2, MovieDirectory, level);
					return this.ProcessDirectory(MovieDirectory);
				}
			}
			string text3 = fi.Name;
			this.Message("Processing File : " + fi.Name, MediaScoutMessage.MessageType.Task, level);
			MovieType movieFileType = this.GetMovieFileType(fi);
			this.Message(movieFileType.ToString().Replace("_", " "), MediaScoutMessage.MessageType.TaskResult, level);
			if (this.options.RenameFiles && (movieFileType == MovieType.Single_File_Rip || movieFileType == MovieType.Multi_File_Rip || movieFileType == MovieType.Trailer || movieFileType == MovieType.Sample))
			{
				System.IO.FileInfo fi3 = new System.IO.FileInfo(fi.FullName);
				string text4 = string.Format(this.options.FileRenameFormat, this.m.Title, this.m.ProductionYear);
				text4 = IOFunctions.GetValidName(text4, this.options.FilenameReplaceChar);
				switch (movieFileType)
				{
				case MovieType.Trailer:
					text4 += " - Trailer";
					break;
				case MovieType.Multi_File_Rip:
				{
					int num = -1;
					Match match;
					if ((match = Regex.Match(fi.Name, ".cd([0-9]+)", RegexOptions.IgnoreCase)).Success)
					{
						num = int.Parse(match.Groups[1].Value);
					}
					text4 = text4 + " - CD" + num;
					break;
				}
				case MovieType.Sample:
					text4 += " - Sample";
					break;
				}
				string text5 = fi.DirectoryName + "\\" + text4 + fi.Extension;
				if (fi.Name != text4 + fi.Extension)
				{
					if ((movieFileType == MovieType.Trailer || movieFileType == MovieType.Sample) && System.IO.File.Exists(text5) && !this.options.overwrite)
					{
						int num2 = 2;
						bool flag = false;
						while (!flag)
						{
							if (num2 > 10)
							{
								break;
							}
							text5 = string.Concat(new object[]
							{
								fi.DirectoryName,
								"\\",
								text4,
								" #",
								num2,
								fi.Extension
							});
							if (!System.IO.File.Exists(text5))
							{
								fi = this.MoveFile(fi.FullName, text5, level, true);
							}
							else
							{
								num2++;
							}
							flag = true;
						}
					}
					else
					{
						fi = this.MoveFile(fi.FullName, text5, level, true);
					}
					if (fi.Name == text4 + fi.Extension)
					{
						this.RenameRelatedFiles(fi3, text4, level + 1);
						text3 = text4 + fi.Extension;
					}
				}
			}
			if (this.options.GetMovieFilePosters && this.options.SaveXBMCMeta)
			{
				this.SaveImage(fi.DirectoryName, this.m.GetXBMCThumbFilename(fi.Name.Replace(fi.Extension, "")), this.posters, 0, MoviePosterType.File_Poster, level + 1);
			}
			if (this.options.GetMovieFilePosters && this.options.SaveXBMCMeta)
			{
				this.SaveImage(fi.DirectoryName, this.m.GetXBMCBackdropFilename(fi.Name.Replace(fi.Extension, "")), this.backdrops, 0, MoviePosterType.File_Backdrop, level + 1);
			}
			return fi.Name;
		}

		private void RenameRelatedFiles(System.IO.FileInfo fi, string NewName, int level)
		{
			this.RenameSubtitle(fi, NewName, level);
			this.RenameXBMCMeta(fi, NewName, level);
		}

		private void MoveRelatedFiles(System.IO.FileInfo fi, string NewPath, int level)
		{
			this.MoveSubtitle(fi, NewPath, level);
			this.MoveXBMCMeta(fi, NewPath, level);
		}

		private void RenameSubtitle(System.IO.FileInfo fi, string NewName, int level)
		{
			string[] allowedSubtitles = this.options.AllowedSubtitles;
			for (int i = 0; i < allowedSubtitles.Length; i++)
			{
				string text = allowedSubtitles[i];
				string text2 = fi.FullName.Replace(fi.Extension, text);
				if (System.IO.File.Exists(text2))
				{
					string dest = fi.DirectoryName + "\\" + NewName + text;
					this.MoveFile(text2, dest, level, true);
				}
			}
		}

		private void MoveSubtitle(System.IO.FileInfo fi, string NewPath, int level)
		{
			string[] allowedSubtitles = this.options.AllowedSubtitles;
			for (int i = 0; i < allowedSubtitles.Length; i++)
			{
				string text = allowedSubtitles[i];
				string text2 = fi.FullName.Replace(fi.Extension, text);
				if (System.IO.File.Exists(text2))
				{
					string dest = NewPath + "\\" + fi.Name + text;
					this.MoveFile(text2, dest, level, false);
				}
			}
		}

		private void RenameXBMCMeta(System.IO.FileInfo fi, string NewName, int level)
		{
			MovieXML movieXML = new MovieXML();
			string text = movieXML.GetXBMCThumbFile(fi.DirectoryName, fi.Name.Replace(fi.Extension, ""));
			if (System.IO.File.Exists(text))
			{
				string xBMCThumbFile = movieXML.GetXBMCThumbFile(fi.DirectoryName, NewName);
				this.MoveFile(text, xBMCThumbFile, level, true);
			}
			text = movieXML.GetXBMCBackdropFile(fi.DirectoryName, fi.Name.Replace(fi.Extension, ""));
			if (System.IO.File.Exists(text))
			{
				string xBMCBackdropFile = movieXML.GetXBMCBackdropFile(fi.DirectoryName, NewName);
				this.MoveFile(text, xBMCBackdropFile, level, true);
			}
		}

		private void MoveXBMCMeta(System.IO.FileInfo fi, string NewPath, int level)
		{
			MovieXML movieXML = new MovieXML();
			string text = movieXML.GetXBMCThumbFile(fi.DirectoryName, fi.Name.Replace(fi.Extension, ""));
			if (System.IO.File.Exists(text))
			{
				string xBMCThumbFile = movieXML.GetXBMCThumbFile(NewPath, fi.Name.Replace(fi.Extension, ""));
				this.MoveFile(text, xBMCThumbFile, level, false);
			}
			text = movieXML.GetXBMCBackdropFile(fi.DirectoryName, fi.Name.Replace(fi.Extension, ""));
			if (System.IO.File.Exists(text))
			{
				string xBMCBackdropFile = movieXML.GetXBMCBackdropFile(NewPath, fi.Name.Replace(fi.Extension, ""));
				this.MoveFile(text, xBMCBackdropFile, level, false);
			}
		}

		private System.IO.FileInfo MoveFile(string src, string dest, int level, bool IsRenameOperation)
		{
			System.IO.FileInfo fileInfo = new System.IO.FileInfo(src);
			bool flag = false;
			try
			{
				if (IsRenameOperation)
				{
					this.Message("Renaming " + fileInfo.Name + " to", MediaScoutMessage.MessageType.Task, level);
				}
				else
				{
					this.Message("Moving " + fileInfo.FullName + " to", MediaScoutMessage.MessageType.Task, level);
				}
				fileInfo = IOFunctions.MoveFile(fileInfo, dest, this.options.overwrite);
			}
			catch (System.Exception ex)
			{
				this.Message(ex.Message, MediaScoutMessage.MessageType.TaskError, level);
				flag = true;
			}
			if (fileInfo.FullName == dest)
			{
				if (IsRenameOperation)
				{
					this.Message(fileInfo.Name, MediaScoutMessage.MessageType.TaskResult, level);
				}
				else
				{
					this.Message(fileInfo.DirectoryName + "\\", MediaScoutMessage.MessageType.TaskResult, level);
				}
			}
			else if (!flag)
			{
				this.Message("Canceled", MediaScoutMessage.MessageType.TaskError, level);
			}
			return fileInfo;
		}

		private void SaveMeta(string directory, int level)
		{
			try
			{
				if (this.options.SaveXBMCMeta)
				{
					this.Message("Saving Metadata as " + this.m.GetNFOFile(directory), MediaScoutMessage.MessageType.Task, level);
					if (this.options.ForceUpdate || !System.IO.File.Exists(this.m.GetNFOFile(directory)))
					{
						this.m.SaveNFO(directory);
						this.Message("Done", MediaScoutMessage.MessageType.TaskResult, level);
					}
					else
					{
						this.Message("Already Exists, skipping", MediaScoutMessage.MessageType.TaskResult, level);
					}
				}
				if (this.options.SaveMyMoviesMeta)
				{
					this.Message("Saving Metadata as " + this.m.GetXMLFile(directory), MediaScoutMessage.MessageType.Task, level);
					if (this.options.ForceUpdate || !System.IO.File.Exists(this.m.GetXMLFile(directory)))
					{
						this.m.SaveXML(directory);
						this.Message("Done", MediaScoutMessage.MessageType.TaskResult, level);
					}
					else
					{
						this.Message("Already Exists, skipping", MediaScoutMessage.MessageType.TaskResult, level);
					}
				}
			}
			catch (System.Exception ex)
			{
				this.Message(ex.Message, MediaScoutMessage.MessageType.TaskError, level);
			}
		}

		private void SaveImage(string directory, string filename, Posters[] images, int index, MoviePosterType ptype, int level)
		{
			this.Message("Saving " + ptype.ToString().Replace("_", " ") + " as " + filename, MediaScoutMessage.MessageType.Task, level);
			if (System.IO.File.Exists(directory + "\\" + filename))
			{
				if (!this.options.ForceUpdate)
				{
					goto IL_F7;
				}
			}
			try
			{
				if (images == null)
				{
					images = this.tmdb.GetPosters(this.m.ID, ptype);
				}
				if (images != null)
				{
					images[index].SavePoster(directory + "\\" + filename);
					this.Message("Done", MediaScoutMessage.MessageType.TaskResult, level);
				}
				else
				{
					this.Message("No " + ptype.ToString().Replace("_", " ") + "s Found", MediaScoutMessage.MessageType.TaskError, level);
				}
				return;
			}
			catch (System.Exception ex)
			{
				this.Message(ex.Message, MediaScoutMessage.MessageType.TaskError, level);
				return;
			}
			IL_F7:
			this.Message(ptype.ToString().Replace("_", " ") + " already exists, skipping", MediaScoutMessage.MessageType.TaskResult, level);
		}
	}
}
