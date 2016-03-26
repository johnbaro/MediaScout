using System;
using System.Xml.Serialization;

namespace MediaScout
{
	public class Person
	{
		public string Name;

		public string Type;

		public string Role;

		[XmlIgnore]
		public string Thumb;

		[XmlIgnore]
		public string ID;

		public string GetXBMCFilename()
		{
			return this.Name.Replace(" ", "_") + ".jpg";
		}

		public string GetXBMCDirectory()
		{
			return ".actors";
		}

		public string GetMyMoviesFilename()
		{
			return "folder.jpg";
		}

		public string GetMyMoviesDirectory()
		{
			return this.Name;
		}

		public void SaveThumb(string Filepath)
		{
			Posters posters = new Posters
			{
				Poster = this.Thumb
			};
			posters.SavePoster(Filepath);
		}
	}
}
