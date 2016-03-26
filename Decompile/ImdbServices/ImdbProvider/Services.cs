using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace ImdbProvider
{
	public class Services
	{
		private struct SearchState
		{
			public string Code;

			public string CacheKey;

			public bool FullInfo;
		}

		public delegate void FoundMoviesEventHandler(MoviesResultset M);

		public delegate void MovieParsedEventHandler(Movie M);

		private const string BaseUrl = "http://www.imdb.com/";

		private const string IMDB_ID_REGEX = "(?<=\\w\\w)\\d{7}";

		private const string MOVIE_TITLE_YEAR_PATTERN = "(.*)\\((\\d{4}).*";

		private const string USER_RATING_PATTERN = "\\b(?<score>\\d\\.\\d)/10";

		private const string USER_VOTES_PATTERN = "\\d+\\,\\d+";

		private const string FILM_RATING_PATTERN = "(?<=Rated\\s).*(?=\\sfor)";

		private const string RELEASE_DATE_PATTERN = "\\d+\\s\\w+\\s\\d{4}\\s";

		private const string PERSON_ID = "(?<=nm)\\d+";

		public event Services.FoundMoviesEventHandler FoundMovies;

		public event Services.MovieParsedEventHandler MovieParsed;

		private Match Match(string s, string pattern)
		{
			return Regex.Match(s, pattern, RegexOptions.IgnoreCase);
		}

		private MatchCollection MatchMany(string s, string pattern)
		{
			return Regex.Matches(s, pattern, RegexOptions.IgnoreCase);
		}

		private WebClient CreateWebClient()
		{
			return new WebClient
			{
				Headers = 
				{
					{
						HttpRequestHeader.AcceptEncoding,
						"gzip,deflate"
					},
					{
						HttpRequestHeader.UserAgent,
						"Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 3.5;)"
					}
				}
			};
		}

		private string UncompressResponse(byte[] Response, WebHeaderCollection ResponseHeaders)
		{
			string result = "";
			using (MemoryStream memoryStream = new MemoryStream(Response))
			{
				string a;
				if ((a = ResponseHeaders[HttpResponseHeader.ContentEncoding]) != null)
				{
					if (a == "gzip")
					{
						result = new StreamReader(new GZipStream(memoryStream, CompressionMode.Decompress)).ReadToEnd();
						goto IL_69;
					}
					if (a == "deflate")
					{
						result = new StreamReader(new DeflateStream(memoryStream, CompressionMode.Decompress)).ReadToEnd();
						goto IL_69;
					}
				}
				result = new StreamReader(memoryStream).ReadToEnd();
				IL_69:;
			}
			return result;
		}

		public void FindMovie(string movieTitle)
		{
			string requestUriString = "http://www.imdb.com/find?s=all&q=" + movieTitle.UrlEncode();
			WebRequest webRequest = WebRequest.Create(requestUriString);
			webRequest.Timeout = 15000;
			webRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
			webRequest.BeginGetResponse(new AsyncCallback(this.ResponseCallback), new RequestState
			{
				Request = webRequest,
				MovieTitle = movieTitle
			});
		}

		private void ResponseCallback(IAsyncResult result)
		{
			MoviesResultset moviesResultset = new MoviesResultset();
			try
			{
				RequestState requestState = (RequestState)result.AsyncState;
				WebRequest request = requestState.Request;
				HttpWebResponse httpWebResponse = (HttpWebResponse)request.EndGetResponse(result);
				Match match = this.Match(httpWebResponse.ResponseUri.ToString(), "(?<=\\w\\w)\\d{7}");
				if (match.Success)
				{
					moviesResultset.ExactMatches = new List<Movie>();
					moviesResultset.ExactMatches.Add(new Movie
					{
						Id = match.ToString(),
						Title = requestState.MovieTitle.CapitalizeAll()
					});
					this.OnFoundMovies(moviesResultset);
				}
				else
				{
					string a;
					string html;
					if ((a = httpWebResponse.Headers[HttpResponseHeader.ContentEncoding]) != null)
					{
						if (a == "gzip")
						{
							html = new StreamReader(new GZipStream(httpWebResponse.GetResponseStream(), CompressionMode.Decompress)).ReadToEnd();
							goto IL_10F;
						}
						if (a == "deflate")
						{
							html = new StreamReader(new DeflateStream(httpWebResponse.GetResponseStream(), CompressionMode.Decompress)).ReadToEnd();
							goto IL_10F;
						}
					}
					html = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
					IL_10F:
					HtmlDocument htmlDocument = new HtmlDocument();
					htmlDocument.LoadHtml(html);
					moviesResultset.PopularTitles = this.GetTitles(htmlDocument, "Popular Titles");
					moviesResultset.ExactMatches = this.GetTitles(htmlDocument, "Titles (Exact Matches)");
					moviesResultset.PartialMatches = this.GetTitles(htmlDocument, "Titles (Partial Matches)");
				}
			}
			catch (Exception ex)
			{
				moviesResultset.Error = true;
				moviesResultset.ErrorMessage = ex.Message;
			}
			this.OnFoundMovies(moviesResultset);
		}

		private List<Movie> GetTitles(HtmlDocument h, string listTitle)
		{
			List<Movie> list = new List<Movie>();
			HtmlNodeCollection htmlNodeCollection = h.DocumentNode.SelectNodes("id('main')//p/b[contains(.,'" + listTitle + "')]/following-sibling::*[1]/tr/td[3]");
			if (htmlNodeCollection != null)
			{
				try
				{
					foreach (HtmlNode current in ((IEnumerable<HtmlNode>)htmlNodeCollection))
					{
						HtmlNode htmlNode = current.SelectSingleNode("a");
						list.Add(new Movie
						{
							Id = this.Match(htmlNode.OuterHtml, "(?<=\\w\\w)\\d{7}").ToString(),
							Title = htmlNode.InnerText.HtmlDecode()
						});
					}
					return list;
				}
				catch
				{
				}
			}
			return null;
		}

		protected virtual void OnFoundMovies(MoviesResultset R)
		{
			this.ProcessDelegate(this.FoundMovies, new object[]
			{
				R
			});
		}

		public Movie GetMovie(string code, bool fullInfo)
		{
			string text = "http://www.imdb.com/title/tt" + code;
			if (fullInfo)
			{
				text += "/combined";
			}
			FileCache fileCache = new FileCache();
			Services.SearchState searchState = new Services.SearchState
			{
				Code = code,
				CacheKey = code + (fullInfo ? "-f" : ""),
				FullInfo = fullInfo
			};
			object obj = fileCache.Read(searchState.CacheKey);
			string value = "";
			if (obj != null)
			{
				value = obj.ToString();
				return this.ParseMovieHTML(code, fullInfo, ref value, true);
			}
			Movie result;
			using (WebClient webClient = this.CreateWebClient())
			{
				byte[] response = webClient.DownloadData(new Uri(text));
				value = this.UncompressResponse(response, webClient.ResponseHeaders);
				fileCache.Save(searchState.CacheKey, value);
				result = this.ParseMovieHTML(code, fullInfo, ref value, true);
			}
			return result;
		}

		public void GetMovieAsync(string code, bool fullInfo)
		{
			string text = "http://www.imdb.com/title/tt" + code;
			if (fullInfo)
			{
				text += "/combined";
			}
			FileCache fileCache = new FileCache();
			Services.SearchState searchState = new Services.SearchState
			{
				Code = code,
				CacheKey = code + (fullInfo ? "-f" : ""),
				FullInfo = fullInfo
			};
			object obj = fileCache.Read(searchState.CacheKey);
			if (obj != null)
			{
				string text2 = obj.ToString();
				this.ParseMovieHTML(code, fullInfo, ref text2, false);
				return;
			}
			using (WebClient webClient = this.CreateWebClient())
			{
				webClient.DownloadDataCompleted += new DownloadDataCompletedEventHandler(this.Wc_DownloadDataCompleted);
				webClient.DownloadDataAsync(new Uri(text), searchState);
			}
		}

		private void Wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
		{
			string value = "";
			WebClient webClient = (WebClient)sender;
			Services.SearchState searchState = (Services.SearchState)e.UserState;
			if (e.Error == null)
			{
				value = this.UncompressResponse(e.Result, webClient.ResponseHeaders);
				new FileCache().Save(searchState.CacheKey, value);
				this.ParseMovieHTML(searchState.Code, searchState.FullInfo, ref value, false);
				return;
			}
			this.OnMovieParsed(null);
		}

		private Movie ParseMovieHTML(string code, bool fullInfo, ref string html, bool returnValue)
		{
			CultureInfo provider = new CultureInfo("en-US");
			Movie movie = new Movie();
			HtmlDocument htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(html);
			html = null;
			movie.Id = code;
			HtmlNodeCollection htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes("//html/head/title");
			if (htmlNodeCollection == null)
			{
				this.OnMovieParsed(null);
				return null;
			}
			GroupCollection groups = this.Match(htmlNodeCollection[0].InnerText, "(.*)\\((\\d{4}).*").Groups;
			movie.Title = groups[1].ToString().CleanTitle();
			movie.Year = int.Parse(groups[2].ToString());
			movie.Description = this.GetDescription(htmlDocument);
			movie.PosterUrl = this.GetImageUrl(htmlDocument);
			movie.Genres = this.GetGenres(htmlDocument);
			movie.Cast = this.GetCast(htmlDocument);
			movie.Directors = this.GetDirectors(htmlDocument);
			movie.Writers = this.GetWriters(htmlDocument);
			htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes("id('tn15rating')");
			double userRating = 0.0;
			double votes = 0.0;
			HtmlNode htmlNode;
			if (htmlNodeCollection != null)
			{
				htmlNode = htmlNodeCollection[0];
				double.TryParse(this.Match(htmlNode.InnerText, "\\b(?<score>\\d\\.\\d)/10").Groups["score"].ToString(), NumberStyles.Float, provider, out userRating);
				double.TryParse(this.Match(htmlNode.InnerText, "\\d+\\,\\d+").ToString(), NumberStyles.Number, provider, out votes);
			}
			movie.UserRating = userRating;
			movie.Votes = votes;
			htmlNode = htmlDocument.DocumentNode.SelectSingleNode("id('tn15content')//div[@class='info']/a[@href='/mpaa']/../../div[@class]");
			string rated = string.Empty;
			if (htmlNode != null)
			{
				rated = this.Match(htmlNode.InnerText, "(?<=Rated\\s).*(?=\\sfor)").ToString();
			}
			movie.Rated = rated;
			htmlNode = htmlDocument.DocumentNode.SelectSingleNode("id('tn15content')//h5[contains(.,'Release Date:')]/../div[@class]");
			DateTime? releaseDate = null;
			DateTime value;
			if (htmlNode != null && DateTime.TryParse(this.Match(htmlNode.InnerText, "\\d+\\s\\w+\\s\\d{4}\\s").ToString(), out value))
			{
				releaseDate = new DateTime?(value);
			}
			movie.ReleaseDate = releaseDate;
			htmlNode = htmlDocument.DocumentNode.SelectSingleNode("id('tn15content')//h5[contains(.,'Tagline:')]/../div[@class]");
			if (htmlNode != null)
			{
				movie.Tagline = htmlNode.InnerText;
			}
			htmlNode = htmlDocument.DocumentNode.SelectSingleNode("id('tn15content')//h5[contains(.,'Runtime:')]/../div[@class]");
			int runtime = 0;
			if (htmlNode != null)
			{
				int.TryParse(this.Match(htmlNode.InnerText, "\\d+").ToString(), out runtime);
			}
			movie.Runtime = runtime;
			movie.Languages = this.GetLanguages(htmlDocument);
			movie.Seasons = this.GetSeasons(htmlDocument);
			movie.IsTvSerie = movie.Seasons.Any<int>();
			movie.KnownTitles = this.GetKnownTitles(htmlDocument);
			movie.RecommendedFilms = this.GetRecommendedTitles(htmlDocument);
			if (fullInfo)
			{
				movie.ProductionCompanies = this.GetCompanies(htmlDocument, "Production Companies");
				movie.Distributors = this.GetCompanies(htmlDocument, "Distributors");
				movie.SpecialEffectsCompanies = this.GetCompanies(htmlDocument, "Special Effects");
				movie.OtherCompanies = this.GetCompanies(htmlDocument, "Other Companies");
				movie.MusicBy = this.GetPersons(htmlDocument, "Original Music by");
				movie.Producers = this.GetPersons(htmlDocument, "Produced by");
				movie.SpecialEffects = this.GetPersons(htmlDocument, "Special Effects by");
				movie.CastingBy = this.GetPersons(htmlDocument, "Casting by");
				movie.CostumeDesignBy = this.GetPersons(htmlDocument, "Costume Design by");
			}
			if (returnValue)
			{
				return movie;
			}
			this.OnMovieParsed(movie);
			return null;
		}

		protected void OnMovieParsed(Movie M)
		{
			if (this.MovieParsed != null)
			{
				this.MovieParsed(M);
			}
		}

		private string GetDescription(HtmlDocument H)
		{
			HtmlNodeCollection htmlNodeCollection = H.DocumentNode.SelectNodes("id('tn15content')//div[@class='info']/h5[contains(.,'Plot:')]/../div[@class][1]");
			if (htmlNodeCollection != null)
			{
				foreach (HtmlNode current in ((IEnumerable<HtmlNode>)htmlNodeCollection[0].SelectNodes("a")))
				{
					current.Remove();
				}
				return htmlNodeCollection[0].InnerText.CleanHtml();
			}
			return string.Empty;
		}

		private string GetImageUrl(HtmlDocument H)
		{
			HtmlNodeCollection htmlNodeCollection = H.DocumentNode.SelectNodes("id('tn15lhs')//div[@class='photo']/a/img[1]");
			if (htmlNodeCollection != null)
			{
				return htmlNodeCollection[0].Attributes["src"].Value;
			}
			return string.Empty;
		}

		private List<Person> GetCast(HtmlDocument H)
		{
			List<Person> list = new List<Person>();
			HtmlNodeCollection htmlNodeCollection = H.DocumentNode.SelectNodes("id('tn15content')//table[@class='cast']/tr");
			if (htmlNodeCollection != null)
			{
				foreach (HtmlNode current in ((IEnumerable<HtmlNode>)htmlNodeCollection))
				{
					HtmlNode htmlNode = current.SelectSingleNode("td[@class='nm']");
					if (htmlNode != null)
					{
						string id = this.Match(htmlNode.OuterHtml, "(?<=nm)\\d+").ToString();
						list.Add(new Person
						{
							Id = id,
							Name = htmlNode.InnerText,
							Character = current.SelectSingleNode("td[@class='char']").InnerText
						});
					}
				}
			}
			return list;
		}

		private List<Person> GetDirectors(HtmlDocument H)
		{
			HtmlNodeCollection htmlNodeCollection = H.DocumentNode.SelectNodes("id('director-info')//div/a");
			if (htmlNodeCollection != null)
			{
				return (from N in htmlNodeCollection
				select new Person
				{
					Id = this.Match(N.OuterHtml, "(?<=nm)\\d+").ToString(),
					Name = N.InnerText
				}).ToList<Person>();
			}
			return null;
		}

		private List<Person> GetWriters(HtmlDocument H)
		{
			HtmlNodeCollection htmlNodeCollection = H.DocumentNode.SelectNodes("id('tn15content')//h5[contains(.,'Writer')]/../div[@class]/a");
			if (htmlNodeCollection != null)
			{
				return (from N in htmlNodeCollection
				select new Person
				{
					Id = this.Match(N.OuterHtml, "(?<=nm)\\d+").ToString(),
					Name = N.InnerText
				}).ToList<Person>();
			}
			return null;
		}

		private List<string> GetGenres(HtmlDocument H)
		{
			HtmlNodeCollection htmlNodeCollection = H.DocumentNode.SelectNodes("id('tn15content')//h5[contains(.,'Genre:')]/../div[@class]/a");
			if (htmlNodeCollection != null)
			{
				return (from N in htmlNodeCollection
				select N.InnerText into N
				where N != "more"
				select N).ToList<string>();
			}
			return null;
		}

		private List<string> GetLanguages(HtmlDocument H)
		{
			HtmlNodeCollection htmlNodeCollection = H.DocumentNode.SelectNodes("id('tn15content')//h5[contains(.,'Language:')]/../div[@class]/a");
			if (htmlNodeCollection != null)
			{
				return (from N in htmlNodeCollection
				select N.InnerText.Trim()).ToList<string>();
			}
			return null;
		}

		private List<int> GetSeasons(HtmlDocument H)
		{
			List<int> list = new List<int>();
			HtmlNodeCollection htmlNodeCollection = H.DocumentNode.SelectNodes("id('tn15content')//h5[contains(.,'Seasons:')]/../div[@class]/a");
			if (htmlNodeCollection != null)
			{
				foreach (HtmlNode current in ((IEnumerable<HtmlNode>)htmlNodeCollection))
				{
					int item;
					if (int.TryParse(current.InnerText, out item))
					{
						list.Add(item);
					}
				}
			}
			return list;
		}

		private List<string> GetKnownTitles(HtmlDocument H)
		{
			HtmlNode htmlNode = H.DocumentNode.SelectSingleNode("id('tn15content')//h5[contains(.,'Also Known As:')]/../div[@class]");
			if (htmlNode != null)
			{
				htmlNode.SelectSingleNode("a").Remove();
				return htmlNode.InnerHtml.HtmlDecode().Split(new string[]
				{
					"<br>"
				}, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
			}
			return null;
		}

		private List<Movie> GetRecommendedTitles(HtmlDocument H)
		{
			HtmlNodeCollection htmlNodeCollection = H.DocumentNode.SelectNodes("id('tn15content')//table[@class='recs']/tbody/tr[2]/td/a");
			if (htmlNodeCollection != null)
			{
				return (from N in htmlNodeCollection
				select new Movie
				{
					Id = this.Match(N.OuterHtml, "(?<=\\w\\w)\\d{7}").ToString(),
					Title = N.InnerText
				}).ToList<Movie>();
			}
			return null;
		}

		private List<string> GetCompanies(HtmlDocument H, string sTitle)
		{
			HtmlNodeCollection htmlNodeCollection = H.DocumentNode.SelectNodes("id('tn15content')//b[@class='blackcatheader'][contains(.,'" + sTitle + "')]/following-sibling::*[1]/li/a");
			if (htmlNodeCollection != null)
			{
				return (from N in htmlNodeCollection
				select N.InnerText.HtmlDecode()).Distinct<string>().ToList<string>();
			}
			return null;
		}

		private List<Person> GetPersons(HtmlDocument H, string sTitle)
		{
			List<Person> result = new List<Person>();
			HtmlNodeCollection htmlNodeCollection = H.DocumentNode.SelectNodes("id('tn15content')//h5[contains(.,'" + sTitle + "')]/../../../tr/td[1]");
			if (htmlNodeCollection != null)
			{
				htmlNodeCollection.RemoveAt(0);
				htmlNodeCollection.RemoveAt(htmlNodeCollection.Count - 1);
				return (from N in htmlNodeCollection
				select new Person
				{
					Id = this.Match(N.InnerHtml, "(?<=\\w\\w)\\d{7}").ToString(),
					Name = N.InnerHtml.StripHTML()
				}).ToList<Person>();
			}
			return result;
		}

		private void ProcessDelegate(Delegate del, params object[] args)
		{
			if (del == null)
			{
				return;
			}
			Delegate[] invocationList = del.GetInvocationList();
			Delegate[] array = invocationList;
			for (int i = 0; i < array.Length; i++)
			{
				Delegate del2 = array[i];
				this.InvokeDelegate(del2, args);
			}
		}

		private void InvokeDelegate(Delegate del, object[] args)
		{
			ISynchronizeInvoke synchronizeInvoke = del.Target as ISynchronizeInvoke;
			if (synchronizeInvoke != null)
			{
				if (!synchronizeInvoke.InvokeRequired)
				{
					del.DynamicInvoke(args);
					return;
				}
				try
				{
					synchronizeInvoke.Invoke(del, args);
					return;
				}
				catch
				{
					return;
				}
			}
			del.DynamicInvoke(args);
		}
	}
}
