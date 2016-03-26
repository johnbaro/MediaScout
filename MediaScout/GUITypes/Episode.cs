using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace MediaScoutGUI.GUITypes
{
    public class Episode : INotifyPropertyChanged
    {
        private String name;
        private String filepath;
        private String epname;
        public  Season Parent;
        private String poster = null;
        public String id;
        private String description;

        public String Description
        {
            get { return description; }
            set { description = value; }
        }

        private Boolean hasMetadata;

        public Boolean HasMetadata
        {
            get { return hasMetadata; }
            set { hasMetadata = value; }
        }


        public String Poster
        {
            get { return poster ; }
            set { poster = value; }
        }

        public String Epname
        {
          get { return epname; }
          set { epname = value; }
        }

        public Episode()
        {

        }
        public Episode(String filepath, String name, Season Parent)
        {
            this.filepath = filepath;
            this.name = name;
            this.Parent = Parent;

            MetadataCheck();
        }

        public void MetadataCheck()
        {
            String dir = filepath.Substring(0, filepath.LastIndexOf("\\"));
            String strippedname = name.Substring(0, name.LastIndexOf("."));

            if (File.Exists(dir + @"\metadata\" + strippedname + ".xml"))
                HasMetadata = true;

            NotifyPropertyChanged("HasMetadata");
        }
        public void LoadFromXML()
        {
            String dir = filepath.Substring(0, filepath.LastIndexOf("\\"));
            String strippedname = name.Substring(0, name.LastIndexOf("."));

            try
            {
                if (File.Exists(dir + @"\metadata\" + strippedname + ".xml"))
                {
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.Load(dir + @"\metadata\" + strippedname + ".xml");
                    XmlNode node = xdoc.DocumentElement;

                    id = node.SelectSingleNode("id").InnerText;
                    description = node.SelectSingleNode("Overview").InnerText; 
                    epname = node.SelectSingleNode("EpisodeName").InnerText;
                    poster = node.SelectSingleNode("filename").InnerText;
                    poster = dir + @"\metadata\" + poster.Substring(poster.LastIndexOf("/") + 1);
                }
            }
            catch
            {
            }
        }

        public String Filepath
        {
            get { return filepath; }
            set { filepath = value; }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public void UpdatePoster()
        {
            String dir = filepath.Substring(0, filepath.LastIndexOf("\\"));
            String strippedname = name.Substring(0, name.LastIndexOf("."));


            if (File.Exists(dir + @"\metadata\" + strippedname + ".xml"))
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(dir + @"\metadata\" + strippedname + ".xml");
                XmlNode node = xdoc.DocumentElement;

                node.SelectSingleNode("filename").InnerText = poster.Replace(dir + @"\metadata\","");
                xdoc.Save(dir + @"\metadata\" + strippedname + ".xml");
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public override String ToString()
        {
            return this.Name;
        }
    }
}