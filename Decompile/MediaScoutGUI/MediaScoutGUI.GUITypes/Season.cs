using MediaScout.Providers;
using MediaScoutGUI.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace MediaScoutGUI.GUITypes
{
	public class Season : INotifyPropertyChanged
	{
		public bool IsDeleted;

		private bool isUnsortedEpisodeCollection;

		private TVShow tvshow;

		public string MetadataFolderPath;

		private string name;

		private string folderpath;

		private string poster;

		private string backdrop;

		private ObservableCollection<Episode> episodes;

		public bool isPosterLoading;

		public bool isBackDropLoading;

		public event PropertyChangedEventHandler PropertyChanged;

		public bool IsUnsortedEpisodeCollection
		{
			get
			{
				return this.isUnsortedEpisodeCollection;
			}
			set
			{
				this.isUnsortedEpisodeCollection = value;
				this.NotifyPropertyChanged("IsUnsortedEpisodeCollection");
			}
		}

		public TVShow TVShow
		{
			get
			{
				return this.tvshow;
			}
			set
			{
				this.tvshow = value;
				this.NotifyPropertyChanged("TVShow");
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
				this.NotifyPropertyChanged("Name");
			}
		}

		public string Folderpath
		{
			get
			{
				return this.folderpath;
			}
			set
			{
				this.folderpath = value;
				this.NotifyPropertyChanged("Folderpath");
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

		public ObservableCollection<Episode> Episodes
		{
			get
			{
				return this.episodes;
			}
			set
			{
				this.episodes = value;
				this.NotifyPropertyChanged("Episodes");
			}
		}

		private void NotifyPropertyChanged(string info)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}

		public BitmapImage GetImage(TVShowPosterType type)
		{
			BitmapImage result = null;
			switch (type)
			{
			case TVShowPosterType.Season_Poster:
			{
				string text = this.Folderpath + "\\folder.jpg";
				if (File.Exists(text) && (result = this.GetBitmapImage(text)) != null)
				{
					this.Poster = text;
				}
				break;
			}
			case TVShowPosterType.Season_Backdrop:
			{
				bool flag = false;
				if (Settings.Default.SaveMyMoviesMeta)
				{
					string text = this.Folderpath + "\\backdrop.jpg";
					if (File.Exists(text) && (result = this.GetBitmapImage(text)) != null)
					{
						this.Backdrop = text;
						flag = true;
					}
				}
				if (Settings.Default.SaveXBMCMeta && !flag)
				{
					string text = this.Folderpath + "\\fanart.jpg";
					if (File.Exists(text) && (result = this.GetBitmapImage(text)) != null)
					{
						this.Backdrop = text;
					}
				}
				break;
			}
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

		public int GetNum()
		{
			int result = -1;
			if (this.Name == Settings.Default.SpecialsFolderName)
			{
				result = 0;
			}
			Match match = Regex.Match(this.Name, ".{0,1}([0-9]+)", RegexOptions.IgnoreCase);
			if (match.Success)
			{
				result = int.Parse(match.Value);
			}
			return result;
		}

		public Season(string filepath, string name, TVShow tvshow, bool isUnsortedEpisodeCollection)
		{
			this.IsUnsortedEpisodeCollection = isUnsortedEpisodeCollection;
			this.TVShow = tvshow;
			this.Folderpath = filepath;
			this.Name = name;
			this.MetadataFolderPath = filepath + "\\metadata";
			this.LoadEpisodes();
		}

		public void LoadEpisodes()
		{
			this.Episodes = new ObservableCollection<Episode>();
			List<string> list = new List<string>(Settings.Default.allowedFileTypes.Split(new char[]
			{
				';'
			}));
			FileInfo[] files = new DirectoryInfo(this.Folderpath).GetFiles();
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo fileInfo = files[i];
				if (list.Contains(fileInfo.Extension))
				{
					Episode item = new Episode(fileInfo.FullName, fileInfo.Name, this);
					this.Episodes.Add(item);
				}
			}
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}
