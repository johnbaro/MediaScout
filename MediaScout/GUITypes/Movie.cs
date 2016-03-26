using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.IO;

using System.Xml;
using System.Xml.Serialization;

namespace MediaScoutGUI.GUITypes
{
    public class Movie : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public String id;
        string filepath;
        string name;
        bool hasMetadata = false;
        bool hasPoster = false;
        private String desc;

        public String Desc
        {
            get { return desc; }
            set { desc = value; }
        }

        public bool HasPoster
        {
            get { return hasPoster; }
            set { 
                hasPoster = value;
                NotifyPropertyChanged("HasPoster");
            }
        }

        public bool HasMetadata
        {
            get { return hasMetadata; }
            set { 
                hasMetadata = value;
                NotifyPropertyChanged("HasMetadata");
            }
        }

        public string Filepath
        {
            get { return filepath; }
            set { 
                filepath = value;
                NotifyPropertyChanged("Filepath");
            }
        }

        public string Name
        {
            get { return name; }
            set { 
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public Movie(String filepath, String name)
        {
            this.filepath = filepath;
            this.name = name;

            Load();
        }

        public void Load()
        {
            if (File.Exists(filepath + @"\mymovies.xml"))
            {

                XmlSerializer s = new XmlSerializer(typeof(MediaScout.Movie));
                TextReader w = new StreamReader(filepath + @"\mymovies.xml");
                moviebase = (MediaScout.Movie)s.Deserialize(w);
                w.Close();

                NotifyPropertyChanged("Moviebase.Rating");

                hasMetadata = true;
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(filepath + "\\mymovies.xml");
                XmlNode node = xdoc.DocumentElement;
                name = node.SelectSingleNode("LocalTitle").InnerText;
                desc = node.SelectSingleNode("Description").InnerText;

                if (node.SelectSingleNode("ID") != null)
                    id = node.SelectSingleNode("ID").InnerText;
                else if (node.SelectSingleNode("TMDbId") != null)
                    id = node.SelectSingleNode("TMDbId").InnerText;
            }

            if (File.Exists(filepath + @"\folder.jpg"))
                hasPoster = true;
        }

        private MediaScout.Movie moviebase;

        public MediaScout.Movie Moviebase
        {
            get { return moviebase; }
            set { moviebase = value; }
        }

        public override String ToString()
        {
            return this.Name;
        }
    }
}
