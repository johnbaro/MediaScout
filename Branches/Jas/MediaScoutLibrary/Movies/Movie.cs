using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

namespace MediaScout
{
    [XmlRoot("Title")]
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

        /* Certain values need Getters so that they can be databound to in XAML */
        [XmlIgnore]
        private String localTitle;
        public String LocalTitle
        {
            get { return localTitle; }
            set { localTitle = value; }
        }
        public String OriginalTitle;
        public String SortTitle;
        [XmlIgnore]
        private String description;

        public String Description
        {
            get { return description; }
            set { description = value;
                NotifyPropertyChanged("Description");

            }
        }


        public String IMDBrating;

        [XmlIgnore]
        public String Rating
        {
            get { return IMDBrating; }
            set { IMDBrating = value;
            NotifyPropertyChanged("Rating");
            }
        }


        public String ProductionYear;

        [XmlIgnore]
        public String Year
        {
            get { return ProductionYear; }
            set { ProductionYear = value;
            NotifyPropertyChanged("Year");
            }
        }

        public String RunningTime;

        [XmlIgnore]
        public String Length
        {
            get { return RunningTime; }
            set { RunningTime = value;
            NotifyPropertyChanged("Length");
            }
        }

        [XmlElement("TMDbId")]
        public String ID;

        [XmlIgnore]
        public String TMDbId
        {
            get { return ID; }
            set { ID = value;
            NotifyPropertyChanged("TMDbId");
            }
        }

        public List<Person> Persons = new List<Person>();
        public List<Genre> Genres = new List<Genre>();

        [XmlIgnore]
        public String Title
        {
            get
            {
                return LocalTitle;
            }
            set
            {
                LocalTitle = OriginalTitle = SortTitle = value;
                NotifyPropertyChanged("Title");
            }
        }

        [XmlIgnore]
        public String Poster;

        [XmlIgnore]
        public String thumbnail;

        [XmlIgnore]
        public String Thumbnail
        {
            get { return thumbnail; }
            set { thumbnail = value; }
        }

        [XmlIgnore]
        public String Backdrop;
        [XmlIgnore]
        public String backdropThumbnail;
        [XmlIgnore]
        public String BackdropThumbnail
        {
            get { return backdropThumbnail; }
            set { backdropThumbnail = value; }
        }
    }
}
