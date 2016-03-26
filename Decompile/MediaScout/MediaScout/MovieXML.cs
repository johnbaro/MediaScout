using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace MediaScout
{
	[XmlRoot("Title")]
	public class MovieXML
	{
		[XmlIgnore]
		private string localtitle;

		public string OriginalTitle;

		public string SortTitle;

		public string RunningTime;

		public string IMDBrating;

		public string ProductionYear;

		[XmlElement("TMDbId")]
		public string ID;

		[XmlIgnore]
		private string description;

		public string MPAA;

		public System.Collections.Generic.List<Person> Persons = new System.Collections.Generic.List<Person>();

		public System.Collections.Generic.List<Genre> Genres = new System.Collections.Generic.List<Genre>();

		public System.Collections.Generic.List<Studio> Studios = new System.Collections.Generic.List<Studio>();

		[XmlIgnore]
		public bool LoadedFromCache;

		[XmlIgnore]
		public string posterthumb;

		[XmlIgnore]
		public string alt_title;

		[XmlIgnore]
		public string Tagline;

		public string LocalTitle
		{
			get
			{
				return this.localtitle;
			}
			set
			{
				this.localtitle = value;
			}
		}

		public string Description
		{
			get
			{
				return this.description;
			}
			set
			{
				this.description = value;
			}
		}

		[XmlIgnore]
		public string Rating
		{
			get
			{
				return this.IMDBrating;
			}
			set
			{
				this.IMDBrating = value;
			}
		}

		[XmlIgnore]
		public string Year
		{
			get
			{
				return this.ProductionYear;
			}
			set
			{
				this.ProductionYear = value;
			}
		}

		[XmlIgnore]
		public string Length
		{
			get
			{
				return this.RunningTime;
			}
			set
			{
				this.RunningTime = value;
			}
		}

		[XmlIgnore]
		public string TMDbId
		{
			get
			{
				return this.ID;
			}
			set
			{
				this.ID = value;
			}
		}

		[XmlIgnore]
		public string PosterThumb
		{
			get
			{
				return this.posterthumb;
			}
			set
			{
				this.posterthumb = value;
			}
		}

		[XmlIgnore]
		public string Title
		{
			get
			{
				return this.OriginalTitle;
			}
			set
			{
				this.OriginalTitle = value;
				this.SortTitle = value;
				this.LocalTitle = value;
			}
		}

		[XmlIgnore]
		public string Alt_Title
		{
			get
			{
				return this.alt_title;
			}
			set
			{
				this.alt_title = value;
			}
		}

		public string GetXBMCThumbFilename(string FileName)
		{
			return FileName + ".tbn";
		}

		public string GetXBMCThumbFile(string Directory, string FileName)
		{
			return Directory + "\\" + this.GetXBMCThumbFilename(FileName);
		}

		public string GetXBMCBackdropFilename(string FileName)
		{
			return FileName + "_fanart.jpg";
		}

		public string GetXBMCBackdropFile(string Directory, string FileName)
		{
			return Directory + "\\" + this.GetXBMCBackdropFilename(FileName);
		}

		public string GetXMLFilename()
		{
			return "mymovies.xml";
		}

		public string GetNFOFilename()
		{
			return "movie.nfo";
		}

		public string GetXMLFile(string Directory)
		{
			return Directory + "\\" + this.GetXMLFilename();
		}

		public string GetNFOFile(string Directory)
		{
			return Directory + "\\" + this.GetNFOFilename();
		}

		public void SaveXML(string FolderPath)
		{
			string xMLFile = this.GetXMLFile(FolderPath);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(MovieXML));
			System.IO.TextWriter textWriter = new System.IO.StreamWriter(xMLFile);
			xmlSerializer.Serialize(textWriter, this);
			textWriter.Close();
		}

		public void SaveNFO(string FolderPath)
		{
			MovieNFO movieNFO = new MovieNFO();
			movieNFO.title = this.Title;
			movieNFO.rating = this.Rating;
			movieNFO.year = this.Year;
			movieNFO.plot = this.Description;
			movieNFO.tagline = this.Tagline;
			movieNFO.runtime = this.Length;
			movieNFO.id = this.ID;
			movieNFO.mpaa = this.MPAA;
			movieNFO.studio = this.BuildStringFromStudioList(this.Studios);
			movieNFO.genre = this.BuildStringFromGenreList(this.Genres);
			movieNFO.director = this.BuildStringFromPersonsList(this.Persons, "Director");
			movieNFO.credits = this.BuildStringFromPersonsList(this.Persons, "Writer");
			foreach (Person current in this.Persons)
			{
				if (current.Type == "Actor")
				{
					movieNFO.Actors.Add(new ActorsNFO
					{
						name = current.Name,
						role = current.Role,
						thumb = current.Thumb
					});
				}
			}
			movieNFO.Save(this.GetNFOFile(FolderPath));
		}

		public string BuildStringFromGenreList(System.Collections.Generic.List<Genre> Genres)
		{
			string text = null;
			if (Genres != null && Genres.Count > 0)
			{
				text = "";
				foreach (Genre current in Genres)
				{
					text = text + "|" + current.name + "|";
				}
			}
			return text;
		}

		public string BuildStringFromGenreArray(System.Collections.ObjectModel.Collection<Genre> Array)
		{
			string text = null;
			if (Array != null && Array.Count > 0)
			{
				text = "";
				foreach (Genre current in Array)
				{
					text = text + "|" + current.name + "|";
				}
			}
			return text;
		}

		public string BuildStringFromStudioList(System.Collections.Generic.List<Studio> Studios)
		{
			string text = null;
			if (Studios != null && Studios.Count > 0)
			{
				text = "";
				foreach (Studio current in Studios)
				{
					text = text + "|" + current.name + "|";
				}
			}
			return text;
		}

		public string BuildStringFromStudioArray(System.Collections.ObjectModel.Collection<Studio> Array)
		{
			string text = null;
			if (Array != null && Array.Count > 0)
			{
				text = "";
				foreach (Studio current in Array)
				{
					text = text + "|" + current.name + "|";
				}
			}
			return text;
		}

		public string BuildStringFromPersonsList(System.Collections.Generic.List<Person> Persons, string Type)
		{
			string text = null;
			if (Persons != null && Persons.Count > 0)
			{
				text = "";
				foreach (Person current in Persons)
				{
					if (current.Type == Type)
					{
						text = text + "|" + current.Name + "|";
					}
				}
			}
			return text;
		}

		public System.Collections.Generic.List<Genre> BuildGenreListFromGenreArray(System.Collections.ObjectModel.Collection<Genre> Array)
		{
			System.Collections.Generic.List<Genre> list = null;
			if (Array != null && Array.Count > 0)
			{
				list = new System.Collections.Generic.List<Genre>();
				foreach (Genre current in Array)
				{
					list.Add(new Genre
					{
						name = current.name
					});
				}
			}
			return list;
		}

		public System.Collections.ObjectModel.Collection<Genre> BuildGenreArrayFromString(string res)
		{
			System.Collections.ObjectModel.Collection<Genre> collection = new System.Collections.ObjectModel.Collection<Genre>();
			if (!string.IsNullOrEmpty(res))
			{
				if (res.IndexOf('|') >= 0)
				{
					string[] array = res.Split(new char[]
					{
						'|'
					});
					for (int i = 0; i < array.Length; i++)
					{
						string text = array[i];
						if (!string.IsNullOrEmpty(text))
						{
							collection.Add(new Genre
							{
								name = text.Trim()
							});
						}
					}
				}
				else
				{
					string[] array2 = res.Split(new char[]
					{
						'/'
					});
					for (int j = 0; j < array2.Length; j++)
					{
						string text2 = array2[j];
						if (!string.IsNullOrEmpty(text2))
						{
							collection.Add(new Genre
							{
								name = text2.Trim()
							});
						}
					}
				}
			}
			return collection;
		}

		public System.Collections.Generic.List<Studio> BuildStudioListFromStudioArray(System.Collections.ObjectModel.Collection<Studio> Array)
		{
			System.Collections.Generic.List<Studio> list = null;
			if (Array != null && Array.Count > 0)
			{
				list = new System.Collections.Generic.List<Studio>();
				foreach (Studio current in Array)
				{
					list.Add(new Studio
					{
						name = current.name
					});
				}
			}
			return list;
		}

		public System.Collections.ObjectModel.Collection<Studio> BuildStudioArrayFromString(string res)
		{
			System.Collections.ObjectModel.Collection<Studio> collection = new System.Collections.ObjectModel.Collection<Studio>();
			if (!string.IsNullOrEmpty(res))
			{
				if (res.IndexOf('|') >= 0)
				{
					string[] array = res.Split(new char[]
					{
						'|'
					});
					for (int i = 0; i < array.Length; i++)
					{
						string text = array[i];
						if (!string.IsNullOrEmpty(text))
						{
							collection.Add(new Studio
							{
								name = text.Trim()
							});
						}
					}
				}
				else
				{
					string[] array2 = res.Split(new char[]
					{
						'/'
					});
					for (int j = 0; j < array2.Length; j++)
					{
						string text2 = array2[j];
						if (!string.IsNullOrEmpty(text2))
						{
							collection.Add(new Studio
							{
								name = text2.Trim()
							});
						}
					}
				}
			}
			return collection;
		}

		public override string ToString()
		{
			return this.Title;
		}
	}
}
