using System;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Media.Imaging;

namespace MediaScoutGUI.GUITypes
{
    public class Episode : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        #region Properties

        public bool IsDeleted = false;

        private bool hasMetadata = false;
        public bool HasMetadata
        {
            get { return hasMetadata; }
            set
            {
                hasMetadata = value;
                NotifyPropertyChanged("HasMetadata");
            }
        }
        
        private bool metadataChanged = false;
        public bool MetadataChanged
        {
            get { return metadataChanged; }
            set
            {
                metadataChanged = value;
                NotifyPropertyChanged("MetadataChanged");
            }
        }

        private MediaScout.EpisodeXML XMLbase = new MediaScout.EpisodeXML();
        public MediaScout.EpisodeXML XMLBase
        {
            get { return XMLbase; }
            set
            {
                XMLbase = value;
                ID = String.IsNullOrEmpty(XMLBase.EpisodeID) ? XMLBase.ID : XMLBase.EpisodeID;                
                EpisodeName = XMLBase.EpisodeName;
                Description = XMLBase.Overview;
                AirDate = XMLBase.FirstAired;

                PosterFilename = XMLBase.PosterName;

                NotifyPropertyChanged("XMLBase");
            }
        }

        private MediaScout.EpisodeNFO NFObase = new MediaScout.EpisodeNFO();
        public MediaScout.EpisodeNFO NFOBase
        {
            get { return NFObase; }
            set
            {
                NFObase = value;
                ID = NFOBase.id;                
                EpisodeName = NFOBase.title;
                Description = NFOBase.plot;
                AirDate = NFOBase.aired;

                NotifyPropertyChanged("NFOBase");
            }
        }

        private String id = null;
        public String ID
        {
            get { return id; }
            set
            {
                id = value;

                if (Properties.Settings.Default.SaveMyMoviesMeta)
                    XMLBase.EpisodeID = XMLBase.ID = ID;
                if (Properties.Settings.Default.SaveXBMCMeta)
                    NFOBase.id = ID;
                MetadataChanged = true;

                NotifyPropertyChanged("ID");
            }
        }

        private String episodename = null;
        public String EpisodeName
        {
            get { return episodename; }
            set
            {
                episodename = value;

                if (Properties.Settings.Default.SaveMyMoviesMeta)
                    XMLBase.EpisodeName = EpisodeName;
                if (Properties.Settings.Default.SaveXBMCMeta)
                    NFOBase.title = EpisodeName;
                MetadataChanged = true;

                NotifyPropertyChanged("EpisodeName");
            }
        }

        private String airdate = null;
        public String AirDate
        {
            get { return airdate; }
            set
            {
                airdate = value;

                if (Properties.Settings.Default.SaveMyMoviesMeta)
                    XMLBase.FirstAired = AirDate;
                if (Properties.Settings.Default.SaveXBMCMeta)
                    NFOBase.aired = AirDate;
                MetadataChanged = true;

                NotifyPropertyChanged("AirDate");
            }
        }

        private String description = null;
        public String Description
        {
            get { return description; }
            set
            {
                description = value;
                NotifyPropertyChanged("Description");
            }
        }
        
        public String PosterFilename = null;
        public String StrippedFileName = null;
        public String XMLFile = null;
        public String NFOFile = null;

