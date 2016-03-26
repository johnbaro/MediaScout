using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace MediaScout
{
    public class TVScout
    {
        public event MediaScoutMessage.Message Message;

        public TVShow series;
        public TVScoutOptions options;
        private StreamWriter sw;

        public void ProcessDirectory(String directory, TVShow s)
        {
            this.series = s;
            MediaScout.Providers.TheTVDBProvider tvdb = new MediaScout.Providers.TheTVDBProvider();

            //Save Series Poster
            if (options.GetSeriesPosters && (!File.Exists(directory + "\\folder.jpg") || options.ForceUpdate == true))
            {
                Message("Downloading Poster", MediaScoutMessage.MessageType.ProcessSeries, DateTime.Now);
                Posters[] p = tvdb.GetPosters(series.id, MediaScout.Providers.TVShowPosterType.Poster, null);
                if (p.Length > 0)
                    p[0].SavePoster(directory + "\\folder.jpg");

            }
            //Save Series Backdrop
            if (options.GetSeriesPosters && (!File.Exists(directory + "\\backdrop.jpg") || options.ForceUpdate == true))
            {
                Message("Downloading Backdrop", MediaScoutMessage.MessageType.ProcessSeries, DateTime.Now);
                Posters[] p = tvdb.GetPosters(series.id, MediaScout.Providers.TVShowPosterType.Backdrop, null);
                if (p.Length > 0)
                    p[0].SavePoster(directory + "\\backdrop.jpg");
            }
            //Save Series Banner
            if (options.GetSeriesPosters && (!File.Exists(directory + "\\banner.jpg") || options.ForceUpdate == true))
            {
                Message("Downloading Banner", MediaScoutMessage.MessageType.ProcessSeries, DateTime.Now);
                Posters[] p = tvdb.GetPosters(series.id, MediaScout.Providers.TVShowPosterType.Banner, null);
                if (p.Length > 0)
                    p[0].SavePoster(directory + "\\banner.jpg");
            }

            //Save Series XML
            if (options.ForceUpdate == true || !File.Exists(directory + "\\series.xml"))
            {
                this.SaveTVShow(series, directory + "\\series.xml");
            }

            //Get the season folders
            #region Get the season folders
            List<String> seasons = new List<String>();
            List<String> seasonFldrs = new List<String>();

            DirectoryInfo diSeasons = new DirectoryInfo(directory);
            foreach (DirectoryInfo diSeason in diSeasons.GetDirectories())
            {
                Regex rSeasons = new Regex(options.SeasonFolderName + ".{0,1}([0-9]+)", RegexOptions.IgnoreCase);

                MatchCollection mc = rSeasons.Matches(diSeason.Name);
                if (mc.Count > 0)
                {
                    //Using string to prevent "01" becoming "1" later
                    String seasonNum = mc[0].Groups[1].Captures[0].Value;

                    //FreQi - Make sure the discovered season number is valid (in the metadata from theTVDB.com)
                    if (s.Seasons.ContainsKey(Int32.Parse(seasonNum)))
                    {
                        seasons.Add(seasonNum);
                        seasonFldrs.Add(diSeason.Name);
                        Message(String.Format("Found {0} {1} ({2})", options.SeasonFolderName, Int32.Parse(seasonNum), diSeason.Name), MediaScoutMessage.MessageType.NormalOperation, DateTime.Now);
                    }
                    else
                        Message(String.Format("Invalid {0} folder: '{1}", options.SeasonFolderName, diSeason.Name), MediaScoutMessage.MessageType.Error, DateTime.Now);
                }
                else if (diSeason.Name != "metadata")
                    Message(String.Format("{0} Folder not Identified: '{1}'", options.SeasonFolderName, diSeason.Name), MediaScoutMessage.MessageType.Error, DateTime.Now);

            }
            #endregion

            //Move files in the 'root' directory of a series
            #region Identify and Move files that are in the root/series folder but should be in a Season folder
            if (options.MoveFiles == true)
            {
                Message("Sorting loose episodes", MediaScoutMessage.MessageType.NormalOperation, DateTime.Now);

                DirectoryInfo diRoot = new DirectoryInfo(directory);
                List<String> filetypes = new List<String>(options.AllowedFileTypes);
                foreach (FileInfo fiRoot in diRoot.GetFiles())
                {
                    if (filetypes.Contains(fiRoot.Extension.ToLower()))
                    {
                        //Look for a pattern in the file names - we need to find what season this file belongs to
                        Match m = Regex.Match(fiRoot.Name, "(?<se>[0-9]{1,2})x(?<ep>[0-9]{1,2})|S(?<se>[0-9]{1,2})E(?<ep>[0-9]{1,2})|(?<se>[0-9]{1})(?<ep>[0-9]{2})", RegexOptions.IgnoreCase);

                        if (!m.Success)  //We don't know what season it's from so we can't ID it
                            Message(String.Format("{0} unknown: '{1}'", options.SeasonFolderName, fiRoot.Name), MediaScoutMessage.MessageType.Error, DateTime.Now);
                        else
                        {
                            String seasonID = m.Groups["se"].Value;

                            //FreQi - Make sure the discovered season number is valid (in the metadata from theTVDB.com)
                            if (!series.Seasons.ContainsKey(Int32.Parse(seasonID)))
                            {
                                Message(String.Format("{0} invalid: '{1}'", options.SeasonFolderName, fiRoot.Name), MediaScoutMessage.MessageType.Error, DateTime.Now);
                            }
                            else //the season is valid, do we already have a folder for it?
                            {
                                String seasonFolder = null;
                                //I know this is sloppy, but loop through all the known seasons, then compare the numerical
                                // values (not strings) to see if there's a match.  If we find one, set the folder name to
                                // the one that exists so we don't make a "Season 01" when "Season 1" exists.
                                foreach (String knownSeason in seasons)
                                {
                                    if (Int32.Parse(knownSeason) == Int32.Parse(seasonID))
                                        seasonFolder = seasonFldrs[seasons.IndexOf(knownSeason)];
                                }

                                //FreQi - If we don't already have a folder, we'll have to create it.  I figure
                                // if users don't like the zero padded convention I've set here, they'll just have
                                // to rename the folder themselves and stop being so lazy and sloppy.  heh.
                                if (seasonFolder == null)
                                    seasonFolder = options.SeasonFolderName + " " + seasonID.PadLeft(2, '0');

                                //Create the season folder if we have to and then add it to the "known list"
                                if (!Directory.Exists(directory + "\\" + seasonFolder))
                                {
                                    Directory.CreateDirectory(directory + "\\" + seasonFolder);
                                    seasons.Add(seasonID);
                                    seasonFldrs.Add(seasonFolder);
                                    Message(String.Format("Created folder '{0}'", seasonFolder), MediaScoutMessage.MessageType.NormalOperation, DateTime.Now);
                                }

                                //Finally, move the file to it's new reseting place.
                                File.Move(fiRoot.FullName, directory + "\\" + seasonFolder + "\\" + fiRoot.Name);
                                Message(String.Format("Moved '{0}' to {1}\\", fiRoot.Name, seasonFolder), MediaScoutMessage.MessageType.NormalOperation, DateTime.Now);
                            }
                        }
                    }
                }
            }
            #endregion


            #region Process files in season folders
            for (int i = 0; i < seasons.Count; i++)
            {
                string seasonNum = seasons[i];
                string seasonFldr = seasonFldrs[i];
                Message(String.Format("Processing {0} {1}", options.SeasonFolderName, Int32.Parse(seasonNum)), MediaScoutMessage.MessageType.ProcessSeason, DateTime.Now);

                //Make sure the metadata directory exists, else create

                String MetadataFolder = directory + "\\" + seasonFldr + @"\metadata";
                if (!Directory.Exists(MetadataFolder))
                {
                    Directory.CreateDirectory(MetadataFolder);

                    //Make all the metadata folders hidden, work item #2078

                    DirectoryInfo di = new DirectoryInfo(MetadataFolder);
                    if ((di.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        di.Attributes = di.Attributes | FileAttributes.Hidden;
                }

                //Save the season poster, if there is one and we're supposed to
                Season currentSeason = s.Seasons[Int32.Parse(seasonNum)];

                if (options.GetSeasonPosters && (!File.Exists(String.Format("{0}\\{1}\\folder.jpg", directory, seasonFldr)) || options.ForceUpdate == true))
               {
                   Message("Fetching " + options.SeasonFolderName + " Poster", MediaScoutMessage.MessageType.ProcessSeason, DateTime.Now);
                   Posters[] p = tvdb.GetPosters(series.id, MediaScout.Providers.TVShowPosterType.Season, seasonNum);
                   if (p.Length > 0)
                       p[0].SavePoster(String.Format("{0}\\{1}\\folder.jpg", directory, seasonFldr));
               }

                List<String> filetypes = new List<String>(options.AllowedFileTypes);
                DirectoryInfo diEpisodes = new DirectoryInfo(String.Format("{0}\\{1}", directory, seasonFldr));

                foreach (FileInfo fi in diEpisodes.GetFiles())
                {
                    if (filetypes.Contains(fi.Extension.ToLower()))
                    {
                        try
                        {
                            //FreQi - We've got a video file, let's see if we can determine what episode it is
                            Int32 EpisodeID = -1;
                            Int32 SeasonID = -1;

                            //Let's look for a couple patterns...
                            //Is "S##E##" or "#x##" somewhere in there ?
                            Match m = Regex.Match(fi.Name, "S(?<se>[0-9]{1,2})E(?<ep>[0-9]{1,3})|(?<se>[0-9]{1,2})x(?<ep>[0-9]{1,3})", RegexOptions.IgnoreCase);
                            if (m.Success)
                            {
                                EpisodeID = Int32.Parse(m.Groups["ep"].Value);
                                SeasonID = Int32.Parse(m.Groups["se"].Value);
                            }

                            //Does the file START WITH just "###" (SEE) or #### (SSEE) ? (if not found yet)
                            m = Regex.Match(fi.Name, "^(?<se>[0-9]{1,2})(?<ep>[0-9]{2})", RegexOptions.IgnoreCase);
                            if (EpisodeID == -1 && m.Success)
                            {
                                EpisodeID = Int32.Parse(m.Groups["ep"].Value);
                                SeasonID = Int32.Parse(m.Groups["se"].Value);
                            }

                            //Is it just the two digit episode number maybe?
                            m = Regex.Match(fi.Name, "^(?<ep>[0-9]{2})", RegexOptions.IgnoreCase);
                            if (EpisodeID == -1 && m.Success)
                                EpisodeID = Int32.Parse(m.Groups["ep"].Value);

                            //Does the file NOT START WITH just "###" (SEE) or #### (SSEE) ? (if not found yet)
                            m = Regex.Match(fi.Name, "(?<se>[0-9]{1,2})(?<ep>[0-9]{2})", RegexOptions.IgnoreCase);
                            if (EpisodeID == -1 && m.Success)
                            {
                                EpisodeID = Int32.Parse(m.Groups["ep"].Value);
                                SeasonID = Int32.Parse(m.Groups["se"].Value);
                            }

                            //So, did we find an episode number?
                            if (EpisodeID == -1)
                                Message(String.Format("Episode not Identified: '{0}'", fi.Name), MediaScoutMessage.MessageType.Error, DateTime.Now);
                            else
                            {
                                //Do we know what season this file "thinks" it's belongs to and is it in the right folder?
                                if (SeasonID != -1 && SeasonID != Int32.Parse(seasonNum))
                                    Message(String.Format("Wrong {0} ({1}): '{2}'", options.SeasonFolderName, SeasonID, fi.Name), MediaScoutMessage.MessageType.Error, DateTime.Now);

                                //At this point we can assume it matches the current season or we just don't know it
                                //Let's make sure the identified episode number is at least valid for this season in metadata
                                else if (currentSeason.Episodes.ContainsKey(EpisodeID))
                                {
                                    ProcessFile(fi, currentSeason, currentSeason.Episodes[EpisodeID]);
                                }
                                else
                                    Message(String.Format("Invalid Episode ({0}): '{1}'", EpisodeID, fi.Name), MediaScoutMessage.MessageType.Error, DateTime.Now);
                            }
                        }
                        catch (Exception ex)
                        {
                            Message(ex.Message, MediaScoutMessage.MessageType.FatalError, DateTime.Now);
                        }
                    }
                }
            }


            #endregion
        }

        public void ProcessFile(FileInfo file, Season season, Episode episode)
        {
            String oldName = file.Name.Replace(file.Extension,"");
            String metadataFolder = file.DirectoryName + @"\metadata\";

            if (!Directory.Exists(metadataFolder))
                Directory.CreateDirectory(metadataFolder);

            episode.EpisodeName = Regex.Replace(episode.EpisodeName, @"[\<\(\.\|\n\)\*\?\\\/\>]", string.Empty);
            Message(String.Format("Processing episode '{0} - {1}' ({2})", episode.ID, episode.EpisodeName, file.Name),MediaScoutMessage.MessageType.ProcessEpisode, DateTime.Now);

            if (options.RenameFiles == true)
            {
                //Calculate the renamed file
                String newPath = file.DirectoryName
                                + "\\"
                                + String.Format(options.RenameFormat, series.SeriesName, season.ID.ToString().PadLeft(2, '0'), episode.EpisodeName.Trim(), episode.ID.ToString().PadLeft(2, '0')).Replace("?", "").Replace(":", "")
                                + file.Extension;


                //Rename files
                if (file.FullName != newPath)
                {
                    //Rename File
                    File.Move(file.FullName, newPath);
                    file = new FileInfo(newPath);
                    Message(String.Format("Renamed file to {0}", newPath.Substring(newPath.LastIndexOf("\\") + 1)),MediaScoutMessage.MessageType.ProcessEpisode, DateTime.Now);

                    //Rename Subtitle
                    RenameSubtitle(file.DirectoryName, oldName, file.Name.Replace(file.Extension,""));
                }
            }

            //Get the poster
            if (options.GetEpisodePosters && (!File.Exists(metadataFolder + episode.PosterName) || options.ForceUpdate == true))
            {
                if (!String.IsNullOrEmpty(episode.PosterUrl))
                {
                    Message("Fetching Episode Poster", MediaScoutMessage.MessageType.ProcessEpisode, DateTime.Now);
                    SavePoster(episode.PosterUrl, metadataFolder + episode.PosterName);
                }
                else
                {
                    Message("No Poster Found: extracting thumbnail from video", MediaScoutMessage.MessageType.ProcessEpisode, DateTime.Now);
                    SamSoft.VideoBrowser.Util.VideoProcessing.ThumbCreator.CreateThumb(file.FullName, metadataFolder + file.Name.Replace(file.Extension, ".jpg"), 0.25);
                    episode.PosterName = file.Name.Replace(file.Extension, ".jpg");
                }
            }

            //Write the metadata to disk
            String xmlFile = metadataFolder + file.Name.Replace(file.Extension, ".xml");
            if (!File.Exists(xmlFile) || options.ForceUpdate == true)
                SaveEpisode(episode, xmlFile);
        }

        #region Writing to disk functions (SavePoster/SaveEpisode/SaveSeries)

        /// <summary>
        /// Saves a MediaScout.Episode to a serialised XML format, usable by VideoBrowser
        /// </summary>
        /// <param name="m">Episode to serialise</param>
        /// <param name="file">File to save it to</param>
        private void SaveEpisode(Episode ep, String file)
        {
            XmlSerializer s = new XmlSerializer(typeof(Episode));
            TextWriter w = new StreamWriter(file);
            s.Serialize(w, ep);
            w.Close();
        }

        /// <summary>
        /// Saves a MediaScout.TVshow to a serialised XML format, usable by VideoBrowser
        /// </summary>
        /// <param name="m">TVshow to serialise</param>
        /// <param name="file">File to save it to</param>
        private void SaveTVShow(TVShow show, String file)
        {
            XmlSerializer s = new XmlSerializer(typeof(TVShow));
            TextWriter w = new StreamWriter(file);
            s.Serialize(w, show);
            w.Close();
        }
        /// <summary>
        /// Downloads and saves a MediaScout.TVShow poster to file
        /// </summary>
        /// <param name="m">TVShow to save poster from</param>
        /// <param name="file">file to save to</param>
        public static void SavePoster(TVShow m, String file)
        {
            try
            {
                // Create the requests.
                WebRequest requestPic = WebRequest.Create(m.SeriesPosterUrl);
                WebResponse responsePic = requestPic.GetResponse();
                System.Drawing.Image poster = System.Drawing.Image.FromStream(responsePic.GetResponseStream());
                poster.Save(file);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Generic save poster method
        /// </summary>
        /// <param name="posterUrl"></param>
        /// <param name="file"></param>
        private void SavePoster(String posterUrl, String file)
        {
            try
            {
                // Create the requests.
                WebRequest requestPic = WebRequest.Create(posterUrl);
                WebResponse responsePic = requestPic.GetResponse();
                System.Drawing.Image poster = System.Drawing.Image.FromStream(responsePic.GetResponseStream());
                poster.Save(file);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Renames subtitles (using this.options.AllowedSubtitles)
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="OldName"></param>
        /// <param name="NewName"></param>
        private void RenameSubtitle(String Path, String OldName, String NewName)
        {
            foreach (String subtitleExt in options.AllowedSubtitles)
            {
                String src = Path + "\\" + OldName + subtitleExt;
                String dest = Path + "\\" + NewName + subtitleExt;

                if (File.Exists(src))
                {
                    File.Move(src, dest);
                    Message(String.Format("Subtitle '{0}' renamed to '{1}'", OldName + subtitleExt, NewName + subtitleExt), MediaScoutMessage.MessageType.ProcessEpisode, DateTime.Now);
                }
            }
        }
        #endregion


    }
}
