using System;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace MediaScout
{
	[XmlRoot("Item")]
	public class EpisodeXML
	{
		public string ID;

		public string Director;

		public string EpisodeID;

		public string EpisodeName;

		public string EpisodeNumber;

		public string FirstAired;

		public string GuestStars;

		public string Language;

		public string Overview;

		public string ProductionCode;

		public string Writer;

		public string SeasonNumber;

		public string SeasonID;

		public string SeriesID;

		public string LastUpdated;

		[XmlIgnore]
		public string PosterUrl;

		[XmlElement("filename")]
		public string PosterName;

		[XmlIgnore]
		public Image Poster;

		[XmlIgnore]
		public string BannerUrl;

		[XmlIgnore]
		public Image Banner;

		[XmlIgnore]
		public string Rating;

		public string GetMetadataFolder(string Directory)
		{
			return Directory + "\\metadata";
		}

		public string GetXBMCThumbFilename(string FileName)
		{
			return FileName + ".tbn";
		}

		public string GetMyMoviesThumbFilename()
		{
			string result;
			if (this.PosterName != null)
			{
				result = this.PosterName;
			}
			else
			{
				result = this.EpisodeID + ".jpg";
			}
			return result;
		}

		public string GetXBMCThumbFile(string Directory, string FileName)
		{
			return Directory + "\\" + this.GetXBMCThumbFilename(FileName);
		}

		public string GetMyMoviesThumbFile(string Directory)
		{
			return this.GetMetadataFolder(Directory) + "\\" + this.GetMyMoviesThumbFilename();
		}

		public string GetXMLFilename(string FileName)
		{
			return FileName + ".xml";
		}

		public string GetNFOFileName(string FileName)
		{
			return FileName + ".nfo";
		}

		public string GetXMLFile(string Directory, string FileName)
		{
			return this.GetMetadataFolder(Directory) + "\\" + this.GetXMLFilename(FileName);
		}

		public string GetNFOFile(string Directory, string FileName)
		{
			return Directory + "\\" + this.GetNFOFileName(FileName);
		}

		public void SaveXML(string FolderPath, string Filename)
		{
			string xMLFile = this.GetXMLFile(FolderPath, Filename);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(EpisodeXML));
			System.IO.TextWriter textWriter = new System.IO.StreamWriter(xMLFile);
			xmlSerializer.Serialize(textWriter, this);
			textWriter.Close();
		}

		public void SaveNFO(string FolderPath, string Filename)
		{
			new EpisodeNFO
			{
				id = this.ID,
				title = this.EpisodeName,
				rating = this.Rating,
				season = this.SeasonNumber,
				episode = this.EpisodeNumber,
				plot = this.Overview,
				aired = this.FirstAired,
				credits = this.Writer,
				director = this.Director
			}.Save(this.GetNFOFile(FolderPath, Filename));
		}
	}
}
