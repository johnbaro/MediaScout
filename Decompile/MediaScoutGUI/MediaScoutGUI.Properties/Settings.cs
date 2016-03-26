using System;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MediaScoutGUI.Properties
{
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0"), CompilerGenerated]
	internal sealed class Settings : ApplicationSettingsBase
	{
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

		public static Settings Default
		{
			get
			{
				return Settings.defaultInstance;
			}
		}

		[UserScopedSetting, DebuggerNonUserCode]
		public StringCollection TVFolders
		{
			get
			{
				return (StringCollection)this["TVFolders"];
			}
			set
			{
				this["TVFolders"] = value;
			}
		}

		[UserScopedSetting, DebuggerNonUserCode]
		public StringCollection MovieFolders
		{
			get
			{
				return (StringCollection)this["MovieFolders"];
			}
			set
			{
				this["MovieFolders"] = value;
			}
		}

		[DefaultSettingValue("{0} - {1}x{3} - {2}"), UserScopedSetting, DebuggerNonUserCode]
		public string TVfileformat
		{
			get
			{
				return (string)this["TVfileformat"];
			}
			set
			{
				this["TVfileformat"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool forceUpdate
		{
			get
			{
				return (bool)this["forceUpdate"];
			}
			set
			{
				this["forceUpdate"] = value;
			}
		}

		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool renameTVFiles
		{
			get
			{
				return (bool)this["renameTVFiles"];
			}
			set
			{
				this["renameTVFiles"] = value;
			}
		}

		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool getSeasonPosters
		{
			get
			{
				return (bool)this["getSeasonPosters"];
			}
			set
			{
				this["getSeasonPosters"] = value;
			}
		}

		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool moveTVFiles
		{
			get
			{
				return (bool)this["moveTVFiles"];
			}
			set
			{
				this["moveTVFiles"] = value;
			}
		}

		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool getEpisodePosters
		{
			get
			{
				return (bool)this["getEpisodePosters"];
			}
			set
			{
				this["getEpisodePosters"] = value;
			}
		}

		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool getSeriesPosters
		{
			get
			{
				return (bool)this["getSeriesPosters"];
			}
			set
			{
				this["getSeriesPosters"] = value;
			}
		}

		[DefaultSettingValue("Season"), UserScopedSetting, DebuggerNonUserCode]
		public string SeasonFolderName
		{
			get
			{
				return (string)this["SeasonFolderName"];
			}
			set
			{
				this["SeasonFolderName"] = value;
			}
		}

		[DefaultSettingValue(".avi;.mkv;.mp4;.mpg;.mpeg;.ogm;.wmv;.divx;.dvr-ms"), UserScopedSetting, DebuggerNonUserCode]
		public string allowedFileTypes
		{
			get
			{
				return (string)this["allowedFileTypes"];
			}
			set
			{
				this["allowedFileTypes"] = value;
			}
		}

		[DefaultSettingValue(".sub;.idx;.srt"), UserScopedSetting, DebuggerNonUserCode]
		public string allowedSubtitles
		{
			get
			{
				return (string)this["allowedSubtitles"];
			}
			set
			{
				this["allowedSubtitles"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool downloadAllTVImages
		{
			get
			{
				return (bool)this["downloadAllTVImages"];
			}
			set
			{
				this["downloadAllTVImages"] = value;
			}
		}

		[DefaultSettingValue("2"), UserScopedSetting, DebuggerNonUserCode]
		public string EpisodeNumZeroPadding
		{
			get
			{
				return (string)this["EpisodeNumZeroPadding"];
			}
			set
			{
				this["EpisodeNumZeroPadding"] = value;
			}
		}

		[DefaultSettingValue("0"), UserScopedSetting, DebuggerNonUserCode]
		public string SeasonNumZeroPadding
		{
			get
			{
				return (string)this["SeasonNumZeroPadding"];
			}
			set
			{
				this["SeasonNumZeroPadding"] = value;
			}
		}

		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool AutoSelectMatch
		{
			get
			{
				return (bool)this["AutoSelectMatch"];
			}
			set
			{
				this["AutoSelectMatch"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool downloadAllTVBackdrops
		{
			get
			{
				return (bool)this["downloadAllTVBackdrops"];
			}
			set
			{
				this["downloadAllTVBackdrops"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool downloadAllTVSeasonPosters
		{
			get
			{
				return (bool)this["downloadAllTVSeasonPosters"];
			}
			set
			{
				this["downloadAllTVSeasonPosters"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool downloadAllTVPosters
		{
			get
			{
				return (bool)this["downloadAllTVPosters"];
			}
			set
			{
				this["downloadAllTVPosters"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool downloadAllTVBanners
		{
			get
			{
				return (bool)this["downloadAllTVBanners"];
			}
			set
			{
				this["downloadAllTVBanners"] = value;
			}
		}

		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool getMoviePosters
		{
			get
			{
				return (bool)this["getMoviePosters"];
			}
			set
			{
				this["getMoviePosters"] = value;
			}
		}

		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool getMovieFilePosters
		{
			get
			{
				return (bool)this["getMovieFilePosters"];
			}
			set
			{
				this["getMovieFilePosters"] = value;
			}
		}

		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool moveMovieFiles
		{
			get
			{
				return (bool)this["moveMovieFiles"];
			}
			set
			{
				this["moveMovieFiles"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool downloadAllMovieImages
		{
			get
			{
				return (bool)this["downloadAllMovieImages"];
			}
			set
			{
				this["downloadAllMovieImages"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool downloadAllMoviePosters
		{
			get
			{
				return (bool)this["downloadAllMoviePosters"];
			}
			set
			{
				this["downloadAllMoviePosters"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool downloadAllMovieBackdrops
		{
			get
			{
				return (bool)this["downloadAllMovieBackdrops"];
			}
			set
			{
				this["downloadAllMovieBackdrops"] = value;
			}
		}

		[DefaultSettingValue("{0}"), UserScopedSetting, DebuggerNonUserCode]
		public string Moviefileformat
		{
			get
			{
				return (string)this["Moviefileformat"];
			}
			set
			{
				this["Moviefileformat"] = value;
			}
		}

		[DefaultSettingValue("{0} ({1})"), UserScopedSetting, DebuggerNonUserCode]
		public string MovieDirformat
		{
			get
			{
				return (string)this["MovieDirformat"];
			}
			set
			{
				this["MovieDirformat"] = value;
			}
		}

		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool renameMovieFiles
		{
			get
			{
				return (bool)this["renameMovieFiles"];
			}
			set
			{
				this["renameMovieFiles"] = value;
			}
		}

		[DefaultSettingValue(""), UserScopedSetting, DebuggerNonUserCode]
		public string SublightCmd
		{
			get
			{
				return (string)this["SublightCmd"];
			}
			set
			{
				this["SublightCmd"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool forceEnterSearchTerm
		{
			get
			{
				return (bool)this["forceEnterSearchTerm"];
			}
			set
			{
				this["forceEnterSearchTerm"] = value;
			}
		}

		[DefaultSettingValue("Specials"), UserScopedSetting, DebuggerNonUserCode]
		public string SpecialsFolderName
		{
			get
			{
				return (string)this["SpecialsFolderName"];
			}
			set
			{
				this["SpecialsFolderName"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool EnableGlassFrame
		{
			get
			{
				return (bool)this["EnableGlassFrame"];
			}
			set
			{
				this["EnableGlassFrame"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool downloadAllTVSeasonBackdrops
		{
			get
			{
				return (bool)this["downloadAllTVSeasonBackdrops"];
			}
			set
			{
				this["downloadAllTVSeasonBackdrops"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool SaveXBMCMeta
		{
			get
			{
				return (bool)this["SaveXBMCMeta"];
			}
			set
			{
				this["SaveXBMCMeta"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool SaveMyMoviesMeta
		{
			get
			{
				return (bool)this["SaveMyMoviesMeta"];
			}
			set
			{
				this["SaveMyMoviesMeta"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool TVFSWatcher
		{
			get
			{
				return (bool)this["TVFSWatcher"];
			}
			set
			{
				this["TVFSWatcher"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool overwriteFiles
		{
			get
			{
				return (bool)this["overwriteFiles"];
			}
			set
			{
				this["overwriteFiles"] = value;
			}
		}

		[DefaultSettingValue(""), UserScopedSetting, DebuggerNonUserCode]
		public string ImagesByNameLocation
		{
			get
			{
				return (string)this["ImagesByNameLocation"];
			}
			set
			{
				this["ImagesByNameLocation"] = value;
			}
		}

		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool SaveTVActors
		{
			get
			{
				return (bool)this["SaveTVActors"];
			}
			set
			{
				this["SaveTVActors"] = value;
			}
		}

		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool SaveMovieActors
		{
			get
			{
				return (bool)this["SaveMovieActors"];
			}
			set
			{
				this["SaveMovieActors"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool MovieFSWatcher
		{
			get
			{
				return (bool)this["MovieFSWatcher"];
			}
			set
			{
				this["MovieFSWatcher"] = value;
			}
		}

		[DefaultSettingValue("1"), UserScopedSetting, DebuggerNonUserCode]
		public string Zoom
		{
			get
			{
				return (string)this["Zoom"];
			}
			set
			{
				this["Zoom"] = value;
			}
		}

		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool AutoSelectMovieTitle
		{
			get
			{
				return (bool)this["AutoSelectMovieTitle"];
			}
			set
			{
				this["AutoSelectMovieTitle"] = value;
			}
		}

		[DefaultSettingValue(""), UserScopedSetting, DebuggerNonUserCode]
		public string SublightUsername
		{
			get
			{
				return (string)this["SublightUsername"];
			}
			set
			{
				this["SublightUsername"] = value;
			}
		}

		[DefaultSettingValue(""), UserScopedSetting, DebuggerNonUserCode]
		public string SublightPassword
		{
			get
			{
				return (string)this["SublightPassword"];
			}
			set
			{
				this["SublightPassword"] = value;
			}
		}

		[DefaultSettingValue(""), UserScopedSetting, DebuggerNonUserCode]
		public string Sublight
		{
			get
			{
				return (string)this["Sublight"];
			}
			set
			{
				this["Sublight"] = value;
			}
		}

		[DefaultSettingValue(""), UserScopedSetting, DebuggerNonUserCode]
		public string TVDropBoxLocation
		{
			get
			{
				return (string)this["TVDropBoxLocation"];
			}
			set
			{
				this["TVDropBoxLocation"] = value;
			}
		}

		[DefaultSettingValue(""), UserScopedSetting, DebuggerNonUserCode]
		public string MovieDropBoxLocation
		{
			get
			{
				return (string)this["MovieDropBoxLocation"];
			}
			set
			{
				this["MovieDropBoxLocation"] = value;
			}
		}

		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool FirstRun
		{
			get
			{
				return (bool)this["FirstRun"];
			}
			set
			{
				this["FirstRun"] = value;
			}
		}

		[DefaultSettingValue("-"), UserScopedSetting, DebuggerNonUserCode]
		public string FilenameReplaceChar
		{
			get
			{
				return (string)this["FilenameReplaceChar"];
			}
			set
			{
				this["FilenameReplaceChar"] = value;
			}
		}

		[DefaultSettingValue("480p|720p|1080p|5\\.1|AC3|hdtv|divx|xvid|x264|dvdrip|dvd|unrated|brrip|LOL|upscaled|bluray|\\d{4}|\\-\\w+\\-|\\(\\w+\\)|\\[\\w+\\]|\\{\\w+\\}|\\<|\\[|\\{|\\(|\\)|\\}|\\]|\\>|\\.|\\-|S[0-9]{1,2}E[0-9]{1,3}|[0-9]{1,2}x[0-9]{1,3}"), UserScopedSetting, DebuggerNonUserCode]
		public string SearchTermFilters
		{
			get
			{
				return (string)this["SearchTermFilters"];
			}
			set
			{
				this["SearchTermFilters"] = value;
			}
		}

		[DefaultSettingValue("18"), UserScopedSetting, DebuggerNonUserCode]
		public int language
		{
			get
			{
				return (int)this["language"];
			}
			set
			{
				this["language"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool SilentMode
		{
			get
			{
				return (bool)this["SilentMode"];
			}
			set
			{
				this["SilentMode"] = value;
			}
		}
	}
}
