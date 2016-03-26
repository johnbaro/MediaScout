using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace MediaScout
{
    [XmlRoot("Series")]
    public class TVShow
    {
        public String id;
        public String Actors;
        public String ContentRating;
        public String FirstAired;
        public String Genre;
        public String IMDB_ID;
        public String Language;
        public String Network;
        public String Rating;
        public String Runtime;
        public String SeriesID;
        public String Status;

        [XmlIgnore]
        private String overview;

        [XmlIgnore]
        private String seriesName;

        [XmlIgnore]
        public SortedList<Int32, Season> Seasons = new SortedList<Int32, Season>();

        [XmlIgnore]
        public String SeriesPosterUrl;

        [XmlIgnore]
        public String seriesBannerUrl;


        public TVShow(String SeriesID, String SeriesName, String SeriesPosterUrl, String SeriesBannerUrl)
        {
            this.SeriesID = id = SeriesID;
            this.SeriesBannerUrl = SeriesBannerUrl;
            this.SeriesPosterUrl = SeriesPosterUrl;
            this.SeriesName = SeriesName;
        }

        public TVShow()
        {

        }

        [XmlIgnore]
        public String SeriesBannerUrl
        {
            get { return seriesBannerUrl; }
            set { seriesBannerUrl = value; }
        }

        public String Overview
        {
            get { return overview; }
            set { overview = value; }
        }

        public String SeriesName
        {
            get { return seriesName; }
            set { seriesName = value; }
        }

        public void Save(String filename)
        {
            XmlSerializer s = new XmlSerializer(typeof(TVShow));
            TextWriter w = new StreamWriter(filename);
            s.Serialize(w, this);
            w.Close();
        }

        public void Load(String filepath)
        {
            if (File.Exists(filepath + @"\series.xml"))
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(filepath + "\\series.xml");
                XmlNode node = xdoc.DocumentElement;
            }
        }
    }
}
