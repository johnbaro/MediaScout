using System;

namespace MediaScout.Providers
{
	public enum MoviePosterType
	{
		[StringValue("poster")]
		Poster,
		[StringValue("backdrop")]
		Backdrop,
		[StringValue("poster")]
		File_Poster,
		[StringValue("backdrop")]
		File_Backdrop
	}
}
