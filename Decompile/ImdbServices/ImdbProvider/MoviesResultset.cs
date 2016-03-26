using System;
using System.Collections.Generic;
using System.Linq;

namespace ImdbProvider
{
	public class MoviesResultset
	{
		public List<Movie> ExactMatches
		{
			get;
			set;
		}

		public List<Movie> PartialMatches
		{
			get;
			set;
		}

		public List<Movie> PopularTitles
		{
			get;
			set;
		}

		public bool Error
		{
			get;
			set;
		}

		public string ErrorMessage
		{
			get;
			set;
		}

		public bool Any()
		{
			bool flag = this.ExactMatches != null && this.ExactMatches.Any<Movie>();
			bool flag2 = this.PartialMatches != null && this.PartialMatches.Any<Movie>();
			bool flag3 = this.PopularTitles != null && this.PopularTitles.Any<Movie>();
			return flag || flag2 || flag3;
		}
	}
}
