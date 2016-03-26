using MediaScout;
using MediaScout.Providers;
using MediaScoutGUI.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace MediaScoutGUI.GUITypes
{
	public class TVShow : INotifyPropertyChanged
	{
		public bool IsDeleted;

		private bool isUnsortedEpisodeCollection;

		private bool hasMetadata;

		private bool metadataChanged;

		private TVShowXML XMLbase = new TVShowXML();

		private TVShowNFO NFObase = new TVShowNFO();

		private string id;

		private string name;

		private string overview;

		private string firstaired;

		private decimal rating;

		private ObservableCollection<MediaScout.Genre> genre;

		private ObservableCollection<Studio> network;

		private string year;

		public string XMLFile;

		public string NFOFile;

		private string foldername;

		private string folderpath;

		private string poster;

		private string backdrop;

		private string banner;

		private string searchterm;

		private ObservableCollection<Season> seasons;

		private ObservableCollection<Episode> unsortedEpisodes;

		private ObservableCollection<Person> actors;

		public bool isPosterLoading;

		public bool isBackDropLoading;

		public bool isBannerLoading;

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

		public bool MetadataChanged
		{
			get
			{
				return this.metadataChanged;
			}
			set
			{
				this.metadataChanged = value;
				this.NotifyPropertyChanged("MetadataChanged");
			}
		}

		public TVShowXML XMLBase
		{
			get
			{
				return this.XMLbase;
			}
			set
			{
				this.XMLbase = value;
				this.ID = this.XMLBase.ID;
				this.Name = this.XMLBase.Title;
				this.Overview = this.XMLBase.Description;
				this.FirstAired = this.XMLBase.FirstAired;
				if (!string.IsNullOrEmpty(this.XMLBase.Rating))
				{
					this.Rating = decimal.Parse(this.XMLBase.Rating);
				}
				this.Genre = new ObservableCollection<MediaScout.Genre>(this.XMLBase.BuildGenreArrayFromString(this.XMLBase.Genre));
				this.Network = new ObservableCollection<Studio>(this.XMLBase.BuildStudioArrayFromString(this.XMLBase.Network));
				if (!string.IsNullOrEmpty(this.XMLBase.Actors))
				{
					this.Actors = new ObservableCollection<Person>();
					string[] array = this.XMLBase.Actors.Split(new char[]
					{
						'|'
					});
					string[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						string value2 = array2[i];
						if (!string.IsNullOrEmpty(value2))
						{
							Person item = new Person(value2, "Actor", null, this, null);
							this.Actors.Add(item);
						}
					}
				}
				this.NotifyPropertyChanged("XMLBase");
			}
		}

		public TVShowNFO NFOBase
		{
			get
			{
				return this.NFObase;
			}
			set
			{
				this.NFObase = value;
				this.ID = this.NFOBase.id;
				this.Name = this.NFOBase.title;
				this.Overview = this.NFOBase.plot;
				this.FirstAired = this.NFOBase.aired;
				if (!string.IsNullOrEmpty(this.NFOBase.rating))
				{
					this.Rating = decimal.Parse(this.NFOBase.rating);
				}
				this.Genre = new ObservableCollection<MediaScout.Genre>(this.XMLBase.BuildGenreArrayFromString(this.NFOBase.genre));
				this.Network = new ObservableCollection<Studio>(this.XMLBase.BuildStudioArrayFromString(this.NFOBase.studio));
				if (this.NFOBase.Actors != null && this.NFOBase.Actors.Count != 0)
				{
					this.Actors = new ObservableCollection<Person>();
					foreach (ActorsNFO current in this.NFOBase.Actors)
					{
						Person item = new Person(current.name, "Actor", current.role, this, null);
						this.Actors.Add(item);
					}
				}
				this.NotifyPropertyChanged("NFOBase");
			}
		}

		public string ID
		{
			get
			{
				return this.id;
			}
			set
			{
				this.id = value;
				if (Settings.Default.SaveMyMoviesMeta)
				{
					this.XMLBase.ID = this.ID;
				}
				if (Settings.Default.SaveXBMCMeta)
				{
					this.NFOBase.id = this.ID;
				}
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("ID");
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
				this.searchterm = value;
				if (Settings.Default.SaveMyMoviesMeta)
				{
					this.XMLBase.Title = this.Name;
				}
				if (Settings.Default.SaveXBMCMeta)
				{
					this.NFOBase.title = this.Name;
				}
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("Name");
			}
		}

		public string Overview
		{
			get
			{
				return this.overview;
			}
			set
			{
				this.overview = value;
				if (Settings.Default.SaveMyMoviesMeta)
				{
					this.XMLBase.Overview = this.Overview;
				}
				if (Settings.Default.SaveXBMCMeta)
				{
					this.NFOBase.plot = this.Overview;
				}
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("Overview");
			}
		}

		public string FirstAired
		{
			get
			{
				return this.firstaired;
			}
			set
			{
				this.firstaired = value;
				if (!string.IsNullOrEmpty(this.firstaired))
				{
					this.Year = this.firstaired.Substring(0, 4);
				}
				if (Settings.Default.SaveMyMoviesMeta)
				{
					this.XMLBase.FirstAired = this.firstaired;
				}
				if (Settings.Default.SaveXBMCMeta)
				{
					this.NFOBase.aired = this.firstaired;
				}
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("FirstAired");
			}
		}

		public decimal Rating
		{
			get
			{
				return this.rating;
			}
			set
			{
				this.rating = value;
				if (Settings.Default.SaveMyMoviesMeta)
				{
					this.XMLBase.Rating = this.rating.ToString();
				}
				if (Settings.Default.SaveXBMCMeta)
				{
					this.NFOBase.rating = this.rating.ToString();
				}
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("Rating");
			}
		}

		public ObservableCollection<MediaScout.Genre> Genre
		{
			get
			{
				return this.genre;
			}
			set
			{
				this.genre = value;
				if (Settings.Default.SaveMyMoviesMeta)
				{
					this.XMLBase.Genre = this.XMLBase.BuildStringFromGenreArray(this.Genre);
				}
				if (Settings.Default.SaveXBMCMeta)
				{
					this.NFOBase.genre = this.XMLBase.BuildStringFromGenreArray(this.Genre);
				}
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("Genre");
			}
		}

		public ObservableCollection<Studio> Network
		{
			get
			{
				return this.network;
			}
			set
			{
				this.network = value;
				if (Settings.Default.SaveMyMoviesMeta)
				{
					this.XMLBase.Network = this.XMLBase.BuildStringFromStudioArray(this.Network);
				}
				if (Settings.Default.SaveXBMCMeta)
				{
					this.NFOBase.studio = this.XMLBase.BuildStringFromStudioArray(this.Network);
				}
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("Network");
			}
		}

		public string Year
		{
			get
			{
				return this.year;
			}
			set
			{
				this.year = value;
				this.NotifyPropertyChanged("Year");
			}
		}

		public string Foldername
		{
			get
			{
				return this.foldername;
			}
			set
			{
				this.foldername = value;
				this.NotifyPropertyChanged("Foldername");
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
				this.XMLFile = this.folderpath + "\\series.xml";
				this.NFOFile = this.folderpath + "\\tvshow.nfo";
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

		public string Banner
		{
			get
			{
				return this.banner;
			}
			set
			{
				this.banner = value;
				this.NotifyPropertyChanged("Banner");
			}
		}

		public string SearchTerm
		{
			get
			{
				return this.searchterm;
			}
			set
			{
				this.searchterm = value;
				this.NotifyPropertyChanged("SearchTerm");
			}
		}

		public ObservableCollection<Season> Seasons
		{
			get
			{
				return this.seasons;
			}
			set
			{
				this.seasons = value;
				this.NotifyPropertyChanged("Seasons");
			}
		}

		public ObservableCollection<Episode> UnsortedEpisodes
		{
			get
			{
				return this.unsortedEpisodes;
			}
			set
			{
				this.unsortedEpisodes = value;
				this.NotifyPropertyChanged("UnsortedEpisodes");
			}
		}

		public ObservableCollection<Person> Actors
		{
			get
			{
				return this.actors;
			}
			set
			{
				this.actors = value;
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("Actors");
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
			case TVShowPosterType.Poster:
			{
				string text = this.Folderpath + "\\folder.jpg";
				if (File.Exists(text) && (result = this.GetBitmapImage(text)) != null)
				{
					this.Poster = text;
				}
				break;
			}
			case TVShowPosterType.Backdrop:
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
			case TVShowPosterType.Banner:
			{
				string text = this.Folderpath + "\\banner.jpg";
				if (File.Exists(text) && (result = this.GetBitmapImage(text)) != null)
				{
					this.Banner = text;
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

		public void Load()
		{
			bool flag = false;
			if (Settings.Default.SaveMyMoviesMeta)
			{
				flag = this.LoadFromXML();
			}
			if (Settings.Default.SaveXBMCMeta && !flag)
			{
				flag = this.LoadFromNFO();
			}
			if (this.Actors != null && this.Actors.Count != 0)
			{
				this.LoadActorsThumb(null);
			}
			if (flag)
			{
				this.MetadataChanged = false;
			}
		}

		public bool LoadFromXML()
		{
			bool result = false;
			if (File.Exists(this.XMLFile))
			{
				try
				{
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(TVShowXML));
					TextReader textReader = new StreamReader(this.XMLFile);
					this.XMLBase = (TVShowXML)xmlSerializer.Deserialize(textReader);
					textReader.Close();
					this.HasMetadata = true;
					result = true;
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
			}
			return result;
		}

		public bool LoadFromNFO()
		{
			bool result = false;
			if (File.Exists(this.NFOFile))
			{
				try
				{
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(TVShowNFO));
					TextReader textReader = new StreamReader(this.NFOFile);
					this.NFOBase = (TVShowNFO)xmlSerializer.Deserialize(textReader);
					textReader.Close();
					this.HasMetadata = true;
					result = true;
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
			}
			return result;
		}

		public string GetSearchTerm(string SearchTerm)
		{
			SearchTerm = Search.GetSearchTerm(SearchTerm);
			return SearchTerm;
		}

		public string GetSearchTerm()
		{
			this.SearchTerm = this.GetSearchTerm(this.Name);
			return this.SearchTerm;
		}

		public string GetYearFromFolder()
		{
			if (this.Year == null)
			{
				MatchCollection matchCollection = Regex.Matches(this.Name, "\\d{4}");
				if (matchCollection.Count > 0)
				{
					this.Year = matchCollection[matchCollection.Count - 1].Value;
				}
			}
			return this.Year;
		}

		public string GetYear()
		{
			this.GetYearFromFolder();
			if (this.Year == null)
			{
				this.Load();
			}
			return this.Year;
		}

		public void LoadSeasons()
		{
			this.Seasons = new ObservableCollection<Season>();
			bool flag = false;
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
					flag = true;
					break;
				}
			}
			if (this.IsUnsortedEpisodeCollection || flag)
			{
				Season season = new Season(this.Folderpath, "Unsorted Episodes", this, flag);
				this.Seasons.Add(season);
				this.LoadUnsortedEpisodes(season);
			}
			if (this.IsUnsortedEpisodeCollection)
			{
				return;
			}
			DirectoryInfo[] directories = new DirectoryInfo(this.Folderpath).GetDirectories();
			for (int j = 0; j < directories.Length; j++)
			{
				DirectoryInfo directoryInfo = directories[j];
				Regex regex = new Regex(Settings.Default.SeasonFolderName + ".{0,1}([0-9]+)", RegexOptions.IgnoreCase);
				MatchCollection matchCollection = regex.Matches(directoryInfo.Name);
				if (matchCollection.Count > 0 || directoryInfo.Name == Settings.Default.SpecialsFolderName)
				{
					Season item = new Season(directoryInfo.FullName, directoryInfo.Name, this, false);
					this.Seasons.Add(item);
				}
			}
		}

		public void LoadUnsortedEpisodes(Season UnsortedSeason)
		{
			this.UnsortedEpisodes = new ObservableCollection<Episode>();
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
					Episode item = new Episode(fileInfo.FullName, fileInfo.Name, UnsortedSeason);
					this.UnsortedEpisodes.Add(item);
				}
			}
		}

		public void LoadActorsThumb(Person p)
		{
			if (p == null)
			{
				using (IEnumerator<Person> enumerator = this.Actors.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Person current = enumerator.Current;
						current.Thumb = current.GetImage(this.Folderpath + "\\.actors");
					}
					return;
				}
			}
			p.Thumb = p.GetImage(this.Folderpath + "\\.actors");
		}

		public TVShow(string folderpath, string foldername, bool IsUnsortedEpisodeCollection)
		{
			this.IsUnsortedEpisodeCollection = IsUnsortedEpisodeCollection;
			this.Folderpath = folderpath;
			this.Foldername = foldername;
			this.Name = foldername;
			this.LoadSeasons();
			this.MetadataChanged = false;
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}
