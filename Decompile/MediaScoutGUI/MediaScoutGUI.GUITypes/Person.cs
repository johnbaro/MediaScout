using MediaScoutGUI.Properties;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace MediaScoutGUI.GUITypes
{
	public class Person : INotifyPropertyChanged
	{
		private bool metadataChanged;

		private TVShow tvshowbase;

		private Movie moviebase;

		private string name;

		private string type;

		private string role;

		private BitmapImage thumb;

		private string XBMCfolderpath;

		private string MyMoviesfolderpath;

		private bool isMovieActor;

		public event PropertyChangedEventHandler PropertyChanged;

		public bool MetadataChanged
		{
			get
			{
				return this.metadataChanged;
			}
			set
			{
				this.metadataChanged = value;
				if (!this.IsMovieActor)
				{
					this.TVShowBase.MetadataChanged = this.metadataChanged;
				}
				else if (this.MovieBase != null)
				{
					this.MovieBase.MetadataChanged = this.metadataChanged;
				}
				this.NotifyPropertyChanged("MetadataChanged");
			}
		}

		public TVShow TVShowBase
		{
			get
			{
				return this.tvshowbase;
			}
			set
			{
				this.tvshowbase = value;
				this.NotifyPropertyChanged("TVShowBase");
			}
		}

		public Movie MovieBase
		{
			get
			{
				return this.moviebase;
			}
			set
			{
				this.moviebase = value;
				this.NotifyPropertyChanged("MovieBase");
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
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("Name");
			}
		}

		public string Type
		{
			get
			{
				return this.type;
			}
			set
			{
				this.type = value;
				this.NotifyPropertyChanged("Type");
			}
		}

		public string Role
		{
			get
			{
				return this.role;
			}
			set
			{
				this.role = value;
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("Role");
			}
		}

		public BitmapImage Thumb
		{
			get
			{
				return this.thumb;
			}
			set
			{
				this.thumb = value;
				this.NotifyPropertyChanged("Thumb");
			}
		}

		public string XBMCFolderPath
		{
			get
			{
				return this.XBMCfolderpath;
			}
			set
			{
				this.XBMCfolderpath = value;
				this.NotifyPropertyChanged("XBMCFolderPath");
			}
		}

		public string MyMoviesFolderPath
		{
			get
			{
				return this.MyMoviesfolderpath;
			}
			set
			{
				this.MyMoviesfolderpath = value;
				this.NotifyPropertyChanged("MyMoviesFolderPath");
			}
		}

		public bool IsMovieActor
		{
			get
			{
				return this.isMovieActor;
			}
			set
			{
				this.isMovieActor = value;
				this.NotifyPropertyChanged("IsMovieActor");
			}
		}

		private void NotifyPropertyChanged(string info)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}

		public BitmapImage GetImage(string Folderpath)
		{
			BitmapImage result = null;
			bool flag = false;
			if (Settings.Default.SaveMyMoviesMeta)
			{
				this.MyMoviesFolderPath = Settings.Default.ImagesByNameLocation;
				string text = this.MyMoviesFolderPath + "\\" + this.name + "\\folder.jpg";
				if (File.Exists(text))
				{
					result = this.GetBitmapImage(text);
					flag = true;
				}
			}
			if (Settings.Default.SaveXBMCMeta && !flag)
			{
				this.XBMCFolderPath = Folderpath;
				string text = this.XBMCFolderPath + "\\" + this.name.Replace(" ", "_") + ".jpg";
				if (File.Exists(text))
				{
					result = this.GetBitmapImage(text);
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

		public Person(string name, string type, string role, TVShow tvshowbase, Movie moviebase)
		{
			this.TVShowBase = tvshowbase;
			this.MovieBase = moviebase;
			this.IsMovieActor = (tvshowbase == null);
			this.Name = name;
			this.Type = type;
			this.Role = role;
		}
	}
}
