using System;
using System.Collections.Generic;
using System.Drawing;

namespace ImdbProvider
{
	public class Movie
	{
		public string Id
		{
			get;
			set;
		}

		public string Title
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		public DateTime? ReleaseDate
		{
			get;
			set;
		}

		public List<Person> Directors
		{
			get;
			set;
		}

		public List<Person> Cast
		{
			get;
			set;
		}

		public List<Person> Producers
		{
			get;
			set;
		}

		public List<Person> Writers
		{
			get;
			set;
		}

		public List<Person> MusicBy
		{
			get;
			set;
		}

		public List<Person> CastingBy
		{
			get;
			set;
		}

		public List<Person> SpecialEffects
		{
			get;
			set;
		}

		public List<Person> Stunts
		{
			get;
			set;
		}

		public List<Person> CostumeDesignBy
		{
			get;
			set;
		}

		public List<string> ProductionCompanies
		{
			get;
			set;
		}

		public List<string> Distributors
		{
			get;
			set;
		}

		public List<string> SpecialEffectsCompanies
		{
			get;
			set;
		}

		public List<string> OtherCompanies
		{
			get;
			set;
		}

		public List<string> Genres
		{
			get;
			set;
		}

		public List<string> Languages
		{
			get;
			set;
		}

		public string Rated
		{
			get;
			set;
		}

		public int Year
		{
			get;
			set;
		}

		public double UserRating
		{
			get;
			set;
		}

		public double Votes
		{
			get;
			set;
		}

		public Image Poster
		{
			get;
			set;
		}

		public string PosterUrl
		{
			get;
			set;
		}

		public bool HasTrailer
		{
			get;
			set;
		}

		public string TrailerUrl
		{
			get;
			set;
		}

		public List<string> KnownTitles
		{
			get;
			set;
		}

		public List<Movie> RecommendedFilms
		{
			get;
			set;
		}

		public int Runtime
		{
			get;
			set;
		}

		public string Tagline
		{
			get;
			set;
		}

		public bool IsTvSerie
		{
			get;
			set;
		}

		public List<int> Seasons
		{
			get;
			set;
		}

		public object Tag
		{
			get;
			set;
		}
	}
}
