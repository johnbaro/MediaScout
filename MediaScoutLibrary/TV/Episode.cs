using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MediaScout
{
    [XmlRoot("Item")]
    public class Episode
    {
        
        public Int32 ID;
        public String Director;
        public String EpisodeID;
        public String EpisodeName;
        public String EpisodeNumber;
        public String FirstAired;
        public String GuestStars;
        public String Language;
        public String Overview;
        public String ProductionCode;
        public String Writer;
        public String SeasonNumber;
        public String SeasonID;
        public String SeriesID;
        public String LastUpdated;

        [XmlIgnore]
        public String PosterUrl;

        [XmlElement("filename")]
        public String PosterName;

        [XmlIgnore]
        public System.Drawing.Image Poster;

        [XmlIgnore]
        public String BannerUrl;

        [XmlIgnore]
        public System.Drawing.Image Banner;

        public Episode(Int32 episodeNumber, String name, String posterUrl)
        {
            ID = episodeNumber;
            EpisodeNumber = episodeNumber.ToString();
            EpisodeName = name;

            if (!String.IsNullOrEmpty(posterUrl))
            {
                PosterName = posterUrl.Substring(posterUrl.LastIndexOf("/"));
                PosterUrl = posterUrl;
            }
            else
                PosterUrl = "";           
        }

        public Episode()
        {

        }


    }
}
