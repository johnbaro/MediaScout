using ImdbProvider;
using System;
using System.Collections.Generic;

namespace MediaScout.Providers
{
	public class ImdbMovieProvider : IMovieMetadataProvider, IMetadataProvider
	{
		public MediaScoutMessage.Message Message;

		private int level;

		private Services Imdb = new Services();

		private System.Collections.Generic.List<MovieXML> movies = new System.Collections.Generic.List<MovieXML>();

		string IMetadataProvider.name
		{
			get
			{
				return "IMDb";
			}
		}

		string IMetadataProvider.version
		{
			get
			{
				return "1.0";
			}
		}

		string IMetadataProvider.url
		{
			get
			{
				return "";
			}
		}

		public ImdbMovieProvider(MediaScoutMessage.Message Message)
		{
			this.Message = Message;
			this.Imdb.FoundMovies += new Services.FoundMoviesEventHandler(this.Imdb_FoundMovies);
		}

		public MovieXML[] Search(string Name, string Year)
		{
			if (this.Message != null)
			{
				this.Message("Querying Imdb for " + Name, MediaScoutMessage.MessageType.Task, this.level);
			}
			Name = Name.Replace("&", " ");
			Name = Name.Replace("and", " ");
			this.Imdb.FindMovie(Name);
			return this.movies.ToArray();
		}

		private void Imdb_FoundMovies(MoviesResultset M)
		{
			if (!M.Any())
			{
				return;
			}
			try
			{
				if (M.ExactMatches != null)
				{
					foreach (Movie current in M.PopularTitles)
					{
						MovieXML item = new MovieXML
						{
							Title = current.Title,
							ID = current.Id,
							Year = current.Year.ToString(),
							Description = current.Tagline,
							Alt_Title = (current.KnownTitles.Count > 0) ? current.KnownTitles[0] : ""
						};
						this.movies.Add(item);
					}
				}
				if (M.PopularTitles != null)
				{
					foreach (Movie current2 in M.PopularTitles)
					{
						MovieXML item2 = new MovieXML
						{
							Title = current2.Title,
							ID = current2.Id,
							Year = current2.Year.ToString(),
							Description = current2.Tagline,
							Alt_Title = (current2.KnownTitles.Count > 0) ? current2.KnownTitles[0] : ""
						};
						this.movies.Add(item2);
					}
				}
				if (M.PartialMatches != null)
				{
					foreach (Movie current3 in M.PopularTitles)
					{
						MovieXML item3 = new MovieXML
						{
							Title = current3.Title,
							ID = current3.Id,
							Year = current3.Year.ToString(),
							Description = current3.Tagline,
							Alt_Title = (current3.KnownTitles.Count > 0) ? current3.KnownTitles[0] : ""
						};
						this.movies.Add(item3);
					}
				}
			}
			catch (System.Exception)
			{
			}
		}

		public MovieXML Get(string MovieID)
		{
			throw new System.NotImplementedException();
		}
	}
}
