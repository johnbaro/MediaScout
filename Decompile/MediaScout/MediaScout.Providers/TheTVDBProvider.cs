using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace MediaScout.Providers
{
	public class TheTVDBProvider : ITVMetadataProvider, IMetadataProvider
	{
		private const string APIKey = "4AD667B666AA62FA";

		public MediaScoutMessage.Message Message;

		private int level;

		private string urlSeriesID = "http://www.thetvdb.com/api/GetSeries.php?seriesname=";

		private string urlMetadata = "http://www.thetvdb.com/api/4AD667B666AA62FA/series/";

		private string urlPoster = "http://thetvdb.com/banners/";

		private string defaultLanguage = "en";

		public string defaultCacheDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\MediaScout\\Cache\\TVCache\\";

		public System.DateTime dtDefaultCache = System.DateTime.Now.Subtract(new System.TimeSpan(14, 0, 0, 0));

		public string name
		{
			get
			{
				return "TheTVDB";
			}
		}

		public string version
		{
			get
			{
				return "2.1";
			}
		}

		public string url
		{
			get
			{
				return "http://www.thetvdb.com";
			}
		}

		public TheTVDBProvider(MediaScoutMessage.Message Message)
		{
			this.Message = Message;
		}

		public TVShowXML[] Search(string Name, string Year)
		{
			return this.Search(Name, Year, this.defaultLanguage);
		}

		public TVShowXML[] Search(string Name, string Year, string Language)
		{
			Name = Name.Replace("&", "");
			Name = Name.Replace("and", "");
			if (this.Message != null)
			{
				this.Message("Seaching TVDb for " + Name + ((!string.IsNullOrEmpty(Year)) ? (", Year- " + Year) : ""), MediaScoutMessage.MessageType.Task, this.level);
			}
			string str = Name + " " + Year;
			XmlDocument xmlDocument = new XmlDocument();
			System.Collections.Generic.List<TVShowXML> list = new System.Collections.Generic.List<TVShowXML>();
			try
			{
				xmlDocument.Load(this.urlSeriesID + str + "&language=" + Language);
				XmlNode documentElement = xmlDocument.DocumentElement;
				XmlNodeList xmlNodeList = documentElement.SelectNodes("/Data/Series");
				for (int i = 0; i < xmlNodeList.Count; i++)
				{
					TVShowXML tVShowXML = new TVShowXML();
					if (xmlNodeList[i]["seriesid"] != null)
					{
						tVShowXML.SeriesID = xmlNodeList[i]["seriesid"].InnerText;
					}
					if (xmlNodeList[i]["SeriesName"] != null)
					{
						tVShowXML.SeriesName = xmlNodeList[i]["SeriesName"].InnerText;
					}
					if (xmlNodeList[i]["Overview"] != null)
					{
						tVShowXML.Overview = xmlNodeList[i]["Overview"].InnerText;
					}
					if (xmlNodeList[i]["id"] != null)
					{
						tVShowXML.id = xmlNodeList[i]["id"].InnerText;
					}
					Posters[] posters = this.GetPosters(tVShowXML.id, TVShowPosterType.Poster, -1);
					if (posters != null && posters.Length > 0)
					{
						tVShowXML.PosterThumb = posters[0].Thumb;
					}
					else
					{
						tVShowXML.PosterThumb = "";
					}
					if (xmlNodeList[i].SelectSingleNode("FirstAired") != null && !string.IsNullOrEmpty(xmlNodeList[i].SelectSingleNode("FirstAired").InnerText))
					{
						tVShowXML.Year = xmlNodeList[i].SelectSingleNode("FirstAired").InnerText.Substring(0, 4);
					}
					list.Add(tVShowXML);
				}
				if (list.Count > 0)
				{
					if (this.Message != null)
					{
						this.Message(list.Count + " Found", MediaScoutMessage.MessageType.TaskResult, this.level);
					}
					return list.ToArray();
				}
				if (this.Message != null)
				{
					this.Message("Nothing Found", MediaScoutMessage.MessageType.TaskError, this.level);
				}
			}
			catch (System.Exception ex)
			{
				if (this.Message != null)
				{
					this.Message(ex.Message, MediaScoutMessage.MessageType.TaskError, this.level);
				}
			}
			return null;
		}

		public TVShowXML GetTVShow(string TVShowID)
		{
			return this.GetTVShow(TVShowID, this.defaultLanguage, this.dtDefaultCache, this.level);
		}

		public TVShowXML GetTVShow(string TVShowID, string Language)
		{
			return this.GetTVShow(TVShowID, Language, this.dtDefaultCache, this.level);
		}

		public TVShowXML GetTVShow(string TVShowID, System.DateTime dtCacheTime, int level)
		{
			return this.GetTVShow(TVShowID, this.defaultLanguage, dtCacheTime, level);
		}

		public TVShowXML GetTVShow(string TVShowID, string Language, System.DateTime dtCacheTime, int level)
		{
			if (level == -1)
			{
				level = this.level;
			}
			XmlDocument xmlDocument = new XmlDocument();
			if (Language == null)
			{
				Language = this.defaultLanguage;
			}
			TVShowXML tVShowXML;
			try
			{
				tVShowXML = new TVShowXML();
				if (System.IO.File.Exists(this.defaultCacheDir + "\\" + TVShowID + ".xml") && System.DateTime.Compare(System.IO.File.GetLastWriteTime(this.defaultCacheDir + "\\" + TVShowID + ".xml"), dtCacheTime) > 0)
				{
					if (this.Message != null)
					{
						this.Message("Loading from cache", MediaScoutMessage.MessageType.Task, level);
					}
					xmlDocument.Load(this.defaultCacheDir + "\\" + TVShowID + ".xml");
					tVShowXML.LoadedFromCache = true;
				}
				else
				{
					if (this.Message != null)
					{
						this.Message("Fetching Metadata", MediaScoutMessage.MessageType.Task, level);
					}
					xmlDocument.Load(string.Concat(new string[]
					{
						this.urlMetadata,
						TVShowID,
						"/all/",
						Language,
						".xml"
					}));
					tVShowXML.LoadedFromCache = false;
				}
				XmlNode documentElement = xmlDocument.DocumentElement;
				XmlNodeList xmlNodeList = documentElement.SelectNodes("/Data/Series");
				TVShowXML arg_122_0 = tVShowXML;
				tVShowXML.ID = TVShowID;
				arg_122_0.SeriesID = TVShowID;
				tVShowXML.SeriesName = xmlNodeList[0].SelectSingleNode("SeriesName").InnerText;
				tVShowXML.PosterThumb = xmlNodeList[0].SelectSingleNode("poster").InnerText;
				if (xmlNodeList[0].SelectSingleNode("Network").InnerText != null)
				{
					tVShowXML.Network = xmlNodeList[0].SelectSingleNode("Network").InnerText;
				}
				if (xmlNodeList[0].SelectSingleNode("Rating").InnerText != null)
				{
					tVShowXML.Rating = xmlNodeList[0].SelectSingleNode("Rating").InnerText;
				}
				if (xmlNodeList[0].SelectSingleNode("Overview").InnerText != null)
				{
					tVShowXML.Overview = xmlNodeList[0].SelectSingleNode("Overview").InnerText;
				}
				if (xmlNodeList[0].SelectSingleNode("Runtime").InnerText != null)
				{
					tVShowXML.Runtime = xmlNodeList[0].SelectSingleNode("Runtime").InnerText;
				}
				if (xmlNodeList[0].SelectSingleNode("Genre").InnerText != null)
				{
					tVShowXML.Genre = xmlNodeList[0].SelectSingleNode("Genre").InnerText;
				}
				if (xmlNodeList[0].SelectSingleNode("FirstAired").InnerText != null)
				{
					tVShowXML.FirstAired = xmlNodeList[0].SelectSingleNode("FirstAired").InnerText;
				}
				if (xmlNodeList[0].SelectSingleNode("ContentRating").InnerText != null)
				{
					tVShowXML.ContentRating = xmlNodeList[0].SelectSingleNode("ContentRating").InnerText;
				}
				if (xmlNodeList[0].SelectSingleNode("Actors").InnerText != null)
				{
					tVShowXML.Actors = xmlNodeList[0].SelectSingleNode("Actors").InnerText;
				}
				if (xmlNodeList[0].SelectSingleNode("FirstAired") != null && !string.IsNullOrEmpty(xmlNodeList[0].SelectSingleNode("FirstAired").InnerText))
				{
					tVShowXML.Year = xmlNodeList[0].SelectSingleNode("FirstAired").InnerText.Substring(0, 4);
				}
				xmlNodeList = documentElement.SelectNodes("/Data/Episode");
				tVShowXML.Persons = this.GetActors(tVShowXML.ID);
				foreach (XmlNode xmlNode in xmlNodeList)
				{
					int num = int.Parse(xmlNode["SeasonNumber"].InnerText);
					int key = int.Parse(xmlNode["EpisodeNumber"].InnerText);
					string innerText = xmlNode["filename"].InnerText;
					if (!tVShowXML.Seasons.ContainsKey(num))
					{
						tVShowXML.Seasons.Add(num, new Season(num, TVShowID));
					}
					if (!tVShowXML.Seasons[num].Episodes.ContainsKey(key))
					{
						EpisodeXML episodeXML = new EpisodeXML();
						episodeXML.EpisodeID = (episodeXML.ID = xmlNode["id"].InnerText);
						episodeXML.EpisodeNumber = key.ToString();
						episodeXML.EpisodeName = xmlNode["EpisodeName"].InnerText;
						episodeXML.SeasonID = xmlNode["seasonid"].InnerText;
						if (string.IsNullOrEmpty(innerText))
						{
							episodeXML.PosterUrl = "";
						}
						else
						{
							episodeXML.PosterUrl = this.urlPoster + innerText;
							episodeXML.PosterName = innerText.Substring(innerText.LastIndexOf("/") + 1);
						}
						episodeXML.FirstAired = xmlNode["FirstAired"].InnerText;
						episodeXML.ProductionCode = xmlNode["ProductionCode"].InnerText;
						episodeXML.Overview = xmlNode["Overview"].InnerText;
						episodeXML.SeasonNumber = num.ToString();
						episodeXML.GuestStars = xmlNode["GuestStars"].InnerText;
						episodeXML.Director = xmlNode["Director"].InnerText;
						episodeXML.Writer = xmlNode["Writer"].InnerText;
						episodeXML.Rating = xmlNode["Rating"].InnerText;
						episodeXML.LastUpdated = xmlNode["lastupdated"].InnerText;
						episodeXML.SeriesID = xmlNode["seriesid"].InnerText;
						tVShowXML.Seasons[num].Episodes.Add(key, episodeXML);
					}
				}
				if (this.Message != null)
				{
					this.Message("Done", MediaScoutMessage.MessageType.TaskResult, level);
				}
				if (!tVShowXML.LoadedFromCache || !System.IO.File.Exists(this.defaultCacheDir + "\\" + TVShowID + ".xml"))
				{
					if (this.Message != null)
					{
						this.Message("Caching Metadata", MediaScoutMessage.MessageType.Task, level);
					}
					if (!System.IO.Directory.Exists(this.defaultCacheDir))
					{
						System.IO.Directory.CreateDirectory(this.defaultCacheDir);
					}
					xmlDocument.Save(this.defaultCacheDir + "\\" + TVShowID + ".xml");
					if (this.Message != null)
					{
						this.Message("Done", MediaScoutMessage.MessageType.TaskResult, level);
					}
				}
			}
			catch (System.Exception ex)
			{
				tVShowXML = null;
				if (this.Message != null)
				{
					this.Message(ex.Message, MediaScoutMessage.MessageType.TaskError, level);
				}
			}
			return tVShowXML;
		}

		public EpisodeXML GetEpisode(string TVShowID, string SeasonID, string EpisodeID)
		{
			return this.GetEpisode(TVShowID, SeasonID, EpisodeID, this.defaultLanguage);
		}

		public EpisodeXML GetEpisode(string TVShowID, string SeasonID, string EpisodeID, string Language)
		{
			XmlDocument xmlDocument = new XmlDocument();
			EpisodeXML episodeXML = new EpisodeXML();
			if (Language == null)
			{
				Language = this.defaultLanguage;
			}
			try
			{
				if (this.Message != null)
				{
					this.Message("Fetching Episode Metadata", MediaScoutMessage.MessageType.Task, this.level);
				}
				xmlDocument.Load(string.Concat(new string[]
				{
					this.urlMetadata,
					TVShowID,
					"/default/",
					SeasonID,
					"/",
					EpisodeID,
					"/",
					Language,
					".xml"
				}));
				XmlNode documentElement = xmlDocument.DocumentElement;
				XmlNodeList xmlNodeList = documentElement.SelectNodes("/Data/Episode");
				episodeXML.EpisodeID = (episodeXML.ID = xmlNodeList[0].SelectSingleNode("id").InnerText);
				episodeXML.EpisodeName = xmlNodeList[0].SelectSingleNode("EpisodeName").InnerText;
				episodeXML.SeasonID = xmlNodeList[0].SelectSingleNode("seasonid").InnerText;
				episodeXML.EpisodeNumber = xmlNodeList[0].SelectSingleNode("EpisodeNumber").InnerText;
				episodeXML.FirstAired = xmlNodeList[0].SelectSingleNode("FirstAired").InnerText;
				episodeXML.GuestStars = xmlNodeList[0].SelectSingleNode("GuestStars").InnerText;
				episodeXML.Director = xmlNodeList[0].SelectSingleNode("Director").InnerText;
				episodeXML.Writer = xmlNodeList[0].SelectSingleNode("Writer").InnerText;
				episodeXML.Rating = xmlNodeList[0].SelectSingleNode("Rating").InnerText;
				episodeXML.Overview = xmlNodeList[0].SelectSingleNode("Overview").InnerText;
				episodeXML.ProductionCode = xmlNodeList[0].SelectSingleNode("ProductionCode").InnerText;
				episodeXML.LastUpdated = xmlNodeList[0].SelectSingleNode("lastupdated").InnerText;
				string innerText = xmlNodeList[0].SelectSingleNode("filename").InnerText;
				if (string.IsNullOrEmpty(innerText))
				{
					episodeXML.PosterUrl = "";
				}
				else
				{
					episodeXML.PosterUrl = this.urlPoster + innerText;
					episodeXML.PosterName = innerText.Substring(innerText.LastIndexOf("/") + 1);
				}
				episodeXML.SeriesID = xmlNodeList[0].SelectSingleNode("seriesid").InnerText;
				episodeXML.SeasonNumber = xmlNodeList[0].SelectSingleNode("SeasonNumber").InnerText;
				if (this.Message != null)
				{
					this.Message("Done", MediaScoutMessage.MessageType.TaskResult, this.level);
				}
			}
			catch (System.Exception ex)
			{
				episodeXML = null;
				if (this.Message != null)
				{
					this.Message(ex.Message, MediaScoutMessage.MessageType.TaskError, this.level);
				}
			}
			return episodeXML;
		}

		public Posters[] GetPosters(string TVShowID, TVShowPosterType type, int seasonNum)
		{
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(this.urlMetadata + TVShowID + "/banners.xml");
				XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("/Banners/Banner");
				System.Collections.Generic.List<Posters> list = new System.Collections.Generic.List<Posters>();
				string stringValue = StringEnum.GetStringValue(type);
				foreach (XmlNode xmlNode in xmlNodeList)
				{
					if (xmlNode.SelectSingleNode("BannerType").InnerText == stringValue && (type != TVShowPosterType.Season_Poster || !(xmlNode.SelectSingleNode("Season").InnerText != seasonNum.ToString())))
					{
						Posters item = new Posters
						{
							Poster = this.urlPoster + xmlNode.SelectSingleNode("BannerPath").InnerText,
							Thumb = (xmlNode.SelectSingleNode("ThumbnailPath") != null) ? (this.urlPoster + xmlNode.SelectSingleNode("ThumbnailPath").InnerText) : (this.urlPoster + xmlNode.SelectSingleNode("BannerPath").InnerText),
							Resolution = (xmlNode.SelectSingleNode("BannerType2") != null) ? xmlNode.SelectSingleNode("BannerType2").InnerText : null
						};
						list.Add(item);
					}
				}
				if (list.Count > 0)
				{
					return list.ToArray();
				}
			}
			catch (System.Exception ex)
			{
				if (this.Message != null)
				{
					this.Message(ex.Message, MediaScoutMessage.MessageType.TaskError, (seasonNum != -1) ? (this.level + 1) : this.level);
				}
			}
			return null;
		}

		public System.Collections.Generic.List<Person> GetActors(string TVShowID)
		{
			System.Collections.Generic.List<Person> list = new System.Collections.Generic.List<Person>();
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(this.urlMetadata + TVShowID + "/actors.xml");
				XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("/Actors/Actor");
				foreach (XmlNode xmlNode in xmlNodeList)
				{
					string name = null;
					string role = null;
					string thumb = null;
					if (xmlNode.SelectSingleNode("Name") != null)
					{
						name = xmlNode.SelectSingleNode("Name").InnerText;
					}
					if (xmlNode.SelectSingleNode("Role") != null && !string.IsNullOrEmpty(xmlNode.SelectSingleNode("Role").InnerText))
					{
						role = xmlNode.SelectSingleNode("Role").InnerText;
					}
					if (xmlNode.SelectSingleNode("Image") != null && !string.IsNullOrEmpty(xmlNode.SelectSingleNode("Image").InnerText))
					{
						thumb = this.urlPoster + xmlNode.SelectSingleNode("Image").InnerText;
					}
					Person item = new Person
					{
						Name = name,
						Type = "Actor",
						Role = role,
						Thumb = thumb
					};
					list.Add(item);
				}
			}
			catch (System.Exception ex)
			{
				if (this.Message != null)
				{
					this.Message(ex.Message, MediaScoutMessage.MessageType.TaskError, this.level);
				}
			}
			return list;
		}
	}
}
