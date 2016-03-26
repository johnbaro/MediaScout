using System;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MediaScout.Providers;
using System.Windows.Media.Imaging;

namespace MediaScoutGUI.GUITypes
{
    public class MovieFile : INotifyPropertyChanged
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

        private Movie movie = null;
        public Movie Movie
        {
            get { return movie; }
            set
            {
                movie = value;
                NotifyPropertyChanged("Movie");
            }
        }

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
                XMLFile = Movie.Folderpath + @"\" + StrippedFileName + ".xml";
                NFOFile = Movie.Folderpath + @"\" + StrippedFileName + ".nfo";
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

        #endregion

        #region Image Members

        public BitmapImage GetImage(MoviePosterType type)
        {
            BitmapImage bi = null;
            String Filename;
            switch (type)
            {
                case MoviePosterType.File_Poster:
                    if (Properties.Settings.Default.SaveXBMCMeta)
                    {
                        Filename = Movie.Folderpath + "\\" + StrippedFileName + ".tbn";
                        if (File.Exists(Filename))
                        {
                            if ((bi = GetBitmapImage(Filename)) != null)
                                Poster = Filename;
                        }
                    }
                    break;
                case MoviePosterType.File_Backdrop:
                    if (Properties.Settings.Default.SaveXBMCMeta)
                    {
                        Filename = Movie.Folderpath + "\\" + StrippedFileName + "_fanart.jpg";
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

        #region Get Search Term And Year Properties

        public String GetSearchTerm()
        {
            return Movie.GetSearchTerm(StrippedFileName);            
        }

        public String GetYearFromFilename()
        {
            String Year = null;
            Match m = Regex.Match(Name, @"\d{4}");
            if (m.Success)
                Year = m.Value;
            return Year;
        }
        public String GetYearFromFolder()
        {
            return Movie.GetYearFromFolder();
        }
        public String GetYear()
        {            
            Movie.Load();
            if(Movie.Year == null)
                GetYearFromFolder();
            if (Movie.Year == null)
                GetYearFromFilename();
            return Movie.Year;
        }

        #endregion

        public MovieFile(String filepath, String name, Movie movie)
        {
            Filepath = filepath;
            Movie = movie;
            Name = name;        
        }

        public override String ToString()
        {
            return this.Name;
        }
    }
}