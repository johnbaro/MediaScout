using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using System.IO;

using System.Xml;

namespace MediaScoutGUI.GUITypes
{
    public class TVShow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        bool hasMetadata = false;
        Int32 id;
        string filepath;
        string name;
        string overview;

        public string Overview
        {
            get { return overview; }
            set { overview = value; }
        }

        ObservableCollection<Season> seasons;

        public ObservableCollection<Season> Seasons
        {
            get { return seasons; }
            set { seasons = value; }
        }

        public bool HasMetadata
        {
            get { return hasMetadata; }
            set { hasMetadata = value; }
        }
        public Int32 Id
        {
            get { return id; }
            set { id = value; }
        }
        public string Filepath
        {
            get { return filepath; }
            set { filepath = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public TVShow(String filepath, string name)
        {
            this.filepath = filepath;
            this.name = name;

            if (File.Exists(filepath + @"\series.xml"))
            {
                hasMetadata = true;
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(filepath + "\\series.xml");
                XmlNode node = xdoc.DocumentElement;

                id = Int32.Parse(node.SelectSingleNode("id").InnerText);

                name = node.SelectSingleNode("SeriesName").InnerText;

                if (node.SelectSingleNode("Overview") != null)
                    overview = node.SelectSingleNode("Overview").InnerText;
            }

            seasons = new ObservableCollection<Season>();

            foreach (DirectoryInfo di in new DirectoryInfo(filepath).GetDirectories())
            {
                if (di.Name != "metadata")
                {
                    GUITypes.Season s = new GUITypes.Season(di.FullName, di.Name, this);
                    seasons.Add(s);
                }
            }
        }
        public override String ToString()
        {
            return this.Name;
        }
    }
}
