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
	public class Movie : INotifyPropertyChanged
	{
		public bool IsDeleted;

		private bool isUnsortedFileCollection;

		private bool hasMetadata;

		private bool metadataChanged;

		private MovieXML XMLbase = new MovieXML();

		private MovieNFO NFObase = new MovieNFO();

		private string id;

		private string name;

		private string description;

		private string year;

		private decimal rating;

		private string runtime;

		private ObservableCollection<MediaScout.Genre> genre;

		private ObservableCollection<Studio> studio;

		public string XMLFile;

		public string NFOFile;

		private string foldername;

		private string folderpath;

		private string poster;

		private string backdrop;

		private string searchterm;

		private ObservableCollection<MovieFile> files;

		private ObservableCollection<Person> actors;

		public bool isPosterLoading;

		public bool isBackDropLoading;

		public event PropertyChangedEventHandler PropertyChanged;

		public bool IsUnsortedFileCollection
		{
			get
			{
				return this.isUnsortedFileCollection;
			}
			set
			{
				this.isUnsortedFileCollection = value;
				this.NotifyPropertyChanged("IsUnsortedFileCollection");
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

		public MovieXML XMLBase
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
				this.Description = this.XMLBase.Description;
				this.Year = this.XMLBase.Year;
				if (!string.IsNullOrEmpty(this.XMLBase.Rating))
				{
					this.Rating = decimal.Parse(this.XMLBase.Rating);
				}
				if (this.XMLBase.Genres != null)
				{
					this.Genre = new ObservableCollection<MediaScout.Genre>(this.XMLBase.Genres);
				}
				if (this.XMLBase.Studios != null)
				{
					this.Studio = new ObservableCollection<Studio>(this.XMLBase.Studios);
				}
				if (this.XMLBase.Persons != null && this.XMLBase.Persons.Count != 0)
				{
					this.Actors = new ObservableCollection<Person>();
					foreach (MediaScout.Person current in this.XMLBase.Persons)
					{
						if (current.Type == "Actor")
						{
							Person item = new Person(current.Name, current.Type, current.Role, null, this);
							this.Actors.Add(item);
						}
					}
				}
				this.NotifyPropertyChanged("XMLBase");
			}
		}

		public MovieNFO NFOBase
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
				this.Description = this.NFOBase.plot;
				this.Year = this.NFOBase.year;
				if (!string.IsNullOrEmpty(this.NFOBase.rating))
				{
					this.Rating = decimal.Parse(this.NFOBase.rating);
				}
				this.Genre = new ObservableCollection<MediaScout.Genre>(this.XMLBase.BuildGenreArrayFromString(this.NFOBase.genre));
				this.Studio = new ObservableCollection<Studio>(this.XMLBase.BuildStudioArrayFromString(this.NFOBase.studio));
				if (this.NFOBase.Actors != null && this.NFOBase.Actors.Count != 0)
				{
					this.Actors = new ObservableCollection<Person>();
					foreach (ActorsNFO current in this.NFOBase.Actors)
					{
						Person item = new Person(current.name, "Actor", current.role, null, this);
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

		public string Description
		{
			get
			{
				return this.description;
			}
			set
			{
				this.description = value;
				if (Settings.Default.SaveMyMoviesMeta)
				{
					this.XMLBase.Description = this.Description;
				}
				if (Settings.Default.SaveXBMCMeta)
				{
					this.NFOBase.plot = this.Description;
				}
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("Description");
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
				if (Settings.Default.SaveMyMoviesMeta)
				{
					this.XMLBase.Year = this.year;
				}
				if (Settings.Default.SaveXBMCMeta)
				{
					this.NFOBase.year = this.year;
				}
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("Year");
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

		public string Runtime
		{
			get
			{
				return this.runtime;
			}
			set
			{
				this.runtime = value;
				if (Settings.Default.SaveMyMoviesMeta)
				{
					this.XMLBase.RunningTime = this.runtime;
				}
				if (Settings.Default.SaveXBMCMeta)
				{
					this.NFOBase.runtime = this.runtime;
				}
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("Runtime");
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
					this.XMLBase.Genres = this.XMLBase.BuildGenreListFromGenreArray(this.Genre);
				}
				if (Settings.Default.SaveXBMCMeta)
				{
					this.NFOBase.genre = this.XMLBase.BuildStringFromGenreArray(this.Genre);
				}
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("Genre");
			}
		}

		public ObservableCollection<Studio> Studio
		{
			get
			{
				return this.studio;
			}
			set
			{
				this.studio = value;
				if (Settings.Default.SaveMyMoviesMeta)
				{
					this.XMLBase.Studios = this.XMLBase.BuildStudioListFromStudioArray(this.Studio);
				}
				if (Settings.Default.SaveXBMCMeta)
				{
					this.NFOBase.studio = this.XMLBase.BuildStringFromStudioArray(this.Studio);
				}
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("Studio");
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
				this.XMLFile = this.folderpath + "\\mymovies.xml";
				this.NFOFile = this.folderpath + "\\movie.nfo";
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

		public ObservableCollection<MovieFile> Files
		{
			get
			{
				return this.files;
			}
			set
			{
				this.files = value;
				this.NotifyPropertyChanged("Files");
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

		public BitmapImage GetImage(MoviePosterType type)
		{
			BitmapImage result = null;
			switch (type)
			{
			case MoviePosterType.Poster:
			{
				string text = this.Folderpath + "\\folder.jpg";
				if (File.Exists(text) && (result = this.GetBitmapImage(text)) != null)
				{
					this.Poster = text;
				}
				break;
			}
			case MoviePosterType.Backdrop:
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
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(MovieXML));
					TextReader textReader = new StreamReader(this.XMLFile);
					this.XMLBase = (MovieXML)xmlSerializer.Deserialize(textReader);
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
					this.HasMetadata = true;
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(MovieNFO));
					TextReader textReader = new StreamReader(this.NFOFile);
					this.NFOBase = (MovieNFO)xmlSerializer.Deserialize(textReader);
					textReader.Close();
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
			this.Load();
			if (this.Year == null)
			{
				this.GetYearFromFolder();
			}
			return this.Year;
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

		public Movie(string folderpath, string foldername, bool IsUnsortedFileCollection)
		{
			this.IsUnsortedFileCollection = IsUnsortedFileCollection;
			this.Folderpath = folderpath;
			this.Foldername = foldername;
			this.Name = foldername;
			this.LoadFiles();
			this.GetYearFromFolder();
			this.MetadataChanged = false;
		}

		public Movie(string folderpath, string name, FileInfo file)
		{
			this.IsUnsortedFileCollection = true;
			this.Folderpath = folderpath;
			this.Name = (this.Foldername = this.foldername);
			this.Files = new ObservableCollection<MovieFile>();
			MovieFile movieFile = new MovieFile(file.FullName, file.Name, this);
			this.Year = movieFile.GetYearFromFilename();
			if (this.Year == null)
			{
				this.GetYearFromFolder();
			}
			this.Files.Add(movieFile);
		}

		public void LoadFiles()
		{
			this.Files = new ObservableCollection<MovieFile>();
			List<string> list = new List<string>(Settings.Default.allowedFileTypes.Split(new char[]
			{
				';'
			}));
			FileInfo[] array = new DirectoryInfo(this.Folderpath).GetFiles();
			for (int i = 0; i < array.Length; i++)
			{
				FileInfo fileInfo = array[i];
				if (list.Contains(fileInfo.Extension))
				{
					MovieFile item = new MovieFile(fileInfo.FullName, fileInfo.Name, this);
					this.Files.Add(item);
				}
			}
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}
