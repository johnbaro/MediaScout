using MediaScout;
using MediaScoutGUI.Properties;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;

namespace MediaScoutGUI.GUITypes
{
	public class Episode : INotifyPropertyChanged
	{
		public bool IsDeleted;

		private bool hasMetadata;

		private bool metadataChanged;

		private EpisodeXML XMLbase = new EpisodeXML();

		private EpisodeNFO NFObase = new EpisodeNFO();

		private string id;

		private string episodename;

		private string airdate;

		private string description;

		public string PosterFilename;

		public string StrippedFileName;

		public string XMLFile;

		public string NFOFile;

		private string name;

		private string filepath;

		private string poster;

		private Season season;

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

		public EpisodeXML XMLBase
		{
			get
			{
				return this.XMLbase;
			}
			set
			{
				this.XMLbase = value;
				this.ID = (string.IsNullOrEmpty(this.XMLBase.EpisodeID) ? this.XMLBase.ID : this.XMLBase.EpisodeID);
				this.EpisodeName = this.XMLBase.EpisodeName;
				this.Description = this.XMLBase.Overview;
				this.AirDate = this.XMLBase.FirstAired;
				this.PosterFilename = this.XMLBase.PosterName;
				this.NotifyPropertyChanged("XMLBase");
			}
		}

		public EpisodeNFO NFOBase
		{
			get
			{
				return this.NFObase;
			}
			set
			{
				this.NFObase = value;
				this.ID = this.NFOBase.id;
				this.EpisodeName = this.NFOBase.title;
				this.Description = this.NFOBase.plot;
				this.AirDate = this.NFOBase.aired;
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
					this.XMLBase.EpisodeID = (this.XMLBase.ID = this.ID);
				}
				if (Settings.Default.SaveXBMCMeta)
				{
					this.NFOBase.id = this.ID;
				}
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("ID");
			}
		}

		public string EpisodeName
		{
			get
			{
				return this.episodename;
			}
			set
			{
				this.episodename = value;
				if (Settings.Default.SaveMyMoviesMeta)
				{
					this.XMLBase.EpisodeName = this.EpisodeName;
				}
				if (Settings.Default.SaveXBMCMeta)
				{
					this.NFOBase.title = this.EpisodeName;
				}
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("EpisodeName");
			}
		}

		public string AirDate
		{
			get
			{
				return this.airdate;
			}
			set
			{
				this.airdate = value;
				if (Settings.Default.SaveMyMoviesMeta)
				{
					this.XMLBase.FirstAired = this.AirDate;
				}
				if (Settings.Default.SaveXBMCMeta)
				{
					this.NFOBase.aired = this.AirDate;
				}
				this.MetadataChanged = true;
				this.NotifyPropertyChanged("AirDate");
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
				this.NotifyPropertyChanged("Description");
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
				this.XMLFile = this.Season.MetadataFolderPath + "\\" + this.StrippedFileName + ".xml";
				this.NFOFile = this.Season.Folderpath + "\\" + this.StrippedFileName + ".nfo";
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

		public Season Season
		{
			get
			{
				return this.season;
			}
			set
			{
				this.season = value;
				this.NotifyPropertyChanged("Season");
			}
		}

		private void NotifyPropertyChanged(string info)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}

		public BitmapImage GetImage(string posterfilename)
		{
			BitmapImage result = null;
			bool flag = false;
			if (Settings.Default.SaveMyMoviesMeta)
			{
				string path = this.Season.MetadataFolderPath + "\\" + posterfilename;
				if (File.Exists(path) && (result = this.GetBitmapImage(path)) != null)
				{
					this.Poster = path;
					this.PosterFilename = posterfilename;
					this.UpdatePosterFileNameInXMLFile();
					flag = true;
				}
			}
			if (Settings.Default.SaveXBMCMeta && !flag)
			{
				string path = this.Season.Folderpath + "\\" + this.StrippedFileName + ".tbn";
				if (File.Exists(path) && (result = this.GetBitmapImage(path)) != null)
				{
					this.Poster = path;
					this.PosterFilename = null;
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

		public void UpdatePosterFileNameInXMLFile()
		{
			if (File.Exists(this.XMLFile))
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(this.XMLFile);
				XmlNode documentElement = xmlDocument.DocumentElement;
				if (documentElement.SelectSingleNode("filename") == null)
				{
					xmlDocument.CreateNode(XmlNodeType.Element, "filename", null);
				}
				documentElement.SelectSingleNode("filename").InnerText = this.PosterFilename;
				xmlDocument.Save(this.XMLFile);
			}
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
			if (flag)
			{
				this.MetadataChanged = false;
			}
		}

		public bool LoadFromXML()
		{
			bool result = false;
			try
			{
				if (File.Exists(this.XMLFile))
				{
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(EpisodeXML));
					TextReader textReader = new StreamReader(this.XMLFile);
					this.XMLBase = (EpisodeXML)xmlSerializer.Deserialize(textReader);
					textReader.Close();
					this.HasMetadata = true;
					result = true;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			return result;
		}

		public bool LoadFromNFO()
		{
			bool result = false;
			try
			{
				if (File.Exists(this.NFOFile))
				{
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(EpisodeNFO));
					TextReader textReader = new StreamReader(this.NFOFile);
					this.NFOBase = (EpisodeNFO)xmlSerializer.Deserialize(textReader);
					textReader.Close();
					this.HasMetadata = true;
					result = true;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			return result;
		}

		public Episode(string filepath, string name, Season season)
		{
			this.Filepath = filepath;
			this.Season = season;
			this.Name = name;
			this.EpisodeName = name;
			this.MetadataChanged = false;
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}
