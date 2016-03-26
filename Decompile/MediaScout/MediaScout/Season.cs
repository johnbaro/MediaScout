using System;
using System.Collections.Generic;

namespace MediaScout
{
	public class Season
	{
		public SortedList<int, EpisodeXML> Episodes = new SortedList<int, EpisodeXML>();

		public int ID;

		private string TVShowID;

		public Season(int seasonNumber, string TVShowID)
		{
			this.ID = seasonNumber;
			this.TVShowID = TVShowID;
		}
	}
}
