using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace MediaScoutGUI.GUITypes
{
    public class Person : INotifyPropertyChanged
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

        private String name = null;
        public String Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

        private String type = null;
        public String Type
        {
            get { return type; }
            set
            {
                type = value;
                NotifyPropertyChanged("Type");
            }
        }

        private String role = null;
        public String Role
        {
            get { return role; }
            set
            {
                role = value;
                NotifyPropertyChanged("Role");
            }
        }

        private BitmapImage thumb = null;
        public BitmapImage Thumb
        {
            get { return thumb; }
            set
            {
                thumb = value;
                NotifyPropertyChanged("Thumb");
            }
        }

        private String XBMCfolderpath = null;
        public String XBMCFolderPath
        {
            get { return XBMCfolderpath; }
            set
            {
                XBMCfolderpath = value;
                NotifyPropertyChanged("XBMCFolderPath");
            }
        }

        private String MyMoviesfolderpath = null;
        public String MyMoviesFolderPath
        {
            get { return MyMoviesfolderpath; }
            set
            {
                MyMoviesfolderpath = value;
                NotifyPropertyChanged("MyMoviesFolderPath");
            }
        }
        
        private bool isMovieActor = false;
        public bool IsMovieActor
        {
            get { return isMovieActor; }
            set
            {
                isMovieActor = value;
                NotifyPropertyChanged("IsMovieActor");
            }
        }
        #endregion
        
        #region Image Members

        public BitmapImage GetImage(String Folderpath)
        {
            BitmapImage bi = null;
            String Filename;
            if (Properties.Settings.Default.SaveMyMoviesMeta)
            {
                MyMoviesFolderPath = Properties.Settings.Default.ImagesByNameLocation;
                Filename = MyMoviesFolderPath + "\\" + name + @"\folder.jpg";
                if (File.Exists(Filename))
                    bi = GetBitmapImage(Filename);
            }
            else if (Properties.Settings.Default.SaveXBMCMeta)
            {
                XBMCFolderPath = Folderpath;
                Filename = XBMCFolderPath + "\\" + name.Replace(" ", "_") + ".jpg";
                if (File.Exists(Filename))
                    bi = GetBitmapImage(Filename);
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

        public Person(String name, String type, String role, bool isMovieActor)
        {
            Name = name;
            Type = type;
            Role = role;
            IsMovieActor = isMovieActor;
            //GetImage(FolderPath);
        }

    }
        
}
