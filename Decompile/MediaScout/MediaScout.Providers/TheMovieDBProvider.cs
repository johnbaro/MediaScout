using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace MediaScout.Providers
{
	public class TheMovieDBProvider : IMovieMetadataProvider, IMetadataProvider
	{
		public MediaScoutMessage.Message Message;

		private int level;

		private static string APIKey = "1a9efd23fff9c2ed07c90358e2b3d280";

		private static string osUri = "http://a9.com/-/spec/opensearch/1.1/";

		private string defaultLanguage = "en";

		private string defaultCacheDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\MediaScout\\Cache\\MovieCache\\";

		private System.DateTime dtDefaultCache = System.DateTime.Now.Subtract(new System.TimeSpan(14, 0, 0, 0));

		private string urlSearchPerson = string.Format("http://api.themoviedb.org/2.1/Person.search/{0}/xml/{1}/", "en", TheMovieDBProvider.APIKey);

		private string urlPersonInfo = string.Format("http://api.themoviedb.org/2.1/Person.getInfo/{0}/xml/{1}/", "en", TheMovieDBProvider.APIKey);

		string IMetadataProvider.name
		{
			get
			{
				return "TheMovieDB";
			}
		}

		string IMetadataProvider.version
		{
			get
			{
				return "2.1";
			}
		}

		string IMetadataProvider.url
		{
			get
			{
				return "";
			}
		}

		public TheMovieDBProvider(MediaScoutMessage.Message Message)
		{
			this.Message = Message;
		}

		private string SearchURL(string Language)
		{
			return string.Format("http://api.themoviedb.org/2.1/Movie.search/{0}/xml/{1}/", Language, TheMovieDBProvider.APIKey);
		}

		private string InfoURL(string Language)
		{
			return string.Format("http://api.themoviedb.org/2.1/Movie.getInfo/{0}/xml/{1}/", Language, TheMovieDBProvider.APIKey);
		}

		public MovieXML[] Search(string Name, string Year)
		{
			return this.Search(Name, Year, this.defaultLanguage);
		}

		public MovieXML[] Search(string Name, string Year, string Language)
		{
			Name = Name.Replace("&", "");
			Name = Name.Replace("and", "");
			if (this.Message != null)
			{
				this.Message("Seaching TMDb for " + Name + ((!string.IsNullOrEmpty(Year)) ? (", Year-" + Year) : ""), MediaScoutMessage.MessageType.Task, this.level);
			}
			Name = Name.Replace(" ", "+");
			string str = Name + " " + Year;
			XmlDocument xmlDocument = new XmlDocument();
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
			xmlNamespaceManager.AddNamespace("opensearch", TheMovieDBProvider.osUri);
			System.Collections.Generic.List<MovieXML> list = new System.Collections.Generic.List<MovieXML>();
			try
			{
				xmlDocument.Load(this.SearchURL(Language) + str);
				XmlNode documentElement = xmlDocument.DocumentElement;
				XmlNodeList xmlNodeList = documentElement.SelectNodes("./movies/movie");
				for (int i = 0; i < xmlNodeList.Count; i++)
				{
					MovieXML movieXML = new MovieXML();
					if (xmlNodeList[i]["name"] != null)
					{
						movieXML.Title = xmlNodeList[i]["name"].InnerText;
					}
					if (xmlNodeList[i]["alternative_name"] != null)
					{
						movieXML.Alt_Title = xmlNodeList[i]["alternative_name"].InnerText;
					}
					if (xmlNodeList[i]["id"] != null)
					{
						movieXML.ID = xmlNodeList[i]["id"].InnerText;
					}
					if (xmlNodeList[i]["overview"] != null)
					{
						movieXML.Description = xmlNodeList[i]["overview"].InnerText;
					}
					if (xmlNodeList[i].SelectSingleNode("./images/image[@type='poster'][@size='thumb']") != null)
					{
						movieXML.PosterThumb = xmlNodeList[i].SelectSingleNode("./images/image[@type='poster'][@size='thumb']").Attributes["url"].Value;
					}
					if (xmlNodeList[i].SelectSingleNode("released") != null && !string.IsNullOrEmpty(xmlNodeList[i].SelectSingleNode("released").InnerText))
					{
						movieXML.ProductionYear = xmlNodeList[i].SelectSingleNode("released").InnerText.Substring(0, 4);
					}
					list.Add(movieXML);
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

		public MovieXML Get(string MovieID)
		{
			return this.Get(MovieID, this.defaultLanguage, this.dtDefaultCache);
		}

		public MovieXML Get(string MovieID, string Language)
		{
			return this.Get(MovieID, this.defaultLanguage, this.dtDefaultCache);
		}

		public MovieXML Get(string MovieID, string Language, System.DateTime dtCacheTime)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
			xmlNamespaceManager.AddNamespace("opensearch", TheMovieDBProvider.osUri);
			MovieXML movieXML;
			try
			{
				movieXML = new MovieXML();
				if (System.IO.File.Exists(this.defaultCacheDir + "\\" + MovieID + ".xml") && System.DateTime.Compare(System.IO.File.GetLastWriteTime(this.defaultCacheDir + "\\" + MovieID + ".xml"), dtCacheTime) > 0)
				{
					if (this.Message != null)
					{
						this.Message("Loading from cache", MediaScoutMessage.MessageType.Task, this.level);
					}
					xmlDocument.Load(this.defaultCacheDir + "\\" + MovieID + ".xml");
					movieXML.LoadedFromCache = true;
				}
				else
				{
					if (this.Message != null)
					{
						this.Message("Fetching Metadata", MediaScoutMessage.MessageType.Task, this.level);
					}
					xmlDocument.Load(this.InfoURL(Language) + MovieID);
					movieXML.LoadedFromCache = false;
				}
				XmlNode documentElement = xmlDocument.DocumentElement;
				XmlNodeList xmlNodeList = documentElement.SelectNodes("./movies/movie");
				if (xmlNodeList[0].FirstChild == null)
				{
					throw new System.Exception("no results");
				}
				movieXML.Title = xmlNodeList[0].SelectSingleNode("name").InnerText;
				movieXML.ID = ((xmlNodeList[0].SelectSingleNode("id") == null) ? xmlNodeList[0].SelectSingleNode("TMDbId").InnerText : xmlNodeList[0].SelectSingleNode("id").InnerText);
				if (xmlNodeList[0].SelectSingleNode("released") != null && !string.IsNullOrEmpty(xmlNodeList[0].SelectSingleNode("released").InnerText))
				{
					movieXML.ProductionYear = xmlNodeList[0].SelectSingleNode("released").InnerText.Substring(0, 4);
				}
				if (xmlNodeList[0].SelectSingleNode("overview") != null)
				{
					movieXML.Description = xmlNodeList[0].SelectSingleNode("overview").InnerText;
				}
				if (xmlNodeList[0].SelectSingleNode("runtime") != null)
				{
					movieXML.RunningTime = xmlNodeList[0].SelectSingleNode("runtime").InnerText;
				}
				if (xmlNodeList[0].SelectSingleNode("rating") != null)
				{
					movieXML.Rating = xmlNodeList[0].SelectSingleNode("rating").InnerText;
				}
				if (xmlNodeList[0].SelectSingleNode("tagline") != null)
				{
					movieXML.Tagline = xmlNodeList[0].SelectSingleNode("tagline").InnerText;
				}
				if (xmlNodeList[0].SelectSingleNode("alternative_name") != null)
				{
					movieXML.Alt_Title = xmlNodeList[0].SelectSingleNode("alternative_name").InnerText;
				}
				if (xmlNodeList[0].SelectSingleNode("certification") != null)
				{
					movieXML.MPAA = xmlNodeList[0].SelectSingleNode("certification").InnerText;
				}
				XmlNodeList xmlNodeList2 = xmlNodeList[0].SelectNodes("./categories/category");
				foreach (XmlNode xmlNode in xmlNodeList2)
				{
					movieXML.Genres.Add(new Genre
					{
						name = xmlNode.Attributes["name"].Value
					});
				}
				XmlNodeList xmlNodeList3 = xmlNodeList[0].SelectNodes("./studios/studio");
				foreach (XmlNode xmlNode2 in xmlNodeList3)
				{
					movieXML.Studios.Add(new Studio
					{
						name = xmlNode2.Attributes["name"].Value
					});
				}
				XmlNodeList xmlNodeList4 = xmlNodeList[0].SelectNodes("./cast/person");
				foreach (XmlNode xmlNode3 in xmlNodeList4)
				{
					movieXML.Persons.Add(new Person
					{
						ID = xmlNode3.Attributes["id"].Value,
						Name = xmlNode3.Attributes["name"].Value,
						Type = xmlNode3.Attributes["job"].Value,
						Role = (xmlNode3.Attributes["name"].Value == null) ? "" : xmlNode3.Attributes["character"].Value.Trim(),
						Thumb = xmlNode3.Attributes["thumb"].Value
					});
				}
				if (this.Message != null)
				{
					this.Message("Done", MediaScoutMessage.MessageType.TaskResult, this.level);
				}
				if (!System.IO.File.Exists(this.defaultCacheDir + "\\" + MovieID + ".xml"))
				{
					if (this.Message != null)
					{
						this.Message("Caching Metadata", MediaScoutMessage.MessageType.Task, this.level);
					}
					if (!System.IO.Directory.Exists(this.defaultCacheDir))
					{
						System.IO.Directory.CreateDirectory(this.defaultCacheDir);
					}
					xmlDocument.Save(this.defaultCacheDir + "\\" + MovieID + ".xml");
					if (this.Message != null)
					{
						this.Message("Done", MediaScoutMessage.MessageType.TaskResult, this.level);
					}
				}
			}
			catch (System.Exception ex)
			{
				movieXML = null;
				if (this.Message != null)
				{
					this.Message(ex.Message, MediaScoutMessage.MessageType.TaskError, this.level);
				}
			}
			return movieXML;
		}

		public Posters[] GetPosters(string MovieID, MoviePosterType type)
		{
			return this.GetPosters(MovieID, this.defaultLanguage, type);
		}

		public Posters[] GetPosters(string MovieID, string Language, MoviePosterType type)
		{
			try
			{
				string xpath = string.Format("./images/image[@type='{0}'][@size='original']", StringEnum.GetStringValue(type));
				string str = string.Format("./images/image[@type='{0}'][@size='thumb']", StringEnum.GetStringValue(type));
				XmlDocument xmlDocument = new XmlDocument();
				XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
				xmlNamespaceManager.AddNamespace("opensearch", TheMovieDBProvider.osUri);
				xmlDocument.Load(this.InfoURL(Language) + MovieID);
				XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("./movies/movie");
				System.Collections.Generic.List<Posters> list = new System.Collections.Generic.List<Posters>();
				XmlNodeList xmlNodeList2 = xmlNodeList[0].SelectNodes(xpath);
				foreach (XmlNode xmlNode in xmlNodeList2)
				{
					string value = xmlNode.Attributes["url"].Value;
					string resolution = xmlNode.Attributes["width"].Value + "x" + xmlNode.Attributes["height"].Value;
					string value2 = xmlNodeList[0].SelectSingleNode(string.Format(str + "[@id='{0}']", xmlNode.Attributes["id"].Value)).Attributes["url"].Value;
					Posters item = new Posters
					{
						Poster = value,
						Thumb = value2,
						Resolution = resolution
					};
					list.Add(item);
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
					this.Message(ex.Message, MediaScoutMessage.MessageType.TaskError, this.level);
				}
			}
			return null;
		}

		public System.Collections.Generic.List<Person> GetActors(string MovieID)
		{
			return this.GetActors(MovieID, this.defaultLanguage);
		}

		public System.Collections.Generic.List<Person> GetActors(string MovieID, string Language)
		{
			System.Collections.Generic.List<Person> list = new System.Collections.Generic.List<Person>();
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(this.InfoURL(Language) + MovieID);
				XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("./movies/movie/cast/person");
				foreach (XmlNode xmlNode in xmlNodeList)
				{
					if (xmlNode.Attributes["job"].Value == "Actor")
					{
						Person item = new Person
						{
							ID = xmlNode.Attributes["id"].Value,
							Name = xmlNode.Attributes["name"].Value,
							Type = xmlNode.Attributes["job"].Value,
							Role = (xmlNode.Attributes["name"].Value == null) ? "" : xmlNode.Attributes["character"].Value.Trim(),
							Thumb = xmlNode.Attributes["thumb"].Value
						};
						list.Add(item);
					}
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

		public System.Collections.Generic.List<Person> SearchPerson(string Name)
		{
			System.Collections.Generic.List<Person> list = new System.Collections.Generic.List<Person>();
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(this.urlSearchPerson + Name);
				XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("./people/person");
				foreach (XmlNode xmlNode in xmlNodeList)
				{
					string iD = null;
					string name = null;
					string thumb = null;
					if (xmlNode.SelectSingleNode("id") != null)
					{
						iD = xmlNode.SelectSingleNode("id").InnerText;
					}
					if (xmlNode.SelectSingleNode("name") != null)
					{
						name = xmlNode.SelectSingleNode("name").InnerText;
					}
					if (xmlNode.SelectSingleNode("./images/image[@size='thumb']") != null)
					{
						thumb = xmlNode.SelectSingleNode("./images/image[@size='thumb']").Attributes["url"].Value;
					}
					Person item = new Person
					{
						ID = iD,
						Name = name,
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

		public System.Collections.Generic.List<Posters> GetPersonImage(string PersonID)
		{
			System.Collections.Generic.List<Posters> list = new System.Collections.Generic.List<Posters>();
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(this.urlPersonInfo + PersonID);
				XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("./people/person");
				string xpath = "./images/image[@size='original']";
				string str = "./images/image[@size='thumb']";
				XmlNodeList xmlNodeList2 = xmlNodeList[0].SelectNodes(xpath);
				foreach (XmlNode xmlNode in xmlNodeList2)
				{
					string value = xmlNode.Attributes["url"].Value;
					string value2 = xmlNodeList[0].SelectSingleNode(string.Format(str + "[@id='{0}']", xmlNode.Attributes["id"].Value)).Attributes["url"].Value;
					Posters item = new Posters
					{
						Poster = value,
						Thumb = value2,
						Resolution = xmlNode.Attributes["width"].Value + "x" + xmlNode.Attributes["height"].Value
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
