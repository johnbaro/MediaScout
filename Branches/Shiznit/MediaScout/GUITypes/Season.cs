using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;

using System.IO;

namespace MediaScoutGUI.GUITypes
{
    public class Season : INotifyPropertyChanged
    {
        ObservableCollection<Episode> episodes;

        public ObservableCollection<Episode> Episodes
        {
            get { return episodes; }
            set { episodes = value; }
        }

        TVShow parent;

        public TVShow Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        string filepath;

        public string Filepath
        {
            get { return filepath; }
            set { filepath = value; }
        }
        string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Season(String filepath, String name, TVShow parent)
        {
            this.parent = parent;
            this.filepath = filepath;
            this.name = name;

            episodes = new ObservableCollection<Episode>();

            List<String> allowedFiles = new List<String>(MediaScoutGUI.Properties.Settings.Default.allowedFileTypes.Split(';'));

            foreach (FileInfo fi in new DirectoryInfo(filepath).GetFiles())
            { 
                if (allowedFiles.Contains(fi.Extension))
                {
                    GUITypes.Episode e = new GUITypes.Episode(fi.FullName, fi.Name, this);
                    episodes.Add(e);
                }
            }            
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public override String ToString()
        {
            return this.Name;
        }
    }
}
