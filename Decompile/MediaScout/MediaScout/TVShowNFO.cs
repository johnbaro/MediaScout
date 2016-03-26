using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace MediaScout
{
	[XmlRoot("tvshow")]
	public class TVShowNFO
	{
		public string title;

		public string rating;

		public string year;

		public string top250;

		public string season;

		public string episode;

		public string votes;

		public string outline;

		public string plot;

		public string tagline;

		public string runtime;

		public string mpaa;

		public string id;

		public string set;

		public string aired;

		public string status;

		public string code;

		public string studio;

		public string genre;

		public string director;

		public string credits;

		[XmlElement("actor")]
		public System.Collections.Generic.List<ActorsNFO> Actors = new System.Collections.Generic.List<ActorsNFO>();

		public string trailer;

		public string premiered
		{
			get
			{
				return this.aired;
			}
			set
			{
				this.aired = value;
			}
		}

		public void Save(string FilePath)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(TVShowNFO));
			System.IO.TextWriter textWriter = new System.IO.StreamWriter(FilePath);
			xmlSerializer.Serialize(textWriter, this);
			textWriter.Close();
		}
	}
}
