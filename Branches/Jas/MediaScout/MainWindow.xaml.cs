using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.IO;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;
using MediaScoutGUI.GUITypes;
using System.Text.RegularExpressions;
using System.Xml;
using System.Windows.Controls.Primitives;

namespace MediaScoutGUI
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Delegates
        public delegate void StringDelegate(String s);
        public delegate void SetPosterDelegate(String filename, Image target);

        public delegate void MetadataCompletedHandler(String reason);
        public event MetadataCompletedHandler MetadataCompleted;


        //Collections
        private ObservableCollection<TVShow> tvshows = new ObservableCollection<TVShow>();
        private ObservableCollection<GUITypes.Movie> movies = new ObservableCollection<Movie>();
        private DispatchingCollection<ObservableCollection<TVShow>, TVShow> dispatchtvshows;
        private DispatchingCollection<ObservableCollection<Movie>, Movie> dispatchmovies;

        //Settings
        private String TVFolder;
        private String MovieFolder;
        private List<String> ignoredFiles = new List<String>();
        private List<String> AllowedFileTypes;
        private List<String> AllowedSubtitleTypes;
        private String SeasonFolderName = "";
        private String tvTabFolder;
        private String language;

        private bool RenameFiles;
        private bool ForceUpdate;
        private bool GetSeriesPosters;
        private bool GetSeasonPosters;
        private bool GetEpisodePosters;
        private bool MoveFiles;
        private String RenameFormat;
        private String LanguageID;

        //Objects
        private MediaScout.MovieScout MovieScout = new MediaScout.MovieScout();
        private MediaScout.TVScout TVScout = new MediaScout.TVScout();
        private FileSystemWatcher FSWatcher;
        private MediaScout.Providers.TheTVDBProvider tvdb = new MediaScout.Providers.TheTVDBProvider();
        private Thread tvThread;

        //
        private void ResetUI(String reason)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new MetadataCompletedHandler(ResetUI), reason);
                return;
            }
            btnFetch.Content = "Fetch Data";
            TVScout_Message(reason, MediaScout.MediaScoutMessage.MessageType.NormalOperation, DateTime.Now);
            tvThread = null; //Can I even do this? Hasn't crashed on my tests.

        }


        public MainWindow()
        {
            this.InitializeComponent();
            tvdb.Message += new MediaScout.MediaScoutMessage.Message(TVScout_Message);
            this.MetadataCompleted += new MetadataCompletedHandler(ResetUI);

            try
            {
                dispatchtvshows = new DispatchingCollection<ObservableCollection<TVShow>, TVShow>(tvshows, Dispatcher);
                dispatchmovies = new DispatchingCollection<ObservableCollection<Movie>, Movie>(movies, Dispatcher);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error");
            }
        }

        /// <summary>
        /// Handles the loading of various tabs as the tabs are selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tcTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (tcTabs.SelectedIndex)
            {
                case 0:     //TV Tab
                    if (Directory.Exists(TVFolder))
                        txtBaseDir.Text = TVFolder;

                    break;


                case 1:     //Manage tab

                    //Make sure the TVFolder is set before attempting to load in all the TV show data.
                    if (TVFolder == String.Empty || TVFolder == null || !Directory.Exists(TVFolder))
                    {
                        MessageBox.Show("Please set your TV folder in the Options tab");
                        break;
                    }


                    if (tvshows == null || tvshows.Count == 0 || tvTabFolder != TVFolder)
                    {
                        tvTabFolder = TVFolder;
                        tvshows.Clear();
                        tvTVShows.ItemsSource = dispatchtvshows;

                        //Load it on a seperate thread (hence the use of the DispatchingCollection) so as not to lag UI
                        Thread thread = new Thread(delegate()
                        {
                            foreach (DirectoryInfo di in new DirectoryInfo(TVFolder).GetDirectories())
                            {
                                try
                                {
                                    GUITypes.TVShow t = new GUITypes.TVShow(di.FullName, di.Name);
                                    tvshows.Add(t);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message + "\n\r" + ex.StackTrace);
                                }
                            }
                        });

                        thread.SetApartmentState(ApartmentState.STA);
                        thread.Start();
                    }
                    break;

                case 2: //Movies tab
                    //Make sure the MovieFolder is set before attempting to load in all the movies data.
                    if (MovieFolder == String.Empty || MovieFolder == null || !Directory.Exists(MovieFolder))
                    {
                        MessageBox.Show("Please set your Movie folder in the Options tab");
                        break;
                    }

                    if (movies == null || movies.Count == 0)
                    {
                        lstMovies.ItemsSource = dispatchmovies;

                        //Load it on a seperate thread (hence the use of the DispatchingCollection) so as not to lag UI
                        Thread thread = new Thread(delegate()
                        {
                            foreach (DirectoryInfo di in new DirectoryInfo(MovieFolder).GetDirectories())
                            {
                                GUITypes.Movie m = new GUITypes.Movie(di.FullName, di.Name);
                                movies.Add(m);
                            }

                        });

                        thread.SetApartmentState(ApartmentState.STA);
                        thread.Start();

                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Set details (poster, titles, visibility of other details) based on what is selected, ie, Episode, Season or Series
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvTVShows_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (tvTVShows.SelectedItem is GUITypes.TVShow)
            {
                grdSeriesView.Visibility = Visibility.Visible;
                grdSeasonView.Visibility = Visibility.Hidden;
                grdEpisodeView.Visibility = Visibility.Hidden;

                GUITypes.TVShow s = (GUITypes.TVShow)tvTVShows.SelectedItem;

                SetManagedPoster(s.Filepath + @"\folder.jpg", imgTVPoster);
                SetManagedPoster(s.Filepath + @"\backdrop.jpg", imgTVBackdrop);
                lblSelectedSeries.Content = s.Name;
                lblSelectedSeason.Visibility = Visibility.Hidden;
                tbShowDesc.Text = s.Overview;

                btnChangeTVBackdrop.IsEnabled = true;

            }
            else if (tvTVShows.SelectedItem is GUITypes.Season)
            {
                grdSeriesView.Visibility = Visibility.Hidden;
                grdSeasonView.Visibility = Visibility.Visible;
                grdEpisodeView.Visibility = Visibility.Hidden;

                GUITypes.Season s = (GUITypes.Season)tvTVShows.SelectedItem;

                lblSelectedSeason.Visibility = Visibility.Visible;
                lblSelectedSeason.Content = s.Name;
                lblSelectedSeries.Content = s.Parent.Name;
                tbShowDesc.Text = "";

                SetManagedPoster(s.Filepath + @"\folder.jpg", imgSeasonPoster);
            }
            else if (tvTVShows.SelectedItem is GUITypes.Episode)
            {
                grdSeriesView.Visibility = Visibility.Hidden;
                grdSeasonView.Visibility = Visibility.Hidden;
                grdEpisodeView.Visibility = Visibility.Visible;

                GUITypes.Episode ep = (GUITypes.Episode)tvTVShows.SelectedItem;
                ep.LoadFromXML();
                tbShowDesc.Text = ep.Description;

                if (!File.Exists(ep.Poster) && ep.Poster != null)
                {
                    String postername = ep.Parent.Parent.Id.ToString() + "-" + ep.id + ".jpg";
                    ep.Poster += postername;
                    SamSoft.VideoBrowser.Util.VideoProcessing.ThumbCreator.CreateThumb(ep.Filepath, ep.Poster, 0.25);
                    ep.UpdatePoster();
                }

                SetManagedPoster(ep.Poster, imgEpisodePoster);
            }

            if (tvTVShows.SelectedValue != null)
            {
                lblSelectedSeries.Visibility = Visibility.Visible;
                btnFetchSelectedTV.IsEnabled = true;
                btnChangeTVPoster.IsEnabled = true;

            }

        }

        /// <summary>
        /// Fetches the selected movie
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void btnFetchSelectedMovie_Click(object sender, RoutedEventArgs e)
        {
            GUITypes.Movie m = (GUITypes.Movie)lstMovies.SelectedItem;
            MediaScout.Movie selected = null;

            MediaScout.Movie[] results = MovieScout.Search(m.Name);

            if (results.Length > 0)
            {
                //If the result is 'empty', don't allow it to process
                if (results[0].ID == null)
                    selected = null;

                //If there is only one result, skip the selection dialog
                else if (results.Length == 1)
                    selected = results[0];

                //Else pop up a selection dialog
                else
                {
                    MovieResults mr = new MovieResults(results);
                    mr.Owner = this;
                    mr.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    if (mr.ShowDialog() == true)
                        selected = mr.Selected;
                }

                //If there are no results, ask user to refine or broaden their search terms.
                // ie if a folder is "007 - From russia with love", it may not match "From Russia With Love"
                while (selected == null)
                {
                    NoMovieResults nmr = new NoMovieResults(m.Name);
                    nmr.Owner = this;
                    nmr.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    if (nmr.ShowDialog() == true)
                    {
                        results = MovieScout.Search(nmr.Term);

                        MovieResults mr = new MovieResults(results);
                        mr.Owner = this;
                        mr.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        if (mr.ShowDialog() == true)
                            selected = mr.Selected;
                    }
                    else
                        break;
                }

                //Fetch all the information
                if (selected != null)
                {
                    selected = MovieScout.Get(selected.ID);
                    MovieScout.SaveMovie(selected, m.Filepath + @"\mymovies.xml");
                    m.Load();
                    grdMovies.DataContext = m.Moviebase;

                    Thread thread = new Thread(delegate()
                    {
                        if (!File.Exists(m.Filepath + @"\folder.jpg"))
                            MovieScout.SavePoster(selected.Poster, m.Filepath + @"\folder.jpg");
                        SetManagedPoster(m.Filepath + @"\folder.jpg", imgMoviePoster);

                        if (!File.Exists(m.Filepath + @"\backdrop.jpg"))
                            MovieScout.SavePoster(selected.Backdrop, m.Filepath + @"\backdrop.jpg");
                        SetManagedPoster(m.Filepath + @"\backdrop.jpg", imgMovieBackdrop);


                        m.HasPoster = true;
                        m.HasMetadata = true;
                    });

                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();

                }
            }
            else
            {
                MessageBox.Show("No results found");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOptions();

            if (txtBaseDir.Text != String.Empty)
                btnFetch.IsEnabled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveOptions();
        }

        /// <summary>
        /// Sets the Movies Folder used in the Movie's tab
        /// </summary>
        private void btnBrowseMovies_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowser myBrowser = new FolderBrowser();

            myBrowser.Title = "Select Movies folder";
            myBrowser.Flags = BrowseFlags.BIF_NEWDIALOGSTYLE |
                              BrowseFlags.BIF_STATUSTEXT |
                              BrowseFlags.BIF_EDITBOX; ;
            System.Windows.Forms.DialogResult res = myBrowser.ShowDialog();

            if (res == System.Windows.Forms.DialogResult.OK)
            {
                txtMoviesFolder.Text = myBrowser.DirectoryPath;
                SaveOptions();
                LoadOptions();
            }
        }

        /// <summary>
        /// Sets the TV Folder used in the Manage TV tab, and for preliminary start directory for the TV tab
        /// </summary>
        private void btnBrowseTVFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowser myBrowser = new FolderBrowser();

            myBrowser.Title = "Select TV Shows folder";
            myBrowser.Flags = BrowseFlags.BIF_NEWDIALOGSTYLE |
                              BrowseFlags.BIF_STATUSTEXT |
                              BrowseFlags.BIF_EDITBOX; ;
            System.Windows.Forms.DialogResult res = myBrowser.ShowDialog();

            if (res == System.Windows.Forms.DialogResult.OK)
            {
                txtTVFolder.Text = myBrowser.DirectoryPath;
                SaveOptions();
                LoadOptions();
            }
        }

        /// <summary>
        /// Loads all the options from the user configuration file
        /// </summary>
        private void LoadOptions()
        {
            txtMoviesFolder.Text = MediaScoutGUI.Properties.Settings.Default.MovieFolder;
            txtTVFolder.Text = MediaScoutGUI.Properties.Settings.Default.TVFolder;

            TVFolder = MediaScoutGUI.Properties.Settings.Default.TVFolder;
            MovieFolder = MediaScoutGUI.Properties.Settings.Default.MovieFolder;

            /* Load GUI */
            txtRenameFormat.Text = MediaScoutGUI.Properties.Settings.Default.fileformat;
            chkForceUpdate.IsChecked = MediaScoutGUI.Properties.Settings.Default.forceUpdate;
            chkRename.IsChecked = MediaScoutGUI.Properties.Settings.Default.renameFiles;
            chkSeasonPosters.IsChecked = MediaScoutGUI.Properties.Settings.Default.getSeasonPosters;
            chkMove.IsChecked = MediaScoutGUI.Properties.Settings.Default.moveFiles;
            chkEpPosters.IsChecked = MediaScoutGUI.Properties.Settings.Default.getEpisodePosters;
            chkSeriesPosters.IsChecked = MediaScoutGUI.Properties.Settings.Default.getSeriesPosters;
            lstLanguages.SelectedIndex = MediaScoutGUI.Properties.Settings.Default.language;
            txtSeasonFolderName.Text = MediaScoutGUI.Properties.Settings.Default.SeasonFolderName;
            txtAllowedFiletypes.Text = MediaScoutGUI.Properties.Settings.Default.allowedFileTypes;

            /* Load Variables */
            AllowedFileTypes = new List<string>(MediaScoutGUI.Properties.Settings.Default.allowedFileTypes.Split(';'));
            AllowedSubtitleTypes = new List<string>(MediaScoutGUI.Properties.Settings.Default.allowedSubtitles.Split(';'));
            SeasonFolderName = txtSeasonFolderName.Text;
            try
            {
                language = (((XmlNode)this.lstLanguages.SelectedItem).ParentNode).SelectSingleNode("abbreviation").InnerText;
            }
            catch (Exception)
            {
                //TODO: Add Log Message
                language = "en";
            }

            //Change tabs last as other settings need to be set first
            //tcTabs.SelectedIndex = MediaScoutGUI.Properties.Settings.Default.SelectedTab;
            //JMCode
            SelectTabFromHeader(hdrTV, 1);
            SelectTabFromHeader2(hdr2Manage, 1);
        }

        /// <summary>
        /// Saves all the options to the user configuration file
        /// </summary>
        private void SaveOptions()
        {
            MediaScoutGUI.Properties.Settings.Default.SelectedTab = tcTabs.SelectedIndex;
            MediaScoutGUI.Properties.Settings.Default.MovieFolder = txtMoviesFolder.Text;
            MediaScoutGUI.Properties.Settings.Default.TVFolder = txtTVFolder.Text;

            MediaScoutGUI.Properties.Settings.Default.fileformat = txtRenameFormat.Text;
            MediaScoutGUI.Properties.Settings.Default.forceUpdate = chkForceUpdate.IsChecked.Value;
            MediaScoutGUI.Properties.Settings.Default.renameFiles = chkRename.IsChecked.Value;
            MediaScoutGUI.Properties.Settings.Default.getSeasonPosters = chkSeriesPosters.IsChecked.Value;
            MediaScoutGUI.Properties.Settings.Default.getSeasonPosters = chkSeasonPosters.IsChecked.Value;
            MediaScoutGUI.Properties.Settings.Default.getEpisodePosters = chkEpPosters.IsChecked.Value;
            MediaScoutGUI.Properties.Settings.Default.moveFiles = chkMove.IsChecked.Value;
            MediaScoutGUI.Properties.Settings.Default.language = lstLanguages.SelectedIndex;
            MediaScoutGUI.Properties.Settings.Default.SeasonFolderName = txtSeasonFolderName.Text;
            MediaScoutGUI.Properties.Settings.Default.allowedFileTypes = txtAllowedFiletypes.Text;

            MediaScoutGUI.Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Attempts to set the source of a System.Windows.Control.Image object to a string
        /// </summary>
        /// <param name="filename">String to set image to</param>
        /// <param name="target">Target Image to change the image of</param>
        /// <returns>true if poster was set, will clear the source otherwise</returns>
        private void SetManagedPoster(String filename, Image target)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new SetPosterDelegate(SetManagedPoster), filename, target);
                return;
            }


            //Set image, if it exists
            if (File.Exists(filename))
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();

                //Caching is so that the image can be deleted
                bi.CacheOption = BitmapCacheOption.OnLoad;

                //Ignoring the cache every time seems stupid, but it means that it can be deleted AND refreshed. Chalk it down to a WPF bug
                bi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;

                bi.UriSource = new Uri(filename);
                bi.EndInit();

                target.Source = bi;

            }
            else
                target.Source = null;
        }

        /// <summary>
        /// Change the poster for a movie, based on its ID, in the Movies tab
        /// If the ID does not exist, look it up first.
        /// If the movie cannot be found in the given database, ignore
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChangePoster_Click(object sender, RoutedEventArgs e)
        {
            //Extract the id (ID), if it doesn't exist, popup a search dialog!
            if (lstMovies.SelectedItem is GUITypes.Movie)
            {
                MediaScout.Providers.TheMovieDBProvider_21 tmdb = new MediaScout.Providers.TheMovieDBProvider_21();
                GUITypes.Movie m = lstMovies.SelectedItem as GUITypes.Movie;

                String id = null;

                if (m.id != null)
                    id = m.id;
                else
                {
                    MediaScout.Movie[] results = tmdb.Search(m.Name);

                    MovieResults mr = new MovieResults(results);
                    mr.Owner = this;
                    mr.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    if (mr.ShowDialog() == true)
                        id = mr.Selected.ID;
                    else
                        id = null;
                }

                if (id != null)
                {
                    ChangeMoviePoster cmp = new ChangeMoviePoster(tmdb.GetPosters(id, MediaScout.Providers.MoviePosterType.Poster));
                    cmp.Owner = this;
                    cmp.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    if (cmp.ShowDialog() == true)
                    {
                        imgMoviePoster.Source = null;

                        String file = m.Filepath + @"\folder.jpg";
                        if (File.Exists(file))
                            File.Delete(file);

                        cmp.Selected.Save(file);

                        SetManagedPoster(file, imgMoviePoster);
                    }
                }
                else
                {
                    //Cancelled 
                }
            }
        }

        private void btnMoviesRefresh_Click(object sender, RoutedEventArgs e)
        {
            movies = null;
            tcTabs.SelectedIndex = -1;
            tcTabs.SelectedIndex = 2;

        }

        private void lstMovies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstMovies.SelectedItem is GUITypes.Movie)
            {

                GUITypes.Movie m = lstMovies.SelectedItem as GUITypes.Movie;

                grdMovies.DataContext = m.Moviebase;

                btnChangePoster.IsEnabled = true;
                btnChangeMovieBackdrop.IsEnabled = true;
                lblMovieTitle.Visibility = Visibility.Visible;
                lblMovieTitle.Content = m.Name;
                tbMovieDesc.Text = m.Desc;

                SetManagedPoster(m.Filepath + @"\folder.jpg", imgMoviePoster);
                SetManagedPoster(m.Filepath + @"\backdrop.jpg", imgMovieBackdrop);
            }
            else //if its not a movie selected, clean it up by hiding stuff. This should never happen, but just incase.
            {
                lblMovieTitle.Visibility = Visibility.Hidden;
                SetManagedPoster(null, imgMoviePoster);
                btnChangePoster.IsEnabled = false;
                btnChangeMovieBackdrop.IsEnabled = false;
                tbMovieDesc.Text = "";
            }
        }

        private void btnFetchSelectedTV_Click(object sender, RoutedEventArgs e)
        {
            if (tvTVShows.SelectedItem is GUITypes.TVShow)
            {
                GUITypes.TVShow s = (GUITypes.TVShow)tvTVShows.SelectedItem;
                MediaScout.TVShow[] results = tvdb.Search(s.Name, language);
                MediaScout.TVShow selected = null;

                TVShowResults tsr = new TVShowResults(results);
                tsr.Owner = this;
                tsr.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (tsr.ShowDialog() == true)
                {
                    selected = tsr.Selected;
                    selected = tvdb.GetTVShow(selected.id, language);
                    selected.Save(s.Filepath + @"\series.xml");
                }

                tvshows.Remove(s);
                s = new TVShow(s.Filepath, s.Name);
                tvshows.Add(s);
            }
        }

        private void btnChangeTVPoster_Click(object sender, RoutedEventArgs e)
        {
            //Changing poster if its a TVShow...
            if (tvTVShows.SelectedItem is GUITypes.TVShow)
            {
                GUITypes.TVShow s = (GUITypes.TVShow)tvTVShows.SelectedItem;

                String id = "";

                //if the ID is set, use that ID to fetch posters on
                if (s.Id != 0)
                    id = s.Id.ToString();

                //else, the GUIType doesn't have an ID (ie, if the metadata hasn't been fetched before)
                else
                {
                    MediaScout.TVShow[] results = tvdb.Search(s.Name, language);

                    TVShowResults tsr = new TVShowResults(results);
                    tsr.Owner = this;
                    tsr.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    if (tsr.ShowDialog() == true)
                        id = tsr.Selected.id;
                    else
                        id = null;
                }

                if (id != null)
                {
                    ChangeTVPoster ctp = new ChangeTVPoster(tvdb.GetPosters(id, MediaScout.Providers.TVShowPosterType.Poster, null));
                    ctp.Owner = this;
                    ctp.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    if (ctp.ShowDialog() == true)
                    {
                        imgTVPoster.Source = null;

                        String file = s.Filepath + @"\folder.jpg";
                        if (File.Exists(file))
                            File.Delete(file);

                        ctp.Selected.Save(file);

                        SetManagedPoster(s.Filepath + @"\folder.jpg", imgTVPoster);
                    }
                }
            }

        }

        private void txtAllowedFiletypes_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveOptions();
        }

        private void chkAutotron_Unchecked(object sender, RoutedEventArgs e)
        {
            AutotronLog("Autotron Stopped");
            rtbAutotronLog.IsEnabled = false;

            FSWatcher.EnableRaisingEvents = false;
        }

        private void chkAutotron_Checked(object sender, RoutedEventArgs e)
        {
            rtbAutotronLog.IsEnabled = true;

            SeasonFolderName = txtSeasonFolderName.Text;
            RenameFiles = (bool)chkRename.IsChecked;
            ForceUpdate = (bool)chkForceUpdate.IsChecked;
            GetSeriesPosters = (bool)chkSeriesPosters.IsChecked;
            GetSeasonPosters = (bool)chkSeasonPosters.IsChecked;
            GetEpisodePosters = (bool)chkEpPosters.IsChecked;
            MoveFiles = (bool)chkMove.IsChecked;
            RenameFormat = txtRenameFormat.Text;
            LanguageID = lstLanguages.SelectedIndex.ToString();

            FSWatcher = new FileSystemWatcher();
            FSWatcher.Path = TVFolder;
            FSWatcher.IncludeSubdirectories = true;
            FSWatcher.NotifyFilter = NotifyFilters.FileName;
            FSWatcher.Created += new FileSystemEventHandler(FSWatcher_Changed);
            FSWatcher.Changed += new FileSystemEventHandler(FSWatcher_Changed);
            FSWatcher.EnableRaisingEvents = true;

            AutotronLog("Autotron Started");
        }

        void FSWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name.Contains("."))       //If it doesn't contain a "." (and thus extension), assume its not a file
            {
                FileInfo fi = new FileInfo(e.FullPath);

                if (AllowedFileTypes.Contains(fi.Extension) //make sure its an acceptable filetype
                    && !FileInUse(e.FullPath)               //make sure the file isn't in use, otherwise it can't be processed.
                    && !ignoredFiles.Contains(e.FullPath))
                {
                    if (fi.DirectoryName == TVFolder)
                    {
                        //ignore the file for now

                    }
                    else
                    {

                        #region Identify Season/Episode number
                        //Attempt to identify the season and episode number.
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


                        #endregion

                        DirectoryInfo seriesFolder = fi.Directory;

                        if (seriesFolder.Name.Contains(SeasonFolderName))
                        {
                            //If the season still isn't set, but if the folder contains "Season" (or SeasonFolderName), infer it.
                            if (SeasonID == -1)
                                SeasonID = Int32.Parse(seriesFolder.Name.Replace(SeasonFolderName, ""));
                            seriesFolder = seriesFolder.Parent;

                        }
                        String showName = seriesFolder.Name;

                        AutotronLog("New episode for '" + showName + "' found");

                        if (EpisodeID == -1)
                            AutotronLog(String.Format("Episode not Identified: '{0}'", fi.Name));
                        else
                        {
                            AutotronLog("Season: " + SeasonID.ToString() + ", Episode: " + EpisodeID.ToString());

                            if (File.Exists(seriesFolder + @"\series.xml"))
                            {
                                XmlDocument xdoc = new XmlDocument();
                                xdoc.Load(seriesFolder + "\\series.xml");
                                XmlNode node = xdoc.DocumentElement;
                                String TVShowID = node.SelectSingleNode("id").InnerText;

                                MediaScout.TVShow s = tvdb.GetTVShow(TVShowID, language);

                                TVScout = new MediaScout.TVScout();
                                TVScout.Message += new MediaScout.MediaScoutMessage.Message(Autotron_Message);
                                TVScout.options = new MediaScout.TVScoutOptions()
                                {
                                    SeasonFolderName = SeasonFolderName,
                                    RenameFiles = RenameFiles,
                                    ForceUpdate = ForceUpdate,
                                    GetSeriesPosters = GetSeriesPosters,
                                    GetSeasonPosters = GetSeasonPosters,
                                    GetEpisodePosters = GetEpisodePosters,
                                    MoveFiles = MoveFiles,
                                    RenameFormat = RenameFormat,
                                    LanguageID = LanguageID,
                                    AllowedFileTypes = AllowedFileTypes.ToArray(),
                                    AllowedSubtitles = AllowedSubtitleTypes.ToArray()
                                };
                                TVScout.series = s;
                                TVScout.ProcessFile(fi, s.Seasons[SeasonID], s.Seasons[SeasonID].Episodes[EpisodeID]);
                            }
                        }
                        //Add the new filename to the ignored list so an infinite loop isn't created
                        ignoredFiles.Add(e.FullPath);

                        //Log it
                        AutotronLog(e.FullPath);
                    }
                }
            }
        }
        void Autotron_Message(string msg, MediaScout.MediaScoutMessage.MessageType mt, DateTime time)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new MediaScout.MediaScoutMessage.Message(Autotron_Message), msg, mt, time);
                return;
            }

            Int32 TabSize = 4;
            String tab = new String(' ', TabSize);
            TextRange tr = new TextRange(rtbAutotronLog.Document.ContentEnd, rtbAutotronLog.Document.ContentEnd);
            msg = msg + "\r";

            switch (mt)
            {
                case MediaScout.MediaScoutMessage.MessageType.ProcessSeason:
                    tr.Text = tab + tab + msg;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkGreen);
                    break;

                case MediaScout.MediaScoutMessage.MessageType.ProcessEpisode:
                    tr.Text = tab + tab + tab + msg;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkGreen);
                    break;

                case MediaScout.MediaScoutMessage.MessageType.ProcessSeries:
                    tr.Text = tab + msg;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkGreen);
                    break;

                case MediaScout.MediaScoutMessage.MessageType.NormalOperation:
                    tr.Text = msg;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkGreen);
                    break;

                case MediaScout.MediaScoutMessage.MessageType.Error:
                    tr.Text = msg;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                    break;

                case MediaScout.MediaScoutMessage.MessageType.FatalError:
                    tr.Text = msg;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkRed);
                    break;

                case MediaScout.MediaScoutMessage.MessageType.CompletedOperation:
                    tr.Text = msg + "\r";
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                    break;
            }

            rtbAutotronLog.ScrollToEnd();
        }

        private void AutotronLog(String s)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new StringDelegate(AutotronLog), s);
                return;
            }

            TextRange tr = new TextRange(rtbAutotronLog.Document.ContentEnd, rtbAutotronLog.Document.ContentEnd);
            tr.Text = s + "\r";
        }

        /// <summary>
        /// Check to see if the file is in use by attempting to open or create it. It will throw a (caught) exception if it is
        /// </summary>
        /// <param name="file">File to check</param>
        /// <returns></returns>
        private bool FileInUse(String file)
        {
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate))
                {

                }
                return false;
            }
            catch
            {
                return true;
            }

        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowser myBrowser = new FolderBrowser();

            myBrowser.Title = "Select folder";
            myBrowser.Flags = BrowseFlags.BIF_NEWDIALOGSTYLE |
                              BrowseFlags.BIF_STATUSTEXT |
                              BrowseFlags.BIF_EDITBOX; ;
            System.Windows.Forms.DialogResult res = myBrowser.ShowDialog();

            if (res == System.Windows.Forms.DialogResult.OK)
            {
                txtBaseDir.Text = myBrowser.DirectoryPath;
                btnFetch.IsEnabled = true;
            }
        }

        private void btnFetch_Click(object sender, RoutedEventArgs e)
        {
            if (tvThread != null)
            {
                tvThread.Abort();
                tvThread = null;
                MetadataCompleted("Operation aborted");
            }
            else
            {

                //change UI related stuff
                btnFetch.Content = "Cancel";
                rtbLog.Document.Blocks.Clear();


                bool bulkMode = (bool)chkRoot.IsChecked;
                TVScout = new MediaScout.TVScout();
                TVScout.Message += new MediaScout.MediaScoutMessage.Message(TVScout_Message);
                TVScout.options = new MediaScout.TVScoutOptions()
                {
                    SeasonFolderName = txtSeasonFolderName.Text,
                    RenameFiles = (bool)chkRename.IsChecked,
                    ForceUpdate = (bool)chkForceUpdate.IsChecked,
                    GetSeriesPosters = (bool)chkSeriesPosters.IsChecked,
                    GetSeasonPosters = (bool)chkSeasonPosters.IsChecked,
                    GetEpisodePosters = (bool)chkEpPosters.IsChecked,
                    MoveFiles = (bool)chkMove.IsChecked,
                    RenameFormat = txtRenameFormat.Text,
                    LanguageID = lstLanguages.SelectedIndex.ToString(),
                    AllowedFileTypes = AllowedFileTypes.ToArray(),
                    AllowedSubtitles = AllowedSubtitleTypes.ToArray()
                };

                String baseDirText = txtBaseDir.Text;
                if ((bool)chkRoot.IsChecked)
                {
                    tvThread = new Thread(delegate()
                    {
                        DirectoryInfo di = new DirectoryInfo(baseDirText);
                        foreach (DirectoryInfo idi in di.GetDirectories())
                        {
                            if (!File.Exists(idi.FullName + "\\ignore"))
                            {
                                ProcessFolder(idi.FullName);
                                MetadataCompleted("Done.");
                            }
                            else
                            {
                                MetadataCompleted("Skipped " + idi.FullName);
                            }

                        }
                    });
                    tvThread.SetApartmentState(ApartmentState.STA);
                    tvThread.Start();
                }
                else
                {
                    tvThread = new Thread(delegate()
                    {
                        if (!File.Exists(baseDirText + "\\ignore"))
                        {
                            ProcessFolder(baseDirText);
                            MetadataCompleted("Done.");
                        }
                        else
                        {
                            MetadataCompleted("Skipped " + baseDirText);
                        }

                    });
                    tvThread.SetApartmentState(ApartmentState.STA);
                    tvThread.Start();
                }
            }
        }

        private void ProcessFolder(String folder)
        {
            DirectoryInfo idi = new DirectoryInfo(folder);
            TVScout_Message("Scanning " + idi.Name, MediaScout.MediaScoutMessage.MessageType.NormalOperation, DateTime.Now);

            String TVShowID = null;
            MediaScout.TVShow selected = null;


            if (File.Exists(folder + @"\series.xml"))
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(folder + "\\series.xml");
                XmlNode node = xdoc.DocumentElement;
                TVShowID = node.SelectSingleNode("id").InnerText;
            }
            else
            {
                MediaScout.TVShow[] results = tvdb.Search(idi.Name, language);

                //If there are no results, ask the user to refine their search
                if (results.Length == 0)
                {
                    NoTVResults ntvr = new NoTVResults(idi.Name);
                    //ntvr.Owner = this;
                    //ntvr.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    while (results.Length == 0 && ntvr.ShowDialog() == true)
                    {
                        results = tvdb.Search(ntvr.Term, language);
                        ntvr = new NoTVResults(ntvr.Term);
                    }
                }

                //if there is only one result, it must be this TV Show
                if (results.Length == 1)
                    TVShowID = results[0].id;

                //if there is more than one result, show the clarification dialog
                else if (results.Length > 1)
                {
                    TVShowResults tsr = new TVShowResults(results);
                    //tsr.Owner = this;
                    //tsr.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    if (tsr.ShowDialog() == true)
                        TVShowID = tsr.Selected.id;
                }

            }

            if (!String.IsNullOrEmpty(TVShowID))
            {
                try
                {
                    selected = tvdb.GetTVShow(TVShowID, language);
                }
                catch (Exception ex)
                {
                    TVScout_Message("There was an error with the TVDB, mostly likely corrupt XML. This show will not be processed.", MediaScout.MediaScoutMessage.MessageType.Error, DateTime.Now);
                }
                if (selected != null)
                    TVScout.ProcessDirectory(idi.FullName, selected);
            }
        }

        void TVScout_Message(string msg, MediaScout.MediaScoutMessage.MessageType mt, DateTime time)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new MediaScout.MediaScoutMessage.Message(TVScout_Message), msg, mt, time);
                return;
            }

            Int32 TabSize = 4;
            String tab = new String(' ', TabSize);
            TextRange tr = new TextRange(rtbLog.Document.ContentEnd, rtbLog.Document.ContentEnd);
            msg = msg + "\r";

            switch (mt)
            {
                case MediaScout.MediaScoutMessage.MessageType.ProcessSeason:
                    tr.Text = tab + tab + msg;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkGreen);
                    break;

                case MediaScout.MediaScoutMessage.MessageType.ProcessEpisode:
                    tr.Text = tab + tab + tab + msg;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkGreen);
                    break;

                case MediaScout.MediaScoutMessage.MessageType.ProcessSeries:
                    tr.Text = tab + msg;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkGreen);
                    break;

                case MediaScout.MediaScoutMessage.MessageType.NormalOperation:
                    tr.Text = msg;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkGreen);
                    break;

                case MediaScout.MediaScoutMessage.MessageType.Error:
                    tr.Text = msg;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                    break;

                case MediaScout.MediaScoutMessage.MessageType.FatalError:
                    tr.Text = msg;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkRed);
                    break;

                case MediaScout.MediaScoutMessage.MessageType.CompletedOperation:
                    tr.Text = msg + "\r";
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                    break;
            }

            rtbLog.ScrollToEnd();
        }

        private void btnChangeTVBackdrop_Click(object sender, RoutedEventArgs e)
        {
            //Changing poster if its a TVShow...
            if (tvTVShows.SelectedItem is GUITypes.TVShow)
            {
                GUITypes.TVShow s = (GUITypes.TVShow)tvTVShows.SelectedItem;

                String id = "";

                //if the ID is set, use that ID to fetch posters on
                if (s.Id != 0)
                    id = s.Id.ToString();

                //else, the GUIType doesn't have an ID (ie, if the metadata hasn't been fetched before)
                else
                {
                    MediaScout.TVShow[] results = tvdb.Search(s.Name, language);

                    TVShowResults tsr = new TVShowResults(results);
                    tsr.Owner = this;
                    tsr.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    if (tsr.ShowDialog() == true)
                        id = tsr.Selected.id;
                    else
                        id = null;
                }

                if (id != null)
                {
                    ChangeTVBackdrop ctb = new ChangeTVBackdrop(tvdb.GetPosters(id, MediaScout.Providers.TVShowPosterType.Backdrop, null));
                    ctb.Owner = this;
                    ctb.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    if (ctb.ShowDialog() == true)
                    {
                        imgTVBackdrop.Source = null;

                        String file = s.Filepath + @"\backdrop.jpg";
                        if (File.Exists(file))
                            File.Delete(file);

                        ctb.Selected.Save(file);

                        SetManagedPoster(s.Filepath + @"\backdrop.jpg", imgTVBackdrop);
                    }
                }
            }
        }

        private void btnEpisodePosterFromFile_Click(object sender, RoutedEventArgs e)
        {
            if (tvTVShows.SelectedItem is GUITypes.Episode)
            {
                GUITypes.Episode ep = (GUITypes.Episode)tvTVShows.SelectedItem;
                String postername = ep.Parent.Parent.Id.ToString() + "-" + ep.id + ".jpg";
                String directory = ep.Filepath.Substring(0, ep.Filepath.LastIndexOf(@"\")) + "\\metadata\\";

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                SamSoft.VideoBrowser.Util.VideoProcessing.ThumbCreator.CreateThumb(ep.Filepath, directory + postername, 0.25);
                ep.UpdatePoster();


                SetManagedPoster(ep.Poster, imgEpisodePoster);

            }
        }

        private void btnChangeMovieBackdrop_Click(object sender, RoutedEventArgs e)
        {
            //Extract the id (ID), if it doesn't exist, popup a search dialog!
            if (lstMovies.SelectedItem is GUITypes.Movie)
            {
                MediaScout.Providers.TheMovieDBProvider_21 tmdb = new MediaScout.Providers.TheMovieDBProvider_21();
                GUITypes.Movie m = lstMovies.SelectedItem as GUITypes.Movie;

                String id = null;

                if (m.id != null)
                    id = m.id;
                else
                {
                    MediaScout.Movie[] results = tmdb.Search(m.Name);

                    MovieResults mr = new MovieResults(results);
                    mr.Owner = this;
                    mr.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    if (mr.ShowDialog() == true)
                        id = mr.Selected.ID;
                    else
                        id = null;
                }

                if (id != null)
                {
                    ChangeMovieBackdrop cmp = new ChangeMovieBackdrop(tmdb.GetPosters(id, MediaScout.Providers.MoviePosterType.Backdrop));
                    cmp.Owner = this;
                    cmp.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    if (cmp.ShowDialog() == true)
                    {
                        imgMovieBackdrop.Source = null;

                        String file = m.Filepath + @"\backdrop.jpg";
                        if (File.Exists(file))
                            File.Delete(file);

                        cmp.Selected.Save(file);

                        SetManagedPoster(file, imgMovieBackdrop);
                    }
                }
                else
                {
                    //Cancelled 
                }
            }
        }

        private void btnSaveMovie_Click(object sender, RoutedEventArgs e)
        {
            if (lstMovies.SelectedItem is GUITypes.Movie)
            {
                GUITypes.Movie m = lstMovies.SelectedItem as GUITypes.Movie;
                MovieScout.SaveMovie(m.Moviebase, m.Filepath + @"\mymovies.xml");
            }
        }

        // JMCode
        // Close Maximize and Minimize
        void closeButtonRectangle_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void maximizeButtonRectangle_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else
                this.WindowState = WindowState.Normal;
        }
        private void minimizeButtonRectangle_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            if (e.ClickCount == 2)
            {
                if (this.WindowState == WindowState.Normal)
                    this.WindowState = WindowState.Maximized;
                else
                    this.WindowState = WindowState.Normal;
            }
        }

       

        private void ClearHeaderColors()
        {
            hdrTV.Foreground = hdrMovies.Foreground = hdrSettings.Foreground = hdr2Batch.Foreground =
                hdr2Manage.Foreground = hdr2Autotron.Foreground = new SolidColorBrush(Colors.Gray);
            hdrTV.FontSize = hdrMovies.FontSize = hdr2Batch.FontSize =
                hdr2Manage.FontSize = hdr2Autotron.FontSize = 18;
            hdrSettings.FontSize = 12;
            HideHeader2();
        }
        private void ClearHeaderColors2()
        {
            hdr2Batch.Foreground = hdr2Manage.Foreground = hdr2Autotron.Foreground =
                new SolidColorBrush(Colors.Gray);
            hdr2Batch.FontSize = hdr2Manage.FontSize = hdr2Autotron.FontSize = 18;
        }

        private void SelectTabFromHeader(Label header, int selectedTabIndex)
        {
            ClearHeaderColors();            
            header.Foreground = new SolidColorBrush(Colors.Black);
            header.FontSize = 18;
            tcTabs.SelectedIndex = selectedTabIndex;
        }

        private void SelectTabFromHeader2(Label header, int selectedTabIndex)
        {
            ClearHeaderColors2();
            ShowHeader2();
            header.Foreground = new SolidColorBrush(Colors.Black);
            header.FontSize = 18;
            tcTabs.SelectedIndex = selectedTabIndex;
        }
        private void SelectTabFromMenu(Label header, int selectedTabIndex)
        {
            ClearHeaderColors();
            header.Foreground = new SolidColorBrush(Colors.Black);
            header.FontSize = 14;
            tcTabs.SelectedIndex = selectedTabIndex;
        }

        private void hdrTV_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectTabFromHeader(hdrTV, 1);
            SelectTabFromHeader2(hdr2Manage, 1);
        }
        private void HideHeader2()
        {
            hdr2Batch.Visibility = hdr2Manage.Visibility = Visibility.Hidden;
        }
        private void ShowHeader2()
        {
            hdr2Batch.Visibility = hdr2Manage.Visibility = Visibility.Visible;
        }

        private void hdrMovies_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectTabFromHeader(hdrMovies, 2);
        }

        private void hdrSettings_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectTabFromMenu(hdrSettings, 3);
        }

        private void hdr2Batch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectTabFromHeader(hdrTV, 0);
            SelectTabFromHeader2(hdr2Batch, 0);
        }

        private void hdr2Manage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectTabFromHeader(hdrTV, 1);
            SelectTabFromHeader2(hdr2Manage, 1);
        }

        private void hdr2Autotron_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectTabFromHeader(hdr2Autotron, 4);
        }

        private void labelHelp_MouseDown(object sender, MouseButtonEventArgs e)
        {            
            //Popup About Window
            Help h = new Help();
            h.Owner = this;
            h.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            h.ShowDialog();
        }

    }
}