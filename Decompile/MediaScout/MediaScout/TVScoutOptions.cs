using System;

namespace MediaScout
{
	public class TVScoutOptions
	{
		public bool SaveXBMCMeta;

		public bool SaveMyMoviesMeta;

		public bool GetSeriesPosters;

		public bool GetSeasonPosters;

		public bool GetEpisodePosters;

		public bool MoveFiles;

		public string SeasonFolderName;

		public string SpecialsFolderName;

		public bool DownloadAllPosters;

		public bool DownloadAllBackdrops;

		public bool DownloadAllBanners;

		public bool DownloadAllSeasonPosters;

		public bool DownloadAllSeasonBackdrops;

		public bool RenameFiles;

		public string RenameFormat;

		public int SeasonNumZeroPadding;

		public int EpisodeNumZeroPadding;

		public string[] AllowedFileTypes;

		public string[] AllowedSubtitles;

		public bool ForceUpdate;

		public bool overwrite;

		public bool SaveActors;

		public string FilenameReplaceChar;
	}
}
