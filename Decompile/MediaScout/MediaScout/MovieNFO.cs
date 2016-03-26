using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace MediaScout
{
	[XmlRoot("movie")]
	public class MovieNFO
	{
		private string originaltitle;

		private string sorttitle;

		public string set;

		public string rating;

		public string year;

		public string top250;

		public string votes;

		public string outline;

		public string plot;

		public string tagline;

		public string runtime;

		public string mpaa;

		public string id;

		public string genre;

		public string director;

		public string credits;

		public string studio;

		[XmlElement("actor")]
		public System.Collections.Generic.List<ActorsNFO> Actors = new System.Collections.Generic.List<ActorsNFO>();

		public string trailer;

		[XmlIgnore]
		private string localtitle;

		public string title
		{
			get
			{
				return this.localtitle;
			}
			set
			{
				this.sorttitle = value;
				this.originaltitle = value;
				this.localtitle = value;
			}
		}

		public void Save(string FilePath)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(MovieNFO));
			System.IO.TextWriter textWriter = new System.IO.StreamWriter(FilePath);
			xmlSerializer.Serialize(textWriter, this);
			textWriter.Close();
		}
	}
}
