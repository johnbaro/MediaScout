using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;
using MediaScout.Providers;
using System.Text.RegularExpressions;

namespace MediaScoutGUI.GUITypes
{
    public class Movie : INotifyPropertyChanged
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

        private bool isUnsortedFileCollection = false;
        public bool IsUnsortedFileCollection
        {
            get { return isUnsortedFileCollection; }
            set
            {
                isUnsortedFileCollection = value;
                NotifyPropertyChanged("IsUnsortedFile");
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

        private MediaScout.MovieXML XMLbase = new MediaScout.MovieXML();
        public MediaScout.MovieXML XMLBase
        {
            get { return XMLbase; }
            set
            {
                XMLbase = value;
                ID = XMLBase.ID;
                Name = XMLBase.Title;
                Description = XMLBase.Description;
                Year = XMLBase.Year;

                if (XMLBase.Persons != null && XMLBase.Persons.Count != 0)
                {
                    Actors = new ObservableCollection<Person>();
                    foreach (MediaScout.Person actor in XMLBase.Persons)
                    {
                        if (actor.Type == "Actor")
                        {
                            Person p = new Person(actor.Name, actor.Type, actor.Role, true);
                            Actors.Add(p);
                        }
                    }
                }

                NotifyPropertyChanged("XMLBase");
            }
        }

        private MediaScout.MovieNFO NFObase = new MediaScout.MovieNFO();
        public MediaScout.MovieNFO NFOBase
        {
            get { return NFObase; }
            set
            {
                NFObase = value;
                ID = NFOBase.id;
                Name = NFOBase.title;
                Description = NFOBase.plot;
                Year = NFOBase.year;

                if (NFOBase.Actors != null && NFOBase.Actors.Count != 0)
                {
                    Actors = new ObservableCollection<Person>();
                    foreach (MediaScout.ActorsNFO actor in NFOBase.Actors)
                    {
                        Person p = new Person(actor.name, "Actor", actor.role, true);
                        Actors.Add(p);
                    }
                }
                NotifyPropertyChanged("NFOBase");
            }
        }

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

        public String Rating;
        public String Runtime;
        public String Genre;
        public String Network;

        public String XMLFile = null;
        public String NFOFile = null;

        private String id = null;
        public String ID
        {
            get { return id; }
            set
            {
                id = value;
                if (Properties.Settings.Default.SaveMyMoviesMeta)
                    XMLBase.ID = ID;
                if (Properties.Settings.Default.SaveXBMCMeta)
                    NFOBase.id = ID;
                MetadataChanged = true;
                NotifyPropertyChanged("ID");
            }
        }

        private String name = null;
        public string Name
        {
            get { return name; }
            set
            {
                searchterm = name = value;
                if (Properties.Settings.Default.SaveMyMoviesMeta)
                    XMLBase.Title = Name;
                if (Properties.Settings.Default.SaveXBMCMeta)
                    NFOBase.title = Name;
                MetadataChanged = true;
                NotifyPropertyChanged("Name"); }
        }

        private String year = null;
        public string Year
        {
            get { return year; }
            set
            {
                year = value;
                NotifyPropertyChanged("Year");
            }
        }

        private String folderpath = null;
        public String Folderpath
        {
            get { return folderpath; }
            set
            {
                folderpath = value;
                XMLFile = folderpath + @"\mymovies.xml";
                NFOFile = folderpath + @"\movie.nfo";
                NotifyPropertyChanged("Folderpath");
            }
        }

        private String description;
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                if (Properties.Settings.Default.SaveMyMoviesMeta)
                    XMLBase.Description = Description;
                if (Properties.Settings.Default.SaveXBMCMeta)
                    NFOBase.plot = Description;
                MetadataChanged = true;
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

        private String backdrop = null;
        public String Backdrop
        {
            get { return backdrop; }
            set
            {
                backdrop = value;
                NotifyPropertyChanged("Backdrop");
            }
        }        

        private String searchterm = null;
        public string SearchTerm
        {
            get { return searchterm; }
            set
            {
                searchterm = value;
                NotifyPropertyChanged("SearchTerm");
            }
        }

        private ObservableCollection<MovieFile> files;
        public ObservableCollection<MovieFile> Files
        {
            get { return files; ; }
            set
            {
                files = value;
                NotifyPropertyChanged("Files");
            }
        }

        private ObservableCollection<Person> actors;
        public ObservableCollection<Person> Actors
        {
            get { return actors; }
            set
            {
                actors = value;
                NotifyPropertyChanged("Actors");
            }
        }

        #endregion

        #region Image Members

        public BitmapImage GetImage(MoviePosterType type)
        {
            BitmapImage bi = null;
            String Filename;
            switch (type)
            {
                case MoviePosterType.Poster:
                    Filename = Folderpath + @"\folder.jpg";
                    if (File.Exists(Filename))
                    {
                        if ((bi = GetBitmapImage(Filename)) != null)
                            Poster = Filename;
                    }
                    break;
                case MoviePosterType.Backdrop:
                    if (Properties.Settings.Default.SaveMyMoviesMeta)
                    {
                        Filename = Folderpath + @"\backdrop.jpg";
                        if (File.Exists(Filename))
                        {
                            if ((bi = GetBitmapImage(Filename)) != null)
                                Backdrop = Filename;
                        }
                    }
                    else if (Properties.Settings.Default.SaveXBMCMeta)
                    {
                        Filename = Folderpath + @"\fanart.jpg";
                        if (File.Exists(Filename))
                        {
                            if ((bi = GetBitmapImage(Filename)) != null)
                                Backdrop = Filename;
                        }
                    }
                    break;
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

        #endregion
        
        #region Load Properties

        public void Load()
        {
            bool success = false;

            if (Properties.Settings.Default.SaveMyMoviesMeta)
                success = LoadFromXML();
            else if (Properties.Settings.Default.SaveXBMCMeta)
                success = LoadFromNFO();

            if (Actors != null && Actors.Count != 0)
                LoadActorsThumb(null);

            if (success)
                MetadataChanged = false;
        }
        public bool LoadFromXML()
        {
            bool success = false;
            if (File.Exists(XMLFile))
            {
                try
                {
                    XmlSerializer s = new XmlSerializer(typeof(MediaScout.MovieXML));
                    TextReader w = new StreamReader(XMLFile);
                    XMLBase = (MediaScout.MovieXML)s.Deserialize(w);
                    w.Close();

                    HasMetadata = true;

                    //XmlDocument xdoc = new XmlDocument();
                    //xdoc.Load(XMLFile);
                    //XmlNode node = xdoc.DocumentElement;

                    //if (node.SelectSingleNode("ID") != null)
                    //    ID = node.SelectSingleNode("ID").InnerText;
                    //else if (node.SelectSingleNode("TMDbId") != null)
                    //    ID = node.SelectSingleNode("TMDbId").InnerText;

                    //if(node.SelectSingleNode("OriginalTitle") != null)
                    //    Name = node.SelectSingleNode("OriginalTitle").InnerText;

                    //if(name == null)
                    //    if (node.SelectSingleNode("LocalTitle") != null)
                    //        Name = node.SelectSingleNode("LocalTitle").InnerText;
                    
                    //if(node.SelectSingleNode("Description") != null)
                    //    Description = node.SelectSingleNode("Description").InnerText;

                    //if (node.SelectSingleNode("ProductionYear") != null)
                    //    Year = node.SelectSingleNode("ProductionYear").InnerText;

                    //Actors = new ObservableCollection<Person>();
                    //XmlNodeList xnl = xdoc.DocumentElement.SelectNodes("Persons/Person");
                    //foreach (XmlNode x in xnl)
                    //{
                    //    String Type = x.Attributes["Type"].Value;
                    //    if (Type == "Actor")
                    //    {
                    //        Person p = new Person(x.Attributes["Name"].Value, Type, x.Attributes["Role"].Value, true);
                    //        Actors.Add(p);
                    //    }
                    //}

                    success = true;
                }
                catch
                {
                    
                }
            }
            return success;
        }
        public bool LoadFromNFO()
        {
            bool success = false;
            if (File.Exists(NFOFile))
            {
                try
                {
                    HasMetadata = true;

                    XmlSerializer s = new XmlSerializer(typeof(MediaScout.MovieNFO));
                    TextReader w = new StreamReader(NFOFile);
                    NFOBase = (MediaScout.MovieNFO)s.Deserialize(w);
                    w.Close();

                    //XmlDocument xdoc = new XmlDocument();
                    //xdoc.Load(NFOFile);
                    //XmlNode node = xdoc.DocumentElement;

                    //if (node.SelectSingleNode("id") != null)
                    //    ID = node.SelectSingleNode("id").InnerText;

                    //if(node.SelectSingleNode("title") != null)
                    //    Name = node.SelectSingleNode("title").InnerText;
                    
                    //if(node.SelectSingleNode("plot") !=null)
                    //    Description = node.SelectSingleNode("plot").InnerText;

                    //if (node.SelectSingleNode("Year") != null)
                    //    Year = node.SelectSingleNode("Year").InnerText;

                    //Actors = new ObservableCollection<Person>();
                    //XmlNodeList xnl = xdoc.DocumentElement.SelectNodes("actor");
                    //foreach (XmlNode x in xnl)
                    //{
                    //    String ActorName = null;
                    //    String Role = null;
                    //    if (x.SelectSingleNode("name") != null)
                    //        ActorName = x.SelectSingleNode("name").InnerText;
                    //    if (x.SelectSingleNode("role") != null)
                    //        Role = x.SelectSingleNode("role").InnerText;

                    //    Person p = new Person(ActorName, "Actor", Role, true);
                    //    Actors.Add(p);
                    //}
                    
                    success = true;
                }
                catch
                {
                    
                }
            }
            return success;
        }

        #endregion

        #region Get Search Term And Year Properties

        public String GetSearchTerm(String SearchTerm)
        {
            //unable to solve [5.1] to  ""            
            SearchTerm = Regex.Replace(SearchTerm, @".\d{4}", "");
            SearchTerm = Regex.Replace(SearchTerm, @"\{.*\)", "");
            SearchTerm = Regex.Replace(SearchTerm, @"\[.*\]", "");
            SearchTerm = Regex.Replace(SearchTerm, @"\{\.*\}", "");
            SearchTerm = Regex.Replace(SearchTerm, ".hdtv|.2hd|.ac3|.x264", "", RegexOptions.IgnoreCase);
            SearchTerm = Regex.Replace(SearchTerm, @".720p|.1080p|.5\.1", "", RegexOptions.IgnoreCase);
            SearchTerm = Regex.Replace(SearchTerm, ".divx|.xvid", "", RegexOptions.IgnoreCase);
            SearchTerm = Regex.Replace(SearchTerm, ".dvdrip|.dvd|.rip|.subs|.upscaled", "", RegexOptions.IgnoreCase);
            SearchTerm = Regex.Replace(SearchTerm, @"\<|\[|\{|\(|\)|\}|\]|\>", "");
            SearchTerm = Regex.Replace(SearchTerm, @"\.", " ");
            SearchTerm = Regex.Replace(SearchTerm, @"\-", " ");
            SearchTerm = Regex.Replace(SearchTerm, @"\s+", " ");
            SearchTerm = SearchTerm.Trim();

            return SearchTerm;
        }
        public String GetSearchTerm()
        {
            SearchTerm = GetSearchTerm(Name);
            return SearchTerm;
        }

        public String GetYearFromFolder()
        {
            if (Year == null)
            {
                Match m = Regex.Match(Name, @"\d{4}");
                if (m.Success)
                    Year = m.Value;
            }
            return Year;
        }
        public String GetYear()
        {            
            Load();
            if (Year == null)
                GetYearFromFolder();            
            return Year;
        }

        #endregion

        #region Load Actors Thumb
        public void LoadActorsThumb(Person p)
        {
            if(p==null)
                foreach (Person actor in Actors)
                    actor.Thumb = actor.GetImage(Folderpath + "\\.actors");
            else
                p.Thumb = p.GetImage(Folderpath + "\\.actors");
        }
        #endregion
        
        public Movie(String folderpath, String name, bool IsUnsortedFileCollection)
        {
            this.IsUnsortedFileCollection = IsUnsortedFileCollection;
            Folderpath = folderpath;
            Name = name;
            LoadFiles();
            GetYearFromFolder();
            MetadataChanged = false;
        }

        //for autotron use - can be used in all movie dialog also
        public Movie(String folderpath, String name, FileInfo file)
        {
            IsUnsortedFileCollection = true;
            Folderpath = folderpath;
            Name = name;
            Files = new ObservableCollection<MovieFile>();
            MovieFile mf = new MovieFile(file.FullName, file.Name, this);
            Year = mf.GetYearFromFilename();
            if (Year == null)
                GetYearFromFolder();
            Files.Add(mf);
        }

        public void LoadFiles()
        {
            Files = new ObservableCollection<MovieFile>();

            List<String> allowedFiles = new List<String>(MediaScoutGUI.Properties.Settings.Default.allowedFileTypes.Split(';'));

            foreach (FileInfo fi in new DirectoryInfo(this.Folderpath).GetFiles())
            {
                if (allowedFiles.Contains(fi.Extension))
                {
                    MovieFile mf = new MovieFile(fi.FullName, fi.Name, this);
                    Files.Add(mf);
                }
            }
        }
        
        public override String ToString()
        {
            return this.Name;
        }
    }
}
