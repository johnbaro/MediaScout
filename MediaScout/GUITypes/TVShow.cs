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
        string filepath;
        string foldername;
        Int32 id;
        string overview;
        string name;
        string actors;
        string firstaired;
        string rating;
        string runtime;
        string genre;
        string network;

        public bool HasMetadata
        {
            get { return hasMetadata; }
            set { hasMetadata = value; }
        }
        public string Filepath
        {
            get { return filepath; }
            set { filepath = value; }
        }
        public string FolderName
        {
            get { return foldername; }
            set { foldername = value; }
        }
        public Int32 Id
        {
            get { return id; }
            set { id = value; }
        }
        public string Overview
        {
            get { return overview; }
            set { overview = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string Actors
        {
            get { return actors; }
            set { actors = value; }
        }
        public string FirstAired
        {
            get { return firstaired; }
            set { firstaired = value; }
        }
        public string Rating
        {
            get { return rating; }
            set { rating = value; }
        }
        public string Runtime
        {
            get { return runtime; }
            set { runtime = value; }
        }
        public string Genre
        {
            get { return genre; }
            set { genre = value; }
        }
        public string Network
        {
            get { return network; }
            set { network = value; }
        }
        ObservableCollection<Season> seasons;
        public ObservableCollection<Season> Seasons
        {
            get { return seasons; }
            set { seasons = value; }
        }

        public TVShow(String filepath, string name)
        {

            string xmlFile = filepath + @"\series.xml";
            try
            {
                this.filepath = filepath;
                this.foldername = name;
                this.name = name;


                if (File.Exists(xmlFile))
                {
                    hasMetadata = true;
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.Load(filepath + "\\series.xml");
                    XmlNode node = xdoc.DocumentElement;

                    XmlNode idNode = node.SelectSingleNode("id");

                    this.id = Int32.Parse(idNode.InnerText);

                    this.name = node.SelectSingleNode("SeriesName").InnerText;
                    this.actors = node.SelectSingleNode("Actors").InnerText;
                    this.firstaired = node.SelectSingleNode("FirstAired").InnerText;
                    this.rating = node.SelectSingleNode("Rating").InnerText;
                    this.runtime = node.SelectSingleNode("Runtime").InnerText;
                    this.genre = node.SelectSingleNode("Genre").InnerText;
                    this.network = node.SelectSingleNode("Network").InnerText;

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
            catch (Exception ex)
            {
                string xmlFileBackup = xmlFile + ".bak";

                Console.WriteLine("Error processing xml file {0}{1}Exception: {2}{1}Moved file to {3}", xmlFile, Environment.NewLine, ex.Message, xmlFileBackup);

                File.Move(xmlFile, xmlFileBackup);

                Console.WriteLine(ex.ToString());
                
            }
        }
        public override String ToString()
        {
            return this.Name;
        }
    }
}
