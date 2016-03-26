using System;
using System.Text.RegularExpressions;

namespace MediaScout
{
	public static class GetID
	{
		public static EpisodeInfo GetSeasonAndEpisodeIDFromFile(string FileName)
		{
			EpisodeInfo episodeInfo = new EpisodeInfo();
			Match match = Regex.Match(FileName, "S(?<se>[0-9]{1,2})E(?<ep>[0-9]{1,3})|(?<se>[0-9]{1,2})x(?<ep>[0-9]{1,3})", RegexOptions.IgnoreCase);
			if (match.Success)
			{
				episodeInfo.EpisodeID = int.Parse(match.Groups["ep"].Value);
				episodeInfo.SeasonID = int.Parse(match.Groups["se"].Value);
			}
			match = Regex.Match(FileName, "S(?<se>[0-9]{1,2}).EP(?<ep>[0-9]{1,3})", RegexOptions.IgnoreCase);
			if (match.Success)
			{
				episodeInfo.EpisodeID = int.Parse(match.Groups["ep"].Value);
				episodeInfo.SeasonID = int.Parse(match.Groups["se"].Value);
			}
			match = Regex.Match(FileName, "^(?<se>[0-9]{1,2})(?<ep>[0-9]{2})", RegexOptions.IgnoreCase);
			if (episodeInfo.EpisodeID == -1 && match.Success)
			{
				episodeInfo.EpisodeID = int.Parse(match.Groups["ep"].Value);
				episodeInfo.SeasonID = int.Parse(match.Groups["se"].Value);
			}
			match = Regex.Match(FileName, "^(?<ep>[0-9]{2})", RegexOptions.IgnoreCase);
			if (episodeInfo.EpisodeID == -1 && match.Success)
			{
				episodeInfo.EpisodeID = int.Parse(match.Groups["ep"].Value);
			}
			match = Regex.Match(FileName, "(?<se>[0-9]{1,2})(?<ep>[0-9]{2})", RegexOptions.IgnoreCase);
			if (episodeInfo.EpisodeID == -1 && match.Success)
			{
				episodeInfo.EpisodeID = int.Parse(match.Groups["ep"].Value);
				episodeInfo.SeasonID = int.Parse(match.Groups["se"].Value);
			}
			return episodeInfo;
		}
	}
}
