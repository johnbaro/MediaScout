using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace MediaScout
{
	[XmlRoot("Series")]
	public class TVShowXML
	{
		public string id;

		public string Actors;

		public string ContentRating;

		public string FirstAired;

		public string Genre;

		public string IMDB_ID;

		public string Language;

		public string Network;

		public string Rating;

		public string Runtime;

		public string SeriesID;

		public string Status;

		[XmlIgnore]
		private string overview;

		[XmlIgnore]
		private string seriesName;

		[XmlIgnore]
		public System.Collections.Generic.List<Person> Persons = new System.Collections.Generic.List<Person>();

		[XmlIgnore]
		public bool LoadedFromCache;

		[XmlIgnore]
		public SortedList<int, Season> Seasons = new SortedList<int, Season>();

		[XmlIgnore]
		public string posterthumb;

		[XmlIgnore]
		private string year;

		[XmlIgnore]
		public string Tagline;

		public string Overview
		{
			get
			{
				return this.overview;
			}
			set
			{
				this.overview = value;
			}
		}

		public string SeriesName
		{
			get
			{
				return this.seriesName;
			}
			set
			{
				this.seriesName = value;
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
		public string ID
		{
			get
			{
				return this.id;
			}
			set
			{
				this.id = value;
			}
		}

		[XmlIgnore]
		public string Title
		{
			get
			{
				return this.SeriesName;
			}
			set
			{
				this.SeriesName = value;
			}
		}

		[XmlIgnore]
		public string Description
		{
			get
			{
				return this.Overview;
			}
			set
			{
				this.Overview = value;
			}
		}

		[XmlIgnore]
		public string Year
		{
			get
			{
				return this.year;
			}
			set
			{
				this.year = value;
			}
		}

		public string GetXMLFilename()
		{
			return "series.xml";
		}

		public string GetNFOFileName()
		{
			return "tvshow.nfo";
		}

		public string GetXMLFile(string Directory)
		{
			return Directory + "\\" + this.GetXMLFilename();
		}

		public string GetNFOFile(string Directory)
		{
			return Directory + "\\" + this.GetNFOFileName();
		}

		public void SaveXML(string Folderpath)
		{
			string path = Folderpath + "\\series.xml";
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(TVShowXML));
			System.IO.TextWriter textWriter = new System.IO.StreamWriter(path);
			xmlSerializer.Serialize(textWriter, this);
			textWriter.Close();
		}

		public void SaveNFO(string Folderpath)
		{
			TVShowNFO tVShowNFO = new TVShowNFO();
			tVShowNFO.title = this.Title;
			tVShowNFO.rating = this.Rating;
			tVShowNFO.year = this.Year;
			tVShowNFO.plot = this.Description;
			tVShowNFO.tagline = this.Tagline;
			tVShowNFO.runtime = this.Runtime;
			tVShowNFO.mpaa = this.ContentRating;
			tVShowNFO.id = this.ID;
			tVShowNFO.premiered = this.FirstAired;
			tVShowNFO.status = this.Status;
			tVShowNFO.studio = this.Network;
			tVShowNFO.genre = this.Genre;
			foreach (Person current in this.Persons)
			{
				tVShowNFO.Actors.Add(new ActorsNFO
				{
					name = current.Name,
					role = current.Role,
					thumb = current.Thumb
				});
			}
			tVShowNFO.Save(this.GetNFOFile(Folderpath));
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
