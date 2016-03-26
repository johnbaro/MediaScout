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
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading;
using System.Windows.Threading;
using MediaScoutGUI.GUITypes;
using System.Text.RegularExpressions;
using System.Xml;
////using System.Reflection;
////using System.ComponentModel;
using System.Diagnostics;


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
        private ObservableCollection<Movie> movies = new ObservableCollection<Movie>();
        private ObservableCollection<Episode> eplist = new ObservableCollection<Episode>();
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
        private MediaScout.Providers.TVRageProvider tvrage = new MediaScout.Providers.TVRageProvider();
        private MediaScout.Providers.EpGuideProvider epguide = new MediaScout.Providers.EpGuideProvider();
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
                MessageBox.Show(ex.Message);
            }
		}

        /// <summary>
        /// Handles the loading of various tabs as the tabs are selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tcTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Find out what tab is selected.  If the program is just loading up, then the
            // tab control might not be generated or have a selected tab yet, so we have
            // to take into account a "null" scenario.
            TabItem CurrentTab = tcTabs.SelectedItem as TabItem;
            String CurrentTabName = "";
            if (CurrentTab != null)
            {
                CurrentTabName = CurrentTab.Name;
            }

            Debug.WriteLine("Selected tab: " + CurrentTabName);

            //switch (tcTabs.SelectedIndex)
            switch (CurrentTabName)
            {
                //case "": //No tab selected
                //    break;

                case "tabTV":
                    if (Directory.Exists(TVFolder))
                        txtBaseDir.Text = TVFolder;
                    break;

                case "tabManage":
                    //Make sure the TVFolder is set before attempting to load in all the TV show data.
                    if (TVFolder == String.Empty || TVFolder == null || !Directory.Exists(TVFolder))
                    {
                        MessageBox.Show("Please set your TV folder in the Options tab");
                        break;
                    }

                    if (tvshows == null || tvshows.Count == 0 || tvTabFolder != TVFolder)
                    {
                        tvTabFolder = TVFolder;
                        loadTVShows();
                    }
                    tvTVShows.ItemsSource = dispatchtvshows;
                    break;

                case "tabDropTV":
                    // Load up the Series Name DDL
                    if (TVFolder == String.Empty || TVFolder == null || !Directory.Exists(TVFolder))
                    {
                        MessageBox.Show("Please set your TV folder in the Options tab");
                        break;
                    }

                    if (tvshows == null || tvshows.Count == 0)
                    {
                        loadTVShows();
                    }
                    cboSeriesName.ItemsSource = dispatchtvshows;
                    break;

                case "tabMovies":
                    //Make sure the MovieFolder is set before attempting to load in all the movies data.
                    if (MovieFolder == String.Empty || MovieFolder == null || !Directory.Exists(MovieFolder))
                    {
                        MessageBox.Show("Please set your Movie folder in the Options tab");
                        break;
                    }

                    if (movies == null  || movies.Count == 0)
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

        private void loadTVShows()
        {
            tvshows.Clear();
            //tvTVShows.ItemsSource = dispatchtvshows;

            //Load it on a seperate thread (hence the use of the DispatchingCollection) so as not to lag UI
            Thread thread = new Thread(delegate()
            {
                foreach (DirectoryInfo di in new DirectoryInfo(TVFolder).GetDirectories())
                {
                    try
                    {
                        GUITypes.TVShow t = new GUITypes.TVShow(di.FullName, di.Name);
                        Debug.WriteLine("Adding series folder " + t.FolderName + " (" + t.Name + ")");
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
                SetManagedPoster(s.Filepath + @"\banner.jpg", imgTVBanner);

                tbSelectedSeries.Text = s.Name;
                tbShowDesc.Text = s.Overview;
                tbActors.Text = s.Actors;
                tbAired.Text = s.FirstAired;
                tbRating.Text = s.Rating;
                tbGenres.Text = s.Genre;
                tbNetwork.Text = s.Network;

            } else if (tvTVShows.SelectedItem is GUITypes.Season)
            {
                grdSeriesView.Visibility = Visibility.Hidden;
                grdSeasonView.Visibility = Visibility.Visible;
                grdEpisodeView.Visibility = Visibility.Hidden;

                GUITypes.Season s = (GUITypes.Season)tvTVShows.SelectedItem;

                lblSelectedSeason.Content = s.Name;
                //tbSelectedSeries.Text = s.Parent.Name;
                //tbShowDesc.Text = "";
                
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
                //tbSelectedSeries.Visibility = Visibility.Visible;
                btnFetchSelectedTV.IsEnabled = true;
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
                    if (mr.ShowDialog() == true)
                        selected = mr.Selected;
                }

                //If there are no results, ask user to refine or broaden their search terms.
                // ie if a folder is "007 - From russia with love", it may not match "From Russia With Love"
                while (selected == null)
                {
                    NoMovieResults nmr = new NoMovieResults(m.Name);
                    if (nmr.ShowDialog() == true)
                    {
                        results = MovieScout.Search(nmr.Term);

                        MovieResults mr = new MovieResults(results);
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
            Debug.WriteLine("Saving options.");
            SaveOptions();
            Debug.WriteLine("Exiting.");
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
            Debug.WriteLine("Loading Options...");
            txtMoviesFolder.Text = MediaScoutGUI.Properties.Settings.Default.MovieFolder;
            txtTVFolder.Text = MediaScoutGUI.Properties.Settings.Default.TVFolder;

            TVFolder = MediaScoutGUI.Properties.Settings.Default.TVFolder;
            MovieFolder = MediaScoutGUI.Properties.Settings.Default.MovieFolder;

            /* Load GUI */
            txtSpacingChar.Text = MediaScoutGUI.Properties.Settings.Default.spaceReplacement;
            txtNumberPadding.Text = MediaScoutGUI.Properties.Settings.Default.numPadding;
            txtRenameFormat.Text = MediaScoutGUI.Properties.Settings.Default.fileformat;
            // AARGGHHH! I hate how the combobox pretends to be a simple DropDownList but ISN'T AT ALL!!
            //// Set the selected item in the cboBox by looping through the items and selecting the index of the matching tag
            //for (int row = 0; row < cboNumberPadding.Items.Count; row++)
            //{
            //    object padding = cboNumberPadding.Items[row];
            //    Debug.WriteLine("row: " + row + " tag? " + padding.ToString());
            //    if (MediaScoutGUI.Properties.Settings.Default.numPadding == padding.ToString())
            //    {
            //        Debug.WriteLine("Selecting row " + row);
            //        cboNumberPadding.SelectedIndex = row;
            //        //lvEpGuide.ScrollIntoView(lvEpGuide.SelectedItem);
            //    }
            //}
            //cboNumberPadding.Tag = MediaScoutGUI.Properties.Settings.Default.numPadding;
            //cboNumberPadding.SelectedValue = MediaScoutGUI.Properties.Settings.Default.numPadding;
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
            language = (((XmlNode)this.lstLanguages.SelectedItem).ParentNode).SelectSingleNode("abbreviation").InnerText;


            //Change tabs last as other settings need to be set first
            tcTabs.SelectedIndex = MediaScoutGUI.Properties.Settings.Default.SelectedTab;
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
            MediaScoutGUI.Properties.Settings.Default.spaceReplacement = txtSpacingChar.Text;
            MediaScoutGUI.Properties.Settings.Default.numPadding = txtNumberPadding.Text;
            //MediaScoutGUI.Properties.Settings.Default.numPadding = cboNumberPadding.SelectedItem.ToString();
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
                MediaScout.Providers.TheMovieDBProvider tmdb = new MediaScout.Providers.TheMovieDBProvider();
                GUITypes.Movie m = lstMovies.SelectedItem as GUITypes.Movie;

                String id = null;

                if (m.id != null)
                    id = m.id;
                else
                {
                    MediaScout.Movie[] results = tmdb.Search(m.Name);

                    MovieResults mr = new MovieResults(results);
                    if (mr.ShowDialog() == true)
                        id = mr.Selected.ID;
                    else
                        id = null;
                }

                if (id != null)
                {
                    ChangeMoviePoster cmp = new ChangeMoviePoster(tmdb.GetPosters(id, MediaScout.Providers.MoviePosterType.Poster));
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
                MediaScout.TVShow[] results = tvdb.Search(tbSelectedSeries.Text, language);
                if (results.Length == 0)
                {
                    Debug.WriteLine("Attempting to refine search paramaters");
                    NoTVResults ntvr = new NoTVResults(tbSelectedSeries.Text, s.Filepath, true);
                    while (results.Length == 0 && ntvr.ShowDialog() == true)
                    {
                        TVScout_Message("No results found.  Please refine the search.", MediaScout.MediaScoutMessage.MessageType.NormalOperation, DateTime.Now);
                        results = tvdb.Search(ntvr.Term, language);
                        ntvr = new NoTVResults(ntvr.Term, s.Filepath, true);
                    }
                    ntvr.Close();

                    Debug.WriteLine("Decision: " + ntvr.Decision);
                }

                if (results.Length > 0)
                {
                    MediaScout.TVShow selected = null;
                    TVShowResults tsr = new TVShowResults(results);

                    if (tsr.ShowDialog() == true)
                    {
                        Debug.WriteLine("Selected Series " + tsr.Selected.SeriesID);
                        selected = tsr.Selected;
                        Debug.WriteLine("Selected ID " + tsr.Selected.id);
                        selected = tvdb.GetTVShow(selected.id, language);

                        selected.Save(s.Filepath + "\\series.xml");
                        //Refresh the series GUI by removing and re-adding it
                        int sindex = tvshows.IndexOf(s);
                        tvshows.Remove(s);
                        s = new TVShow(s.Filepath, s.FolderName);
                        tvshows.Insert(sindex, s);
                        this.tvTVShows.SelectItem(s);

                        //Fetch the images too, allowing the user to select their favorites
                        btnChangeTVBanner_Click(null, null);
                        btnChangeTVPoster_Click(null, null);
                        btnChangeTVBackdrop_Click(null, null);
                    }
                }

            }
        }

        private void tbSelectedSeries_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnSaveSelectedTV.IsEnabled = true;
        }

        private void tbShowDesc_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnSaveSelectedTV.IsEnabled = true;
        }

        private void btnSaveSelectedTV_Click(object sender, RoutedEventArgs e)
        {
            if (tvTVShows.SelectedItem is GUITypes.TVShow)
            {
                GUITypes.TVShow s = (GUITypes.TVShow)tvTVShows.SelectedItem;
                s.Name = tbSelectedSeries.Text;
                s.Overview = tbShowDesc.Text;
                s.Actors = tbActors.Text;
                s.FirstAired = tbAired.Text;
                s.Rating = tbRating.Text;
                s.Genre = tbGenres.Text;
                s.Network = tbNetwork.Text;

                FileInfo seriesxml = new FileInfo(s.Filepath + @"\series.xml");
                if (seriesxml.Exists)
                {
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.Load(seriesxml.FullName);
                    XmlNode node = xdoc.DocumentElement;
                    node.SelectSingleNode("SeriesName").InnerText = s.Name;
                    node.SelectSingleNode("Overview").InnerText = s.Overview;
                    node.SelectSingleNode("Actors").InnerText = s.Actors;
                    node.SelectSingleNode("FirstAired").InnerText = s.FirstAired;
                    node.SelectSingleNode("Rating").InnerText = s.Rating;
                    node.SelectSingleNode("Genre").InnerText = s.Genre;
                    node.SelectSingleNode("Network").InnerText = s.Network;
                    xdoc.Save(s.Filepath + @"\series.xml");
                }
                else
                {
                    Debug.WriteLine("xml for this series does not exist.  Must fetch it first.");
                }

                //Refresh the series GUI by removing and re-adding it
                int sindex = tvshows.IndexOf(s);
                tvshows.Remove(s);
                s = new TVShow(s.Filepath, s.FolderName);
                tvshows.Insert(sindex, s);
                this.tvTVShows.SelectItem(s);
            }
        }

        private void btnStripSelectedTV_Click(object sender, RoutedEventArgs e)
        {
            if (tvTVShows.SelectedItem is GUITypes.TVShow)
            {
                GUITypes.TVShow s = (GUITypes.TVShow)tvTVShows.SelectedItem;

                String msg = "Are you sure you want to delete all Metadata and images for this series?";
                if (MessageBox.Show(msg, s.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    Debug.WriteLine("Delete all of the Metadata for series " + s.Name);
                    String file;
                    file = s.Filepath + @"\folder.jpg";
                    if (File.Exists(file)) File.Delete(file);
                    file = s.Filepath + @"\backdrop.jpg";
                    if (File.Exists(file)) File.Delete(file);
                    file = s.Filepath + @"\banner.jpg";
                    if (File.Exists(file)) File.Delete(file);
                    file = s.Filepath + @"\series.xml";
                    if (File.Exists(file)) File.Delete(file);
                    file = s.Filepath + @"\metadata";
                    if (Directory.Exists(file)) Directory.Delete(file, true);

                    //Refresh the series GUI by removing and re-adding it
                    int sindex = tvshows.IndexOf(s);
                    tvshows.Remove(s);
                    s = new TVShow(s.Filepath, s.FolderName);
                    tvshows.Insert(sindex, s);
                    this.tvTVShows.SelectItem(s);
                }
                else
                {
                    Debug.WriteLine("Leaving Metadata alone.");
                }
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
                    if (tsr.ShowDialog() == true)
                        id = tsr.Selected.id;
                    else
                        id = null;
                }

                if (id != null)
                {
                    ChangeTVPoster ctp = new ChangeTVPoster(tvdb.GetPosters(id, MediaScout.Providers.TVShowPosterType.Poster,null));
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
                                XmlDocument  xdoc = new XmlDocument();
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
                        bool ContinueProcessing = true;
                        DirectoryInfo di = new DirectoryInfo(baseDirText);

                        foreach (DirectoryInfo idi in di.GetDirectories())
                        {
                            if (!ContinueProcessing)
                            {
                                Debug.WriteLine("Aborting...");
                                break;
                            }
                            if (!File.Exists(idi.FullName + "\\ignore"))
                            {
                                ContinueProcessing = ProcessFolder(idi.FullName);
                                MetadataCompleted("Done.");
                            }
                            else
                            {
                                MetadataCompleted("Ignoring " + idi.FullName);
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
                            MetadataCompleted("Ignored " + baseDirText);
                        }

                    });
                    tvThread.SetApartmentState(ApartmentState.STA);
                    tvThread.Start();
                }
            }
        }

        private bool ProcessFolder(String folder)
        {
            DirectoryInfo idi = new DirectoryInfo(folder);
            TVScout_Message("Scanning folder " + idi.Name, MediaScout.MediaScoutMessage.MessageType.NormalOperation, DateTime.Now);

            String TVShowID = null;
            MediaScout.TVShow selected = null;
           

            if (File.Exists(folder + @"\series.xml"))
            { //We already know what series this is, pull the ID from the saved XML file
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(folder + "\\series.xml");
                XmlNode node = xdoc.DocumentElement;
                TVShowID = node.SelectSingleNode("id").InnerText;
            }
            else
            { //Series folder is unknown, let's search TheTVDb to get it's ID
                MediaScout.TVShow[] results = tvdb.Search(idi.Name, language);

                #region This whole block needs to be resueable (maybe not)
                //If there are no results, ask the user to refine their search
                if (results.Length == 0)
                {
                    Debug.WriteLine("Attempting to refine search paramaters");
                    NoTVResults ntvr = new NoTVResults(idi.Name, folder, false);
                    while (results.Length == 0 && ntvr.ShowDialog() == true)
                    {
                        TVScout_Message("No results found.  Please refine the search.", MediaScout.MediaScoutMessage.MessageType.NormalOperation, DateTime.Now);
                        results = tvdb.Search(ntvr.Term, language);
                        ntvr = new NoTVResults(ntvr.Term, folder, false);
                    }
                    ntvr.Close(); // Not sure if this is necessary...

                    Debug.WriteLine("Decision: " + ntvr.Decision);
                    switch (ntvr.Decision)
                    {
                        case NoTVResults.DecisionType.Cancel:
                            Debug.WriteLine("ProcessFolder() Canceled, returning FALSE");
                            TVScout_Message("Search Canceled.", MediaScout.MediaScoutMessage.MessageType.NormalOperation, DateTime.Now);
                            return false;
                        case NoTVResults.DecisionType.Skip:
                            Debug.WriteLine("ProcessFolder() Skipped, returning TRUE(1)");
                            TVScout_Message("Skipping " + idi.Name, MediaScout.MediaScoutMessage.MessageType.NormalOperation, DateTime.Now);
                            return true;
                        case NoTVResults.DecisionType.Ignore:
                            Debug.WriteLine("ProcessFolder() Ignored, returning TRUE(1)");
                            TVScout_Message("Ignoring " + idi.Name, MediaScout.MediaScoutMessage.MessageType.NormalOperation, DateTime.Now);
                            return true;
                        default:
                            Debug.WriteLine("ProcessFolder() Continuing...");
                            break;
                    }
                }
                #endregion

                //if there is only one result, it must be this TV Show
                if (results.Length == 1)
                    TVShowID = results[0].id;

                //if there is more than one result, show the clarification dialog
                else if (results.Length > 1)
                {
                    TVShowResults tsr = new TVShowResults(results);
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
                    Debug.WriteLine(ex.Message);
                    TVScout_Message("There was an error with the TVDB, mostly likely corrupt XML. This show will not be processed.", MediaScout.MediaScoutMessage.MessageType.Error, DateTime.Now);
                }
                if (selected != null)
                    TVScout.ProcessDirectory(idi.FullName, selected);
            }

            Debug.WriteLine("ProcessFolder() returning TRUE(2)");
            return true;
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
                    if (tsr.ShowDialog() == true)
                        id = tsr.Selected.id;
                    else
                        id = null;
                }

                if (id != null)
                {
                    ChangeTVBackdrop ctb = new ChangeTVBackdrop(tvdb.GetPosters(id, MediaScout.Providers.TVShowPosterType.Backdrop,null));
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

        private void btnChangeTVBanner_Click(object sender, RoutedEventArgs e)
        {
            //Changing banner if its a TVShow...
            if (tvTVShows.SelectedItem is GUITypes.TVShow)
            {
                GUITypes.TVShow s = (GUITypes.TVShow)tvTVShows.SelectedItem;

                String id = "";

                //if the ID is set, use that ID to fetch images on
                if (s.Id != 0)
                    id = s.Id.ToString();

                //else, the GUIType doesn't have an ID (ie, if the metadata hasn't been fetched before)
                else
                {
                    MediaScout.TVShow[] results = tvdb.Search(s.Name, language);

                    TVShowResults tsr = new TVShowResults(results);
                    if (tsr.ShowDialog() == true)
                        id = tsr.Selected.id;
                    else
                        id = null;
                }

                if (id != null)
                {
                    ChangeTVBanner ctb = new ChangeTVBanner(tvdb.GetPosters(id, MediaScout.Providers.TVShowPosterType.Banner, null));
                    if (ctb.ShowDialog() == true)
                    {
                        imgTVBanner.Source = null;

                        String file = s.Filepath + @"\banner.jpg";
                        if (File.Exists(file))
                            File.Delete(file);

                        ctb.Selected.Save(file);

                        SetManagedPoster(s.Filepath + @"\banner.jpg", imgTVBanner);
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
                String directory = ep.Filepath.Substring(0,ep.Filepath.LastIndexOf(@"\")) + "\\metadata\\";

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
                MediaScout.Providers.TheMovieDBProvider tmdb = new MediaScout.Providers.TheMovieDBProvider();
                GUITypes.Movie m = lstMovies.SelectedItem as GUITypes.Movie;

                String id = null;

                if (m.id != null)
                    id = m.id;
                else
                {
                    MediaScout.Movie[] results = tmdb.Search(m.Name);

                    MovieResults mr = new MovieResults(results);
                    if (mr.ShowDialog() == true)
                        id = mr.Selected.ID;
                    else
                        id = null;
                }

                if (id != null)
                {
                    ChangeMovieBackdrop cmp = new ChangeMovieBackdrop(tmdb.GetPosters(id, MediaScout.Providers.MoviePosterType.Backdrop));
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

        public static string ExtractFilename(string filepath)
        {
            // If path ends with a "\", it's a path only so return String.Empty.
            if (filepath.Trim().EndsWith(@"\"))
                return String.Empty;

            // Determine where last backslash is.
            int position = filepath.LastIndexOf('\\');
            // If there is no backslash, assume that this is a filename.
            if (position == -1)
            {
                // Determine whether file exists in the current directory.
                if (File.Exists(Environment.CurrentDirectory + System.IO.Path.DirectorySeparatorChar + filepath))
                    return filepath;
                else
                    return String.Empty;
            }
            else
            {
                // Determine whether file exists using filepath.
                if (File.Exists(filepath))
                    // Return filename without file path.
                    return filepath.Substring(position + 1);
                else
                    return String.Empty;
            }
        }

        private void blankDropTVfields()
        {
            tbTVFileSource.Text = "";
        }

        private void chkSeriesNameLock_CheckChanged(object sender, RoutedEventArgs e)
        {
            cboSeriesName.IsEnabled = !(bool)chkSeriesNameLock.IsChecked;
        }

        private void CheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            //cboSuffix.IsEnabled = !(bool)chkSuffixLock.IsChecked;
            tbSuffix.IsEnabled = !(bool)chkSuffixLock.IsChecked;
        }

        private void ManageTVDrop(object sender, System.Windows.DragEventArgs e)
        {
        	string[] droppedFilesPaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
			foreach (string droppedFilePath in droppedFilesPaths)
			{
				Debug.WriteLine("Dropped "+ droppedFilePath);
                TVEpisodeDroped(droppedFilePath);
			}				
        }

        private void TVEpisodeDroped(string droppedFilePath)
        {
            // validate that the dropped file is something we can work with
            if (setDroppedFileSource(droppedFilePath))
            {  // work with it then.
                setDroppedEpisodeDetails();
                //updateTVFileTarget();
            }
        }

        private bool setDroppedFileSource(string droppedFilePath)
        {
            if ( System.IO.File.Exists(droppedFilePath) )
            {
                // ToDo: do some honest sanity checking like is this a file type we can
                //       work with?  Does it exist?  Is it read-only?
                tbTVFileTarget.Text = "";
                tbTVFileSource.Text = droppedFilePath;
                return true;
            }
            else
            {
                Debug.WriteLine(droppedFilePath +" NOT FOUND");
                blankDropTVfields();
                return false;
            }
        }

        private void setDroppedEpisodeDetails()
        {
            // Get the filename of the dropped episode
            string droppedFileName = ExtractFilename(tbTVFileSource.Text);
            if (droppedFileName.Length < 3)
            {   // the filename is invalid or blank, quit.
                return;
            }

            // Let's fill in some of the fields based on what we can find in the filename
            Hashtable epDetails = new Hashtable();
            epDetails = parseEpisodeDetails(droppedFileName);

            tbSeasonNum.Text = epDetails["s"].ToString();
            tbEpNum.Text = epDetails["e"].ToString();
            tbEpName.Text = epDetails["episode"].ToString();

            // NOTE: It is important to set the Series name after the Seaon and Episode numbers because
            // setting the series name could trigger the "selection changed" event that Auto-Searches
            // the series and Auto-Selects the episode.  If the season and episode is set first, the
            // auto-select  will use whatever values are currently there.
            if ((epDetails["series"].ToString().Length > 0) && (!(bool)chkSeriesNameLock.IsChecked))
            {
                // I don't need to loop through the combobox.  By setting the text, the UI will
                // auto select any item that matches it
                //
                // MAYBE I DO
                // Turns out if the case is not the same, the auto-select fails.
                // For example, "breaking bad" != "Breaking Bad" and "Battlestar Galactica" != "Battlestar Galactica (2003)"
                //
                //foreach (object item in cboSeriesName.Items)
                //{
                //    GUITypes.TVShow s = (GUITypes.TVShow)item;
                //    Debug.WriteLine(epDetails["series"].ToString() + " == " + s.Name);
                //}

                cboSeriesName.Text = epDetails["series"].ToString();
            }

            // ToDo: Pre-populate the list of known "suffixes" to match against
            if ((epDetails["suffix"].ToString().Length > 0) && (!(bool)chkSuffixLock.IsChecked))
            {
                //cboSuffix.Text = epDetails["suffix"].ToString();
                tbSuffix.Text = epDetails["suffix"].ToString();
                //ToDo: Match the Suffix to the contents of the DDL, if it's there
            }

        }

        private Hashtable parseEpisodeDetails(string episodeFileName)
        {
            Debug.WriteLine("Parsing episode details from " + episodeFileName);
            //Regex ptrn;
            //ptrn = new Regex("(.+)[.\\-_]s?(\\d{1,2})\\.?(e|x)?(\\d{2,3})[.\\-_](.*)\\-(.*)\\.\\w{3}", RegexOptions.IgnoreCase);

            // Maybe should be a class or just an extention of the Episode class
            Hashtable epInfo = new Hashtable();
            epInfo.Add("series", "");
            epInfo.Add("s", "");
            epInfo.Add("e", "");
            epInfo.Add("episode", "");
            epInfo.Add("suffix", "");

            Regex rgx;
            Match rxm;

            // looks for something like series.s1e07.episode-group.avi
            rgx = new Regex("(.+)[.\\-_]s?(\\d{1,2})\\.?(e|x)?(\\d{2,3})[.\\-_](.*)\\-(.*)\\.\\w{3}", RegexOptions.IgnoreCase);
            rxm = rgx.Match(episodeFileName);
            if (rxm.Success)
            {
                Debug.WriteLine("identified pattern: series.s1e07.episode-group.avi");
                //Debug.WriteLine("matched: " + rxm.Value + " (" + rxm.Groups.Count + ")");
                //Debug.WriteLine("grp1: " + rxm.Groups[1].ToString());
                // file name matches the expected format, get the release info out of it
                epInfo["series"] = scrubForDisplay(rxm.Groups[1].ToString());
                epInfo["s"] = rxm.Groups[2].ToString();
                epInfo["e"] = rxm.Groups[4].ToString();
                epInfo["episode"] = scrubForDisplay(rxm.Groups[5].ToString());
                epInfo["suffix"] = rxm.Groups[6].ToString().ToLower();
                return epInfo;
            }

            // looks for something like series.s1e07.episode.avi
            rgx = new Regex("(.+)[.\\-_]s?(\\d{1,2}?)(e|x)?(\\d{1,3})[._](.*)\\.\\w{3}", RegexOptions.IgnoreCase);
            rxm = rgx.Match(episodeFileName);
            if (rxm.Success)
            {
                Debug.WriteLine("identified pattern: series.s1e07.episode.avi");
                epInfo["series"] = scrubForDisplay(rxm.Groups[1].ToString());
                epInfo["s"] = rxm.Groups[2].ToString();
                epInfo["e"] = rxm.Groups[4].ToString();
                epInfo["episode"] = scrubForDisplay(rxm.Groups[5].ToString());
                return epInfo;
            }

            // Asumes something like series.107-group.avi
            rgx = new Regex("(.+)[.\\-_](\\d)(\\d{1,2})\\-(.*)\\.\\w{3}", RegexOptions.IgnoreCase);
            rxm = rgx.Match(episodeFileName);
            if (rxm.Success)
            {
                Debug.WriteLine("identified pattern: series.107-group.avi");
                epInfo["series"] = scrubForDisplay(rxm.Groups[1].ToString());
                epInfo["s"] = rxm.Groups[2].ToString();
                epInfo["e"] = rxm.Groups[3].ToString();
                epInfo["suffix"] = rxm.Groups[4].ToString().ToLower();
                return epInfo;
            }

            // looks for something like series.1x07.avi
            rgx = new Regex("(.+)[.\\-_]s?(\\d{1,2})(e|x)?(\\d{1,3})(.*)\\.\\w{3}", RegexOptions.IgnoreCase);
            rxm = rgx.Match(episodeFileName);
            if (rxm.Success)
            {
                Debug.WriteLine("identified pattern: series.1x07.avi");
                epInfo["series"] = scrubForDisplay(rxm.Groups[1].ToString());
                epInfo["s"] = rxm.Groups[2].ToString();
                epInfo["e"] = rxm.Groups[4].ToString();
                return epInfo;
            }

            // If we're reaching this point, we haven't ben able to identify any details
            // based on the filename, so we'll just be returning all blanks.
            return epInfo;
        }

        private String scrubForDisplay(String text)
        {
            // Values taken from a filename might need some touching up
            //String text;

            //// Series Name
            //text = epInfo["series"].ToString();
            //Debug.WriteLine("Scrubbing Series Name: " + text);
            //text = htmlSpecialCharReplace(text);
            //text = text.Replace('.', ' ');
            //text = text.Replace('_', ' ');
            //text = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);
            //Debug.WriteLine("Scrubbed Series Name: " + text);
            //epInfo["series"] = text;
            
            ////// Episode Name
            //text = epInfo["episode"].ToString();
            Debug.WriteLine("Scrubbing text: " + text);
            text = htmlSpecialCharReplace(text);

            // Prefix multi-part episode names
            // takes "Episode (2)" and turns it into "P2 Episode"
            text = Regex.Replace(text, "(.+)\\((\\d)\\)$", "P$2 $1").Trim();

            // Shorten long hour descriptions
            // examples... "8:00 AM" and "8:00 A.M." becomes "8am"
            // note: using 2 regexes to make the "AM" and "PM" lowercase without getting fancy
            text = Regex.Replace(text, "(\\d{1,2}):00\\s?a\\.?m\\.?", "$1am", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "(\\d{1,2}):00\\s?p\\.?m\\.?", "$1pm", RegexOptions.IgnoreCase);

            // Strip dots when preceeded by letters and not followed by another dot
            // cleans up abreviations "A.K.A.", "Dr.", "Mrs." but leaves "2.0" and "..." alone
            text = text.Replace("([a-zA-Z])\\.[^\\.]", "$1");

            // make sure ellipsis are no more than 3 dots, and they are not book ended with spaces
            text = text.Replace("(\\.\\.\\.+\\.)", "...");
            text = text.Replace("([\\.]{2})\\s", "$1");
            text = text.Replace("\\s([\\.]{2})", "$1");

            // Trade spaced hyphens with " to ".  Not sure if I am going to like this one.
            // examples: "one - six" becomes "one to six"
            text = text.Replace("\\s\\-\\s", " to ");

            text = htmlSpecialCharReplace(text);
            text = text.Replace('.', ' ');
            text = text.Replace('_', ' ');
            text = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);

            string tmpText = "";
            string alwaysLower = "a;and;at;but;for;from;in;is;it;nor;of;on;or;so;the;to";
            foreach (string t in text.Split(' '))
            {
                string word = t;
                foreach (string lowWord in alwaysLower.Split(';'))
                {
                    if (t.ToLower() == lowWord)
                    {
                        word = lowWord;
                        break;
                    }
                }
                tmpText = (tmpText + " " + word);
            }
            text = tmpText.Trim();

            Debug.WriteLine("Scrubbed Text: " + text);
            //epInfo["episode"] = text;

            //return epInfo;
            return text;
        }

        public string htmlSpecialCharReplace(String text)
        {
            text = Regex.Replace(text, "&#039;", "'", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "&amp;", "&", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "&cent;", "cent", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "&bull;", "*", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "&lsaquo;", "<", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "&rsaquo;", ">", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "&trade;", "(tm)", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "&frasl;", "/", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "&lt;", "<", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "&gt;", ">", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "&copy;", "(c)", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "&reg;", "(r)", RegexOptions.IgnoreCase);
            // This should strip all the others (instead of replacing them), which seems kinda "dangerous"
            // text = Regex.Replace(text, "&(.{2,6});", String.Empty, RegexOptions.IgnoreCase)
            return text;
        }


        private void btnSearchSeries_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            fetchTheTVDB(cboSeriesName.Text);
            AutoSelectEpisode();
        }


        private void btnsSearch_theTVDB_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            fetchTheTVDB(cboSeriesName.Text);
            AutoSelectEpisode();
        }

        // I need to move TVSeries to a more global (or class private) var

        private void fetchTheTVDB(String SeriesName)
        {
            Debug.WriteLine("Searching for " + SeriesName + " on TheTVDB");
            MediaScout.TVShow[] results = tvdb.Search(SeriesName);

            //If there are no results, ask the user to refine their search
            if (results.Length == 0)
            {
                Debug.WriteLine("Attempting to refine search paramaters");
                NoTVResults ntvr = new NoTVResults(SeriesName, null, true);
                while (results.Length == 0 && ntvr.ShowDialog() == true)
                {
                    TVScout_Message("No results found.  Please refine the search.", MediaScout.MediaScoutMessage.MessageType.NormalOperation, DateTime.Now);
                    results = tvdb.Search(ntvr.Term, language);
                    ntvr = new NoTVResults(ntvr.Term, null, false);
                }
                ntvr.Close(); // Not sure if this is necessary...

                if (ntvr.Decision == NoTVResults.DecisionType.Cancel)
                {
                    Debug.WriteLine("Search Cancelled.");
                    return;
                }
            }
            
            MediaScout.TVShow TVSeries = new MediaScout.TVShow();

            //if there is only one result, it must be this TV Show
            if (results.Length == 1)
                TVSeries = results[0];

            //if there is more than one result, show the clarification dialog
            else if (results.Length > 1)
            {
                Debug.WriteLine("Found " + results.Length + " possible matches.");
                TVShowResults tsr = new TVShowResults(results);
                if (tsr.ShowDialog() == true)
                    TVSeries = tsr.Selected;
            }

            Debug.WriteLine("TVShow ID: " + TVSeries.id);
            // ToDo: Somewhere I need to pre-populate the SeriesName cbo box with the list
            // of all known tvseries.  Then I can try selecting it and auto-pickup it's SeriesID
            if ((TVSeries.SeriesName.Length > 0) && (!(bool)chkSeriesNameLock.IsChecked))
            {
                cboSeriesName.Text = TVSeries.SeriesName;
                //ToDo: Match the Series to the contents of the DDL, if it's there
            }

            //Now that we have the Series ID, let's actually fetch the episode list...
            TVSeries = tvdb.GetTVShow(TVSeries.id);
            Debug.WriteLine("Seasons found: " + TVSeries.Seasons.Count);
            LoadEpList(TVSeries);
        }


        private void btnsSearch_EpGuides_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // A (failed) attempt to run the search in a background thread to prevent
            // the UI from locking.  I think I need a DispatchingCollection or something.
            Thread thread = new Thread(
                    new ThreadStart(() =>
                    {
                        Dispatcher.BeginInvoke(
                            DispatcherPriority.Background,
                            new Action(
                                delegate()
                                {
                                    fetchEpGuide(cboSeriesName.Text);
                                    AutoSelectEpisode();
                                }
                            )
                        );
                    }
                    )
                );
            Debug.WriteLine("Starting fetching thread");
            thread.Start();
            Debug.WriteLine("Fetch thread started");
        }

        private void fetchEpGuide(String SeriesName)
        {
            // EpGuides doesn't have a real search, it just returns the HTML page, if it finds it
            Debug.WriteLine("Searching for " + SeriesName + " on EpGuides");
            MediaScout.TVShow[] results = epguide.Search(SeriesName);

            MediaScout.TVShow TVSeries = new MediaScout.TVShow();

            //if there is only one result, it must be this TV Show
            if (results.Length == 1)
                TVSeries = results[0];

            //if there is more than one result, show the clarification dialog
            else if (results.Length > 1)
            {
                Debug.WriteLine("Found " + results.Length + " possible matches.");
                TVShowResults tsr = new TVShowResults(results);
                if (tsr.ShowDialog() == true)
                    TVSeries = tsr.Selected;
            }

            Debug.WriteLine("TVShow ID: " + TVSeries.id);
            // ToDo: Somewhere I need to pre-populate the SeriesName cbo box with the list
            // of all known tvseries.  Then I can try selecting it and auto-pickup it's SeriesID
            if ((TVSeries.SeriesName.Length > 0) && (!(bool)chkSeriesNameLock.IsChecked))
            {
                cboSeriesName.Text = TVSeries.SeriesName;
                //ToDo: Match the Series to the contents of the DDL, if it's there
            }

            //Now that we have the Series ID, let's actually fetch the episode list...
            //NOTE: The EpGuideProvider actually fetches the list using Search, but
            //      GetTVShow pases the HTML and returns the TVShow type.
            TVSeries = epguide.GetTVShow(TVSeries.id);
            Debug.WriteLine("Seasons found: " + TVSeries.Seasons.Count);
            LoadEpList(TVSeries);
        }


        private void btnsSearch_TVRage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //ToDo: Once I have the combo box loading the known tv series, I should already have the
            // series ID, so I could pass that along to skip the need for searching for the series
            fetchTVRage(cboSeriesName.Text);
            AutoSelectEpisode();
        }

        private void fetchTVRage(String SeriesName)
        {
            Debug.WriteLine("Searching for " + SeriesName + " on TVRage");
            MediaScout.TVShow[] results = tvrage.Search(SeriesName);

            //If there are no results, ask the user to refine their search
            if (results.Length == 0)
            {
                Debug.WriteLine("Attempting to refine search paramaters");
                NoTVResults ntvr = new NoTVResults(SeriesName, null, true);
                while (results.Length == 0 && ntvr.ShowDialog() == true)
                {
                    TVScout_Message("No results found.  Please refine the search.", MediaScout.MediaScoutMessage.MessageType.NormalOperation, DateTime.Now);
                    results = tvrage.Search(ntvr.Term, language);
                    ntvr = new NoTVResults(ntvr.Term, null, false);
                }
                ntvr.Close(); // Not sure if this is necessary...

                if (ntvr.Decision == NoTVResults.DecisionType.Cancel)
                {
                    Debug.WriteLine("Search Cancelled.");
                    return;
                }
            }

            MediaScout.TVShow TVSeries = new MediaScout.TVShow();

            //if there is only one result, it must be this TV Show
            if (results.Length == 1)
                TVSeries = results[0];

            //if there is more than one result, show the clarification dialog
            else if (results.Length > 1)
            {
                Debug.WriteLine("Found " + results.Length + " possible matches.");
                TVShowResults tsr = new TVShowResults(results);
                if (tsr.ShowDialog() == true)
                    TVSeries = tsr.Selected;
            }

            Debug.WriteLine("TVShow ID: " + TVSeries.id);
            // ToDo: Somewhere I need to pre-populate the SeriesName cbo box with the list
            // of all known tvseries.  Then I can try selecting it and auto-pickup it's SeriesID
            if ((TVSeries.SeriesName.Length > 0) && (!(bool)chkSeriesNameLock.IsChecked))
            {
                cboSeriesName.Text = TVSeries.SeriesName;
                //ToDo: Match the Series to the contents of the DDL, if it's there
            }

            //Now that we have the Series ID, let's actually fetch the episode list...
            TVSeries = tvrage.GetTVShow(TVSeries.id);
            Debug.WriteLine("Seasons found: " + TVSeries.Seasons.Count);
            LoadEpList(TVSeries);
        }


        private void LoadEpList(MediaScout.TVShow TVSeries)
        {
            eplist.Clear();

            for (int seCount = 0; seCount < TVSeries.Seasons.Count; seCount++)
            { // Get the Key for each season in this Series
                int seKey = TVSeries.Seasons.Keys[seCount];
                for (int epCount = 0; epCount < TVSeries.Seasons[seKey].Episodes.Count; epCount++)
                { // Get the Key for each Episode in this Season
                    int epKey = TVSeries.Seasons[seKey].Episodes.Keys[epCount];
                    String _SE = int.Parse(TVSeries.Seasons[seKey].Episodes[epKey].EpisodeNumber).ToString();
                    if (_SE.Length == 1)
                    {
                        // add the leading space to single digit episode numbers
                        _SE = (int.Parse(TVSeries.Seasons[seKey].Episodes[epKey].SeasonNumber) + ("- " + _SE));
                    }
                    else
                    {
                        _SE = (int.Parse(TVSeries.Seasons[seKey].Episodes[epKey].SeasonNumber) + ("-" + _SE));
                    }

                    eplist.Add(
                        new Episode()
                        {
                            SE = _SE,
                            ProdNum = TVSeries.Seasons[seKey].Episodes[epKey].ProductionCode,
                            AirDate = TVSeries.Seasons[seKey].Episodes[epKey].FirstAired,
                            Epname = TVSeries.Seasons[seKey].Episodes[epKey].EpisodeName
                        });
                }
            }

            Debug.WriteLine("Found " + eplist.Count + " episodes.");
            lvEpGuide.ItemsSource = eplist;
        }

        private void AutoSelectEpisode()
        {
            Debug.WriteLine("Number of items in ListView: " + lvEpGuide.Items.Count);

            //determine if the list is populated
            if (lvEpGuide.Items.Count < 1) {
                Debug.WriteLine("No items found in ListView, cannot AutoSelect Episode.");
                return;
            }

            //determine if the Season textbox is populated
            if (tbSeasonNum.Text == "") {
                Debug.WriteLine("Season Number not specified in UI, cannot AutoSelect Episode.");
                return;
            }

            // determine if the Episode textbox is populated
            if (tbEpNum.Text == "") {
                Debug.WriteLine("Episode Number not specified in UI, cannot AutoSelect Episode.");
                return;
            }

            // temporarily cast as an int to strip any leading 0's (white space should already be gone)
            String SE = int.Parse(tbEpNum.Text).ToString();
            if (SE.Length == 1) {
                // add the leading space to single digit episode numbers
                SE = (int.Parse(tbSeasonNum.Text) + ("- " + SE));
            }
            else {
                SE = (int.Parse(tbSeasonNum.Text) + ("-" + SE));
            }
            Debug.WriteLine("Looking for "+ SE + " in ListView");

            for (int eplrow = 0; eplrow < eplist.Count; eplrow++)
            {
                if (SE == eplist[eplrow].SE)
                {
                    Debug.WriteLine("Auto-Selecting row " + eplrow);
                    lvEpGuide.SelectedIndex = eplrow;
                    lvEpGuide.ScrollIntoView(lvEpGuide.SelectedItem);
                }
            }
        }

        private void lvEpGuide_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int epIndex = lvEpGuide.SelectedIndex;
            if (epIndex >= 0)
            {
                Debug.WriteLine("Selected row " + lvEpGuide.SelectedIndex);
                tbEpName.Text = eplist[epIndex].Epname;
            }
        }

        private void btnClearList_Click(object sender, RoutedEventArgs e)
        {
            eplist.Clear();
        }

        //private bool boolSeriesName_TextEntered = false;

        private void cboSeriesName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("Combobox Selection changed!");
            //if (boolSeriesName_TextEntered)
            //{
            //    Debug.WriteLine("Text was entered, blocking auto-search.");
            //    // this event fired because the cboBox has text entered into it in
            //    // which case I don't want to do any auto-searching.  Reset the bool
            //    // and quit.
            //    boolSeriesName_TextEntered = false;
            //    return;
            //}

            int shIndex = cboSeriesName.SelectedIndex;
            Debug.WriteLine("cboSeriesName selected: " + shIndex.ToString());
            Debug.WriteLine("cboSeriesName text: " + cboSeriesName.Text);
            if (shIndex > -1)
            {
                Debug.WriteLine("Show name: " + tvshows[shIndex].Name);
                Debug.WriteLine("Show ID: " + tvshows[shIndex].Id);

                if (chkAutosearch.IsChecked == true && tvshows[shIndex].Id > 0)
                {
                    Debug.WriteLine("Autosearching TheTVDB for " + tvshows[shIndex].Id);
                    MediaScout.TVShow TVSeries = new MediaScout.TVShow();
                    TVSeries = tvdb.GetTVShow(tvshows[shIndex].Id.ToString());
                    Debug.WriteLine("Seasons found: " + TVSeries.Seasons.Count);
                    LoadEpList(TVSeries);
                    AutoSelectEpisode();
                }
            }
            //else if (chkAutosearch.IsChecked == true && cboSeriesName.Text != "")
            //{
            //    // The DDL selection has changed but it doesn't match a show in the list
            //    Debug.WriteLine("Autosearching TheTVDB for " + cboSeriesName.Text);
            //    fetchTheTVDB(cboSeriesName.Text);
            //    AutoSelectEpisode();
            //}

            //if (TVFolder == String.Empty || TVFolder == null || !Directory.Exists(TVFolder))
            //{
            //    MessageBox.Show("Please set your TV folder in the Options tab");
            //}

            // let's figure out the target file name now
            //tbTVFileTarget.Text = "";
        }

        private void cboSeriesName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Debug.WriteLine("Combobox text changed!");
            //// Prevent auto-searchnig based on text entry on the combo box
            //boolSeriesName_TextEntered = true;
            updateTVFileTarget();
        }

        private void tbEpName_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateTVFileTarget();
        }

        private void tbSuffix_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateTVFileTarget();
        }

        private void tbSeasonNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateTVFileTarget();
        }

        private void tbEpNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateTVFileTarget();
        }

        private String makeEpisodeTargetName(String SeriesName, String SeasonNum, String EpisodeName, String EpisodeNum, String Suffix, String FileExtention)
        {
            //Wrap this whole thing in a TRY/CATCH because an invalid format pettern would cause a crash
            try
            {
                //If the user has specified a "spacing character", then replace all whitespace with it
                if (txtSpacingChar.Text != "")
                {
                    SeriesName = Regex.Replace(SeriesName, "\\s", txtSpacingChar.Text);
                    EpisodeName = Regex.Replace(EpisodeName, "\\s", txtSpacingChar.Text);
                    Suffix = Regex.Replace(Suffix, "\\s", txtSpacingChar.Text);
                }

                RenameFormat = txtRenameFormat.Text;
                //String renameFormat = RenameFormat;
                if (Suffix == "")
                {
                    //Strip out the suffix from the rename format, alog with any leading characters that are not {varibles}
                    RenameFormat = Regex.Replace(RenameFormat, @"([^\}]*?\{4\}[^\{]*)", @"");
                }

                //I couldn't figure out how to get the combobox to comply with my wishes
                // so I took the cheap route and used a dangerous text box.
                String TargetName = String.Format(RenameFormat + FileExtention,
                    SeriesName,
                    SeasonNum.PadLeft(Int32.Parse(txtNumberPadding.Text), '0'),
                    EpisodeName,
                    EpisodeNum.PadLeft(Int32.Parse(txtNumberPadding.Text), '0'),
                    Suffix);
                //String TargetName = String.Format(RenameFormat + FileExtention,
                //    SeriesName,
                //    SeasonNum.PadLeft(Int32.Parse(cboNumberPadding.SelectedValue.ToString()), '0'),
                //    EpisodeName,
                //    EpisodeNum.PadLeft(Int32.Parse(cboNumberPadding.SelectedValue.ToString()), '0'),
                //    Suffix);
                // Should do sone "file safe" character checking here (or earlier)
                return TargetName;
            }
            catch
            {
                // Could probably do something more graceful
                return null;
            }
        }

        private void updateTVFileTarget()
        {
            Debug.WriteLine("Setting target file...");
            if (tbTVFileSource.Text == "")
            {
                Debug.WriteLine("... source File empty, quitting.");
                return;
            }
            if (tbSeasonNum.Text == "")
            {
                Debug.WriteLine("... season number empty, quitting.");
                return;
            }
            if (tbEpNum.Text == "")
            {
                Debug.WriteLine("... episode number empty, quitting.");
                return;
            }

            // Make sure the text box is blank since it's used as a sanity check later.
            tbTVFileTarget.Text = "";

            // Generate the target file using the Rename File setting on the option tab
            String TargetName = System.IO.Path.GetExtension(tbTVFileSource.Text);
            TargetName = makeEpisodeTargetName(cboSeriesName.Text, tbSeasonNum.Text, tbEpName.Text, tbEpNum.Text, tbSuffix.Text, TargetName);
            if (TargetName == "")
            {
                Debug.WriteLine("... unable to generate Target Name, quitting.");
                return;
            }
            else
            {
                Debug.WriteLine("... " + TargetName);
            }

            // Include the Series and Season folders in the target path
            int shIndex = cboSeriesName.SelectedIndex;
            if (shIndex == -1)
            {
                Debug.WriteLine("New or Unknown Series.");
                // This is going to need some "file safe" text box scrubbing
                tbTVFileTarget.Text = TVFolder + "\\" + cboSeriesName.Text + "\\" + SeasonFolderName + "." + tbSeasonNum.Text + "\\" + TargetName;
            }
            else
            {
                Debug.WriteLine("Existing Series: " + tvshows[shIndex].FolderName);
                // See if we can find the season that this episode belongs to
                Regex rSeasons = new Regex(SeasonFolderName + ".{0,1}([0-9]+)", RegexOptions.IgnoreCase);
                String SeasonNum = "";
                for (int seCount = 0; seCount < tvshows[shIndex].Seasons.Count; seCount++)
                {
                    //Debug.WriteLine("Name: " + tvshows[shIndex].Seasons[seCount].Name);
                    Debug.WriteLine("Folder: " + tvshows[shIndex].Seasons[seCount].Filepath);
                    MatchCollection mc = rSeasons.Matches(tvshows[shIndex].Seasons[seCount].Name);
                    if (mc.Count > 0)
                    {
                        SeasonNum = mc[0].Groups[1].Captures[0].Value;
                        Debug.WriteLine("Season " + SeasonNum);
                        if (int.Parse(SeasonNum) == int.Parse(tbSeasonNum.Text))
                        {
                            Debug.WriteLine("Found it! " + tvshows[shIndex].Seasons[seCount].Filepath + "\\" + TargetName);
                            tbTVFileTarget.Text = tvshows[shIndex].Seasons[seCount].Filepath + "\\" + TargetName;
                            seCount = tvshows[shIndex].Seasons.Count; // forces an exit of the loop
                        }
                    }
                }

                if (tbTVFileTarget.Text == "")
                {
                    Debug.WriteLine("Season not found, new? " + tvshows[shIndex].FolderName + "\\" + SeasonFolderName + "." + tbSeasonNum.Text + "\\" + TargetName);
                    tbTVFileTarget.Text = tvshows[shIndex].FolderName + "\\" + SeasonFolderName + "." + tbSeasonNum.Text + "\\" + TargetName;
                }
            }

        }

        private void updateTVFileTarget_BAK()
        {
            Debug.WriteLine("Setting target file...");
            if (tbTVFileSource.Text == "")
            {
                Debug.WriteLine("... source File empty, quitting.");
                return;
            }
            if (tbSeasonNum.Text == "")
            {
                Debug.WriteLine("... season number empty, quitting.");
                return;
            }
            if (tbEpNum.Text == "")
            {
                Debug.WriteLine("... episode number empty, quitting.");
                return;
            }

            // Make sure the text box is blank since it's used as a sanity check later.
            tbTVFileTarget.Text = "";

            //// Try renaming the target file using the Rename File setting on the option tab
            ////String renameFormat = RenameFormat;
            ////if (tbSuffix.Text == "")
            ////{
            ////    //Strip out the suffix from the rename format, alog with any leading characters that are not {varibles}
            ////    renameFormat = Regex.Replace(renameFormat, @"([^\}]*?\{4\}[^\{]*)", @"");
            ////}
            ////
            //////Let's pull the file extention from the Source name
            ////String TargetName = System.IO.Path.GetExtension(tbTVFileSource.Text);
            ////
            ////try
            ////{
            ////    if (txtSpacingChar.Text == "")
            ////    {
            ////        TargetName = String.Format(txtRenameFormat.Text,
            ////            cboSeriesName.Text,
            ////            tbSeasonNum.Text.PadLeft(2, '0'),
            ////            tbEpName.Text,
            ////            tbEpNum.Text.PadLeft(2, '0'),
            ////            tbSuffix.Text)
            ////            + TargetName;
            ////    }
            ////    else
            ////    {
            ////        TargetName = String.Format(txtRenameFormat.Text,
            ////            cboSeriesName.Text.Replace(' ', txtSpacingChar.Text[0]),
            ////            tbSeasonNum.Text.PadLeft(2, '0'),
            ////            tbEpName.Text.Replace(' ', txtSpacingChar.Text[0]),
            ////            tbEpNum.Text.PadLeft(2, '0'),
            ////            tbSuffix.Text.Replace(' ', txtSpacingChar.Text[0])
            ////            + TargetName);
            ////    }
            ////}
            ////catch
            ////{
            ////    Debug.WriteLine("Unable to set target name");
            ////}

            //Let's pull the file extention off the Source name
            String TargetName = System.IO.Path.GetExtension(tbTVFileSource.Text);
            TargetName = "[Series].S[Season]E[Episode].[EpName]-[Suffix]" + TargetName;
            
            TargetName = TargetName.Replace("[Series]", cboSeriesName.Text.Replace(' ', '.'));
            TargetName = TargetName.Replace("[Season]", tbSeasonNum.Text);
            TargetName = TargetName.Replace("[Episode]", tbEpNum.Text);
            TargetName = TargetName.Replace("[EpName]", tbEpName.Text.Replace(' ', '.'));
            if (tbSuffix.Text == "")
            {
                TargetName = TargetName.Replace("-[Suffix]", "");
            }
            else
            {
                TargetName = TargetName.Replace("[Suffix]", tbSuffix.Text);
            }

            Debug.WriteLine("... " + TargetName);

            // Include the Series and Season folders in the target path
            int shIndex = cboSeriesName.SelectedIndex;
            if (shIndex == -1)
            {
                Debug.WriteLine("New or Unknown Series.");
                // This is going to need some "file safe" text box scrubbing
                tbTVFileTarget.Text = TVFolder + "\\" + cboSeriesName.Text + "\\" + SeasonFolderName + "." + tbSeasonNum.Text + "\\" + TargetName;
            }
            else
            {
                Debug.WriteLine("Existing Series: " + tvshows[shIndex].FolderName);
                // See if we can find the season that this episode belongs to
                Regex rSeasons = new Regex(SeasonFolderName + ".{0,1}([0-9]+)", RegexOptions.IgnoreCase);
                String SeasonNum = "";
                for (int seCount = 0; seCount < tvshows[shIndex].Seasons.Count; seCount++)
                {
                    //Debug.WriteLine("Name: " + tvshows[shIndex].Seasons[seCount].Name);
                    Debug.WriteLine("Folder: " + tvshows[shIndex].Seasons[seCount].Filepath);
                    MatchCollection mc = rSeasons.Matches(tvshows[shIndex].Seasons[seCount].Name);
                    if (mc.Count > 0)
                    {
                        SeasonNum = mc[0].Groups[1].Captures[0].Value;
                        Debug.WriteLine("Season " + SeasonNum);
                        if (int.Parse(SeasonNum) == int.Parse(tbSeasonNum.Text))
                        {
                            Debug.WriteLine("Found it! " + tvshows[shIndex].Seasons[seCount].Filepath + "\\" + TargetName);
                            tbTVFileTarget.Text = tvshows[shIndex].Seasons[seCount].Filepath + "\\" + TargetName;
                            seCount = tvshows[shIndex].Seasons.Count; // forces an exit of the loop
                        }
                    }
                }

                if (tbTVFileTarget.Text == "")
                {
                    Debug.WriteLine("Season not found, new? " + tvshows[shIndex].FolderName + "\\" + SeasonFolderName + "." + tbSeasonNum.Text + "\\" + TargetName);
                    tbTVFileTarget.Text = tvshows[shIndex].FolderName + "\\" + SeasonFolderName + "." + tbSeasonNum.Text + "\\" + TargetName;
                }
            }
            
        }

        private void txtRenameFormat_TextChanged(object sender, TextChangedEventArgs e)
        {
            setRenameExample();
        }

        private void txtSpacingChar_TextChanged(object sender, TextChangedEventArgs e)
        {
            setRenameExample();
        }

        private void txtNumberPadding_TextChanged(object sender, TextChangedEventArgs e)
        {
            setRenameExample();
        }

        private void setRenameExample()
        {
            //Wrap this in a try because when MediaScout is initially loading the user set options, some of the UI
            // elements do not exist yet and can crash
            try
            {
                String exampleRename = makeEpisodeTargetName("Series Name", "1", "Episode Name", "3", "Suffix", ".mkv");
                if (exampleRename.Length > 0)
                {
                    lblRenameExample.Content = "Ex: " + exampleRename;
                }
                else
                {
                    lblRenameExample.Content = "Ex: (invalid renaming template)";
                }
            }
            catch { }
        }

        private void btnRenameEpisode_Click(object sender, RoutedEventArgs e)
        {
            renameDroppedFile();
        }

        private bool renameDroppedFile()
        {
            //Doing an in-pace rename
            String TargetDir = System.IO.Path.GetDirectoryName(tbTVFileSource.Text);
            String SourceFile = System.IO.Path.GetFileName(tbTVFileSource.Text);
            String TargetFile = System.IO.Path.GetFileName(tbTVFileTarget.Text);

            if (SourceFile == TargetFile)
            {
                Debug.WriteLine("Name of file already matches the Target.  Rename not required.");
                // Returning TRUE because I figure if it's already named what you want, that's sucessful right?
                return true;
            }

            Debug.WriteLine("Rename Target: " + TargetDir + "\\" + TargetFile);
            try
            {
                System.IO.File.Move(TargetDir + "\\" + SourceFile, TargetDir + "\\" + TargetFile);
                tbTVFileSource.Text = TargetDir + "\\" + TargetFile;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error renaming: " + ex.ToString());
                return false;
            }
        }

        private void btnImportEpisode_Click(object sender, RoutedEventArgs e)
        {
            queueEpisodeImport(cboSeriesName.Text, tbSeasonNum.Text, tbEpNum.Text, tbTVFileSource.Text, tbTVFileTarget.Text);
        }

        private void queueEpisodeImport(String SeriesName, String SeNum, String EpNum, String SourceFile, String TargetFile)
        {
            EpisodeImporter ei = new EpisodeImporter();
            ei.FetchMetadata = chkFetchMetadata.IsChecked.Value;
            ei.Series = SeriesName;
            ei.SeasonNum = SeNum;
            ei.EpisodeNum = EpNum;
            ei.SourceFile = SourceFile;
            ei.TargetFile = TargetFile;
            lbMoveEpisodeQueue.Items.Add(ei);
        }


        //private bool moveDroppedFile()
        //{
        //    return moveDroppedFile(tbTVFileTarget.Text);
        //}
        //
        //private bool moveDroppedFile(String TargetFilePath)
        //{
        //    Debug.WriteLine("Move Target: " + TargetFilePath);

        //    // See if the target filename is already taken
        //    if (System.IO.File.Exists(TargetFilePath))
        //    {
        //        Debug.WriteLine("Target file already exists!");
        //        return false;
        //    }
        //    else
        //    {
        //        try
        //        {
        //            //System.IO.File.Move(tbTVFileSource.Text, (TargetFilePath));
        //            FileSystem oFS = new FileSystem();
        //            oFS.CopyProgress += new EventHandler<FileSystem.CopyProgressEventArgs>(oFS_CopyProgress);
        //            bool success = oFS.MoveFile(tbTVFileSource.Text, TargetFilePath);
        //            tbTVFileSource.Text = (TargetFilePath);
        //            return success;
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine("Error moving: " + ex.ToString());
        //            return false;
        //        }
        //    }
           
        //}

        //void oFS_CopyProgress(object sender, FileSystem.CopyProgressEventArgs e)
        //{
        //    //lblDropCopyStatus.Content = string.Format("Copied {0} of {1}, {2:P} done, ETA:{3}",
        //    //    e.copiedBytes, e.totalBytes, e.percentage,
        //    //    new TimeSpan(0, 0, 0, 0, (int)e.eta).ToString());
        //    lblDropCopyStatus.Content = string.Format("Copied {0} of {1}, {2:P}",
        //        e.copiedBytes, e.totalBytes, e.percentage);
        //    pbDropCopy.Value = (int)(e.percentage * 100);
        //    //Application.DoEvents();
        //    //if (stopCopy)
        //    //{
        //    //    e.Cancel = true;
        //    //}
        //}

        private bool fetchEpisodeMetadata(String EpisodeFile, int SeasonNum, int EpisodeNum)
        {
            // Make sure we are dealing with a known series
            int shIndex = cboSeriesName.SelectedIndex;
            if (shIndex == -1)
            {
                Debug.WriteLine("New or Unknown Series, not fetching metadata.");
                return false;
            }
            else
            {
                Debug.WriteLine("Existing Series: " + tvshows[shIndex].FolderName);
                Debug.WriteLine("Don't know how to fetch MetaData from here yet.");
                return false;
            }

            
            //FileInfo fi = new FileInfo(EpisodeFile);
            //MediaScout.Season se = new MediaScout.Season(int.Parse(tbSeasonNum.Text), );
        }

        private bool fetchEpisodeMetadata(FileInfo EpisodeFile, MediaScout.Season season, MediaScout.Episode episode)
        {
            // Move the Source File to the Target and then fetch the metadata for it.
            return true;
        }

        private void btnMoveStart_Click(object sender, RoutedEventArgs e)
        {
            foreach (EpisodeImporter ei in lbMoveEpisodeQueue.Items)
            {
                Debug.WriteLine(ei.lblSeriesSSEE.Content.ToString());
            }

            Thread thread = new Thread(delegate()
            {
                foreach (EpisodeImporter ei in lbMoveEpisodeQueue.Items)
                {
                    ei.DoImport();
                }

            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

    }

}