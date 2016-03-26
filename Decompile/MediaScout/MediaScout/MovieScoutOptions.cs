using System;

namespace MediaScout
{
	public class MovieScoutOptions
	{
		public bool SaveXBMCMeta;

		public bool SaveMyMoviesMeta;

		public bool GetMoviePosters;

		public bool GetMovieFilePosters;

		public bool MoveFiles;

		public bool DownloadAllPosters;

		public bool DownloadAllBackdrops;

		public bool RenameFiles;

		public string FileRenameFormat;

		public string DirRenameFormat;

		public string[] AllowedFileTypes;

		public string[] AllowedSubtitles;

		public bool ForceUpdate;

		public bool overwrite;

		public bool SaveActors;

		public string FilenameReplaceChar;
	}
}
