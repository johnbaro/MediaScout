using System;

namespace MediaScout.Providers
{
	public enum TVShowPosterType
	{
		[StringValue("poster")]
		Poster,
		[StringValue("fanart")]
		Backdrop,
		[StringValue("series")]
		Banner,
		[StringValue("season")]
		Season_Poster,
		[StringValue("fanart")]
		Season_Backdrop
	}
}