        private String name = null;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                StrippedFileName = name.Substring(0, name.LastIndexOf("."));
                XMLFile = Season.MetadataFolderPath + @"\" + StrippedFileName + ".xml";
                NFOFile = Season.Folderpath + @"\" + StrippedFileName + ".nfo"; 
                NotifyPropertyChanged("Name");
            }
        }

        private String filepath = null;
        public String Filepath
        {
            get { return filepath; }
            set
            {
                filepath = value;
                NotifyPropertyChanged("Filepath");
            }
        }

        private String poster = null;
        public String Poster
        {
            get { return poster; }
            set
            {
                poster = value;
                NotifyPropertyChanged("Poster");
            }
        }

        private Season season = null;
        public Season Season
        {
            get { return season; }
            set
            {
                season = value;
                NotifyPropertyChanged("Season");
            }
        }

        #endregion

        #region Image Members

        public BitmapImage GetImage(String posterfilename)
        {
            BitmapImage bi = null;
            String Filename = null;
            bool success = false;
            if (Properties.Settings.Default.SaveMyMoviesMeta)
            {
                Filename = Season.MetadataFolderPath + "\\" + posterfilename;
                if (File.Exists(Filename))
                {
                    if ((bi = GetBitmapImage(Filename)) != null)
                    {
                        Poster = Filename;
                        PosterFilename = posterfilename;
                        UpdatePosterFileNameInXMLFile();
                        success = true;
                    }
                }
            }
            if (Properties.Settings.Default.SaveXBMCMeta && !success)
            {
                Filename = Season.Folderpath + "\\" + StrippedFileName + ".tbn";
                if (File.Exists(Filename))
                {
                    if ((bi = GetBitmapImage(Filename)) != null)
                    {
                        Poster = Filename;
                        PosterFilename = null;
                    }
                }
            }
            return bi;
        }       
        public BitmapImage GetBitmapImage(String Filepath)
        {
            BitmapImage bi = null;

            try
            {
                bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bi.UriSource = new Uri(Filepath);
                bi.EndInit();
            }
            catch
            {
                bi = null;
            }

            return bi;
        }

        public void UpdatePosterFileNameInXMLFile()
        {
            if (File.Exists(XMLFile))
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(XMLFile);
                XmlNode node = xdoc.DocumentElement;

                if (node.SelectSingleNode("filename") == null)
                    xdoc.CreateNode(XmlNodeType.Element, "filename", null);
                node.SelectSingleNode("filename").InnerText = PosterFilename;
                xdoc.Save(XMLFile);
            }

        }

        #endregion           

        #region Load Properties
        
        public void Load()
        {
            bool success = false;

            if (Properties.Settings.Default.SaveMyMoviesMeta)
                success = LoadFromXML();
            if (Properties.Settings.Default.SaveXBMCMeta && !success)
                success = LoadFromNFO();

            if (success)
                MetadataChanged = false;
        }
        public bool LoadFromXML()
        {
            bool success = false;
            try
            {
                if (File.Exists(XMLFile))
                {
                    XmlSerializer s = new XmlSerializer(typeof(MediaScout.EpisodeXML));
                    TextReader w = new StreamReader(XMLFile);
                    XMLBase = (MediaScout.EpisodeXML)s.Deserialize(w);
                    w.Close();

                    HasMetadata = true;

                    //XmlDocument xdoc = new XmlDocument();
                    //xdoc.Load(XMLFile);
                    //XmlNode node = xdoc.DocumentElement;

                    //if(node.SelectSingleNode("ID") != null)
                    //    ID = node.SelectSingleNode("ID").InnerText;

                    //if (node.SelectSingleNode("Overview") != null)
                    //    Description = node.SelectSingleNode("Overview").InnerText;

                    //if (node.SelectSingleNode("EpisodeName") != null)
                    //    EpName = node.SelectSingleNode("EpisodeName").InnerText;

                    //if (node.SelectSingleNode("filename") != null)
                    //{
                    //    PosterFilename = node.SelectSingleNode("filename").InnerText;
                    //    PosterFilename = PosterFilename.Replace(PosterFilename.Substring(PosterFilename.LastIndexOf('.')), "");
                    //}

                    success = true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                //System.Windows.MessageBox.Show("Corrupted Xml : " + ex.Message, strippedname);
                //HasMetadata = false;
            }
            return success;
        }
        public bool LoadFromNFO()
        {
            bool success = false;
            try
            {
                if (File.Exists(NFOFile))
                {
                    XmlSerializer s = new XmlSerializer(typeof(MediaScout.EpisodeNFO));
                    TextReader w = new StreamReader(NFOFile);
                    NFOBase = (MediaScout.EpisodeNFO)s.Deserialize(w);
                    w.Close();

                    HasMetadata = true;

                    //XmlDocument xdoc = new XmlDocument();
                    //xdoc.Load(NFOFile);
                    //XmlNode node = xdoc.DocumentElement;

                    //if (node.SelectSingleNode("plot") != null)
                    //    Description = node.SelectSingleNode("plot").InnerText;

                    //if (node.SelectSingleNode("title") != null)
                    //    EpName = node.SelectSingleNode("title").InnerText;                    

                    success = true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                //System.Windows.MessageBox.Show("Corrupted Xml : " + ex.Message, strippedname);
                //HasMetadata = false;
            }
            return success;
        }
        
        #endregion

        public Episode(String filepath, String name, Season season)
        {
            Filepath = filepath;
            Season = season;
            EpisodeName = Name = name;
            MetadataChanged = false;
        }

        public override String ToString()
        {
            return this.Name;
        }
    }
}