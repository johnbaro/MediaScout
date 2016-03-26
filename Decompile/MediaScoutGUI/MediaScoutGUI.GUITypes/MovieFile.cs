using MediaScout.Providers;
using MediaScoutGUI.Properties;
using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace MediaScoutGUI.GUITypes
{
	public class MovieFile : INotifyPropertyChanged
	{
		public bool IsDeleted;

		private bool hasMetadata;

		private Movie movie;

		public string StrippedFileName;

		public string XMLFile;

		public string NFOFile;

		private string name;

		private string filepath;

		private string poster;

		private string backdrop;

		public bool isPosterLoading;

		public bool isBackDropLoading;

		public event PropertyChangedEventHandler PropertyChanged;

		public bool HasMetadata
		{
			get
			{
				return this.hasMetadata;
			}
			set
			{
				this.hasMetadata = value;
				this.NotifyPropertyChanged("HasMetadata");
			}
		}

		public Movie Movie
		{
			get
			{
				return this.movie;
			}
			set
			{
				this.movie = value;
				this.NotifyPropertyChanged("Movie");
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				this.name = value;
				this.StrippedFileName = this.name.Substring(0, this.name.LastIndexOf("."));
				this.XMLFile = this.Movie.Folderpath + "\\" + this.StrippedFileName + ".xml";
				this.NFOFile = this.Movie.Folderpath + "\\" + this.StrippedFileName + ".nfo";
				this.NotifyPropertyChanged("Name");
			}
		}

		public string Filepath
		{
			get
			{
				return this.filepath;
			}
			set
			{
				this.filepath = value;
				this.NotifyPropertyChanged("Filepath");
			}
		}

		public string Poster
		{
			get
			{
				return this.poster;
			}
			set
			{
				this.poster = value;
				this.NotifyPropertyChanged("Poster");
			}
		}

		public string Backdrop
		{
			get
			{
				return this.backdrop;
			}
			set
			{
				this.backdrop = value;
				this.NotifyPropertyChanged("Backdrop");
			}
		}

		private void NotifyPropertyChanged(string info)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}

		public BitmapImage GetImage(MoviePosterType type)
		{
			BitmapImage result = null;
			switch (type)
			{
			case MoviePosterType.File_Poster:
				if (Settings.Default.SaveXBMCMeta)
				{
					string path = this.Movie.Folderpath + "\\" + this.StrippedFileName + ".tbn";
					if (File.Exists(path) && (result = this.GetBitmapImage(path)) != null)
					{
						this.Poster = path;
					}
				}
				break;
			case MoviePosterType.File_Backdrop:
				if (Settings.Default.SaveXBMCMeta)
				{
					string path = this.Movie.Folderpath + "\\" + this.StrippedFileName + "_fanart.jpg";
					if (File.Exists(path) && (result = this.GetBitmapImage(path)) != null)
					{
						this.Backdrop = path;
					}
				}
				break;
			}
			return result;
		}

		public BitmapImage GetBitmapImage(string Filepath)
		{
			BitmapImage bitmapImage = null;
			try
			{
				bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
				bitmapImage.UriSource = new Uri(Filepath);
				bitmapImage.EndInit();
			}
			catch
			{
				bitmapImage = null;
			}
			return bitmapImage;
		}

		public string GetSearchTerm()
		{
			return this.Movie.GetSearchTerm(this.StrippedFileName);
		}

		public string GetYearFromFilename()
		{
			string result = null;
			MatchCollection matchCollection = Regex.Matches(this.Name, "\\d{4}");
			if (matchCollection.Count > 0)
			{
				result = matchCollection[matchCollection.Count - 1].Value;
			}
			return result;
		}

		public string GetYearFromFolder()
		{
			return this.Movie.GetYearFromFolder();
		}

		public string GetYear()
		{
			this.Movie.Load();
			if (this.Movie.Year == null)
			{
				this.GetYearFromFolder();
			}
			if (this.Movie.Year == null)
			{
				this.GetYearFromFilename();
			}
			return this.Movie.Year;
		}

		public MovieFile(string filepath, string name, Movie movie)
		{
			this.Filepath = filepath;
			this.Movie = movie;
			this.Name = name;
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}
