using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.IO;
using MediaScout.Providers;

namespace MediaScoutGUI.GUITypes
{
    public class Season : INotifyPropertyChanged
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

        private TVShow tvshow;
        public TVShow TVShow
        {
            get { return tvshow; }
            set
            {
                tvshow = value;
                NotifyPropertyChanged("TVShow");
            }
        }
        
        public String MetadataFolderPath;

        private String name = null;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

        private String folderpath = null;
        public String Folderpath
        {
            get { return folderpath; }
            set
            {
                folderpath = value;
                NotifyPropertyChanged("Folderpath");
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

        private ObservableCollection<Episode> episodes;
        public ObservableCollection<Episode> Episodes
        {
            get { return episodes;}
            set { episodes = value;
                NotifyPropertyChanged("Episodes");
            }
        }
        
        #endregion

        #region Image Members

        public BitmapImage GetImage(TVShowPosterType type)
        {
            BitmapImage bi = null;
            String Filename;
            switch (type)
            {
                case TVShowPosterType.Season_Poster:
                    Filename = Folderpath + @"\folder.jpg";
                    if (File.Exists(Filename))
                    {
                        if ((bi = GetBitmapImage(Filename)) != null)
                            Poster = Filename;
                    }
                    break;
                case TVShowPosterType.Season_Backdrop:
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

        #region Get Number
        
        public String GetNum()
        {
            String num = null;
            if (this.Name == Properties.Settings.Default.SpecialsFolderName)
                num = "0";
            Match m = Regex.Match(this.Name, ".{0,1}([0-9]+)", RegexOptions.IgnoreCase);
            if(m.Success)
                num = Int32.Parse(m.Value).ToString();
            return num;
        }
        
        #endregion
        
        public Season(String filepath, String name, TVShow tvshow)
        {
            TVShow = tvshow;
            Folderpath = filepath;
            Name = name;
            MetadataFolderPath = filepath + @"\metadata";

            LoadEpisodes();        
        }

        public void LoadEpisodes()
        {
            Episodes = new ObservableCollection<Episode>();

            List<String> allowedFiles = new List<String>(MediaScoutGUI.Properties.Settings.Default.allowedFileTypes.Split(';'));

            foreach (FileInfo fi in new DirectoryInfo(this.Folderpath).GetFiles())
            {
                if (allowedFiles.Contains(fi.Extension))
                {
                    Episode e = new Episode(fi.FullName, fi.Name, this);
                    Episodes.Add(e);
                }
            }
        }
        
        public override String ToString()
        {
            return this.Name;
        }
    }
}
