using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace MediaScout
{
	[XmlRoot("episodedetails")]
	public class EpisodeNFO
	{
		public string id;

		public string title;

		public string rating;

		public string season;

		public string episode;

		public string plot;

		public string director;

		public string credits;

		public string aired;

		[XmlElement("actor")]
		public System.Collections.Generic.List<ActorsNFO> Actors = new System.Collections.Generic.List<ActorsNFO>();

		public void Save(string FilePath)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(EpisodeNFO));
			System.IO.TextWriter textWriter = new System.IO.StreamWriter(FilePath);
			xmlSerializer.Serialize(textWriter, this);
			textWriter.Close();
		}
	}
}
