using System;
using System.Collections.Generic;
using System.Text;

namespace MediaScout
{
    public class TVScoutOptions
    {
        public bool ForceUpdate;
        public bool RenameFiles;
        public bool GetSeriesPosters;
        public bool GetSeasonPosters;
        public bool GetEpisodePosters;
        public bool GetSeriesBackdrop;
        public bool MoveFiles;

        public String RenameFormat;
        public String SeasonFolderName;
        public String LanguageID;
        public String[] AllowedFileTypes;
        public String[] AllowedSubtitles;

        public String CacheDirectory;
    }
}