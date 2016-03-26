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

        private String id = null;
        public String ID
        {
            get { return id; }
            set
            {
                id = value;
                NotifyPropertyChanged("ID");
            }
        }

        public String ProdNum;
        public String AirDate;
        public String EpName;

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

        private String descrition = null;
        public String Description
        {
            get { return descrition; }
            set
            {
                descrition = value;
                NotifyPropertyChanged("Description");
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
        
        #endregion

        #region Image Members

        public BitmapImage GetImage(String posterfilename)
        {
            BitmapImage bi = null;
            String Filename = null;
            if (Properties.Settings.Default.SaveMyMoviesMeta)
            {
                Filename = Season.MetadataFolderPath + "\\" + posterfilename + ".jpg";
                if (File.Exists(Filename))
                {
                    if ((bi = GetBitmapImage(Filename)) != null)
                    {
                        Poster = Filename;
                        PosterFilename = posterfilename;
                        UpdatePosterFileNameInXMLFile();
                    }
                }
            }
            else if (Properties.Settings.Default.SaveXBMCMeta)
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
                node.SelectSingleNode("filename").InnerText = PosterFilename + ".jpg";
                xdoc.Save(XMLFile);
            }

        }

        #endregion           

        #region Get Number
        
        public String GetNum()
        {
            Match m = Regex.Match(this.Name, "S(?<se>[0-9]{1,2})E(?<ep>[0-9]{1,3})|(?<se>[0-9]{1,2})x(?<ep>[0-9]{1,3})", RegexOptions.IgnoreCase);
            if (m.Success)
                return Int32.Parse(m.Groups["ep"].Value).ToString();

            //Does the file START WITH just "###" (SEE) or #### (SSEE) ? (if not found yet)
            m = Regex.Match(this.Name, "^(?<se>[0-9]{1,2})(?<ep>[0-9]{2})", RegexOptions.IgnoreCase);
            if (m.Success)
                return Int32.Parse(m.Groups["ep"].Value).ToString();

            //Is it just the two digit episode number maybe?
            m = Regex.Match(this.Name, "^(?<ep>[0-9]{2})", RegexOptions.IgnoreCase);
            if (m.Success)
                return Int32.Parse(m.Groups["ep"].Value).ToString();

            //Does the file NOT START WITH just "###" (SEE) or #### (SSEE) ? (if not found yet)
            m = Regex.Match(this.Name, "(?<se>[0-9]{1,2})(?<ep>[0-9]{2})", RegexOptions.IgnoreCase);
            if (m.Success)
                return Int32.Parse(m.Groups["ep"].Value).ToString(); ;

            return null;
        }
        public String GetSeasonNum()
        {
            Match m = Regex.Match(this.Name, "S(?<se>[0-9]{1,2})E(?<ep>[0-9]{1,3})|(?<se>[0-9]{1,2})x(?<ep>[0-9]{1,3})", RegexOptions.IgnoreCase);
            if (m.Success)
                return Int32.Parse(m.Groups["se"].Value).ToString();

            //Does the file START WITH just "###" (SEE) or #### (SSEE) ? (if not found yet)
            m = Regex.Match(this.Name, "^(?<se>[0-9]{1,2})(?<ep>[0-9]{2})", RegexOptions.IgnoreCase);
            if (m.Success)
                return Int32.Parse(m.Groups["se"].Value).ToString();

            //Is it just the two digit episode number maybe?
            m = Regex.Match(this.Name, "^(?<ep>[0-9]{2})", RegexOptions.IgnoreCase);
            if (m.Success)
                return Int32.Parse(m.Groups["se"].Value).ToString();

            //Does the file NOT START WITH just "###" (SEE) or #### (SSEE) ? (if not found yet)
            m = Regex.Match(this.Name, "(?<se>[0-9]{1,2})(?<ep>[0-9]{2})", RegexOptions.IgnoreCase);
            if (m.Success)
                return Int32.Parse(m.Groups["se"].Value).ToString(); ;

            return null;
        }
        
        #endregion
        
        #region Overview Members
        
        public void GetOverview()
        {
            if (Description == null)
                LoadFromXML();
        }
        
        #endregion

        #region Load Properties
        
        public void Load()
        {
            if(!LoadFromXML())
            { LoadFromNFO(); }
        }
        public bool LoadFromXML()
        {
            bool success = false;
            try
            {
                if (File.Exists(XMLFile))
                {
                    HasMetadata = true;
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.Load(XMLFile);
                    XmlNode node = xdoc.DocumentElement;

                    if(node.SelectSingleNode("ID") != null)
                        ID = node.SelectSingleNode("ID").InnerText;

                    if (node.SelectSingleNode("Overview") != null)
                        Description = node.SelectSingleNode("Overview").InnerText;

                    if (node.SelectSingleNode("EpisodeName") != null)
                        EpName = node.SelectSingleNode("EpisodeName").InnerText;

                    if (node.SelectSingleNode("filename") != null)
                    {
                        PosterFilename = node.SelectSingleNode("filename").InnerText;
                        PosterFilename = PosterFilename.Replace(PosterFilename.Substring(PosterFilename.LastIndexOf('.')), "");
                    }

                    success = true;
                }
            }
            catch
            {
                //System.Windows.MessageBox.Show("Corrupted Xml : " + ex.Message, strippedname);
                HasMetadata = false;
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
                    HasMetadata = true;
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.Load(NFOFile);
                    XmlNode node = xdoc.DocumentElement;

                    if (node.SelectSingleNode("plot") != null)
                        Description = node.SelectSingleNode("plot").InnerText;

                    if (node.SelectSingleNode("title") != null)
                        EpName = node.SelectSingleNode("title").InnerText;                    

                    success = true;
                }
            }
            catch
            {
                //System.Windows.MessageBox.Show("Corrupted Xml : " + ex.Message, strippedname);
                HasMetadata = false;
            }
            return success;
        }
        
        #endregion

        public Episode(String filepath, String name, Season season)
        {
            Filepath = filepath;
            Season = season;
            Name = name;        
        }

        public override String ToString()
        {
            return this.Name;
        }
    }
}