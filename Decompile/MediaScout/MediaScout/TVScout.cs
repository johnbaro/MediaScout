using MediaScout.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MediaScout
{
	public class TVScout
	{
		private MediaScoutMessage.Message Message;

		private TVScoutOptions options;

		private TheTVDBProvider tvdb;

		private string ImagesByNameLocation;

		public TVShowXML series;

		private Posters[] posters;

		private Posters[] backdrops;

		private Posters[] banners;

		private int level = 1;

		public TVScout(TVScoutOptions options, MediaScoutMessage.Message Message, string ImagesByNameLocation)
		{
			this.options = options;
			this.Message = Message;
			this.ImagesByNameLocation = ImagesByNameLocation;
			this.tvdb = new TheTVDBProvider(Message);
		}

		public string ProcessDirectory(string directory)
		{
			int num = this.level;
			string text = null;
			text = IOFunctions.GetDirectoryName(directory);
			if (this.options.RenameFiles)
			{
				string validName = IOFunctions.GetValidName(this.series.SeriesName, this.options.FilenameReplaceChar);
				if (validName != text)
				{
					try
					{
						string text2 = System.IO.Path.GetDirectoryName(directory) + "\\" + validName;
						this.Message(string.Concat(new string[]
						{
							"Renaming ",
							text,
							"/ to ",
							validName,
							"/"
						}), MediaScoutMessage.MessageType.Task, num);
						if (IOFunctions.MergeDirectories(directory, text2, this.options.overwrite))
						{
							text = "d:" + validName;
						}
						else
						{
							text = validName;
						}
						directory = text2;
						this.Message("Done", MediaScoutMessage.MessageType.TaskResult, num);
					}
					catch (System.Exception ex)
					{
						this.Message(ex.Message, MediaScoutMessage.MessageType.TaskError, num);
						return null;
					}
				}
			}
			this.SaveMeta(directory, num);
			if (this.options.GetSeriesPosters)
			{
				this.SaveImage(directory, "folder.jpg", this.posters, 0, -1, TVShowPosterType.Poster, num);
			}
			if (this.options.DownloadAllPosters)
			{
				this.Message("Downloading All Posters", MediaScoutMessage.MessageType.Task, num);
				if (this.posters == null)
				{
					this.posters = this.tvdb.GetPosters(this.series.ID, TVShowPosterType.Poster, -1);
				}
				if (this.posters != null)
				{
					int num2 = 0;
					Posters[] array = this.posters;
					for (int i = 0; i < array.Length; i++)
					{
						Posters arg_184_0 = array[i];
						this.SaveImage(directory, "folder" + num2 + ".jpg", this.posters, num2, -1, TVShowPosterType.Poster, num + 1);
						num2++;
					}
				}
				else
				{
					this.Message("No Posters Found", MediaScoutMessage.MessageType.TaskResult, num);
				}
			}
			if (this.options.GetSeriesPosters)
			{
				if (this.options.SaveXBMCMeta)
				{
					this.SaveImage(directory, "fanart.jpg", this.backdrops, 0, -1, TVShowPosterType.Backdrop, num);
				}
				if (this.options.SaveMyMoviesMeta)
				{
					this.SaveImage(directory, "backdrop.jpg", this.backdrops, 0, -1, TVShowPosterType.Backdrop, num);
				}
			}
			if (this.options.DownloadAllBackdrops)
			{
				this.Message("Download All Backdrops", MediaScoutMessage.MessageType.Task, num);
				if (this.backdrops == null)
				{
					this.backdrops = this.tvdb.GetPosters(this.series.id, TVShowPosterType.Backdrop, -1);
				}
				if (this.backdrops != null)
				{
					int num3 = 0;
					Posters[] array2 = this.backdrops;
					for (int j = 0; j < array2.Length; j++)
					{
						Posters arg_295_0 = array2[j];
						if (this.options.SaveXBMCMeta)
						{
							this.SaveImage(directory, "fanart" + num3 + ".jpg", this.backdrops, num3, -1, TVShowPosterType.Backdrop, num + 1);
						}
						if (this.options.SaveMyMoviesMeta)
						{
							this.SaveImage(directory, "backdrop" + num3 + ".jpg", this.backdrops, num3, -1, TVShowPosterType.Backdrop, num + 1);
						}
						num3++;
					}
				}
				else
				{
					this.Message("No Backdrops Found", MediaScoutMessage.MessageType.TaskResult, num);
				}
			}
			if (this.options.GetSeriesPosters)
			{
				this.SaveImage(directory, "banner.jpg", this.banners, 0, -1, TVShowPosterType.Banner, num);
			}
			if (this.options.DownloadAllBanners)
			{
				this.Message("Downloading All Banners ", MediaScoutMessage.MessageType.Task, num);
				if (this.banners == null)
				{
					this.banners = this.tvdb.GetPosters(this.series.id, TVShowPosterType.Banner, -1);
				}
				if (this.banners != null)
				{
					int num4 = 0;
					Posters[] array3 = this.banners;
					for (int k = 0; k < array3.Length; k++)
					{
						Posters arg_3B7_0 = array3[k];
						this.SaveImage(directory, "banner" + num4 + ".jpg", this.banners, num4, -1, TVShowPosterType.Banner, num + 1);
						num4++;
					}
				}
				else
				{
					this.Message("No Banners Found", MediaScoutMessage.MessageType.TaskResult, num);
				}
			}
			if (this.options.SaveActors)
			{
				if (this.options.SaveXBMCMeta)
				{
					try
					{
						this.Message("Saving Actors Thumb in " + new Person().GetXBMCDirectory() + "\\", MediaScoutMessage.MessageType.Task, num);
						if (this.series.Persons.Count != 0)
						{
							string text3 = directory + "\\" + new Person().GetXBMCDirectory();
							if (!System.IO.Directory.Exists(text3))
							{
								IOFunctions.CreateHiddenFolder(text3);
							}
							foreach (Person current in this.series.Persons)
							{
								if (current.Type == "Actor" && !string.IsNullOrEmpty(current.Thumb))
								{
									string xBMCFilename = current.GetXBMCFilename();
									string text4 = text3 + "\\" + xBMCFilename;
									if (!System.IO.File.Exists(text4) || this.options.ForceUpdate)
									{
										current.SaveThumb(text4);
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
					catch (System.Exception ex2)
					{
						this.Message(ex2.Message, MediaScoutMessage.MessageType.TaskError, num);
					}
				}
				if (this.options.SaveMyMoviesMeta)
				{
					try
					{
						this.Message("Saving Actors Thumb in ImagesByName\\", MediaScoutMessage.MessageType.Task, num);
						if (System.IO.Directory.Exists(this.ImagesByNameLocation))
						{
							if (this.series.Persons.Count != 0)
							{
								foreach (Person current2 in this.series.Persons)
								{
									if (current2.Type == "Actor" && !string.IsNullOrEmpty(current2.Thumb))
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
					catch (System.Exception ex3)
					{
						this.Message(ex3.Message, MediaScoutMessage.MessageType.TaskError, num);
					}
				}
			}
			if (this.options.MoveFiles)
			{
				this.Message("Sorting loose episodes", MediaScoutMessage.MessageType.Task, num);
				System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(directory);
				System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>(this.options.AllowedFileTypes);
				System.IO.FileInfo[] files = directoryInfo.GetFiles();
				for (int l = 0; l < files.Length; l++)
				{
					System.IO.FileInfo fileInfo = files[l];
					if (list.Contains(fileInfo.Extension.ToLower()))
					{
						this.MoveFileToAppropriateFolder(directory, fileInfo, -1, -1, num + 1);
					}
				}
			}
			System.IO.DirectoryInfo directoryInfo2 = new System.IO.DirectoryInfo(directory);
			System.IO.DirectoryInfo[] directories = directoryInfo2.GetDirectories();
			for (int m = 0; m < directories.Length; m++)
			{
				System.IO.DirectoryInfo diSeason = directories[m];
				this.ProcessSeasonDirectory(directory, diSeason, num + 1);
			}
			return text;
		}

		public string ProcessSeasonDirectory(string ShowDirectory, System.IO.DirectoryInfo diSeason, int level)
		{
			string result = diSeason.Name;
			if (level == -1)
			{
				level = this.level;
			}
			Regex regex = new Regex(this.options.SeasonFolderName + ".{0,1}([0-9]+)", RegexOptions.IgnoreCase);
			MatchCollection matchCollection = regex.Matches(diSeason.Name);
			if (matchCollection.Count > 0 || diSeason.Name == this.options.SpecialsFolderName)
			{
				this.Message("Processing " + diSeason.Name, MediaScoutMessage.MessageType.Task, level);
				int num = 0;
				if (diSeason.Name != this.options.SpecialsFolderName)
				{
					num = int.Parse(matchCollection[0].Groups[1].Captures[0].Value);
				}
				if (this.series.Seasons.ContainsKey(num))
				{
					if (diSeason.Name != this.options.SpecialsFolderName)
					{
						string text = this.options.SeasonFolderName + " " + num.ToString().PadLeft(this.options.SeasonNumZeroPadding, '0');
						if (this.options.RenameFiles)
						{
							string dest = ShowDirectory + "\\" + text;
							if (diSeason.Name != text)
							{
								if (IOFunctions.MergeDirectories(diSeason.FullName, dest, this.options.overwrite))
								{
									result = "d:" + text;
								}
								else
								{
									result = text;
								}
							}
						}
						this.ProcessSeason(ShowDirectory, text, num, level + 1);
					}
					else
					{
						this.ProcessSeason(ShowDirectory, diSeason.Name, num, level + 1);
					}
				}
				else
				{
					this.Message("Invalid Season folder:" + diSeason.Name, MediaScoutMessage.MessageType.TaskError, level);
				}
			}
			return result;
		}

		public void ProcessSeason(string directory, string seasonFldr, int seasonNum, int level)
		{
			this.Message("Valid", MediaScoutMessage.MessageType.TaskResult, level);
			if (this.options.GetSeasonPosters)
			{
				this.SaveImage(directory + "\\" + seasonFldr, "folder.jpg", this.posters, 0, seasonNum, TVShowPosterType.Season_Poster, level);
			}
			if (this.options.DownloadAllSeasonPosters)
			{
				this.Message("Downloading All " + this.options.SeasonFolderName + " Poster", MediaScoutMessage.MessageType.Task, level);
				if (this.posters == null)
				{
					this.posters = this.tvdb.GetPosters(this.series.id, TVShowPosterType.Season_Poster, seasonNum);
				}
				if (this.posters != null)
				{
					int num = 0;
					Posters[] array = this.posters;
					for (int i = 0; i < array.Length; i++)
					{
						Posters arg_BC_0 = array[i];
						this.SaveImage(directory + "\\" + seasonFldr, "folder" + num + ".jpg", this.posters, num, seasonNum, TVShowPosterType.Season_Poster, level + 1);
						num++;
					}
				}
				else
				{
					this.Message("No Posters Found", MediaScoutMessage.MessageType.TaskResult, level);
				}
			}
			if (this.options.GetSeasonPosters)
			{
				if (this.options.SaveXBMCMeta)
				{
					this.SaveImage(directory + "\\" + seasonFldr, "fanart.jpg", this.backdrops, 0, seasonNum, TVShowPosterType.Season_Backdrop, level);
				}
				if (this.options.SaveMyMoviesMeta)
				{
					this.SaveImage(directory + "\\" + seasonFldr, "backdrop.jpg", this.backdrops, 0, seasonNum, TVShowPosterType.Season_Backdrop, level);
				}
			}
			if (this.options.DownloadAllSeasonBackdrops)
			{
				this.Message("Downloading All " + seasonFldr + " Backdrops", MediaScoutMessage.MessageType.Task, level + 1);
				if (this.backdrops == null)
				{
					this.backdrops = this.tvdb.GetPosters(this.series.id, TVShowPosterType.Season_Backdrop, seasonNum);
				}
				if (this.backdrops != null)
				{
					int num2 = 0;
					Posters[] array2 = this.backdrops;
					for (int j = 0; j < array2.Length; j++)
					{
						Posters arg_1FB_0 = array2[j];
						if (this.options.SaveXBMCMeta)
						{
							this.SaveImage(directory + "\\" + seasonFldr, "fanart" + num2 + ".jpg", this.backdrops, num2, seasonNum, TVShowPosterType.Season_Backdrop, level + 1);
						}
						if (this.options.SaveMyMoviesMeta)
						{
							this.SaveImage(directory + "\\" + seasonFldr, "backdrop" + num2 + ".jpg", this.backdrops, num2, seasonNum, TVShowPosterType.Season_Backdrop, level + 1);
						}
						num2++;
					}
				}
				else
				{
					this.Message("No Backdrops Found", MediaScoutMessage.MessageType.TaskResult, level);
				}
			}
			System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>(this.options.AllowedFileTypes);
			System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(string.Format("{0}\\{1}", directory, seasonFldr));
			System.IO.FileInfo[] files = directoryInfo.GetFiles();
			for (int k = 0; k < files.Length; k++)
			{
				System.IO.FileInfo fileInfo = files[k];
				if (list.Contains(fileInfo.Extension.ToLower()))
				{
					this.ProcessEpisode(directory, fileInfo, seasonNum, true, level + 1);
				}
			}
		}

		public string ProcessEpisode(string ShowDirectory, System.IO.FileInfo fi, int seasonNum, bool IsInsideShowDir, int level)
		{
			if (level == -1)
			{
				level = this.level;
			}
			if (this.options.MoveFiles && !IsInsideShowDir)
			{
				string text = ShowDirectory + "\\" + IOFunctions.GetValidName(this.series.SeriesName, this.options.FilenameReplaceChar);
				if (!System.IO.Directory.Exists(text))
				{
					System.IO.Directory.CreateDirectory(text);
				}
				ShowDirectory = text;
				System.IO.FileInfo fi2 = new System.IO.FileInfo(fi.FullName);
				string text2 = ShowDirectory + "\\" + fi.Name;
				fi = this.MoveFile(fi.FullName, text2, level, false);
				if (fi.FullName == text2)
				{
					this.MoveRelatedFiles(fi2, ShowDirectory, level);
				}
			}
			this.Message("Processing File : " + fi.Name, MediaScoutMessage.MessageType.Task, level);
			try
			{
				EpisodeInfo seasonAndEpisodeIDFromFile = GetID.GetSeasonAndEpisodeIDFromFile(fi.Name);
				string str = (seasonAndEpisodeIDFromFile.SeasonID != -1) ? seasonAndEpisodeIDFromFile.SeasonID.ToString() : "Unable to extract";
				string str2 = (seasonAndEpisodeIDFromFile.EpisodeID != -1) ? seasonAndEpisodeIDFromFile.EpisodeID.ToString() : "Unable to extract";
				this.Message("Season : " + str + ", Episode : " + str2, (seasonAndEpisodeIDFromFile.SeasonID != -1 && seasonAndEpisodeIDFromFile.EpisodeID != -1) ? MediaScoutMessage.MessageType.TaskResult : MediaScoutMessage.MessageType.TaskError, level);
				if (seasonAndEpisodeIDFromFile.SeasonID != -1 && seasonAndEpisodeIDFromFile.EpisodeID != -1)
				{
					if (seasonAndEpisodeIDFromFile.SeasonID != seasonNum && this.options.MoveFiles)
					{
						fi = this.MoveFileToAppropriateFolder(ShowDirectory, fi, seasonAndEpisodeIDFromFile.SeasonID, seasonAndEpisodeIDFromFile.EpisodeID, level + 1);
					}
					if (this.series.Seasons[seasonAndEpisodeIDFromFile.SeasonID].Episodes.ContainsKey(seasonAndEpisodeIDFromFile.EpisodeID))
					{
						string result = this.ProcessFile(ShowDirectory, fi, seasonAndEpisodeIDFromFile.SeasonID, seasonAndEpisodeIDFromFile.EpisodeID, level + 1);
						return result;
					}
					this.Message(string.Format("Episode {0} Not Found In Season {1}", seasonAndEpisodeIDFromFile.EpisodeID, seasonAndEpisodeIDFromFile.SeasonID), MediaScoutMessage.MessageType.Error, level + 1);
					if (this.series.LoadedFromCache)
					{
						this.Message("Updating Cache", MediaScoutMessage.MessageType.Task, level + 1);
						this.series = this.tvdb.GetTVShow(this.series.SeriesID, System.DateTime.Now, level + 2);
						if (this.series != null)
						{
							if (this.series.Seasons[seasonAndEpisodeIDFromFile.SeasonID].Episodes.ContainsKey(seasonAndEpisodeIDFromFile.EpisodeID))
							{
								string result = this.ProcessFile(ShowDirectory, fi, seasonAndEpisodeIDFromFile.SeasonID, seasonAndEpisodeIDFromFile.EpisodeID, level + 1);
								return result;
							}
							this.Message("Invalid Episode, Skipping", MediaScoutMessage.MessageType.Error, level + 1);
						}
					}
				}
			}
			catch (System.Exception ex)
			{
				this.Message(ex.Message, MediaScoutMessage.MessageType.Error, level);
			}
			return null;
		}

		public string ProcessFile(string ShowDirectory, System.IO.FileInfo file, int seasonID, int episodeID, int level)
		{
			string result = null;
			EpisodeXML episodeXML = this.series.Seasons[seasonID].Episodes[episodeID];
			if (this.options.SaveMyMoviesMeta)
			{
				string metadataFolder = episodeXML.GetMetadataFolder(file.DirectoryName);
				if (!System.IO.Directory.Exists(metadataFolder))
				{
					IOFunctions.CreateHiddenFolder(metadataFolder);
				}
			}
			if (this.options.RenameFiles)
			{
				string text = string.Format(this.options.RenameFormat, new object[]
				{
					this.series.SeriesName,
					seasonID.ToString().PadLeft(this.options.SeasonNumZeroPadding, '0'),
					episodeXML.EpisodeName.Trim(),
					episodeID.ToString().PadLeft(this.options.EpisodeNumZeroPadding, '0')
				});
				text = IOFunctions.GetValidName(text, this.options.FilenameReplaceChar);
				string dest = file.DirectoryName + "\\" + text + file.Extension;
				if (file.Name != text + file.Extension)
				{
					System.IO.FileInfo fi = new System.IO.FileInfo(file.FullName);
					file = this.MoveFile(file.FullName, dest, level, true);
					if (file.Name == text + file.Extension)
					{
						this.RenameRelatedFiles(fi, text, level);
					}
				}
			}
			result = file.Name;
			if (this.options.SaveXBMCMeta)
			{
				this.Message("Saving Metadata as " + episodeXML.GetNFOFileName(file.Name.Replace(file.Extension, "")), MediaScoutMessage.MessageType.Task, level);
				if (this.options.ForceUpdate || !System.IO.File.Exists(episodeXML.GetNFOFile(file.DirectoryName, file.Name.Replace(file.Extension, ""))))
				{
					episodeXML.SaveNFO(file.DirectoryName, file.Name.Replace(file.Extension, ""));
					this.Message("Done", MediaScoutMessage.MessageType.TaskResult, level);
				}
				else
				{
					this.Message("Already Exists, skipping", MediaScoutMessage.MessageType.TaskResult, level);
				}
			}
			if (this.options.SaveMyMoviesMeta)
			{
				this.Message("Saving Metadata as " + episodeXML.GetXMLFilename(file.Name.Replace(file.Extension, "")), MediaScoutMessage.MessageType.Task, level);
				if (this.options.ForceUpdate || !System.IO.File.Exists(episodeXML.GetXMLFile(file.DirectoryName, file.Name.Replace(file.Extension, ""))))
				{
					episodeXML.SaveXML(file.DirectoryName, file.Name.Replace(file.Extension, ""));
					this.Message("Done", MediaScoutMessage.MessageType.TaskResult, level);
				}
				else
				{
					this.Message("Already Exists, skipping", MediaScoutMessage.MessageType.TaskResult, level);
				}
			}
			if (this.options.GetEpisodePosters)
			{
				if (!string.IsNullOrEmpty(episodeXML.PosterUrl))
				{
					Posters posters = new Posters();
					posters.Poster = episodeXML.PosterUrl;
					try
					{
						if (this.options.SaveXBMCMeta)
						{
							this.Message("Saving Episode Poster as " + episodeXML.GetXBMCThumbFilename(file.Name.Replace(file.Extension, "")), MediaScoutMessage.MessageType.Task, level);
							string xBMCThumbFile = episodeXML.GetXBMCThumbFile(file.DirectoryName, file.Name.Replace(file.Extension, ""));
							if (!System.IO.File.Exists(xBMCThumbFile))
							{
								posters.SavePoster(xBMCThumbFile);
								this.Message("Done", MediaScoutMessage.MessageType.TaskResult, level);
							}
							else
							{
								this.Message("Already Exists, skipping", MediaScoutMessage.MessageType.TaskResult, level);
							}
						}
						if (this.options.SaveMyMoviesMeta)
						{
							this.Message("Saving Episode Poster as " + episodeXML.GetMyMoviesThumbFilename(), MediaScoutMessage.MessageType.Task, level);
							string myMoviesThumbFile = episodeXML.GetMyMoviesThumbFile(file.DirectoryName);
							if (!System.IO.File.Exists(myMoviesThumbFile))
							{
								posters.SavePoster(myMoviesThumbFile);
								this.Message("Done", MediaScoutMessage.MessageType.TaskResult, level);
							}
							else
							{
								this.Message("Already Exists, skipping", MediaScoutMessage.MessageType.TaskResult, level);
							}
						}
						return result;
					}
					catch (System.Exception ex)
					{
						this.Message(ex.Message, MediaScoutMessage.MessageType.TaskError, level);
						return result;
					}
				}
				if (this.options.SaveXBMCMeta)
				{
					this.Message("Saving thumbnail from video as " + episodeXML.GetXBMCThumbFilename(file.Name.Replace(file.Extension, "")), MediaScoutMessage.MessageType.Task, level);
					string xBMCThumbFile2 = episodeXML.GetXBMCThumbFile(file.DirectoryName, file.Name.Replace(file.Extension, ""));
					if (!System.IO.File.Exists(xBMCThumbFile2))
					{
						VideoInfo.SaveThumb(file.FullName, xBMCThumbFile2, 0.25);
						this.Message("Done", MediaScoutMessage.MessageType.TaskResult, level);
					}
					else
					{
						this.Message("Already Exists, skipping", MediaScoutMessage.MessageType.TaskResult, level);
					}
				}
				if (this.options.SaveMyMoviesMeta)
				{
					this.Message("Saving thumbnail from video as " + episodeXML.GetMyMoviesThumbFilename(), MediaScoutMessage.MessageType.Task, level);
					string myMoviesThumbFile2 = episodeXML.GetMyMoviesThumbFile(file.DirectoryName);
					if (!System.IO.File.Exists(myMoviesThumbFile2))
					{
						VideoInfo.SaveThumb(file.FullName, myMoviesThumbFile2, 0.25);
						episodeXML.PosterName = episodeXML.GetMyMoviesThumbFilename();
						this.Message("Done", MediaScoutMessage.MessageType.TaskResult, level);
					}
					else
					{
						this.Message("Already Exists, skipping", MediaScoutMessage.MessageType.TaskResult, level);
					}
				}
			}
			return result;
		}

		public System.IO.FileInfo MoveFileToAppropriateFolder(string ShowDirectory, System.IO.FileInfo fi, int seasonID, int episodeID, int level)
		{
			EpisodeInfo episodeInfo = null;
			if (seasonID == -1 || episodeID == -1)
			{
				episodeInfo = GetID.GetSeasonAndEpisodeIDFromFile(fi.Name);
			}
			if (seasonID == -1)
			{
				seasonID = episodeInfo.SeasonID;
			}
			if (episodeID == -1)
			{
				episodeID = episodeInfo.EpisodeID;
			}
			this.Message("File " + fi.Name, MediaScoutMessage.MessageType.Task, level);
			string str = (seasonID != -1) ? seasonID.ToString() : "Unable to extract";
			string str2 = (episodeID != -1) ? episodeID.ToString() : "Unable to extract";
			this.Message("Season : " + str + ", Episode : " + str2, (seasonID != -1 && episodeID != -1) ? MediaScoutMessage.MessageType.TaskResult : MediaScoutMessage.MessageType.TaskError, level);
			if (seasonID != -1 && episodeID != -1)
			{
				if (this.series.Seasons.ContainsKey(seasonID))
				{
					if (this.series.Seasons[seasonID].Episodes.ContainsKey(episodeID))
					{
						string str3 = (seasonID != 0) ? (this.options.SeasonFolderName + " " + seasonID.ToString().PadLeft(this.options.SeasonNumZeroPadding, '0')) : this.options.SpecialsFolderName;
						string text = ShowDirectory + "\\" + str3;
						if (!System.IO.Directory.Exists(text))
						{
							System.IO.Directory.CreateDirectory(text);
						}
						string text2 = text + "\\" + fi.Name;
						if (fi.FullName != text2)
						{
							System.IO.FileInfo fi2 = new System.IO.FileInfo(fi.FullName);
							fi = this.MoveFile(fi.FullName, text2, level, false);
							if (fi.FullName == text2)
							{
								this.MoveRelatedFiles(fi2, text, level + 1);
							}
						}
					}
					else
					{
						this.Message(string.Format("Episode {0} Not Found In Season {1}, Skipping", episodeID, seasonID), MediaScoutMessage.MessageType.Error, level);
					}
				}
				else
				{
					this.Message(string.Format("Season {0} Not Found In {1}, Skipping", seasonID, this.series.Title), MediaScoutMessage.MessageType.Error, level);
				}
			}
			return fi;
		}

		private void RenameRelatedFiles(System.IO.FileInfo fi, string NewName, int level)
		{
			this.RenameSubtitle(fi, NewName, level);
			this.RenameXBMCMeta(fi, NewName, level);
			this.RenameMyMoviesMeta(fi, NewName, level);
		}

		private void MoveRelatedFiles(System.IO.FileInfo fi, string NewPath, int level)
		{
			this.MoveSubtitle(fi, NewPath, level);
			this.MoveXBMCMeta(fi, NewPath, level);
			this.MoveMyMoviesMeta(fi, NewPath, level);
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
			EpisodeXML episodeXML = new EpisodeXML();
			string text = episodeXML.GetNFOFile(fi.DirectoryName, fi.Name.Replace(fi.Extension, ""));
			if (System.IO.File.Exists(text))
			{
				string nFOFile = episodeXML.GetNFOFile(fi.DirectoryName, NewName);
				this.MoveFile(text, nFOFile, level, true);
			}
			text = episodeXML.GetXBMCThumbFile(fi.DirectoryName, fi.Name.Replace(fi.Extension, ""));
			if (System.IO.File.Exists(text))
			{
				string xBMCThumbFile = episodeXML.GetXBMCThumbFile(fi.DirectoryName, NewName);
				this.MoveFile(text, xBMCThumbFile, level, true);
			}
		}

		private void MoveXBMCMeta(System.IO.FileInfo fi, string NewPath, int level)
		{
			EpisodeXML episodeXML = new EpisodeXML();
			string text = episodeXML.GetNFOFile(fi.DirectoryName, fi.Name.Replace(fi.Extension, ""));
			if (System.IO.File.Exists(text))
			{
				string nFOFile = episodeXML.GetNFOFile(NewPath, fi.Name.Replace(fi.Extension, ""));
				this.MoveFile(text, nFOFile, level, false);
			}
			text = episodeXML.GetXBMCThumbFile(fi.DirectoryName, fi.Name.Replace(fi.Extension, ""));
			if (System.IO.File.Exists(text))
			{
				string xBMCThumbFile = episodeXML.GetXBMCThumbFile(NewPath, fi.Name.Replace(fi.Extension, ""));
				this.MoveFile(text, xBMCThumbFile, level, false);
			}
		}

		private void RenameMyMoviesMeta(System.IO.FileInfo fi, string NewName, int level)
		{
			EpisodeXML episodeXML = new EpisodeXML();
			string text = episodeXML.GetXMLFile(fi.DirectoryName, fi.Name.Replace(fi.Extension, ""));
			if (System.IO.File.Exists(text))
			{
				string xMLFile = episodeXML.GetXMLFile(fi.DirectoryName, NewName);
				this.MoveFile(text, xMLFile, level, true);
			}
			text = episodeXML.GetMyMoviesThumbFile(fi.DirectoryName);
			if (System.IO.File.Exists(text))
			{
				string myMoviesThumbFile = episodeXML.GetMyMoviesThumbFile(fi.DirectoryName);
				this.MoveFile(text, myMoviesThumbFile, level, true);
			}
		}

		private void MoveMyMoviesMeta(System.IO.FileInfo fi, string NewPath, int level)
		{
			EpisodeXML episodeXML = new EpisodeXML();
			string text = episodeXML.GetXMLFile(fi.DirectoryName, fi.Name.Replace(fi.Extension, ""));
			if (System.IO.File.Exists(text))
			{
				string xMLFile = episodeXML.GetXMLFile(NewPath, fi.Name.Replace(fi.Extension, ""));
				this.MoveFile(text, xMLFile, level, false);
			}
			text = episodeXML.GetMyMoviesThumbFile(fi.DirectoryName);
			if (System.IO.File.Exists(text))
			{
				string myMoviesThumbFile = episodeXML.GetMyMoviesThumbFile(NewPath);
				this.MoveFile(text, myMoviesThumbFile, level, false);
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
					this.Message("Saving Metadata as " + this.series.GetNFOFile(directory), MediaScoutMessage.MessageType.Task, level);
					if (this.options.ForceUpdate || !System.IO.File.Exists(this.series.GetNFOFile(directory)))
					{
						this.series.SaveNFO(directory);
						this.Message("Done", MediaScoutMessage.MessageType.TaskResult, level);
					}
					else
					{
						this.Message("Already Exists, skipping", MediaScoutMessage.MessageType.TaskResult, level);
					}
				}
				if (this.options.SaveMyMoviesMeta)
				{
					this.Message("Saving Metadata as " + this.series.GetXMLFile(directory), MediaScoutMessage.MessageType.Task, level);
					if (this.options.ForceUpdate || !System.IO.File.Exists(this.series.GetXMLFile(directory)))
					{
						this.series.SaveXML(directory);
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

		private void SaveImage(string directory, string filename, Posters[] images, int index, int SeasonNum, TVShowPosterType ptype, int level)
		{
			this.Message("Saving " + ptype.ToString().Replace("_", (SeasonNum != -1) ? (" " + SeasonNum + " ") : " ") + " as " + filename, MediaScoutMessage.MessageType.Task, level);
			if (System.IO.File.Exists(directory + "\\" + filename))
			{
				if (!this.options.ForceUpdate)
				{
					goto IL_133;
				}
			}
			try
			{
				if (images == null)
				{
					images = this.tvdb.GetPosters(this.series.ID, ptype, SeasonNum);
				}
				if (images != null)
				{
					images[index].SavePoster(directory + "\\" + filename);
					this.Message("Done", MediaScoutMessage.MessageType.TaskResult, level);
				}
				else
				{
					this.Message("No " + ptype.ToString().Replace("_", (SeasonNum != -1) ? (" " + SeasonNum + " ") : " ") + "s Found", MediaScoutMessage.MessageType.TaskError, level);
				}
				return;
			}
			catch (System.Exception ex)
			{
				this.Message(ex.Message, MediaScoutMessage.MessageType.TaskError, level);
				return;
			}
			IL_133:
			this.Message("Already Exists, skipping", MediaScoutMessage.MessageType.TaskResult, level);
		}
	}
}
