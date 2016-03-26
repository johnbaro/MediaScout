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
using System.Windows.Interop;
using System.IO;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Windows.Shell;
using MediaScoutGUI.GUITypes;
using MediaScout.Providers;

namespace MediaScoutGUI
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        //Delegates

        public MediaScout.MediaScoutMessage.Message Message;

        public delegate void MetadataCompletedHandler(Thread th, String reason, bool Reset);
        public event MetadataCompletedHandler MetadataCompleted;

        public delegate void TVShowChangedHandler(TVShow ts, bool IsRemoved);
        public event TVShowChangedHandler TVShowChanged;
        public delegate void SeasonChangedHandler(Season s, bool IsRemoved);
        public event SeasonChangedHandler SeasonChanged;
        public delegate void EpisodeChangedHandler(Episode e, bool IsRemoved);
        public event EpisodeChangedHandler EpisodeChanged;
        public delegate void MovieChangedHandler(Movie m, bool IsRemoved);
        public event MovieChangedHandler MovieChanged;
        public delegate void MovieFileChangedHandler(MovieFile mf, bool IsRemoved);
        public event MovieFileChangedHandler MovieFileChanged;
        public delegate void ActorThumbChangedHandler(Person p);
        public event ActorThumbChangedHandler ActorThumbChanged;

        public delegate void FocusChangedHandler();

        public delegate void TaskbarProgressValueChangeHandler(int value);
        public delegate void TaskbarProgressStatusChangeHandler(TaskbarItemProgressState state);

        public delegate void TVShowImageChangedHandler(TVShow ts, TVShowPosterType type, bool IsLoading);
        public delegate void SeasonImageChangedHandler(Season s, TVShowPosterType type, bool IsLoading);
        public delegate void EpisodeImageChangedHandler(Episode e, String filename, bool IsLoading);
        public delegate void MovieImageChangedHandler(Movie m, MoviePosterType type, bool IsLoading);
        public delegate void MovieFileImageChangedHandler(MovieFile mf, MoviePosterType type, bool IsLoading);

        public delegate void PosterChangedHandler(Object obj, String id, bool IsMovie, Season s, String file);
        public delegate void BackdropChangedHandler(Object obj, String id, bool IsMovie, Season s, String file, String file1);        

        //Collections
        private ObservableCollection<TVShow> tvshows = new ObservableCollection<TVShow>();
        private ObservableCollection<Movie> movies = new ObservableCollection<Movie>();
        private DispatchingCollection<ObservableCollection<TVShow>, TVShow> dispatchtvshows;
        private DispatchingCollection<ObservableCollection<Movie>, Movie> dispatchmovies;

        //Settings
        private List<String> ignoredFiles = new List<String>();
        private List<String> AllowedFileTypes;
        private List<String> AllowedSubtitleTypes;
      
        MediaScoutApp app;
        private bool WindowLoaded = false;
        private bool WindowRendered = false;
        //private System.Windows.Forms.NotifyIcon notifyIcon;
        //private System.Windows.Forms.ToolStripMenuItem mnuCancel;
        private JumpList jumplist;
        private JumpTask jumpCancel;
        private JumpTask jumpCancelAll;
        private JumpTask jumpOperationsSeparator;

        //Objects        
        private MediaScout.TVScout TVScout = null;
        private MediaScout.MovieScout MovieScout = null;
        private FileSystemWatcher TVFSWatcher = null;
        private FileSystemWatcher MovieFSWatcher = null;
        private TheTVDBProvider tvdb = null;
        private TheMovieDBProvider tmdb = null;
        private Collection<Thread> tvThreads = new Collection<Thread>();
        private bool resetTVfolder = false;
        private bool resetMoviefolder = false;

        int currentvalue = 0;
        int maxvalue = 0;

        private TVShow SelectedTVShow;
        private int SelectedTVShowIndex = 0;
        private TVShow UnsortedEpisodes;
        private Season SelectedSeason;
        private int SelectedSeasonIndex = 0;
        private Episode SelectedEpisode;
        private int SelectedEpisodeIndex = 0;
        private Movie SelectedMovie;
        private int SelectedMovieIndex = 0;
        private Movie UnsortedFiles;
        private MovieFile SelectedMovieFile;
        private int SelectedMovieFileIndex = 0;
        private Person SelectedPerson;
        //private int SelectedPersonIndex = 0;

        private int LogTabIndex = 1;

        private int SelectedTabIndex = 0;

        #region To Enable/Disable for the Aero glass effect

        public Rect GetBoundsForGlassFrame()
        {
            return VisualTreeHelper.GetContentBounds(tcTabs);
        }
        public bool SetGlassFrame(bool ExtendGlass)
        {
            bool success = false;
            if (ExtendGlass)
            {
                // Extend glass
                Rect bounds = GetBoundsForGlassFrame();
                success = GlassHelper.ExtendGlassFrame(this, new Thickness(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom));
            }
            else
            {
                // turn off glass...
                GlassHelper.DisableGlassFrame(this);
                success = true;
            }
            return success;
        }
        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            int WM_DWMCOMPOSITIONCHANGED = 0x031E; // for glass (when DWM / glass setting is changed)
            // handle the message for DWM when the aero glass is turned on or off
            if (msg == WM_DWMCOMPOSITIONCHANGED)
            {
                SetGlassFrame(GlassHelper.IsGlassEnabled);
                handled = true;
            }

            return IntPtr.Zero;
        }

        #endregion


        #region Window Routines
        
        public MainWindow(int SelectedTabIndex)
        {
            try
            {
                this.InitializeComponent();
                app = ((MediaScoutApp)Application.Current);
                //notifyIcon = app.notifyIcon;
                jumplist = app.jumplist;

                this.SelectedTabIndex = SelectedTabIndex;
                this.Message = new MediaScout.MediaScoutMessage.Message(TVScout_Message);
                tvdb = new TheTVDBProvider(Message);
                tmdb = new TheMovieDBProvider(Message);

                this.MetadataCompleted += new MetadataCompletedHandler(ResetUI);
                this.TVShowChanged += new TVShowChangedHandler(ResetTVShow);
                this.SeasonChanged += new SeasonChangedHandler(ResetSeason);
                this.EpisodeChanged += new EpisodeChangedHandler(ResetEpisode);
                this.MovieChanged += new MovieChangedHandler(ResetMovie);
                this.MovieFileChanged += new MovieFileChangedHandler(ResetMovieFile);
                this.ActorThumbChanged += new ActorThumbChangedHandler(ResetActorThumb);

                //creating directories
                String CacheDir =  Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\MediaScout\Cache";
                String TVCacheDir = CacheDir + @"\TVCache";
                String MovieCacheDir = CacheDir + @"\MovieCache";
                
                if (!Directory.Exists(CacheDir))
                    Directory.CreateDirectory(CacheDir);
                if (!Directory.Exists(TVCacheDir))
                    Directory.CreateDirectory(TVCacheDir);
                if (!Directory.Exists(MovieCacheDir))
                    Directory.CreateDirectory(MovieCacheDir);

                SetCancelButtons();

                dispatchtvshows = new DispatchingCollection<ObservableCollection<TVShow>, TVShow>(tvshows, Dispatcher);
                dispatchmovies = new DispatchingCollection<ObservableCollection<Movie>, Movie>(movies, Dispatcher);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }                                   
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Rect bounds = GetBoundsForGlassFrame();
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));
            
            WindowLoaded = true;
            LoadOptions();
            tcTabs.SelectedIndex = SelectedTabIndex;
            
            //Dispatcher.Hooks.OperationPosted += new DispatcherHookEventHandler(Hooks_OperationPosted);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Properties.Settings.Default.Zoom = zoomslider.Value.ToString();            
            //Properties.Settings.Default.Save();

            //When the application is closed, check wether the application is 
            //exiting from menu or forms close button                                   
            //if (!isAppExiting)
            //{
            //    //if the forms close button is triggered, cancel the event and hide the form
            //    //then show the notification ballon tip
                
            //    e.Cancel = true;
            //}            
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {

        //    if (this.WindowState == WindowState.Minimized)
        //    {
        //        //Hide();
        //        if (app.showballoontip)
        //        {
        //            notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
        //            notifyIcon.BalloonTipTitle = "Media Scout";
        //            notifyIcon.BalloonTipText = "Application minimized to the system tray. To open the application, double-click the icon in the system tray.";
        //            notifyIcon.ShowBalloonTip(400);
        //        }
        //    }
        }
        private void Window_Closed(object sender, EventArgs e)
        {            
            AbortAllThreads();
            app.Shutdown();
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            WindowRendered = true;
            StartLoadingItems();
        }
        
        #endregion


        #region Loads all the options from the user configuration file
        private void LoadOptions()
        {
            //TV Options
            txtTVDropBox.Text = Properties.Settings.Default.TVDropBoxLocation;
            ChangeMonitorTVFolder();
            if(chkTVFSWatcher.IsEnabled)
                chkTVFSWatcher.IsChecked = Properties.Settings.Default.TVFSWatcher;

            chkSeriesPosters.IsChecked = Properties.Settings.Default.getSeriesPosters;
            chkSeasonPosters.IsChecked = Properties.Settings.Default.getSeasonPosters;
            chkEpPosters.IsChecked = Properties.Settings.Default.getEpisodePosters;
            chkMoveTVFiles.IsChecked = Properties.Settings.Default.moveTVFiles;
            chkSaveTVActors.IsChecked = Properties.Settings.Default.SaveTVActors;

            txtSeasonFolderName.Text = Properties.Settings.Default.SeasonFolderName;
            txtSpecialsFolderName.Text = Properties.Settings.Default.SpecialsFolderName;

            chkdownloadAllTVImages.IsChecked = Properties.Settings.Default.downloadAllTVImages;
            chkdownloadAllTVPosters.IsChecked = Properties.Settings.Default.downloadAllTVPosters;
            chkdownloadAllTVBackdrops.IsChecked = Properties.Settings.Default.downloadAllTVBackdrops;
            chkdownloadAllTVBanners.IsChecked = Properties.Settings.Default.downloadAllTVBanners;
            chkdownloadAllTVSeasonPosters.IsChecked = Properties.Settings.Default.downloadAllTVSeasonPosters;
            chkdownloadAllTVSeasonBackdrops.IsChecked = Properties.Settings.Default.downloadAllTVSeasonBackdrops;

            chkRenameTVFiles.IsChecked = Properties.Settings.Default.renameTVFiles;
            txtTVRenameFormat.Text = Properties.Settings.Default.TVfileformat;
            txtSeasonNumZeroPadding.Text = Properties.Settings.Default.SeasonNumZeroPadding;
            txtEpisodeNumZeroPadding.Text = Properties.Settings.Default.EpisodeNumZeroPadding;

            //Movie Options
            txtMovieDropBox.Text = Properties.Settings.Default.MovieDropBoxLocation;
            ChangeMonitorMovieFolder();            
            if(chkMovieFSWatcher.IsEnabled)
                chkMovieFSWatcher.IsChecked = Properties.Settings.Default.MovieFSWatcher;

            chkMoviePosters.IsChecked = Properties.Settings.Default.getMoviePosters;
            chkMovieFilePosters.IsChecked = Properties.Settings.Default.getMovieFilePosters;
            chkMoveMovieFiles.IsChecked = Properties.Settings.Default.moveMovieFiles;

            chkdownloadAllMovieImages.IsChecked = Properties.Settings.Default.downloadAllMovieImages;
            chkdownloadAllMoviePosters.IsChecked = Properties.Settings.Default.downloadAllMoviePosters;
            chkdownloadAllMovieBackdrops.IsChecked = Properties.Settings.Default.downloadAllMovieBackdrops;

            chkSaveMovieActors.IsChecked = Properties.Settings.Default.SaveMovieActors;

            chkRenameMovieFiles.IsChecked = Properties.Settings.Default.renameMovieFiles;
            txtMovieFileRenameFormat.Text = Properties.Settings.Default.Moviefileformat;
            txtMovieDirRenameFormat.Text = Properties.Settings.Default.MovieDirformat;

            //File Options
            txtAllowedFiletypes.Text = Properties.Settings.Default.allowedFileTypes;
            txtAllowedSubtitles.Text = Properties.Settings.Default.allowedSubtitles;
            txtSearchTermFilters.Text = Properties.Settings.Default.SearchTermFilters;

            chkForceUpdate.IsChecked = Properties.Settings.Default.forceUpdate;
            chkOverwrite.IsChecked = Properties.Settings.Default.overwriteFiles;
            chkSilentMode.IsChecked = Properties.Settings.Default.SilentMode;
            chkAutoSelectMovieTitle.IsChecked = Properties.Settings.Default.AutoSelectMovieTitle;
            chkforceEnterSearchTerm.IsChecked = Properties.Settings.Default.forceEnterSearchTerm;
            
            chkEnableGlassFrame.IsChecked = Properties.Settings.Default.EnableGlassFrame;

            chkSaveXBMCMeta.IsChecked = Properties.Settings.Default.SaveXBMCMeta;
            chkSaveMMMeta.IsChecked = Properties.Settings.Default.SaveMyMoviesMeta;

            txtImagesByName.Text = Properties.Settings.Default.ImagesByNameLocation;

            /* Load Variables */
            AllowedFileTypes = new List<string>(Properties.Settings.Default.allowedFileTypes.Split(';'));
            AllowedSubtitleTypes = new List<string>(Properties.Settings.Default.allowedSubtitles.Split(';'));

            //zoomslider.Value = Double.Parse(Properties.Settings.Default.Zoom);
        }
        #endregion


        #region Thread Completeion Handlers

        private void SetTVShowTabItemsVisibility(Visibility v)
        {
            //lbShowTitle.Visibility = v;
            //btnSaveTVShow.Visibility = v;
            //lbShowDesc.Visibility = v;
            //tcTVShowCategories.Visibility = v;
            //tcTVShowPersons.Visibility = v;
            //lbTVShowFirstAired.Visibility = v;
            //dtTVShowFirstAired.Visibility = v;
            //lbShowRating.Visibility = v;
            //ratingTVShow.Visibility = v;
            //tvTVShows.ContextMenu.Visibility = v;
            gridSeriesView.Visibility = v;
            gridSeasonsView.Visibility = v;
            gridEpisodesView.Visibility = v;
            gridSeriesView.ContextMenu.Visibility = v;
        }
        private void SetMovieTabItemsVisibility(Visibility v)
        {
            //lbMovieTitle.Visibility = v;
            //btnSaveMovie.Visibility = v;
            //lbMovieDesc.Visibility = v;
            //tcMovieCategories.Visibility = v;
            //tcMoviePersons.Visibility = v;
            //lbMovieYear.Visibility = v;
            //lbMovieRating.Visibility = v;
            //ratingMovie.Visibility = v;
            //lbMovies.ContextMenu.Visibility = v;
            gridMovie.Visibility = v;
            gridMovieFile.Visibility = v;
            gridMovie.ContextMenu.Visibility = v;
        }
        
        public void AbortAllThreads()
        {
            if (tvThreads.Count > 0)
            {
                foreach (Thread th in tvThreads)
                    th.Abort();
                tvThreads.Clear();
                MetadataCompleted(null, "All Operations aborted", true);
            }            
        }

        private void ResetUI(Thread th, String reason, bool Reset)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new MetadataCompletedHandler(ResetUI), th, reason, Reset);
                return;
            }
            
            if (th != null)
            {                
                th.Abort();
                tvThreads.Remove(th);
                if (th.Name == "Loading TV Shows")
                {
                    LoadingTVShows = false;
                    if (tvshows.Count > 0)
                    {
                        gridSeriesView.ContextMenu.Visibility = Visibility.Visible;                             
                        //SetFocusOnTVShow();
                        txtSearchTVShow.Focus();
                        //tvTVShows.SelectedIndex = 0;
                    }
                    
                }
                if (th.Name == "Loading Movies")
                {
                    LoadingMovies = false;
                    if (movies.Count > 0)
                    {
                        gridMovie.ContextMenu.Visibility = Visibility.Visible;
                        //SetFocusOnMovie();
                        txtSearchMovie.Focus();
                        //lbMovies.SelectedIndex = 0;
                    }
                }
            }

            if (Reset && tvThreads.Count == 0)
            {
                HideCancelButtons();
                maxvalue = 0;
                currentvalue = 0;
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
            }

            if (reason != null)
                Message(((th != null && th.Name != null) ? th.Name + " : " : "") + reason, MediaScout.MediaScoutMessage.MessageType.Task, 0);
        }
        private void ResetTVShow(TVShow ts, bool IsRemoved)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new TVShowChangedHandler(ResetTVShow), ts, IsRemoved);
                return;
            }

            tvTVShows.SelectedIndex = -1;
            int tsindex = tvshows.IndexOf(ts);
            tvshows.RemoveAt(tsindex);
            if (!IsRemoved)
            {
                ts = new TVShow(ts.Folderpath, ts.Foldername, ts.IsUnsortedEpisodeCollection);
                tvshows.Insert(tsindex, ts);

            }
            tvTVShows.SelectedIndex = SelectedTVShowIndex;                            
        }
        private void ResetSeason(Season s, bool IsRemoved)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new SeasonChangedHandler(ResetSeason), s, IsRemoved);
                return;
            }

            TVShow ts = s.TVShow;

            tvSeasons.SelectedIndex = -1;
            int tsindex = tvshows.IndexOf(ts);
            int sindex = tvshows[tsindex].Seasons.IndexOf(s);
            tvshows[tsindex].Seasons.RemoveAt(sindex);
            if (!IsRemoved)
            {
                s = new Season(s.Folderpath, s.Name, ts);
                tvshows[tsindex].Seasons.Insert(sindex, s);
            }
            tvSeasons.SelectedIndex = SelectedSeasonIndex;
        }
        private void ResetEpisode(Episode e, bool IsRemoved)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new EpisodeChangedHandler(ResetEpisode), e, IsRemoved);
                return;
            }

            Season s = e.Season;
            //TVShow ts = s.TVShow;

            tvEpisodes.SelectedIndex = -1;
            //int tsindex = tvshows.IndexOf(ts);
            //int sindex = tvshows[tsindex].Seasons.IndexOf(s);
            //int eindex = tvshows[tsindex].Seasons[sindex].Episodes.IndexOf(e);
            //tvshows[tsindex].Seasons[sindex].Episodes.RemoveAt(eindex);
            int eindex = e.Season.Episodes.IndexOf(e);            
                e.Season.Episodes.RemoveAt(eindex);
            if (!IsRemoved)
            {
                e = new Episode(e.Filepath, e.Name, s);
                //tvshows[tsindex].Seasons[sindex].Episodes.Insert(eindex, e);
                s.Episodes.Insert(eindex, e);
            }
            tvEpisodes.SelectedIndex = SelectedEpisodeIndex;
        }
        private void ResetMovie(Movie m, bool IsRemoved)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new MovieChangedHandler(ResetMovie), m, IsRemoved);
                return;
            }

            lbMovies.SelectedIndex = -1;
            int mindex = movies.IndexOf(m);
            movies.RemoveAt(mindex);
            if (!IsRemoved)
            {
                m = new Movie(m.Folderpath, m.Foldername, m.IsUnsortedFileCollection);
                movies.Insert(mindex, m);
            }
            lbMovies.SelectedIndex = SelectedMovieIndex;
        }
        private void ResetMovieFile(MovieFile mf, bool IsRemoved)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new MovieFileChangedHandler(ResetMovieFile), mf, IsRemoved);
                return;
            }

            Movie m = mf.Movie;

            lbMovieFiles.SelectedIndex = -1;
            int mindex = movies.IndexOf(m);
            int mfindex = movies[mindex].Files.IndexOf(mf);
            movies[mindex].Files.RemoveAt(mfindex);
            if (!IsRemoved)
            {
                mf = new MovieFile(mf.Filepath, mf.Name, m);
                movies[mindex].Files.Insert(mfindex, mf);
            }
            lbMovieFiles.SelectedIndex = SelectedMovieFileIndex;
        }
        private void ResetActorThumb(Person p)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new ActorThumbChangedHandler(ResetActorThumb), p);
                return;
            }

            p.Thumb = p.GetImage(p.XBMCFolderPath);
            
        }
        
        #endregion


        #region Reload TV and Movie Button Click Routines

        private void btnReloadSelectedTV_Click(object sender, RoutedEventArgs e)
        {
            TVShow ts = SelectedTVShow;
            TVShowChanged(ts, false);
        }
        private void btnReloadSelectedTVs_Click(object sender, RoutedEventArgs e)
        {
            foreach(TVShow ts in tvshows)
                TVShowChanged(ts, false);
        }
        private void btnReloadAllTV_Click(object sender, RoutedEventArgs e)
        {
            loadTVShows();
        }

        private void btnReloadSelectedMovie_Click(object sender, RoutedEventArgs e)
        {
            Movie m = SelectedMovie;
            MovieChanged(m, false);
        }
        private void btnReloadSelectedMovies_Click(object sender, RoutedEventArgs e)
        {
            foreach(Movie m in movies)
                MovieChanged(m, false);
        }
        private void btnReloadAllMovie_Click(object sender, RoutedEventArgs e)
        {
            loadMovies();
        }

        #endregion
                

        #region Cancel Button Routines
        
        #region Set/Show/Hide Cancel Button Routines
        
        private void SetCancelButtons()
        {
            ////Set the NotifyIcon Cancel Button
            //mnuCancel = new System.Windows.Forms.ToolStripMenuItem();
            //mnuCancel.Text = "Cancel";
            //mnuCancel.Click += new EventHandler(mnuCancel_Click);
            //mnuCancel.Visible = false;
            //notifyIcon.ContextMenuStrip.Items.Add(mnuCancel);            

            //Set the Jumplist Cancel item            
            jumpCancel = new JumpTask();
            jumpCancel.ApplicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            jumpCancel.Arguments = "/Cancel:";
            jumpCancel.IconResourcePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            jumpCancel.IconResourceIndex = 4;
            jumpCancel.Title = "Cancel";
            jumpCancel.Description = "Cancels Last Operation";

            jumpCancelAll = new JumpTask();
            jumpCancelAll.ApplicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            jumpCancelAll.Arguments = "/CancelAll:";
            jumpCancelAll.IconResourcePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            jumpCancelAll.IconResourceIndex = 4;
            jumpCancelAll.Title = "Cancel All";
            jumpCancelAll.Description = "Cancels All Operation";
            
            jumpOperationsSeparator = new JumpTask();
        }
        private void ShowCancelButtons()
        {
            btnCancelAll.Visibility = Visibility.Visible;
            btnCancel.Visibility = Visibility.Visible;
            //mnuCancel.Visible = true;

            TaskbarItemInfo.Overlay = (BitmapSource)this.FindResource("imgoverlay");
            if (!jumplist.JumpItems.Contains(jumpOperationsSeparator))
            {
                List<JumpItem> jumplistitems = new List<JumpItem>();
                jumplistitems.Add(jumpCancel);
                jumplistitems.Add(jumpCancelAll);
                jumplistitems.Add(jumpOperationsSeparator);

                jumplist.JumpItems.InsertRange(0, jumplistitems);
                jumplist.Apply();

            }
        }
        private void HideCancelButtons()
        {
            btnCancelAll.Visibility = Visibility.Collapsed;
            btnCancel.Visibility = Visibility.Collapsed;
            
            //mnuCancel.Visible = false;

            TaskbarItemInfo.Overlay = null;

            if (jumplist.JumpItems.Contains(jumpOperationsSeparator))
            {
                jumplist.JumpItems.Remove(jumpOperationsSeparator);
                jumplist.JumpItems.Remove(jumpCancel);
                jumplist.JumpItems.Remove(jumpCancelAll);
                jumplist.Apply();
            }
        }
        
        #endregion 
        
        private void btnCancelAll_Click(object sender, RoutedEventArgs e)
        {
            AbortAllThreads();
        }
        private void _tbCancelAllButton_Click(object sender, EventArgs e)
        {
            AbortAllThreads();
        }

        public void CancelOperation(Thread th)
        {
            if (th == null)
                th = tvThreads[tvThreads.Count - 1];
            MetadataCompleted(th, "Operation Aborted", true);
        }       
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelOperation(null);
        }
        private void _tbCancelButton_Click(object sender, EventArgs e)
        {
            CancelOperation(null);
        }

        private void _tbPauseButton_Click(object sender, EventArgs e)
        {
            if (_tbPauseButton.Description == "Pause")
            {
                _tbPauseButton.Description = "Play";
                _tbPauseButton.ImageSource = (BitmapImage)Resources["PlayImage"];
                tvThreads[tvThreads.Count - 1].Suspend();
                SetTasbkBarProgressStatus(TaskbarItemProgressState.Paused);
            }
            else
            {
                _tbPauseButton.Description = "Pause";
                _tbPauseButton.ImageSource = (BitmapImage)Resources["PauseImage"];
                tvThreads[tvThreads.Count - 1].Resume();
                SetTasbkBarProgressStatus(TaskbarItemProgressState.Normal);
            }
        }
        
        #endregion


        #region Focus Set Routines
        
        private void SetFocusOnTVShow()
        {
            //if (!Dispatcher.CheckAccess())
            //{
            //    Dispatcher.Invoke(DispatcherPriority.Normal, new FocusChangedHandler(SetFocusOnTVShow));
            //    return;
            //}
            
            if (tvTVShows.SelectedIndex == -1)
                tvTVShows.SelectedIndex = 0;
            
            if (tvTVShows.Items.Count > 0)
            {
                if (tvTVShows.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                {
                    ListBoxItem lbi = tvTVShows.ItemContainerGenerator.ContainerFromIndex(SelectedTVShowIndex) as ListBoxItem;
                    if (lbi != null)
                    {
                        lbi.IsSelected = true;
                        lbi.Focus();
                    }
                }
            }
        }
        private void SetFocusOnSeason()
        {
            //if (!Dispatcher.CheckAccess())
            //{
            //    Dispatcher.Invoke(DispatcherPriority.Normal, new FocusChangedHandler(SetFocusOnSeason));
            //    return;
            //}

            if (tvSeasons.SelectedIndex == -1)
                tvSeasons.SelectedIndex = 0;

            if (tvSeasons.Items.Count > 0)
            {
                if (tvSeasons.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                {
                    ListBoxItem lbi = tvSeasons.ItemContainerGenerator.ContainerFromIndex(SelectedSeasonIndex) as ListBoxItem;
                    if (lbi != null)
                    {
                        lbi.IsSelected = true;
                        lbi.Focus();
                    }
                }
            }
        }
        private void SetFocusOnEpisode()
        {
            //if (!Dispatcher.CheckAccess())
            //{
            //    Dispatcher.Invoke(DispatcherPriority.Normal, new FocusChangedHandler(SetFocusOnEpisode));
            //    return;
            //}

            if (tvEpisodes.SelectedIndex == -1)
                tvEpisodes.SelectedIndex = 0;

            if (tvEpisodes.Items.Count > 0)
            {
                if (tvEpisodes.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                {
                    ListBoxItem lbi = tvEpisodes.ItemContainerGenerator.ContainerFromIndex(SelectedEpisodeIndex) as ListBoxItem;
                    if (lbi != null)
                    {
                        lbi.IsSelected = true;
                        lbi.Focus();
                    }
                }
            }
        }
        private void SetFocusOnMovie()
        {
            //if (!Dispatcher.CheckAccess())
            //{
            //    Dispatcher.Invoke(DispatcherPriority.Normal, new FocusChangedHandler(SetFocusOnMovie));
            //    return;
            //}

            if (lbMovies.SelectedIndex == -1)
                lbMovies.SelectedIndex = 0;

            if (lbMovies.Items.Count > 0)
            {
                if (lbMovies.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                {
                    ListBoxItem lbi = lbMovies.ItemContainerGenerator.ContainerFromIndex(SelectedMovieIndex) as ListBoxItem;
                    if (lbi != null)
                    {
                        lbi.IsSelected = true;
                        lbi.Focus();
                    }
                }
            }

        }
        private void SetFocusOnMovieFile()
        {
            //if (!Dispatcher.CheckAccess())
            //{
            //    Dispatcher.Invoke(DispatcherPriority.Normal, new FocusChangedHandler(SetFocusOnMovieFile));
            //    return;
            //}

            if (lbMovieFiles.SelectedIndex == -1)
                lbMovieFiles.SelectedIndex = 0;

            if (lbMovieFiles.Items.Count > 0)
            {
                if (lbMovieFiles.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                {
                    ListBoxItem lbi = lbMovieFiles.ItemContainerGenerator.ContainerFromIndex(SelectedMovieFileIndex) as ListBoxItem;
                    if (lbi != null)
                    {
                        lbi.IsSelected = true;
                        lbi.Focus();
                    }
                }
            }
        }
        
        private void SetTVTabFocus()
        {
            if (gridSeriesView.Visibility == Visibility.Visible)
                SetFocusOnTVShow();
            else if (gridSeasonsView.Visibility == Visibility.Visible)
                SetFocusOnSeason();
            else if (gridEpisodesView.Visibility == Visibility.Visible)
                SetFocusOnEpisode();
        }
        private void SetMovieTabFocus()
        {
            if (gridMovie.Visibility == Visibility.Visible)
                SetFocusOnMovie();
            else if (gridMovieFile.Visibility == Visibility.Visible)
                SetFocusOnMovieFile();
        }
        
        #endregion


        #region Tab Selection and Keyboard Routines
        
        private void tabTVSeries_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
                txtSearchTVShow.Focus();
            else if (e.Key == Key.Enter)
            {
                if (gridSeriesView.Visibility == Visibility.Visible)
                {
                    if (SelectedTVShow != null)
                        ShowSeasons();
                }
                else if (gridSeasonsView.Visibility == Visibility.Visible)
                {
                    if (SelectedSeason != null)
                        ShowEpisodeList();
                }
            }
            else if (e.Key == Key.Delete)
            {
                if (gridSeriesView.Visibility == Visibility.Visible)
                {
                    if (tvTVShows.SelectedItem != null)
                        btnStripSelectedTV_Click(null, null);
                }
                else if (gridSeasonsView.Visibility == Visibility.Visible)
                {
                    if (tvSeasons.SelectedItem != null)
                        btnStripSelectedSeason_Click(null, null);
                }
            }
            else if (e.Key == Key.Back)
            {
                if (gridSeasonsView.Visibility == Visibility.Visible)
                    ShowTVShowList();
                else if (gridEpisodesView.Visibility == Visibility.Visible)
                    ShowSeasons();
            }
        }
        private void tabMovies_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
                txtSearchMovie.Focus();
            else if (e.Key == Key.Enter)
            {
                if (gridMovie.Visibility == Visibility.Visible)
                {
                    if (SelectedMovie != null)
                        ShowMovieFileList();
                }
            }
            else if (e.Key == Key.Delete)
            {
                if (gridMovie.Visibility == Visibility.Visible)
                {
                    if (SelectedMovie != null)
                        btnStripSelectedMovie_Click(null, null);
                }
                else if (gridMovieFile.Visibility == Visibility.Visible)
                {
                    if (SelectedMovie != null)
                        btnStripSelectedMovieFile_Click(null, null);
                }
            }
            else if (e.Key == Key.Back)
            {
                if (gridMovieFile.Visibility == Visibility.Visible)
                    ShowMovieList();
            }
        } 
        
        private void tcTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WindowRendered)
                StartLoadingItems();
        }
        private void StartLoadingItems()
        {
            //Find out what tab is selected.  If the program is just loading up, then the
            // tab control might not be generated or have a selected tab yet, so we have
            // to take into account a "null" scenario.
            TabItem CurrentTab = tcTabs.SelectedItem as TabItem;
            String CurrentTabName = "";
            if (CurrentTab != null)
                CurrentTabName = CurrentTab.Name;

            //switch (tcTabs.SelectedIndex)
            switch (CurrentTabName)
            {
                case "tabTVSeries":
                    StartLoadingTVShows();
                    break;

                case "tabMovies":
                    StartLoadingMovies();
                    break;

                default:
                    break;
            }
        }
        #endregion


        #region Taskbar Progress Status Routines
        
        private void SetTasbkBarProgressStatus(TaskbarItemProgressState state)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new TaskbarProgressStatusChangeHandler(SetTasbkBarProgressStatus), state);
                return;
            }

            TaskbarItemInfo.ProgressState = state;
        }
        private void SetTasbkBarProgressValue(int value)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Send, new TaskbarProgressValueChangeHandler(SetTasbkBarProgressValue), value);
                return;
            }
            if (value != maxvalue)
            {
                if (TaskbarItemInfo.ProgressState != TaskbarItemProgressState.Normal)
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;                
                TaskbarItemInfo.ProgressValue = ((double)value/maxvalue);
            }
            else
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
        }
        
        #endregion


        #region Loading TV and Movie Routines

        private void StartLoadingTVShows()
        {
            //Make sure the TVFolder is set before attempting to load in all the TV show data.                    
            System.Collections.Specialized.StringCollection TVFolders = Properties.Settings.Default.TVFolders;

            if (TVFolders == null || TVFolders.Count == 0)
            {
                if (tvshows.Count > 0)
                {
                    tvshows.Clear();
                    SetTVShowTabItemsVisibility(Visibility.Hidden);
                    UpdateTVPoster(null, TVShowPosterType.Banner, false);
                    UpdateTVPoster(null, TVShowPosterType.Poster, false);
                    UpdateTVPoster(null, TVShowPosterType.Backdrop, false);
                }
                return;
            }

            if ((tvshows == null || tvshows.Count == 0 || resetTVfolder) && WindowRendered)
            {
                resetTVfolder = false;
                loadTVShows();
                gridTVSeries.DataContext = dispatchtvshows;
            }
        }
        bool LoadingTVShows = false;
        private void loadTVShows()
        {
            if (!LoadingTVShows)
            {
                LoadingTVShows = true;
                tvTVShows.SelectedIndex = -1;
                tvshows.Clear();
                //Load it on a seperate thread (hence the use of the DispatchingCollection) so as not to lag UI
                ShowCancelButtons();
                Thread th = null;
                th = new Thread(delegate()
                {
                    foreach (String dir in Properties.Settings.Default.TVFolders)
                    {
                        if (Directory.Exists(dir))
                        {
                            DirectoryInfo TVShows = new DirectoryInfo(dir);
                            maxvalue += TVShows.GetDirectories().Length;
                            try
                            {
                                int UnsortedFilesLength = 0;
                                bool containsUnsortedFiles = false;
                                foreach (FileInfo fi in TVShows.GetFiles())
                                {
                                    if (AllowedFileTypes.Contains(fi.Extension))
                                    {
                                        containsUnsortedFiles = true;
                                        UnsortedFilesLength++;
                                    }
                                }
                                if (containsUnsortedFiles)
                                {
                                    maxvalue += UnsortedFilesLength;
                                    TVShow t = new TVShow(TVShows.FullName, "Unsorted Episodes", true);
                                    UnsortedEpisodes = t;
                                    tvshows.Add(t);
                                }
                                foreach (DirectoryInfo di in TVShows.GetDirectories())
                                {
                                    TVShow t = new TVShow(di.FullName, di.Name, false);
                                    tvshows.Add(t);
                                    SetTasbkBarProgressValue(++currentvalue);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (th.ThreadState != System.Threading.ThreadState.AbortRequested)
                                {
                                    SetTasbkBarProgressStatus(TaskbarItemProgressState.Error);
                                    MessageBox.Show(ex.Message);
                                }
                            }
                        }
                    }
                    MetadataCompleted(th, tvshows.Count.ToString(), true);
                });

                th.Name = "Loading TV Shows";
                th.SetApartmentState(ApartmentState.STA);
                th.Start();
                tvThreads.Add(th);
            }
        }

        private void StartLoadingMovies()
        {
            //Make sure the MovieFolder is set before attempting to load in all the movies data.
            System.Collections.Specialized.StringCollection MovieFolders = Properties.Settings.Default.MovieFolders;

            if (MovieFolders == null || MovieFolders.Count == 0)
            {
                if (movies.Count > 0)
                {
                    movies.Clear();
                    SetMovieTabItemsVisibility(Visibility.Hidden);
                    UpdateMoviePoster(null, MoviePosterType.Poster, false);
                    UpdateMoviePoster(null, MoviePosterType.Backdrop, false);
                }
                return;
            }

            if (movies == null || movies.Count == 0 || resetMoviefolder)
            {
                resetMoviefolder = false;
                loadMovies();
                gridMovies.DataContext = dispatchmovies;
            }
        }
        bool LoadingMovies = false;
        private void loadMovies()
        {
            if (!LoadingMovies)
            {
                LoadingMovies = true;
                lbMovies.SelectedIndex = -1;
                movies.Clear();
                ShowCancelButtons();
                Thread th = null;
                th = new Thread(delegate()
                {
                    foreach (String dir in Properties.Settings.Default.MovieFolders)
                    {
                        if (Directory.Exists(dir))
                        {
                            DirectoryInfo Movies = new DirectoryInfo(dir);
                            maxvalue += Movies.GetDirectories().Length;
                            try
                            {
                                int UnsortedFilesLength = 0;
                                bool containsUnsortedFiles = false;
                                foreach (FileInfo fi in Movies.GetFiles())
                                {
                                    if (AllowedFileTypes.Contains(fi.Extension))
                                    {
                                        containsUnsortedFiles = true;
                                        UnsortedFilesLength++;
                                    }
                                }
                                maxvalue += UnsortedFilesLength;
                                if (containsUnsortedFiles)
                                {
                                    Movie m = new Movie(Movies.FullName, "Unsorted Files", true);
                                    UnsortedFiles = m;
                                    movies.Add(m);
                                }
                                foreach (DirectoryInfo di in Movies.GetDirectories())
                                {
                                    Movie m = new Movie(di.FullName, di.Name, false);
                                    movies.Add(m);
                                    SetTasbkBarProgressValue(++currentvalue);
                                }

                            }
                            catch (Exception ex)
                            {
                                if (th.ThreadState != System.Threading.ThreadState.AbortRequested)
                                {
                                    SetTasbkBarProgressStatus(TaskbarItemProgressState.Error);
                                    MessageBox.Show(ex.Message);
                                }
                            }
                        }
                    }
                    MetadataCompleted(th, movies.Count.ToString(), true);
                });

                th.Name = "Loading Movies";
                th.SetApartmentState(ApartmentState.STA);
                th.Start();

                tvThreads.Add(th);
            }
        }

        #endregion


        #region Open In Explorer Routines
        
        private void btnOpenTVShow_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select," + SelectedTVShow.Folderpath);
        }

        private void btnOpenSeason_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select," + SelectedSeason.Folderpath);
        }
        private void btnOpenSeasonMetadata_Click(object sender, RoutedEventArgs e)
        {
            String dir = SelectedSeason.MetadataFolderPath;
            if(Directory.Exists(dir))
                System.Diagnostics.Process.Start("explorer.exe", dir);
            else
                MessageBox.Show("Directory doesn't Exist");
        }

        private void btnOpenEpisode_Click(object sender, RoutedEventArgs e)
        {
            String file = SelectedEpisode.Filepath;
            if (File.Exists(file))
                System.Diagnostics.Process.Start("explorer.exe", "/select," + file);
            else
                MessageBox.Show("File doesn't Exist");
        }
        private void btnOpenEpisodeXMLMetadata_Click(object sender, RoutedEventArgs e)
        {
            String file = SelectedEpisode.XMLFile;
            if(File.Exists(file))
                System.Diagnostics.Process.Start("explorer.exe", "/select," + file);
            else
                MessageBox.Show("File doesn't Exist");
        }

        private void btnOpenMovie_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select," + SelectedMovie.Folderpath);
        }
        private void btnOpenMovieFile_Click(object sender, RoutedEventArgs e)
        {
            String file = SelectedMovieFile.Filepath;
            if (File.Exists(file))
                System.Diagnostics.Process.Start("explorer.exe", "/select," + file);
            else
                MessageBox.Show("File doesn't Exist");
        }

        private void btnOpenActorThumb_Click(object sender, RoutedEventArgs e)
        {
            String file;
            if (Properties.Settings.Default.SaveXBMCMeta)
            {
                file = SelectedPerson.XBMCFolderPath + "\\" +  SelectedPerson.Name.Replace(" ", "_") + ".jpg";
                if (File.Exists(file))
                    System.Diagnostics.Process.Start("explorer.exe", "/select," + file);
                else
                    MessageBox.Show("File doesn't Exist");
            }
            if (Properties.Settings.Default.SaveMyMoviesMeta)
            {
                file = SelectedPerson.MyMoviesFolderPath + "\\" + SelectedPerson.Name.Replace(" ", "_") + @"\folder.jpg";
                if (File.Exists(file))
                    System.Diagnostics.Process.Start("explorer.exe", "/select," + file);
                else
                    MessageBox.Show("File doesn't Exist");
            }
           
        }
        
        #endregion
       

        #region TV Series, Season, Episode And Movie Listbox Routines

        #region TV Shows ListBox Routines
        
        private void tvTVShows_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tvTVShows.SelectedItems.Count > 1)
                tvTVShows.ItemTemplate = FindResource("dtMultipleTVShows") as DataTemplate;
            else
            {
                if (tvTVShows.SelectedIndex != -1)
                {
                    SelectedTVShow = (TVShow)tvTVShows.SelectedItem;
                    SelectedTVShowIndex = tvTVShows.SelectedIndex;
                    if (!SelectedTVShow.IsUnsortedEpisodeCollection)
                    {
                        tvTVShows.ItemTemplate = FindResource("dtTVShows") as DataTemplate;
                        ShowTVShowList();
                        SelectedTVShow.Load();
                        UpdateTVPoster(SelectedTVShow, TVShowPosterType.Poster, SelectedTVShow.isPosterLoading);
                        UpdateTVPoster(SelectedTVShow, TVShowPosterType.Backdrop, SelectedTVShow.isBackDropLoading);
                        UpdateTVPoster(SelectedTVShow, TVShowPosterType.Banner, SelectedTVShow.isBannerLoading);
                    }
                    else
                    {
                        tvTVShows.ItemTemplate = FindResource("dtUnsortedEpisodesCollection") as DataTemplate;
                        ShowUnsortedEpisodeCollection();
                    }
                }                
            }
        }
        private void ShowTVShowList()
        {
            gridEpisodesView.Visibility = Visibility.Hidden;
            gridSeasonsView.Visibility = Visibility.Hidden;
            gridSeriesView.Visibility = Visibility.Visible;
            SetFocusOnTVShow();
        } 
        private void TVShowItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                ShowSeasons();
        }
        private void ShowUnsortedEpisodeCollection()
        {            
            SelectedSeason = SelectedTVShow.UnsortedEpisodes[0].Season;
            ShowEpisodeList();
        }
        private void ShowSeasons()
        {
            TVShow ts = SelectedTVShow;
            if (ts.IsUnsortedEpisodeCollection)
            {
                if (gridEpisodesView.Visibility == Visibility.Visible)
                    ShowTVShowList();
                else
                    ShowUnsortedEpisodeCollection();
            }
            else
                ShowSeasonList();
        }
        private void ShowSeasonList()
        {
            gridSeriesView.Visibility = Visibility.Hidden;
            gridEpisodesView.Visibility = Visibility.Hidden;
            gridSeasonsView.Visibility = Visibility.Visible;
            gridSeasonsView.DataContext = SelectedTVShow;
            //tvSeasons.ItemsSource = SelectedTVShow.Seasons;
            SetFocusOnSeason();
        }

        #endregion
        
        #region Seasons ListBox Routines
        
        private void tvSeasons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (tvSeasons.SelectedItems.Count > 1)
                tvSeasons.ItemTemplate = FindResource("dtMultipleSeasons") as DataTemplate;
            else
            {
                tvSeasons.ItemTemplate = FindResource("dtSeasons") as DataTemplate;
                if (tvSeasons.SelectedIndex != -1)
                {
                    SelectedSeason = (Season)tvSeasons.SelectedItem;
                    SelectedSeasonIndex = tvSeasons.SelectedIndex;
                    UpdateSeasonPoster(SelectedSeason, TVShowPosterType.Season_Poster, SelectedSeason.isPosterLoading);
                    UpdateSeasonPoster(SelectedSeason, TVShowPosterType.Season_Backdrop, SelectedSeason.isBackDropLoading);
                }
            }
        }        
        private void SeasonItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                ShowEpisodeList();
        }
        private void ShowEpisodeList()
        {
            if(SelectedSeason.TVShow.IsUnsortedEpisodeCollection)
                btnEpisodesBack.Visibility = Visibility.Collapsed;
            else
                btnEpisodesBack.Visibility = Visibility.Visible;
            gridSeriesView.Visibility = Visibility.Hidden;
            gridSeasonsView.Visibility = Visibility.Hidden;
            gridEpisodesView.Visibility = Visibility.Visible;
            gridEpisodesView.DataContext = SelectedSeason;
            //tvEpisodes.ItemsSource = SelectedSeason.Episodes;
            SetFocusOnEpisode();
        }
        private void btnSeasonBack_Click(object sender, RoutedEventArgs e)
        {
            ShowTVShowList();
        }

        #endregion
        
        #region Episodes ListBox Routines
        
        private void tvEpisodes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tvEpisodes.SelectedItems.Count > 1)
                tvEpisodes.ItemTemplate = FindResource("dtMultipleEpisodes") as DataTemplate;
            else
            {
                tvEpisodes.ItemTemplate = FindResource("dtEpisodes") as DataTemplate;
                if (tvEpisodes.SelectedIndex != -1)
                {
                    SelectedEpisode = tvEpisodes.SelectedItem as Episode;
                    SelectedEpisodeIndex = tvEpisodes.SelectedIndex;
                    SelectedEpisode.Load();
                    UpdateEpisodePoster(SelectedEpisode, SelectedEpisode.PosterFilename, false);
                }
            }
        }
        private void btnEpisodesBack_Click(object sender, RoutedEventArgs e)
        {
            ShowSeasons();
        }

        #endregion
        
        #region Movies ListBox Routines

        private void lbMovies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (lbMovies.SelectedItems.Count > 1)
                lbMovies.ItemTemplate = FindResource("dtMultipleMovies") as DataTemplate;
            else
            {
                lbMovies.ItemTemplate = FindResource("dtMovies") as DataTemplate;
                if (lbMovies.SelectedIndex != -1)
                {
                    SelectedMovie = (Movie)lbMovies.SelectedItem;
                    SelectedMovieIndex = lbMovies.SelectedIndex;
                    if (!SelectedMovie.IsUnsortedFileCollection)
                    {
                        ShowMovieList();
                        SelectedMovie.Load();
                        UpdateMoviePoster(SelectedMovie, MoviePosterType.Poster, SelectedMovie.isPosterLoading);
                        UpdateMoviePoster(SelectedMovie, MoviePosterType.Backdrop, SelectedMovie.isBackDropLoading);
                    }
                    else
                    {
                        lbMovies.ItemTemplate = FindResource("dtUnsortedFilesCollection") as DataTemplate;
                        ShowMovieFileList();
                    }
                }
            }
        }    
        private void ShowMovieList()
        {
            gridMovieFile.Visibility = Visibility.Hidden;
            gridMovie.Visibility = Visibility.Visible;
            SetFocusOnMovie();
        }
        private void MovieItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                ShowMovieFileList();
        }
        private void ShowMovieFileList()
        {
            if (SelectedMovie.IsUnsortedFileCollection)
                btnMovieFileBack.Visibility = Visibility.Collapsed;
            else
                btnMovieFileBack.Visibility = Visibility.Visible;
            gridMovie.Visibility = Visibility.Hidden;
            gridMovieFile.Visibility = Visibility.Visible;
            SetFocusOnMovieFile();
        }
        
        #endregion

        #region Movie File ListBox Routines

        private void lbMovieFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbMovieFiles.SelectedItems.Count > 1)
                lbMovieFiles.ItemTemplate = FindResource("dtMultipleMovieFiles") as DataTemplate;
            else
            {
                lbMovieFiles.ItemTemplate = FindResource("dtMovieFiles") as DataTemplate;
                if (lbMovieFiles.SelectedIndex != -1)
                {
                    SelectedMovieFile = (MovieFile)lbMovieFiles.SelectedItem;
                    SelectedMovieFileIndex = lbMovieFiles.SelectedIndex;
                    UpdateMovieFilePoster(SelectedMovieFile, MoviePosterType.File_Poster, SelectedMovieFile.isPosterLoading);
                    UpdateMovieFilePoster(SelectedMovieFile, MoviePosterType.File_Backdrop, SelectedMovieFile.isBackDropLoading);
                }
            }
        }
        private void btnMovieFileBack_Click(object sender, RoutedEventArgs e)
        {
            ShowMovieList();
        }
        
        #endregion

        #region Actor Listbox Routines
        private void lstMovieActors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstMovieActors.SelectedIndex != -1)
            {
                SelectedPerson = lstMovieActors.SelectedItem as Person;
            }
        }

        private void lstTVActors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstTVActors.SelectedIndex != -1)
            {
                SelectedPerson = lstTVActors.SelectedItem as Person;
            }
        }
        #endregion
        
        #endregion


        #region Search For IDs From Providers

        private class SearchResultsDecision
        {
            //public String SearchObjectName = null;
            public String SelectedID = null;
            public String SelectedName = null;
            public bool SelectedHasMultipleTitles = false;
            public String SearchTerm = null;
            public object[] results = null;
            public DecisionType Decision = DecisionType.Continue;
        };

        private void SetBestMatchOnTopInMovieResults(object[] results, Movie movie, String SearchTerm)
        {
            int i = 0;
            bool found = false;
            int foundindex = 0;

            if (movie.ID != null)
            {
                foreach (MediaScout.MovieXML m in results)
                {
                    if (movie.ID == m.ID)
                    {
                        foundindex = i;
                        found = true;
                        break;
                    }
                    i++;
                }
            }
            else
            {
                if (movie.Year != null)
                {
                    List<int> SortedByYear = new List<int>();
                    foreach (MediaScout.MovieXML m in results)
                    {
                        if (m.Year != null)
                        {
                            if (m.Year == movie.Year)
                            {
                                if (m.Title == SearchTerm)
                                {
                                    foundindex = i;
                                    found = true;
                                    break;
                                }
                            }
                        }

                        if (m.Title == SearchTerm)
                        {
                            foundindex = i;
                            found = true;
                        }
                        //year not defined or no year and title match items
                        SortedByYear.Add(i);
                        i++;
                    }
                    if (!found)
                    {
                        foreach (int index in SortedByYear)
                        {
                            if ((results[index] as MediaScout.MovieXML).Title.Contains(SearchTerm) && (results[index] as MediaScout.MovieXML).Year != null)
                            {
                                foundindex = index;
                                found = true;
                                break;
                            }
                            if ((results[index] as MediaScout.MovieXML).Title.Contains(SearchTerm))
                            {
                                foundindex = index;
                                found = true;
                            }
                        }
                        if (!found && SortedByYear.Count > 0)
                        {
                            foundindex = SortedByYear[0];
                            found = true;
                        }
                    }
                }
                else
                {
                    foreach (MediaScout.MovieXML m in results)
                    {
                        if (m.Title == SearchTerm)
                        {
                            foundindex = i;
                            found = true;
                            break;
                        }
                        else if (m.Title.Contains(SearchTerm))
                        {
                            foundindex = i;
                            found = true;
                        }
                    }
                }
            }

            if (found)
            {
                object temp;
                temp = results[0];
                results[0] = results[foundindex];
                results[foundindex] = temp;
            }
        }
        private void SetBestMatchOnTopInTVShowResults(object[] results, TVShow tvshow, String SearchTerm)
        {
            int i = 0;
            bool found = false;
            int foundindex = 0;

            if (tvshow.ID != null)
            {
                foreach (MediaScout.TVShowXML m in results)
                {
                    if (tvshow.ID == m.ID)
                    {
                        foundindex = i;
                        found = true;
                        break;
                    }
                    i++;
                }
            }
            else
            {
                if (tvshow.Year != null)
                {
                    List<int> SortedByYear = new List<int>();
                    foreach (MediaScout.TVShowXML m in results)
                    {
                        if (m.Year != null)
                        {
                            if (m.Year == tvshow.Year)
                            {
                                if (m.Title == SearchTerm)
                                {
                                    foundindex = i;
                                    found = true;
                                    break;
                                }
                            }
                        }

                        if (m.Title == SearchTerm)
                        {
                            foundindex = i;
                            found = true;
                        }
                        //year not defined or no year and title match items
                        SortedByYear.Add(i);
                        i++;
                    }
                    if (!found)
                    {
                        foreach (int index in SortedByYear)
                        {
                            if ((results[index] as MediaScout.TVShowXML).Title.Contains(SearchTerm) && (results[index] as MediaScout.TVShowXML).Year != null)
                            {
                                foundindex = index;
                                found = true;
                                break;
                            }
                            if ((results[index] as MediaScout.TVShowXML).Title.Contains(SearchTerm))
                            {
                                foundindex = index;
                                found = true;
                            }
                        }
                        if (!found && SortedByYear.Count > 0)
                        {
                            foundindex = SortedByYear[0];
                            found = true;
                        }
                    }
                }
                else
                {
                    foreach (MediaScout.TVShowXML m in results)
                    {
                        if (m.Title == SearchTerm)
                        {
                            foundindex = i;
                            found = true;
                            break;
                        }
                        else if (m.Title.Contains(SearchTerm))
                        {
                            foundindex = i;
                            found = true;
                        }
                    }
                }
            }

            if (found)
            {
                object temp;
                temp = results[0];
                results[0] = results[foundindex];
                results[foundindex] = temp;
            }
        }
        
        private SearchResultsDecision PromptForSearchTerm(String SearchObjectName, String SearchTerm, bool IsMovie, bool forced, bool CanUserSkip)
        {
            SearchResultsDecision SearchDecision = new SearchResultsDecision();
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        while ((SearchDecision.results == null || SearchDecision.results.Length == 0) && (SearchDecision.Decision == DecisionType.Continue))
                        {
                            NoResultsDialog nrd = new NoResultsDialog(SearchObjectName, SearchTerm, forced, CanUserSkip, IsMovie);
                            nrd.Owner = this;
                            nrd.ShowDialog();
                            SearchTerm = nrd.Term;
                            SearchDecision.Decision = nrd.Decision;
                            if (SearchDecision.Decision == DecisionType.Continue)
                            {
                                if (IsMovie)
                                    SearchDecision.results = tmdb.Search(SearchTerm);
                                else
                                    SearchDecision.results = tvdb.Search(SearchTerm);
                            }
                        }
                    }
            ));
            SearchDecision.SearchTerm = SearchTerm;
            return SearchDecision;
        }
        private SearchResultsDecision PromptForSelection(String SearchObjectName, SearchResultsDecision SearchDecision, object item, bool IsMovie, bool Skip)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        SelectResultDialog rsd = new SelectResultDialog(SearchObjectName, SearchDecision.results, IsMovie, Skip);
                        rsd.Owner = this;
                        rsd.ShowDialog();
                        SearchDecision.Decision = rsd.Decision;
                        if (SearchDecision.Decision == DecisionType.SearchAgain)
                        {
                            SearchDecision = PromptForSearchTerm(SearchObjectName, SearchDecision.SearchTerm, IsMovie, true, Skip);
                            if (SearchDecision.results != null)
                            {
                                if (SearchDecision.results.Length == 0)
                                    SearchDecision = PromptForSearchTerm(SearchObjectName, SearchDecision.SearchTerm, IsMovie, false, Skip);

                                if (SearchDecision.Decision == DecisionType.Continue)
                                {
                                    //if (SearchDecision.results.Length == 1)
                                    //    SearchDecision = GetSelectedIDAndName(SearchDecision, IsMovie, 0);
                                    //else

                                    if (Properties.Settings.Default.SilentMode)
                                    {
                                        if (IsMovie)
                                            SetBestMatchOnTopInMovieResults(SearchDecision.results, item as Movie, SearchDecision.SearchTerm);
                                        else
                                            SetBestMatchOnTopInTVShowResults(SearchDecision.results, item as TVShow, SearchDecision.SearchTerm);
                                        SearchDecision = GetSelectedIDAndName(SearchDecision, IsMovie, 0);
                                    }
                                    else // if (SearchDecision.results.Length > 1)
                                    {
                                        if (IsMovie)
                                            SetBestMatchOnTopInMovieResults(SearchDecision.results, item as Movie, SearchDecision.SearchTerm);
                                        else
                                            SetBestMatchOnTopInTVShowResults(SearchDecision.results, item as TVShow, SearchDecision.SearchTerm);
                                        SearchDecision = PromptForSelection(SearchObjectName, SearchDecision, item, IsMovie, Skip);
                                    }
                                }
                            }
                        }
                        if (rsd.Decision == DecisionType.Continue)
                        {
                            MediaScout.MovieXML m;
                            MediaScout.TVShowXML ts;
                            if (IsMovie)
                            {
                                m = rsd.Selected as MediaScout.MovieXML;
                                SearchDecision.SelectedName = m.Title;
                                SearchDecision.SelectedID = m.ID;
                            }
                            else
                            {
                                ts = rsd.Selected as MediaScout.TVShowXML;
                                SearchDecision.SelectedName = ts.Title;
                                SearchDecision.SelectedID = ts.ID;
                            }                            
                        }
                    }
            ));
            return SearchDecision;
        }
        private SearchResultsDecision SearchForID(object item, bool IsMovie, String SearchObjectName, String SearchTerm, bool CanUserSkip, bool forceEnterSearchTerm)
        {
            SearchResultsDecision SearchDecision = new SearchResultsDecision();

            Movie m = null;
            TVShow ts = null;

            if (IsMovie)
                m = item as Movie;
            else
                ts = item as TVShow;
            
            //try to load ID from the metadata file if it exist
            if (!forceEnterSearchTerm)
            {
                if (IsMovie)
                {
                    if (m.ID == null)
                        m.Load();
                    if (m.HasMetadata || m.ID != null)
                    {
                        SearchDecision.SelectedName = m.Name;
                        SearchDecision.SelectedID = m.ID;                       
                    }
                }
                else
                {
                    if (ts.ID == null)
                        ts.Load();
                    if (ts.HasMetadata || ts.ID != null)
                    {
                        SearchDecision.SelectedName = ts.Name;
                        SearchDecision.SelectedID = ts.ID;                       
                    }
                }
            }

            //else, the GUIType doesn't have an ID (ie, if the metadata hasn't been fetched before)
            if (SearchDecision.SelectedID == null)
            {
                if (SearchTerm == null)
                    if (IsMovie)
                        SearchTerm = m.HasMetadata ? m.SearchTerm : m.GetSearchTerm();
                    else
                        SearchTerm = ts.HasMetadata ? ts.SearchTerm : ts.GetSearchTerm();
                
                SearchDecision.SearchTerm = SearchTerm;

                if (forceEnterSearchTerm)
                    SearchDecision = PromptForSearchTerm(SearchObjectName, SearchDecision.SearchTerm, IsMovie, true, CanUserSkip);
                else
                {
                    if (IsMovie)
                        SearchDecision.results = tmdb.Search(SearchDecision.SearchTerm);
                    else
                        SearchDecision.results = tvdb.Search(SearchDecision.SearchTerm);
                }

                if (SearchDecision.Decision == DecisionType.Continue)
                {
                    if (SearchDecision.results == null)
                        SearchDecision = PromptForSearchTerm(SearchObjectName, SearchDecision.SearchTerm, IsMovie, false, CanUserSkip);

                    if (SearchDecision.Decision == DecisionType.Continue)
                    {
                        //if (SearchDecision.results.Length == 1)
                        //    SearchDecision = GetSelectedIDAndName(SearchDecision, IsMovie, 0);
                        //else
                        if (Properties.Settings.Default.SilentMode)
                        {
                            if (IsMovie)
                                SetBestMatchOnTopInMovieResults(SearchDecision.results, m, SearchDecision.SearchTerm);
                            else
                                SetBestMatchOnTopInTVShowResults(SearchDecision.results, ts, SearchDecision.SearchTerm);
                            SearchDecision = GetSelectedIDAndName(SearchDecision, IsMovie, 0);
                        }
                        else // if (SearchDecision.results.Length > 1)
                        {
                            if (IsMovie)
                                SetBestMatchOnTopInMovieResults(SearchDecision.results, m, SearchDecision.SearchTerm);
                            else
                                SetBestMatchOnTopInTVShowResults(SearchDecision.results, ts, SearchDecision.SearchTerm);
                            SearchDecision = PromptForSelection(SearchObjectName, SearchDecision, item, IsMovie, CanUserSkip);
                        }
                    }
                    
                    //m.SearchTerm = SearchDecision.SearchTerm;
                    if(SearchDecision.SelectedName != null)
                        Message("Selected " + SearchDecision.SelectedName, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                }
            }            

            return SearchDecision;
        }
        private SearchResultsDecision GetSelectedIDAndName(SearchResultsDecision SearchDecision, bool IsMovie, int index)
        {
            MediaScout.MovieXML m;
            MediaScout.TVShowXML ts;
            if (IsMovie)
            {
                m = SearchDecision.results[index] as MediaScout.MovieXML;
                SearchDecision.SelectedName = m.Title;
                SearchDecision.SelectedID = m.ID;               
            }
            else
            {
                ts = SearchDecision.results[index] as MediaScout.TVShowXML;
                SearchDecision.SelectedName = ts.Title;
                SearchDecision.SelectedID = ts.ID;
            }
            return SearchDecision;
        }

        #endregion


        #region  Build Selected Items List
        private ObservableCollection<TVShow> BuildSelectedShowList()
        {
            ObservableCollection<TVShow> tvshows = null;

            if (tvTVShows.SelectedItems.Count > 0)
            {
                tvshows = new ObservableCollection<TVShow>();
                foreach (TVShow ts in tvTVShows.SelectedItems)
                    tvshows.Add(ts);
            }

            return tvshows;
        }
        private ObservableCollection<Season> BuildSelectedSeasonList()
        {
            ObservableCollection<Season> seasons = null;

            if (tvSeasons.SelectedItems.Count > 0)
            {
                seasons = new ObservableCollection<Season>();
                foreach (Season s in tvSeasons.SelectedItems)
                    seasons.Add(s);
            }

            return seasons;
        }
        private ObservableCollection<Episode> BuildSelectedEpisodeList()
        {
            ObservableCollection<Episode> episodes = null;

            if (tvEpisodes.SelectedItems.Count > 0)
            {
                episodes = new ObservableCollection<Episode>();
                foreach (Episode e in tvEpisodes.SelectedItems)
                    episodes.Add(e);
            }

            return episodes;
        }
        private ObservableCollection<Movie> BuildSelectedMovieList()
        {
            ObservableCollection<Movie> movies = null;

            if (lbMovies.SelectedItems.Count > 0)
            {
                movies = new ObservableCollection<Movie>();
                foreach (Movie m in lbMovies.SelectedItems)
                    movies.Add(m);
            }

            return movies;
        }
        private ObservableCollection<MovieFile> BuildSelectedMovieFileList()
        {
            ObservableCollection<MovieFile> moviefiles = null;

            if (lbMovieFiles.SelectedItems.Count > 0)
            {
                moviefiles = new ObservableCollection<MovieFile>();
                foreach (MovieFile m in lbMovieFiles.SelectedItems)
                    moviefiles.Add(m);
            }

            return moviefiles;
        }
        #endregion


        #region  Fetch List Items(TV Series, Movie And Episode) Routines

        #region  Fetch TV
        private void btnFetchSelectedTV_Click(object sender, RoutedEventArgs e)
        {
            ShowCancelButtons();

            TVShow ts = SelectedTVShow;
            Thread th = null;
            th = new Thread(delegate()
            {
                if (ts.IsUnsortedEpisodeCollection)
                    FetchUnsortedEpisodeCollection(th, ts);
                else
                {
                    DecisionType Decison = FetchSelectedTV(ts, false);
                    if (Decison == DecisionType.Cancel)
                        MetadataCompleted(th, "Canceled.", true);
                    else
                        TVShowChanged(ts, false);
                }
                MetadataCompleted(th, "Done.", true);
            });
            th.Name = "Fetching " + ts.Name;
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);

        }
        private DecisionType FetchSelectedTV(TVShow ts, bool CanUserSkip)
        {            
            SearchResultsDecision SearchDecision = SearchForID(ts, false, ts.Name, null, CanUserSkip, Properties.Settings.Default.forceEnterSearchTerm);
            if (SearchDecision.Decision == DecisionType.Continue)
            {
                MediaScout.TVShowXML selected = tvdb.GetTVShow(SearchDecision.SelectedID);

                if (Properties.Settings.Default.SaveXBMCMeta)
                {
                    Message("Saving Metadata as " + selected.GetNFOFile(ts.Folderpath), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    try
                    {
                        selected.SaveNFO(ts.Folderpath);
                    }
                    catch (Exception ex)
                    {
                        Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                    }
                    Message("Done", MediaScout.MediaScoutMessage.MessageType.TaskResult, 0);
                }
                if (Properties.Settings.Default.SaveMyMoviesMeta)
                {
                    Message("Saving Metadata as " + selected.GetXMLFile(ts.Folderpath), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    try
                    {
                        selected.SaveXML(ts.Folderpath);
                    }
                    catch (Exception ex)
                    {
                        Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                    }
                    Message("Done", MediaScout.MediaScoutMessage.MessageType.TaskResult, 0);
                }

                ////Fetch the images too
                //if (Properties.Settings.Default.getSeriesPosters)
                //{
                //    ChangeSelectedTVorSeasonPoster(ts, null, selected.id, true);
                //    ChangeSelectedTVBanner(ts, selected.id, true);
                //    ChangeSelectedTVorSeasonBackdrop(ts, null, selected.id, true);
                //}
            }

            return SearchDecision.Decision;
        }
        private void btnFetchSelectedTVs_Click(object sender, RoutedEventArgs e)
        {
            tcTabs.SelectedIndex = LogTabIndex;
            ShowCancelButtons();
            ObservableCollection<TVShow> selectedtvshows = BuildSelectedShowList();

            Thread th = null;
            th = new Thread(delegate()
            {                
                foreach(TVShow ts in selectedtvshows)
                {
                    if (ts.IsUnsortedEpisodeCollection)
                        FetchUnsortedEpisodeCollection(th, ts);
                    else
                    {
                        DecisionType Decision = FetchSelectedTV(ts, true);
                        if (Decision == DecisionType.Skip)
                            Message("Skipped " + ts.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                        else if (Decision == DecisionType.Cancel)
                            MetadataCompleted(th, "Canceled.", true);
                        else
                            TVShowChanged(ts, false);
                    }
                }
                MetadataCompleted(th, "Done.", true);
            });
            th.Name = "Fetching Selected TVShows";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);
        }
        #endregion

        #region  Fetch Movie
        private void btnFetchSelectedMovie_Click(object sender, RoutedEventArgs e)
        {
            ShowCancelButtons();

            Movie m = SelectedMovie;

            Thread th = null;
            th = new Thread(delegate()
            {
                DecisionType Decision = FetchSelectedMovie(m, false);
                if (Decision == DecisionType.Cancel)
                    MetadataCompleted(th, "Canceled.", true);
                else
                {
                    MovieChanged(m, false);
                    MetadataCompleted(th, "Done.", true);
                }
            });
            th.Name = "Fetching " + m.Name;
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);

        }
        private DecisionType FetchSelectedMovie(Movie m, bool CanUserSkip)
        {
            if (m.IsUnsortedFileCollection)
                return DecisionType.Skip;

            SearchResultsDecision SearchDecision = SearchForID(m, true, m.Name, null, CanUserSkip, Properties.Settings.Default.forceEnterSearchTerm);
            if (SearchDecision.Decision == DecisionType.Continue)
            {
                //Fetch all the information
                MediaScout.MovieXML selected = tmdb.Get(SearchDecision.SelectedID);

                if (Properties.Settings.Default.SaveXBMCMeta)
                {
                    Message("Saving Metadata as " + selected.GetNFOFile(m.Folderpath), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    try
                    {
                        selected.SaveNFO(m.Folderpath);
                    }
                    catch (Exception ex)
                    {
                        Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                    }
                    Message("Done", MediaScout.MediaScoutMessage.MessageType.TaskResult, 0);
                }
                if (Properties.Settings.Default.SaveMyMoviesMeta)
                {
                    Message("Saving Metadata as " + selected.GetXMLFile(m.Folderpath), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    try
                    {
                        selected.SaveXML(m.Folderpath);
                    }
                    catch (Exception ex)
                    {
                        Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                    }
                    Message("Done", MediaScout.MediaScoutMessage.MessageType.TaskResult, 0);
                }

                //if (Properties.Settings.Default.getMoviePosters)
                //    ChangeSelectedMoviePoster(m, selected.ID, true);

                //if (Properties.Settings.Default.getMovieBackdrops)
                //    ChangeSelectedMovieBackdrop(m, selected.ID, true);
            }
            return SearchDecision.Decision;
        }
        private void btnFetchSelectedMovies_Click(object sender, RoutedEventArgs e)
        {
            tcTabs.SelectedIndex = LogTabIndex;
            ShowCancelButtons();
            ObservableCollection<Movie> selectedmovies = BuildSelectedMovieList();

            Thread th = null;
            th = new Thread(delegate()
            {
                foreach (Movie m in selectedmovies)
                {
                    DecisionType Decision = FetchSelectedMovie(m, true);
                    if (Decision == DecisionType.Skip)
                        Message("Skipped " + m.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    else if (Decision == DecisionType.Cancel)
                        MetadataCompleted(th, "Canceled.", true);
                    else
                        MovieChanged(m, false);
                }
                MetadataCompleted(th, "Done.", true);
            });
            th.Name = "Fetching Selected Movies";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);
        }
        #endregion

        #region  Fetch Episode
        private void FetchUnsortedEpisodeCollection(Thread th, TVShow ts)
        {
            foreach (Episode episode in ts.UnsortedEpisodes)
            {
                DecisionType Decision = FetchSelectedEpisode(episode, true);
                if (Decision == DecisionType.Skip)
                    Message("Skipped " + episode.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                else if (Decision == DecisionType.Cancel)
                    MetadataCompleted(th, "Canceled.", true);
                else
                    EpisodeChanged(episode, false);
            }
        }
        private void btnFetchSelectedEpisode_Click(object sender, RoutedEventArgs e)
        {
            ShowCancelButtons();
            
            Episode episode = SelectedEpisode;            

            Thread th = null;
            th = new Thread(delegate()
            {
                DecisionType Decision = FetchSelectedEpisode(episode, false);
                if (Decision == DecisionType.Cancel)
                    MetadataCompleted(th, "Canceled.", true);
                else
                {
                    EpisodeChanged(episode, false);
                    MetadataCompleted(th, "Done.", true);
                }
            });

                th.Name = "Fetcing " + episode.Name;
                th.SetApartmentState(ApartmentState.STA);
                th.Start();
                tvThreads.Add(th);
            
        }
        private DecisionType FetchSelectedEpisode(Episode e, bool CanUserSkip)
        {
            Season s = e.Season;
            TVShow ts = s.TVShow;

            int eid = MediaScout.GetID.GetSeasonAndEpisodeIDFromFile(e.StrippedFileName).EpisodeID;
            int sid = (ts.IsUnsortedEpisodeCollection) ? MediaScout.GetID.GetSeasonAndEpisodeIDFromFile(e.StrippedFileName).SeasonID : s.GetNum();

            if (eid == -1 || sid == -1)
            {
                // Should prompt for Dialog box asking episode/season number instead of Messagebox
                MessageBox.Show("Unable to Get Episode/Season Number from File");
                return DecisionType.Skip;
            }
            String SearchTerm = ts.IsUnsortedEpisodeCollection ? ts.GetSearchTerm(e.StrippedFileName) : null;
            String SearchObjectName = ts.IsUnsortedEpisodeCollection ? e.Name : ts.Name;
            SearchResultsDecision SearchDecision = SearchForID(ts, false, SearchObjectName, SearchTerm, false, Properties.Settings.Default.forceEnterSearchTerm);
            if (SearchDecision.Decision == DecisionType.Continue)
            {
                String id = SearchDecision.SelectedID;
                if (!String.IsNullOrEmpty(id))
                {
                    MediaScout.EpisodeXML selected = tvdb.GetEpisode(id, sid.ToString(), eid.ToString());

                    #region Save Metadata
                    if (Properties.Settings.Default.SaveXBMCMeta)
                    {
                        Message("Saving Metadata as " + selected.GetNFOFile(s.Folderpath, e.StrippedFileName), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                        try
                        {
                            selected.SaveNFO(s.Folderpath, e.StrippedFileName);
                        }
                        catch (Exception ex)
                        {
                            Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                        }
                        Message("Done", MediaScout.MediaScoutMessage.MessageType.TaskResult, 0);
                    }
                    if (Properties.Settings.Default.SaveMyMoviesMeta)
                    {
                        if(!Directory.Exists(s.MetadataFolderPath))
                            MediaScout.IOFunctions.CreateHiddenFolder(s.MetadataFolderPath);

                        Message("Saving Metadata as " + selected.GetXMLFile(s.Folderpath, e.StrippedFileName), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                        try
                        {
                            selected.SaveXML(s.Folderpath, e.StrippedFileName);
                        }
                        catch (Exception ex)
                        {
                            Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                        }
                        Message("Done", MediaScout.MediaScoutMessage.MessageType.TaskResult, 0);
                    }
                    #endregion

                    #region Save Image

                    if (!String.IsNullOrEmpty(selected.PosterUrl))
                    {
                        MediaScout.Posters p = new MediaScout.Posters();
                        p.Poster = selected.PosterUrl;
                        try
                        {
                            if (Properties.Settings.Default.SaveXBMCMeta)
                            {
                                Message("Saving Episode Poster as " + selected.GetXBMCThumbFilename(e.StrippedFileName), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                                String filename = selected.GetXBMCThumbFile(e.Season.Folderpath, e.StrippedFileName);
                                p.SavePoster(filename);
                                Message("Done", MediaScout.MediaScoutMessage.MessageType.TaskResult, 0);
                            }
                            if (Properties.Settings.Default.SaveMyMoviesMeta)
                            {
                                Message("Saving Episode Poster as " + selected.GetMyMoviesThumbFilename(), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                                String filename = selected.GetMyMoviesThumbFile(e.Season.Folderpath);
                                p.SavePoster(filename);
                                Message("Done", MediaScout.MediaScoutMessage.MessageType.TaskResult, 0);
                            }
                        }
                        catch (Exception ex)
                        {
                            Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.TaskError, 0);
                        }
                    }

                    #endregion
                }
            }
            return SearchDecision.Decision;
        }
        private void btnFetchSelectedEpisodes_Click(object sender, RoutedEventArgs e)
        {
            tcTabs.SelectedIndex = LogTabIndex;
            ShowCancelButtons();
            ObservableCollection<Episode> selectedepisodes = BuildSelectedEpisodeList();

            Thread th = null;
            th = new Thread(delegate()
            {
                foreach (Episode episode in selectedepisodes)
                {
                    DecisionType Decision = FetchSelectedEpisode(episode, true);
                    if (Decision == DecisionType.Skip)
                        Message("Skipped " + episode.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    else if (Decision == DecisionType.Cancel)
                        MetadataCompleted(th, "Canceled.", true);
                    else
                        EpisodeChanged(episode, false);
                }
                MetadataCompleted(th, "Done.", true);
            });

            th.Name = "Fetcing Selected Episodes";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
            tvThreads.Add(th);

        }
        #endregion

        #region  Fetch Actor
        private void btnFetchSelectedActorThumb_Click(object sender, RoutedEventArgs e)
        {
            List<MediaScout.Person> Persons = null;
            
            try
            {
                 Persons = (SelectedPerson.IsMovieActor) ? tmdb.GetActors(SelectedMovie.ID) : tvdb.GetActors(SelectedTVShow.ID);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (Persons != null & Persons.Count != 0)
            {
                foreach (MediaScout.Person p in Persons)
                {
                    if (SelectedPerson.Name == p.Name)
                    {
                        //SelectedPerson.Role = p.Role;

                        #region Save Actors Thumb
                        if (Properties.Settings.Default.SaveXBMCMeta)
                        {
                            Message("Saving " + p.Name + " Image in \\" + p.GetXBMCDirectory(), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                            String ActorsDir = SelectedPerson.XBMCFolderPath;
                            if (!Directory.Exists(ActorsDir))
                                MediaScout.IOFunctions.CreateHiddenFolder(ActorsDir);

                            if (!String.IsNullOrEmpty(p.Thumb))
                            {
                                String Filename = p.GetXBMCFilename();
                                String Filepath = ActorsDir + "\\" + Filename;
                                p.SaveThumb(Filepath);
                                Message("Done", MediaScout.MediaScoutMessage.MessageType.TaskResult, 0);
                            }
                            else
                                Message("Image Not Found", MediaScout.MediaScoutMessage.MessageType.TaskError, 0);
                        }
                        if (Properties.Settings.Default.SaveMyMoviesMeta)
                        {
                            Message("Saving" + p.Name + " Image in \\ImagesByName\\" + p.GetMyMoviesDirectory(), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                            if (Directory.Exists(Properties.Settings.Default.ImagesByNameLocation))
                            {
                                if (!String.IsNullOrEmpty(p.Thumb))
                                {
                                    String ActorsDir = SelectedPerson.MyMoviesFolderPath + "\\" + p.GetMyMoviesDirectory();
                                    String Filepath = ActorsDir + "\\" + p.GetMyMoviesFilename();
                                    if (!Directory.Exists(ActorsDir))
                                        Directory.CreateDirectory(ActorsDir);
                                    p.SaveThumb(Filepath);
                                    Message("Done", MediaScout.MediaScoutMessage.MessageType.TaskResult, 0);
                                }
                                else
                                    Message("Image Not Found", MediaScout.MediaScoutMessage.MessageType.TaskError, 0);
                            }
                            else
                                Message("ImagesByName Location Not Defined", MediaScout.MediaScoutMessage.MessageType.TaskError, 0);                            
                        }
                        
                        #endregion

                        if (SelectedPerson.IsMovieActor)
                            SelectedMovie.LoadActorsThumb(SelectedPerson);
                        else
                            SelectedTVShow.LoadActorsThumb(SelectedPerson);

                        break;
                    }
                }
            }           
        }
        #endregion

        #endregion


        #region Strip (TV, Sesaons, And Episode) Routines

        #region Strip Routines
        private void StripFile(String name, String path, int level)
        {
            if (File.Exists(path))
            {
                Message("Deleting " + name, MediaScout.MediaScoutMessage.MessageType.Task, level);
                try
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                    //File.Delete(path);
                }
                catch (Exception ex)
                {
                    Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.TaskError, level);
                }
                Message("Done", MediaScout.MediaScoutMessage.MessageType.TaskResult, level);
            }
        }
        private void StripDir(String name, String path, int level)
        {
            if (Directory.Exists(path))
            {
                Message("Deleting " + name, MediaScout.MediaScoutMessage.MessageType.Task, level);
                try
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                    //Directory.Move(path, true);
                }
                catch (Exception ex)
                {
                    Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.TaskError, level);
                }
                Message("Done", MediaScout.MediaScoutMessage.MessageType.TaskResult, level);
            }
        }
        #endregion

        #region Strip TV Routines
        private void btnStripSelectedTV_Click(object sender, RoutedEventArgs e)
        {
            TVShow ts = SelectedTVShow;

            String msg = "Are you sure you want to delete all Metadata and images for this series?";
            if (MessageBox.Show(msg, ts.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                StripSelectedTV(ts);
                Message("Done.", MediaScout.MediaScoutMessage.MessageType.Task, 0);
            }
        }
        private void StripSelectedTV(TVShow ts)
        {
            if (ts.IsUnsortedEpisodeCollection)
            {
                btnStripAllEpisodes_Click(null, null);
                return;
            }

            Message("Delete all of the Metadata for series " + ts.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
            String path, name;

            name = "folder.jpg";
            path = ts.Folderpath + "\\" + name;
            StripFile(name, path, 1);

            name = "banner.jpg";
            path = ts.Folderpath + "\\" + name;
            StripFile(name, path, 1);

            if (Properties.Settings.Default.SaveMyMoviesMeta)
            {
                name = "backdrop.jpg";
                path = ts.Folderpath + "\\" + name;
                StripFile(name, path, 1);

                name = "series.xml";
                path = ts.Folderpath + "\\" + name;
                StripFile(name, path, 1);
            }
            if (Properties.Settings.Default.SaveXBMCMeta)
            {
                name = "fanart.jpg";
                path = ts.Folderpath + "\\" + name;
                StripFile(name, path, 1);

                name = "tvshow.nfo";
                path = ts.Folderpath + "\\" + name;
                StripFile(name, path, 1);

                name = ".actors";
                path = System.IO.Path.Combine(ts.Folderpath, name);
                StripDir(name, path, 1);
            }

            if (ts.Actors != null)
                for (int i = 0; i < ts.Actors.Count; i++)
                    StripSelectedActorThumb(ts.Actors[i]);

            if (ts.Seasons != null)
                for (int i = 0; i < ts.Seasons.Count; i++)
                    StripSelectedSeason(ts.Seasons[i]);

            TVShowChanged(ts, false);
        }
        private void btnStripSelectedTVs_Click(object sender, RoutedEventArgs e)
        {
            String msg = "Are you sure you want to delete all Metadata and images for selected TV Shows?";
            if (MessageBox.Show(msg, "TV Shows", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                StripMultipleTV(BuildSelectedShowList());
        }
        private void StripMultipleTV(ObservableCollection<TVShow> tvshows)
        {
            tcTabs.SelectedIndex = LogTabIndex;

            Thread th = null;
            th = new Thread(delegate()
            {
                maxvalue += tvshows.Count;
                for (int i = 0; i < tvshows.Count; i++)
                {
                    StripSelectedTV(tvshows[i]);
                    SetTasbkBarProgressValue(++currentvalue);
                }

                //ReloadTVShows();
                MetadataCompleted(th, "Done.", true);
            });
            th.Name = "Stripping TV Shows";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);
        }
        private void btnStripAllTV_Click(object sender, RoutedEventArgs e)
        {
            String msg = "Are you sure you want to delete all Metadata and images for all TV Shows?";
            if (MessageBox.Show(msg, "TV Shows", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                StripMultipleTV(tvshows);
        }
        #endregion

        #region Strip Season Routines
        private void btnStripSelectedSeason_Click(object sender, RoutedEventArgs e)
        {
            Season s = SelectedSeason;

            String msg = "Are you sure you want to delete all Metadata and images for this season?";
            if (MessageBox.Show(msg, s.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                StripSelectedSeason(s);
                Message("Done.", MediaScout.MediaScoutMessage.MessageType.Task, 1);
            }
        }
        private void StripSelectedSeason(Season s)
        {
            Message("Deleting Metadata for " + s.Name, MediaScout.MediaScoutMessage.MessageType.Task, 1);
            String path, name;

            name = "folder.jpg";
            path = s.Folderpath + "\\" + name;
            StripFile(name, path, 2);

            if (Properties.Settings.Default.SaveMyMoviesMeta)
            {
                name = "backdrop.jpg";
                path = s.Folderpath + "\\" + name;
                StripFile(name, path, 2);

                name = "metadata";
                path = System.IO.Path.Combine(s.Folderpath, name);
                StripDir(name, path, 2);
            }
            if (Properties.Settings.Default.SaveXBMCMeta)
            {
                name = "fanart.jpg";
                path = s.Folderpath + "\\" + name;
                StripFile(name, path, 2);

                name = ".actors";
                path = System.IO.Path.Combine(s.Folderpath, name);
                StripDir(name, path, 2);
            }

            if (s.Episodes != null)
                for (int i = 0; i < s.Episodes.Count; i++)
                    StripSelectedEpisode(s.Episodes[i]);

            SeasonChanged(s, false);
        }
        private void btnStripSelectedSeasons_Click(object sender, RoutedEventArgs e)
        {
            TVShow ts = SelectedTVShow;
            String msg = "Are you sure you want to delete all Metadata and images fo selected seasons?";
            if (MessageBox.Show(msg, ts.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                StripMultipleSeason(BuildSelectedSeasonList());
        }
        private void StripMultipleSeason(ObservableCollection<Season> seasons)
        {
            tcTabs.SelectedIndex = LogTabIndex;
            ShowCancelButtons();

            Thread th = null;
            th = new Thread(delegate()
            {
                maxvalue += seasons.Count;
                for (int i = 0; i < seasons.Count; i++)
                {
                    StripSelectedSeason(seasons[i]);
                    SetTasbkBarProgressValue(++currentvalue);
                }

                MetadataCompleted(th, "Done.", true);
            });

            th.Name = "Stripping Seasons";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);

        }
        private void btnStripAllSeasons_Click(object sender, RoutedEventArgs e)
        {
            TVShow ts = SelectedTVShow;
            String msg = "Are you sure you want to delete all Metadata and images fo all seasons?";
            if (MessageBox.Show(msg, ts.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                StripMultipleSeason(ts.Seasons);
        }
        #endregion

        #region Strip Episode Routines
        private void btnStripSelectedEpisode_Click(object sender, RoutedEventArgs e)
        {
            Episode Episode = SelectedEpisode;

            String msg = "Are you sure you want to delete all Metadata and images for this episode?";
            if (MessageBox.Show(msg, Episode.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                StripSelectedEpisode(Episode);
                Message("Done.", MediaScout.MediaScoutMessage.MessageType.Task, 2);
            }
        }
        private void StripSelectedEpisode(Episode e)
        {
            String path, name;
            Message("Stripping Metadata for " + e.Name, MediaScout.MediaScoutMessage.MessageType.Task, 2);
            if (Properties.Settings.Default.SaveMyMoviesMeta)
            {
                name = e.StrippedFileName + ".xml";
                path = e.Season.MetadataFolderPath + "\\" + name;
                StripFile(name, path, 3);

                name = e.PosterFilename;
                path = e.Season.MetadataFolderPath + "\\" + name;
                StripFile(name, path, 3);
            }
            if (Properties.Settings.Default.SaveXBMCMeta)
            {
                name = e.StrippedFileName + ".nfo";
                path = e.Season.Folderpath + "\\" + name;
                StripFile(name, path, 3);

                name = e.StrippedFileName + ".tbn";
                path = e.Season.Folderpath + "\\" + name;
                StripFile(name, path, 3);
            }
            EpisodeChanged(e, false);
        }
        private void btnStripSelectedEpisodes_Click(object sender, RoutedEventArgs e)
        {
            Season s = SelectedSeason;
            String msg = "Are you sure you want to delete all Metadata and images for all episode?";
            if (MessageBox.Show(msg, s.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                StripMultipleEpisodes(BuildSelectedEpisodeList());
        }
        private void StripMultipleEpisodes(ObservableCollection<Episode> episodes)
        {
            tcTabs.SelectedIndex = LogTabIndex;
            ShowCancelButtons();

            Thread th = null;
            th = new Thread(delegate()
            {
                maxvalue += episodes.Count;
                for (int i = 0; i < episodes.Count; i++)
                {
                    StripSelectedEpisode(episodes[i]);
                    SetTasbkBarProgressValue(++currentvalue);
                }

                MetadataCompleted(th, "Done.", true);
            });

            th.Name = "Stripping Episodes";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);

        }
        private void btnStripAllEpisodes_Click(object sender, RoutedEventArgs e)
        {
            Season s = SelectedSeason;
            String msg = "Are you sure you want to delete all Metadata and images for all episode?";
            if (MessageBox.Show(msg, s.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                StripMultipleEpisodes(s.Episodes);
        }
        #endregion

        #region Strip Movie Routines
        private void btnStripSelectedMovie_Click(object sender, RoutedEventArgs e)
        {
            Movie m = SelectedMovie;

            String msg = "Are you sure you want to delete all Metadata and images for this Movie?";
            if (MessageBox.Show(msg, m.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                StripSelectedMovie(m);
                Message("Done.", MediaScout.MediaScoutMessage.MessageType.Task, 0);
            }
            else
                Debug.WriteLine("Leaving Metadata alone.");
        }
        private void StripSelectedMovie(Movie m)
        {
            if (m.IsUnsortedFileCollection)
            {
                btnStripAllMovieFiles_Click(null, null);
                return;
            }
            Message("Deleting all of the Metadata for movie " + m.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
            String path, name;

            name = "folder.jpg";
            path = m.Folderpath + "\\" + name;
            StripFile(name, path, 1);

            if (Properties.Settings.Default.SaveMyMoviesMeta)
            {
                name = "backdrop.jpg";
                path = m.Folderpath + "\\" + name;
                StripFile(name, path, 1);

                name = "mymovies.xml";
                path = m.Folderpath + "\\" + name;
                StripFile(name, path, 1);
            }
            if (Properties.Settings.Default.SaveXBMCMeta)
            {
                name = "fanart.jpg";
                path = m.Folderpath + "\\" + name;
                StripFile(name, path, 1);

                name = "movie.nfo";
                path = m.Folderpath + "\\" + name;
                StripFile(name, path, 1);

                name = ".actors";
                path = System.IO.Path.Combine(m.Folderpath, name);
                StripDir(name, path, 1);
            }

            if (m.Actors != null)
                for (int i = 0; i < m.Actors.Count; i++)
                    StripSelectedActorThumb(m.Actors[i]);

            if (m.Files != null)
                for (int i = 0; i < m.Files.Count; i++)
                    StripSelectedMovieFile(m.Files[i]);

            MovieChanged(m, false);
        }
        private void btnStripSelectedMovies_Click(object sender, RoutedEventArgs e)
        {
            String msg = "Are you sure you want to delete all Metadata and images for selected Movies?";
            if (MessageBox.Show(msg, "Movies", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                StripMultipleMovies(BuildSelectedMovieList());
        }
        private void StripMultipleMovies(ObservableCollection<Movie> movies)
        {
            tcTabs.SelectedIndex = LogTabIndex;
            ShowCancelButtons();

            Thread th = null;
            th = new Thread(delegate()
            {
                maxvalue += movies.Count;
                for (int i = 0; i < movies.Count; i++)
                {
                    StripSelectedMovie(movies[i]);
                    SetTasbkBarProgressValue(++currentvalue);
                }

                //ReloadMovies();
                MetadataCompleted(th, "Done.", true);
            });

            th.Name = "Stripping Movies";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);

        }
        private void btnStripAllMovie_Click(object sender, RoutedEventArgs e)
        {
            String msg = "Are you sure you want to delete all Metadata and images for all Movies?";
            if (MessageBox.Show(msg, "Movies", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                StripMultipleMovies(movies);
        }
        #endregion

        #region Strip Movie File Routines
        private void btnStripSelectedMovieFile_Click(object sender, RoutedEventArgs e)
        {
            MovieFile mf = SelectedMovieFile;
            String msg = "Are you sure you want to delete all Metadata and images for this Movie File?";
            if (MessageBox.Show(msg, mf.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                StripSelectedMovieFile(mf);
                Message("Done.", MediaScout.MediaScoutMessage.MessageType.Task, 0);
            }
        }
        private void StripSelectedMovieFile(MovieFile mf)
        {
            String path, name;

            if (Properties.Settings.Default.SaveXBMCMeta)
            {
                name = mf.StrippedFileName + ".nfo";
                path = mf.Movie.Folderpath + "\\" + name;
                StripFile(name, path, 2);

                name = mf.StrippedFileName + ".tbn";
                path = mf.Movie.Folderpath + "\\" + name;
                StripFile(name, path, 2);
            }

            MovieFileChanged(mf, false);
        }
        private void btnStripSelectedMovieFiles_Click(object sender, RoutedEventArgs e)
        {
            Movie m = SelectedMovie;
            String msg = "Are you sure you want to delete all Metadata and images for selected Movie Files?";
            if (MessageBox.Show(msg, m.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                StripMultipleMovieFiles(BuildSelectedMovieFileList());
        }
        private void StripMultipleMovieFiles(ObservableCollection<MovieFile> files)
        {
            tcTabs.SelectedIndex = LogTabIndex;
            ShowCancelButtons();

            Thread th = null;
            th = new Thread(delegate()
            {
                maxvalue += movies.Count;
                for (int i = 0; i < files.Count; i++)
                {
                    StripSelectedMovieFile(files[i]);
                    SetTasbkBarProgressValue(++currentvalue);
                }

                MetadataCompleted(th, "Done.", true);
            });

            th.Name = "Stripping Movie Files";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);

        }
        private void btnStripAllMovieFiles_Click(object sender, RoutedEventArgs e)
        {
            Movie m = SelectedMovie;
            String msg = "Are you sure you want to delete all Metadata and images for all Movie Files?";
            if (MessageBox.Show(msg, m.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                StripMultipleMovieFiles(m.Files);
        }
        #endregion

        #region Strip Actor Routines
        private void btnStripSelectedActorThumb_Click(object sender, RoutedEventArgs e)
        {
            Person p = SelectedPerson;
            String msg = "Are you sure you want to delete images for this actor?";
            if (MessageBox.Show(msg, p.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                StripSelectedActorThumb(p);
                Message("Done.", MediaScout.MediaScoutMessage.MessageType.Task, 0);
            }
        }
        private void StripSelectedActorThumb(Person p)
        {
            String path, name;

            if (Properties.Settings.Default.SaveMyMoviesMeta)
            {
                name = p.Name + "\\folder.jpg";
                path = p.MyMoviesFolderPath + "\\" + name;
                StripFile(name, path, 2);

                name = p.Name;
                path = System.IO.Path.Combine(p.MyMoviesFolderPath, name);
                StripDir(name, path, 2);
            }
            if (Properties.Settings.Default.SaveXBMCMeta)
            {
                name = p.Name.Replace(" ", "_") + ".jpg";
                path = p.XBMCFolderPath + "\\" + name;
                StripFile(name, path, 2);
            }
            ActorThumbChanged(p);
        }
        #endregion

        #endregion


        #region Processing (TV, Seasons, Episode, Movie and Movie File) Routines

        #region MediaScout Routines
        private MediaScout.TVScout SetTVScout()
        {
            MediaScout.TVScoutOptions Options = new MediaScout.TVScoutOptions()
            {
                GetSeriesPosters = Properties.Settings.Default.getSeriesPosters,
                GetSeasonPosters = Properties.Settings.Default.getSeasonPosters,
                GetEpisodePosters = Properties.Settings.Default.getEpisodePosters,
                MoveFiles = Properties.Settings.Default.moveTVFiles,

                SeasonFolderName = Properties.Settings.Default.SeasonFolderName,
                SpecialsFolderName = Properties.Settings.Default.SpecialsFolderName,
                DownloadAllPosters = Properties.Settings.Default.downloadAllTVPosters,
                DownloadAllBackdrops = Properties.Settings.Default.downloadAllTVBackdrops,
                DownloadAllBanners = Properties.Settings.Default.downloadAllTVBanners,
                DownloadAllSeasonPosters = Properties.Settings.Default.downloadAllTVSeasonPosters,
                DownloadAllSeasonBackdrops = Properties.Settings.Default.downloadAllTVSeasonBackdrops,

                SaveActors = Properties.Settings.Default.SaveTVActors,

                RenameFiles = Properties.Settings.Default.renameTVFiles,
                RenameFormat = Properties.Settings.Default.TVfileformat,
                SeasonNumZeroPadding = int.Parse(Properties.Settings.Default.SeasonNumZeroPadding),
                EpisodeNumZeroPadding = int.Parse(Properties.Settings.Default.EpisodeNumZeroPadding),

                AllowedFileTypes = AllowedFileTypes.ToArray(),
                AllowedSubtitles = AllowedSubtitleTypes.ToArray(),
                ForceUpdate = Properties.Settings.Default.forceUpdate,
                overwrite = Properties.Settings.Default.overwriteFiles,
                SaveXBMCMeta = Properties.Settings.Default.SaveXBMCMeta,
                SaveMyMoviesMeta = Properties.Settings.Default.SaveMyMoviesMeta,

                FilenameReplaceChar = Properties.Settings.Default.FilenameReplaceChar
            };

            TVScout = new MediaScout.TVScout(Options, Message, Properties.Settings.Default.ImagesByNameLocation);
            return TVScout;
        }
        private MediaScout.MovieScout SetMovieScout()
        {            
            MediaScout.MovieScoutOptions Options = new MediaScout.MovieScoutOptions()
            {
                GetMoviePosters = Properties.Settings.Default.getMoviePosters,
                GetMovieFilePosters = Properties.Settings.Default.getMovieFilePosters,
                MoveFiles = Properties.Settings.Default.moveMovieFiles,

                DownloadAllPosters = Properties.Settings.Default.downloadAllMoviePosters,
                DownloadAllBackdrops = Properties.Settings.Default.downloadAllMovieBackdrops,

                SaveActors = Properties.Settings.Default.SaveMovieActors,

                RenameFiles = Properties.Settings.Default.renameMovieFiles,
                FileRenameFormat = Properties.Settings.Default.Moviefileformat,
                DirRenameFormat = Properties.Settings.Default.MovieDirformat,

                AllowedFileTypes = AllowedFileTypes.ToArray(),
                AllowedSubtitles = AllowedSubtitleTypes.ToArray(),
                ForceUpdate = Properties.Settings.Default.forceUpdate,
                overwrite = Properties.Settings.Default.overwriteFiles,
                SaveXBMCMeta = Properties.Settings.Default.SaveXBMCMeta,
                SaveMyMoviesMeta = Properties.Settings.Default.SaveMyMoviesMeta,

                FilenameReplaceChar = Properties.Settings.Default.FilenameReplaceChar
            };

            MovieScout = new MediaScout.MovieScout(Options, Message, Properties.Settings.Default.ImagesByNameLocation);
            return MovieScout;
        }
        
        #endregion

        #region Process TV Routines
        
        private void btnProcessSelectedTV_Click(object sender, RoutedEventArgs e)
        {
            TVShow ts = SelectedTVShow;

            tcTabs.SelectedIndex = LogTabIndex;
            ShowCancelButtons();
            TVScout = SetTVScout();

            if (ts.IsUnsortedEpisodeCollection)
                //ProcessUnsortedEpisodeCollection(th, ts);
                ProcessMultipleEpisodes(ts.Seasons[0].Episodes);
            else
            {
                Thread th = null;
                th = new Thread(delegate()
                {

                    DecisionType Decision = ProcessingSelectedTV(ts, false);
                    if (Decision == DecisionType.Cancel)
                        MetadataCompleted(th, "Canceled.", true);
                    else
                        TVShowChanged(ts, ts.IsDeleted);

                    MetadataCompleted(th, "Done.", true);
                });

                th.Name = "Processing " + ts.Name;
                th.SetApartmentState(ApartmentState.STA);
                th.Start();
                tvThreads.Add(th);
            }
        }
        private DecisionType ProcessingSelectedTV(TVShow ts, bool CanUserSkip)
        {            
            Message("Scanning folder " + ts.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);

            SearchResultsDecision SearchDecision = SearchForID(ts, false, ts.Name, null, CanUserSkip, Properties.Settings.Default.forceEnterSearchTerm);
            if (SearchDecision.Decision == DecisionType.Continue)
            {
                MediaScout.TVShowXML selected = null;
                try
                {
                    selected = tvdb.GetTVShow(SearchDecision.SelectedID);
                }
                catch (Exception ex)
                {
                    Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                }
                if (selected != null)
                {
                    TVScout.series = selected;
                    String name = TVScout.ProcessDirectory(ts.Folderpath);
                    if (name != null)
                    {
                        if (name.Substring(0, 2) != "d:")
                        {
                            ts.Name = name;
                            ts.Folderpath = new DirectoryInfo(ts.Folderpath).Parent.FullName + "\\" + name;
                        }
                        else
                            ts.IsDeleted = true;
                    }
                }
            }
            return SearchDecision.Decision;
        }
        private void btnProcessSelectedTVs_Click(object sender, RoutedEventArgs e)
        {
            ProcessMultipleTVs(BuildSelectedShowList());
        }
        private void ProcessMultipleTVs(ObservableCollection<TVShow> tvshows)
        {
            tcTabs.SelectedIndex = LogTabIndex;
            ShowCancelButtons();
            TVScout = SetTVScout();

            Thread th = null;            
            th = new Thread(delegate()
            {
                maxvalue += tvshows.Count;
                foreach(TVShow ts in tvshows)
                {
                    if (ts.IsUnsortedEpisodeCollection)
                        //ProcessUnsortedEpisodeCollection(th, ts);
                        ProcessMultipleEpisodes(ts.Seasons[0].Episodes);
                    else
                    {
                        DecisionType Decision = ProcessingSelectedTV(ts, true);
                        if (Decision == DecisionType.Skip)
                            Message("Skipped " + ts.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                        else if (Decision == DecisionType.Cancel)
                            MetadataCompleted(th, "Canceled.", true);
                        else
                            TVShowChanged(ts, false);
                    }
                    SetTasbkBarProgressValue(++currentvalue);
                }

                for (int i = 0; i < tvshows.Count; i++)
                {
                    if (tvshows[i].IsDeleted)
                    {
                        TVShowChanged(tvshows[i], true);
                        i--;
                    }
                }

                MetadataCompleted(th, "Done.", true);
            });
            
            th.Name = "Processing TVShows";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);
        }
        private void btnProcessAllTV_Click(object sender, RoutedEventArgs e)
        {
            ProcessMultipleTVs(tvshows);
        }

        #endregion        

        #region Process Season Routines
        private void btnProcessSelectedSeason_Click(object sender, RoutedEventArgs e)
        {
            tcTabs.SelectedIndex = LogTabIndex;

            ShowCancelButtons();

            TVScout = SetTVScout();
            
            Season s = SelectedSeason;

            Thread th = null;            
            th = new Thread(delegate()
            {
                DecisionType Decision = ProcessingSelectedSeason(s, false);
                if(Decision == DecisionType.Cancel)
                    MetadataCompleted(th, "Canceled.", true);
                else
                    SeasonChanged(s, s.IsDeleted);

                MetadataCompleted(th, "Done.", true);
            });
            
            th.Name = "Processing " + s.Name;
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);
        }
        private DecisionType ProcessingSelectedSeason(Season s, bool CanUserSkip)
        {
            TVShow ts = s.TVShow;

            Message("Scanning folder " + s.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);

            SearchResultsDecision SearchDecision = SearchForID(ts, false, ts.Name, null, CanUserSkip, Properties.Settings.Default.forceEnterSearchTerm);
            if (SearchDecision.Decision == DecisionType.Continue)
            {
                MediaScout.TVShowXML selected = null;
                try
                {
                    selected = tvdb.GetTVShow(SearchDecision.SelectedID);
                }
                catch (Exception ex)
                {
                    Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                }
                if (selected != null)
                {
                    TVScout.series = selected;
                    String name = TVScout.ProcessSeasonDirectory(ts.Folderpath, new DirectoryInfo(s.Folderpath), -1);
                    if (name != null)
                    {
                        if (name.Substring(0, 2) != "d:")
                        {
                            s.Name = name;
                            s.Folderpath = ts.Folderpath + "\\" + name;
                        }
                        else
                            s.IsDeleted = true;                       
                    }
                }
            }
            return SearchDecision.Decision;
        }
        private void btnProcessSelectedSeasons_Click(object sender, RoutedEventArgs e)
        {
            ProcessMultipleSeasons(BuildSelectedSeasonList());
        }
        private void ProcessMultipleSeasons(ObservableCollection<Season> seasons)
        {
            tcTabs.SelectedIndex = LogTabIndex;
            ShowCancelButtons();
            TVScout = SetTVScout();

            Thread th = null;
            th = new Thread(delegate()
            {
                maxvalue += seasons.Count;
                foreach (Season s in seasons)
                {
                    DecisionType Decision = ProcessingSelectedSeason(s, true);
                    if (Decision == DecisionType.Skip)
                        Message("Skipped " + s.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    else if (Decision == DecisionType.Cancel)
                        MetadataCompleted(th, "Canceled.", true);
                    else
                        SeasonChanged(s, false);
                    SetTasbkBarProgressValue(++currentvalue);
                }

                for (int i = 0; i < seasons.Count; i++)
                {
                    if (seasons[i].IsDeleted)
                        SeasonChanged(seasons[i], true);
                }

                MetadataCompleted(th, "Done.", true);
            });

            th.Name = "Processing Seasons";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);
        }
        private void btnProcessAllSeasons_Click(object sender, RoutedEventArgs e)
        {
            TVShow ts = SelectedTVShow;
            ProcessMultipleSeasons(ts.Seasons);
        }
        #endregion

        #region Process Episode Routines
        //private void ProcessUnsortedEpisodeCollection(Thread th, TVShow ts)
        //{
        //    foreach (Episode episode in ts.UnsortedEpisodes)
        //    {
        //        DecisionType Decision = ProcessingSelectedEpisode(episode, true);
        //        if (Decision == DecisionType.Skip)
        //            Message("Skipped " + episode.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
        //        else if (Decision == DecisionType.Cancel)
        //            MetadataCompleted(th, "Canceled.", true);
        //        //else
        //        //    EpisodeChanged(episode, false);
        //    }
        //    for (int i = 0; i < ts.UnsortedEpisodes.Count; i++)
        //    {
        //        if (ts.UnsortedEpisodes[i].IsDeleted)
        //            EpisodeChanged(ts.UnsortedEpisodes[i].Season.Episodes[i], true);
        //    }
        //}
        private void btnProcessSelectedEpisode_Click(object sender, RoutedEventArgs e)
        {
            tcTabs.SelectedIndex = LogTabIndex;
            ShowCancelButtons();
            TVScout = SetTVScout();

            Episode Episode = SelectedEpisode;

            Thread th = null;            
            th = new Thread(delegate()
            {
                DecisionType Decision = ProcessingSelectedEpisode(Episode, false);
                if (Decision == DecisionType.Cancel)
                    MetadataCompleted(th, "Canceled.", true);
                else
                    EpisodeChanged(Episode, Episode.IsDeleted);
                
                MetadataCompleted(th, "Done.", true);
            });
            
            th.Name = "Processing " + Episode.Name;
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);
        }
        private DecisionType ProcessingSelectedEpisode(Episode e, bool CanUserSkip)
        {
            Message("Scanning file " + e.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);

            Season s = e.Season;
            TVShow ts = s.TVShow;

            //int sid = (ts.IsUnsortedEpisodeCollection) ? MediaScout.GetID.GetSeasonAndEpisodeIDFromFile(e.StrippedFileName).SeasonID : s.GetNum();
            int sid = (ts.IsUnsortedEpisodeCollection) ? -1 : s.GetNum();
            
            //if (sid == -1)
            //{
            //    // Should prompt for Dialog box asking season number instead of Messagebox
            //    MessageBox.Show("Unable to Get Season Number from File");
            //    return DecisionType.Skip;
            //}

            String SearchTerm = ts.IsUnsortedEpisodeCollection ? ts.GetSearchTerm(e.StrippedFileName) : null;
            String SearchObjectName = ts.IsUnsortedEpisodeCollection ? e.Name : ts.Name;
            SearchResultsDecision SearchDecision = SearchForID(ts, false, SearchObjectName, SearchTerm, CanUserSkip, Properties.Settings.Default.forceEnterSearchTerm);

            if (SearchDecision.Decision == DecisionType.Continue)
            {
                String TVShowID = SearchDecision.SelectedID;
                if (!String.IsNullOrEmpty(TVShowID))
                {
                    MediaScout.TVShowXML selected = null;
                    try
                    {
                        selected = tvdb.GetTVShow(TVShowID);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                    }

                    if (selected != null)
                    {
                        TVScout.series = selected;
                        String name = TVScout.ProcessEpisode(ts.Folderpath, new FileInfo(e.Filepath), sid, !ts.IsUnsortedEpisodeCollection, -1);
                        if (name != null)
                        {
                            if (ts.IsUnsortedEpisodeCollection)
                                e.IsDeleted = true;                                
                            else
                            {
                                e.Name = name;
                                e.Filepath = e.Season.Folderpath + "\\" + e.Name;
                            }
                        }
                    }
                }
            }
            return SearchDecision.Decision;
        }
        private void btnProcessSelectedEpisodes_Click(object sender, RoutedEventArgs e)
        {
            ProcessMultipleEpisodes(BuildSelectedEpisodeList());
        }
        private void ProcessMultipleEpisodes(ObservableCollection<Episode> episodes)
        {
            tcTabs.SelectedIndex = LogTabIndex;
            ShowCancelButtons();
            TVScout = SetTVScout();

            TVShow ts = episodes[0].Season.TVShow;

            Thread th = null;            
            th = new Thread(delegate()
            {
                maxvalue += episodes.Count;
                foreach(Episode e in episodes)
                {
                    DecisionType Decision = ProcessingSelectedEpisode(e, true);
                    if (Decision == DecisionType.Skip)
                        Message("Skipped " + e.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    else if (Decision == DecisionType.Cancel)
                        MetadataCompleted(th, "Canceled.", true);

                    SetTasbkBarProgressValue(++currentvalue);
                }

                for (int i = 0; i < episodes.Count; i++)
                {
                    if (episodes[i].IsDeleted)
                    {
                        EpisodeChanged(episodes[i], true);
                        i--;
                    }
                }

                if (ts.IsUnsortedEpisodeCollection)
                    if (episodes.Count == 0)
                        TVShowChanged(ts, true);

                MetadataCompleted(th, "Done.", true);
            });
            
            th.Name = "Processing Episodes";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);
        }
        private void btnProcessAllEpisodes_Click(object sender, RoutedEventArgs e)
        {
            Season s = SelectedSeason;
            ProcessMultipleEpisodes(s.Episodes);
        }
        
        #endregion

        #region Process Movie Routines
        
        private String PromptForTitleSelectionDialog(MediaScout.MovieXML selected)
        {
            String Title = null;
             Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        TitleSelectionDialog tsd = new TitleSelectionDialog(selected);
                        tsd.Owner = this;
                        tsd.ShowDialog();
                        Title = tsd.SelectedTitle;
                    }
                    ));
            return Title;
        }
        private void btnProcessSelectedMovie_Click(object sender, RoutedEventArgs e)
        {            
            tcTabs.SelectedIndex = LogTabIndex;
            ShowCancelButtons();            
            MovieScout = SetMovieScout();
            Movie m = SelectedMovie;

            if (m.IsUnsortedFileCollection)
                //ProcessUnsortedFileCollection(th, m);
                ProcessMultipleMovieFiles(m.Files);
            else
            {
                Thread th = null;
                th = new Thread(delegate()
                {
                    DecisionType Decision = ProcessingSelectedMovie(m, null, null, false);
                    if (Decision == DecisionType.Cancel)
                        MetadataCompleted(th, "Canceled.", true);
                    else
                        MovieChanged(m, m.IsDeleted);

                    MetadataCompleted(th, "Done.", true);

                });

                th.Name = "Processing " + m.Name;
                th.SetApartmentState(ApartmentState.STA);
                th.Start();
                tvThreads.Add(th);
            }
        }
        private DecisionType ProcessingSelectedMovie(Movie m, String Title, String Year, bool CanUserSkip)
        {
            Message("Processing Movie " + m.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);

            SearchResultsDecision SearchDecision = SearchForID(m, true, m.Name, null, CanUserSkip, Properties.Settings.Default.forceEnterSearchTerm);
            if (SearchDecision.Decision == DecisionType.Continue)
            {                                
                MediaScout.MovieXML selected = null;
                try
                {
                    selected = tmdb.Get(SearchDecision.SelectedID);
                }
                catch (Exception ex)
                {
                    Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                }
                if (selected != null)
                {
                    if (!String.IsNullOrEmpty(selected.LocalTitle) && selected.LocalTitle != selected.Title)
                        SearchDecision.SelectedHasMultipleTitles = true;

                    if (!Properties.Settings.Default.AutoSelectMovieTitle && Title == null && SearchDecision.SelectedHasMultipleTitles)
                        Title = PromptForTitleSelectionDialog(selected);

                    if (Title != null)
                        selected.Title = Title;
                    if (Year != null)
                        selected.Year = Year;

                    MovieScout.m = selected;                    
                    String name = MovieScout.ProcessDirectory(m.Folderpath);
                    if (name != null)
                    {
                        if (name.Substring(0, 2) != "d:")
                        {
                            m.Name = name;
                            m.Folderpath = new DirectoryInfo(m.Folderpath).Parent.FullName + "\\" + name;
                        }
                        else
                            m.IsDeleted = true;
                    }
                }
            }
            return SearchDecision.Decision;
        }
        private void btnProcessSelectedMovies_Click(object sender, RoutedEventArgs e)
        {
            ProcessMultipleMovies(BuildSelectedMovieList());
        }
        private void ProcessMultipleMovies(ObservableCollection<Movie> movies)
        {
            tcTabs.SelectedIndex = LogTabIndex;
            ShowCancelButtons();
            MovieScout = SetMovieScout();

            Thread th = null;
            th = new Thread(delegate()
            {
                maxvalue += movies.Count;
                foreach (Movie m in movies)
                {
                    if (m.IsUnsortedFileCollection)
                        //ProcessUnsortedFileCollection(th, m);
                        ProcessMultipleMovieFiles(m.Files);
                    else
                    {
                        DecisionType Decision = ProcessingSelectedMovie(m, null, null, true);
                        if (Decision == DecisionType.Skip)
                            Message("Skipped " + m.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                        else if (Decision == DecisionType.Cancel)
                            MetadataCompleted(th, "Canceled.", true);
                        else
                            MovieChanged(m, false);
                    }
                    SetTasbkBarProgressValue(++currentvalue);
                }

                for (int i = 0; i < movies.Count; i++)
                {
                    if (movies[i].IsDeleted)
                    {
                        MovieChanged(movies[i], true);
                        i--;
                    }
                }

                MetadataCompleted(th, "Done.", true);
            });

            th.Name = "Processing Movies";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);
        }
        private void btnProcessAllMovie_Click(object sender, RoutedEventArgs e)
        {            
            ProcessAllMoviesDialog pam = new ProcessAllMoviesDialog(movies, UnsortedFiles);
            pam.Owner = this;
            this.Hide();
            pam.ShowDialog();
            this.Show();
            if (pam.DialogResult == true)
            {
                tcTabs.SelectedIndex = LogTabIndex;

                ShowCancelButtons();

                MovieScout = SetMovieScout();

                Thread th = null;                
                th = new Thread(delegate()
                {
                    maxvalue += pam.mslist.Count;
                    foreach (MoviesSearch ms in pam.mslist)
                    {
                        if (ms.Skip)
                            Message("Skipped " + ms.Movie.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                        else
                        {
                            ms.Movie.ID = ms.SelectedMovie.ID;
                            ProcessingSelectedMovie(ms.Movie, ms.SelectedMovie.Title, ms.SelectedMovie.Year, false);
                        }

                        SetTasbkBarProgressValue(++currentvalue);
                    }

                    MetadataCompleted(th, "Done.", true);
                });
                
                th.Name = "Processing All Movies";
                th.SetApartmentState(ApartmentState.STA);
                th.Start();

                tvThreads.Add(th);
            }
        }

        #endregion

        #region Process Movie File Routines
        //private void ProcessUnsortedFileCollection(Thread th, Movie m)
        //{
        //    foreach (MovieFile mf in m.Files)
        //    {
        //        DecisionType Decision = ProcessingSelectedMovieFile(mf, true);
        //        if (Decision == DecisionType.Skip)
        //            Message("Skipped " + mf.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
        //        else if (Decision == DecisionType.Cancel)
        //            MetadataCompleted(th, "Canceled.", true);
        //        else
        //            MovieFileChanged(mf, false);
        //    }
        //}
        private void btnProcessSelectedMovieFile_Click(object sender, RoutedEventArgs e)
        {
            tcTabs.SelectedIndex = LogTabIndex;
            ShowCancelButtons();
            MovieScout = SetMovieScout();
            MovieFile mf = SelectedMovieFile;

            Thread th = null;            
            th = new Thread(delegate()
            {
                DecisionType Decision = ProcessingSelectedMovieFile(mf, false);
                if (Decision == DecisionType.Cancel)
                    MetadataCompleted(th, "Canceled.", true);
                else
                    MovieFileChanged(mf, mf.IsDeleted);

                MetadataCompleted(th, "Done.", true);
            });
            
            th.Name = "Processing " + mf.Name;
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
            tvThreads.Add(th);
        }
        private DecisionType ProcessingSelectedMovieFile(MovieFile mf, bool CanUserSkip)
        {
            Movie m = mf.Movie;
            Message("Processing File " + m.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
            
            String SearchTerm = m.IsUnsortedFileCollection ? m.GetSearchTerm(mf.StrippedFileName) : null;
            String SearchObjectName = m.IsUnsortedFileCollection ? mf.Name : m.Name;
            SearchResultsDecision SearchDecision = SearchForID(m, true, SearchObjectName, SearchTerm, CanUserSkip, Properties.Settings.Default.forceEnterSearchTerm);
            if (SearchDecision.Decision == DecisionType.Continue)
            {
                MediaScout.MovieXML selected = null;
                try
                {
                    selected = tmdb.Get(SearchDecision.SelectedID);
                }
                catch (Exception ex)
                {
                    Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                }
                if (selected != null)
                {
                    if (!String.IsNullOrEmpty(selected.LocalTitle) && selected.LocalTitle != selected.Title)
                        SearchDecision.SelectedHasMultipleTitles = true;

                    if (!Properties.Settings.Default.AutoSelectMovieTitle && SearchDecision.SelectedHasMultipleTitles)
                        selected.Title = PromptForTitleSelectionDialog(selected);

                    MovieScout.m = selected;
                    String name = MovieScout.ProcessFile(m.Folderpath, new FileInfo(mf.Filepath), !m.IsUnsortedFileCollection, -1);
                    if (name != null)
                    {
                        if(m.IsUnsortedFileCollection)                        
                            mf.IsDeleted = true;                                                    
                        else
                        {
                            m.Name = name;
                            m.Folderpath = Properties.Settings.Default.MovieFolders + "\\" + name;
                        }
                    }
                }
            }
            return SearchDecision.Decision;
        }
        private void btnProcessSelectedMovieFiles_Click(object sender, RoutedEventArgs e)
        {            
            ProcessMultipleMovieFiles(BuildSelectedMovieFileList());
        }
        private void ProcessMultipleMovieFiles(ObservableCollection<MovieFile> files)
        {
            tcTabs.SelectedIndex = LogTabIndex;
            ShowCancelButtons();
            MovieScout = SetMovieScout();
            Movie m = files[0].Movie;

            Thread th = null;
            th = new Thread(delegate()
            {
                maxvalue += files.Count;
                foreach (MovieFile mf in files)
                {
                    DecisionType Decision = ProcessingSelectedMovieFile(mf, true);
                    if (Decision == DecisionType.Skip)
                        Message("Skipped " + mf.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    else if (Decision == DecisionType.Cancel)
                        MetadataCompleted(th, "Canceled.", true);

                    SetTasbkBarProgressValue(++currentvalue);
                }

                for (int i = 0; i < files.Count; i++)
                {
                    if (files[i].IsDeleted)
                    {
                        MovieFileChanged(files[i], true);
                        i--;
                    }
                }

                if (m.IsUnsortedFileCollection)
                    if (m.Files.Count == 0)
                        MovieChanged(m, true);

                MetadataCompleted(th, "Done.", true);
            });

            th.Name = "Processing Movie Files";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);
        }
        private void btnProcessAllMovieFile_Click(object sender, RoutedEventArgs e)
        {
            Movie m = SelectedMovie;
            ProcessMultipleMovieFiles(m.Files);
        }
        #endregion
        
        #endregion


        #region Subtitles(TV, Seasons, And Episode) Routines

        #region Sublight Video Hash Computation

        private static byte[] GetLastBytes(byte[] arr, int n)
        {
            List<byte> newarr = new List<byte>();

            //Copies Last n bytes
            for (int j = 0; j < n; j++)
                newarr.Add(arr[(arr.Length - 1) - j]);

            return newarr.ToArray();
        }
        private static byte[] Invert(byte[] arr)
        {
            Array.Reverse(arr);
            return arr;
        }
        private static byte[] ComputeMD5FromFile(string fileName, int nbytes)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();

            byte[] bs = System.Text.Encoding.UTF8.GetBytes(fileName);
            bs = md5.ComputeHash(bs);

           List<byte> newarr = new List<byte>();

            //Copies First n bytes
           for (int i = 0; i < nbytes; i++)
               newarr.Add(bs[i]);

            return newarr.ToArray();
            
            //StringBuilder s = new StringBuilder();            
            
            //for(int i = 0 ; i < nbytes; i++)            
            //{
            //    byte b = bs[i];
            //    s.Append(b.ToString("x2").ToLower());
            //}
            
            //return s.ToString();
        }
        /// <summary>
        /// C# algorithm which computes hash of video file. Computed hash is then used to find matching subtitles.
        /// </summary>
        /// <param name="filePath">Absolute path to video file.</param>
        /// <returns>Computed hash value formated as hexadecimal string (52 characters). Returns null on failure.</returns>
        public static string ComputeSublightVideoHash(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                { return null; }

                List<byte> hash = new List<byte>(26);

                //0: reserved
                hash.Insert(0, 0x0);

                //1-2: video length in seconds
                short runTime = (short)MediaScout.VideoInfo.GetRuntime(filePath);
                if (BitConverter.IsLittleEndian)
                    hash.InsertRange(1, Invert(BitConverter.GetBytes(runTime)));
                else
                    hash.InsertRange(1, BitConverter.GetBytes(runTime));

                //3-8: file length in bytes
                long fileLength = new FileInfo(filePath).Length;
                if (BitConverter.IsLittleEndian)
                    hash.InsertRange(3, GetLastBytes(Invert(BitConverter.GetBytes(fileLength)), 6));
                else
                    hash.InsertRange(3, GetLastBytes(BitConverter.GetBytes(fileLength), 6));

                //9-24: MD5 hash for first 5 MB of file
                hash.InsertRange(9, ComputeMD5FromFile(filePath, 5 * 1024 * 1024));

                //25: control byte
                int sum = 0;
                for (int i = 0; i < 25; i++)
                    sum += hash[i];

                hash.Insert(25, Convert.ToByte(sum % 256));
                //convert to hex string
                StringBuilder sbResult = new StringBuilder();
                for (int i = 0; i < hash.Count; i++)
                {
                    sbResult.AppendFormat("{0:x2}", hash[i]);
                }
                return sbResult.ToString();
            }
            catch
            {
                return null;
            }
        }
        #endregion

        private String GetSublightCmdOptions(String Folderpath)
        {
            if (!String.IsNullOrEmpty(Properties.Settings.Default.SublightUsername))
            {
                String cmdOptionsDir = "downloadbatch " + '"' + Folderpath + '"';
                String cmdOptionsLanguage = " \"en\" ";
                String cmdOptionsfiletypes = " \"" + Properties.Settings.Default.allowedFileTypes.Replace(".", "*.") + "\" ";
                String cmdOptionsOverwrite = Properties.Settings.Default.overwriteFiles ? "" : "/smartDownload ";
                String cmdOptionsMisc = "/recursive:true";
                String cmdOptionsAuthentication = (String.IsNullOrEmpty(Properties.Settings.Default.SublightUsername) ? "" : " /username:\"" + Properties.Settings.Default.SublightUsername + "\"") + (String.IsNullOrEmpty(Properties.Settings.Default.SublightPassword) ? "" : " /password:\"" + Properties.Settings.Default.SublightPassword + "\"");
                return cmdOptionsDir + cmdOptionsfiletypes + cmdOptionsLanguage + cmdOptionsOverwrite + cmdOptionsMisc + cmdOptionsAuthentication;
            }
            else
            {
                MessageBox.Show("Sublight Username is not defined");
                btnSetSublightOptions_Click(null, null);
            }
            return null;
        }
        private String GetSublightCmdFileOptions(String Filepath)
        {
            if (!String.IsNullOrEmpty(Properties.Settings.Default.SublightUsername))
            {
                String cmdOptionsFile = "download " + '"' + Filepath + '"';
                String cmdOptionsLanguage = " \"en\"";
                String cmdOptionsAuthentication = (String.IsNullOrEmpty(Properties.Settings.Default.SublightUsername) ? "" : " /username:\"" + Properties.Settings.Default.SublightUsername + "\"");
                return cmdOptionsFile + cmdOptionsLanguage + cmdOptionsAuthentication;
            }
            else
            {
                MessageBox.Show("Sublight Username is not defined");
                btnSetSublightOptions_Click(null, null);
            }
            return null;
        }
        private ClientInfo GetSublightClientInfo()
        {
            String ClientId = "MediaScout";
            String ApiKey = "6FE1A80F-8874-45ED-B5FB-E979B70DD6E6";

            ClientInfo ci = new ClientInfo();
            ci.ApiKey = ApiKey;
            ci.ClientId = ClientId;
            return ci;
        }
      
        //private void ShowNotImplemented()
        //{
        //    MessageBox.Show("Not Implemented");
        //}

        private void btnDownloadSelectedTVSubtitles_Click(object sender, RoutedEventArgs e)
        {
            if(File.Exists(Properties.Settings.Default.SublightCmd))
            {
                String cmdOptions = GetSublightCmdOptions(SelectedTVShow.Folderpath);
                if(cmdOptions!=null)
                    System.Diagnostics.Process.Start(Properties.Settings.Default.SublightCmd, cmdOptions);
            }
        }
        private void btnDownloadAllTVSubtitles_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Properties.Settings.Default.SublightCmd))
            {
                foreach (String dir in Properties.Settings.Default.TVFolders)
                {
                    String cmdOptions = GetSublightCmdOptions(dir);
                    if (cmdOptions != null)
                        System.Diagnostics.Process.Start(Properties.Settings.Default.SublightCmd, cmdOptions);
                }
            }
        }

        private void btnDownloadSelectedSeasonSubtitles_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Properties.Settings.Default.SublightCmd))
            {
                String cmdOptions = GetSublightCmdOptions(SelectedSeason.Folderpath);
                if (cmdOptions != null)
                    System.Diagnostics.Process.Start(Properties.Settings.Default.SublightCmd, cmdOptions);
            }
        }

        private void btnDownloadSelectedEpisodeSubtitles_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Properties.Settings.Default.SublightCmd))
            {
                String cmdOptions = GetSublightCmdFileOptions(SelectedEpisode.Filepath);
                if (cmdOptions != null)
                    System.Diagnostics.Process.Start(Properties.Settings.Default.SublightCmd, cmdOptions);
            }            
        }
        private void btnFindSelectedEpisodeSubtitles_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Properties.Settings.Default.Sublight))
            {
                String cmdOptions = "file=\"" + SelectedEpisode.Filepath + "\"";
                System.Diagnostics.Process.Start(Properties.Settings.Default.Sublight, cmdOptions);
            }
        }

        private void btnDownloadSelectedMovieSubtitles_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Properties.Settings.Default.SublightCmd))
            {
                String cmdOptions = GetSublightCmdOptions(SelectedMovie.Folderpath);
                if (cmdOptions != null)
                    System.Diagnostics.Process.Start(Properties.Settings.Default.SublightCmd, cmdOptions);
            }
        }
        private void btnDownloadAllMovieSubtitles_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Properties.Settings.Default.SublightCmd))
            {
                foreach (String dir in Properties.Settings.Default.MovieFolders)
                {
                    String cmdOptions = GetSublightCmdOptions(dir);
                    if (cmdOptions != null)
                        System.Diagnostics.Process.Start(Properties.Settings.Default.SublightCmd, cmdOptions);
                }
            }
        }

        private void btnDownloadSelectedMovieFileSubtitles_Click(object sender, RoutedEventArgs e)
        {
            WcfSublightClient client = new WcfSublightClient();
            client.Open();

            String[] args = null;
            String[] settings = null;
            String error = null;
            Guid session = client.LogInAnonymous4(out settings, out error, GetSublightClientInfo(), args);

            List<SubtitleLanguage> langs = new List<SubtitleLanguage>();
            langs.Add(SubtitleLanguage.English);
            Genre[] g = null;
            String videohash = ComputeSublightVideoHash(SelectedMovieFile.Filepath);            
            Subtitle[] subtitles;
            Release[] releases;
            bool IsLimited;

            client.SearchSubtitles3(session, videohash, SelectedMovie.Name, Int32.Parse(SelectedMovieFile.GetYear()),
                new byte(), new int(), langs.ToArray(), g, "MediaScout", new float(),
                out subtitles, out releases, out IsLimited, out error);
            
            String ticket;
            short que;
            String plugin = null;
            client.GetDownloadTicket(out ticket, out que, out error, session, plugin, subtitles[0].SubtitleID.ToString());
            
            //if (File.Exists(Properties.Settings.Default.SublightCmd))
            //{
            //    String cmdOptions = GetSublightCmdFileOptions(SelectedMovieFile.Filepath);
            //    if (cmdOptions != null)
            //        System.Diagnostics.Process.Start(Properties.Settings.Default.SublightCmd, cmdOptions);
            //} 
        }
        private void btnFindSelectedMovieFileSubtitles_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Properties.Settings.Default.Sublight))
            {
                String cmdOptions = "file=\"" + SelectedMovieFile.Filepath + "\"";
                System.Diagnostics.Process.Start(Properties.Settings.Default.Sublight, cmdOptions);
            }
        }
        #endregion


        #region FS Watcher Functions

        #region TV FS Watcher Routines
        
        void TVFSWatcher_Changed(object sender, FileSystemEventArgs e)
        {
                //If it doesn't exist as directory then its is a file
            if (!Directory.Exists(e.FullPath))
            {
                FileInfo fi = new FileInfo(e.FullPath);

                if (AllowedFileTypes.Contains(fi.Extension) //make sure its an acceptable filetype
                    && !FileInUse(e.FullPath)) //make sure the file isn't in use, otherwise it can't be processed.
                {
                    //Log it
                    Message("Autotron : " + e.FullPath, MediaScout.MediaScoutMessage.MessageType.Task, 0);

                    int SeasonNum = -1;

                    DirectoryInfo seriesFolder = fi.Directory;

                    //Check if its a Season Folder or Specials Folder
                    //If the folder contains "Season" (or SeasonFolderName), infer it.
                    if (seriesFolder.Name.Contains(Properties.Settings.Default.SeasonFolderName) || seriesFolder.Name.Contains(Properties.Settings.Default.SpecialsFolderName))
                    {
                        if (seriesFolder.Name == Properties.Settings.Default.SpecialsFolderName)
                            SeasonNum = 0;
                        else 
                            SeasonNum = Int32.Parse(seriesFolder.Name.Replace(Properties.Settings.Default.SeasonFolderName, ""));

                        seriesFolder = seriesFolder.Parent;
                    }

                    String ShowName = (seriesFolder.FullName != Properties.Settings.Default.TVDropBoxLocation) ? seriesFolder.Name : null;
                    StartTVShowProcess(seriesFolder, fi, ShowName, SeasonNum);
                }
            }
        }
        private bool StartTVShowProcess(DirectoryInfo SeriesFolder, FileInfo EpisodeFile, String ShowName, int SeasonNum)
        {
            String SearchTerm = (ShowName != null) ? ShowName : EpisodeFile.Name.Replace(EpisodeFile.Extension, "");
            
            TVShow ts = new TVShow(SeriesFolder.FullName, SeriesFolder.Name, false);
            SearchResultsDecision SearchDecision = SearchForID(ts, false, SearchTerm, SearchTerm, false, (ShowName != null) ? Properties.Settings.Default.forceEnterSearchTerm : true);
            if (SearchDecision.Decision == DecisionType.Continue)
            {
                MediaScout.TVShowXML s = tvdb.GetTVShow(SearchDecision.SelectedID);
                TVScout = SetTVScout();
                TVScout.series = s;
                TVFSWatcher.EnableRaisingEvents = false;
                TVScout.ProcessEpisode(SeriesFolder.FullName, EpisodeFile, SeasonNum, (ShowName != null), -1);
                TVFSWatcher.EnableRaisingEvents = true;
                return true;
            }
            return false;
        }
        
        #endregion

        #region Movie FS Watcher Routines
        
        void MovieFSWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            //If it doesn't exist as directory then its is a file
            if (!Directory.Exists(e.FullPath))
            {
                FileInfo fi = new FileInfo(e.FullPath);

                if (AllowedFileTypes.Contains(fi.Extension) //make sure its an acceptable filetype
                    && !FileInUse(e.FullPath)) //make sure the file isn't in use, otherwise it can't be processed.
                {
                    //Log it
                    Message("Autotron : " + e.FullPath, MediaScout.MediaScoutMessage.MessageType.Task, 0);

                    String MovieName = (fi.Directory.FullName != Properties.Settings.Default.MovieDropBoxLocation) ? fi.Directory.Name : null;
                    StartMovieProcess(fi.Directory, fi, MovieName);
                }
            }
            else //It is a Directory
            {
                DirectoryInfo di = new DirectoryInfo(e.FullPath);
                if (di.Parent.FullName == Properties.Settings.Default.MovieDropBoxLocation)
                {
                    //Log it
                    Message("Autotron : " + e.FullPath, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    StartMovieProcess(di, null, di.Name);
                }
            }
        }
        private bool StartMovieProcess(DirectoryInfo MovieFolder, FileInfo MovieFile, String MovieName)
        {
            Movie mo = null;
            if (MovieFile == null)
                mo = new Movie(MovieFolder.FullName, MovieFolder.Name, false);
            else
                mo = new Movie(MovieFolder.FullName, MovieFolder.Name, MovieFile);
            
            String SearchTerm = (MovieFile == null) ? null : (MovieName!=null) ? mo.GetSearchTerm() : mo.Files[0].GetSearchTerm();

            SearchResultsDecision SearchDecision = SearchForID(mo, true, SearchTerm, SearchTerm, false, Properties.Settings.Default.forceEnterSearchTerm);
            if (SearchDecision.Decision == DecisionType.Continue)
            {
                MediaScout.MovieXML m = tmdb.Get(SearchDecision.SelectedID);
                MovieScout = SetMovieScout();
                MovieScout.m = m;
                MovieFSWatcher.EnableRaisingEvents = false;
                if (MovieFile != null)
                    MovieScout.ProcessFile(MovieFolder.FullName, MovieFile, (MovieName != null), -1);
                else
                    MovieScout.ProcessDirectory(MovieFolder.FullName);
                MovieFSWatcher.EnableRaisingEvents = true;
                return true;
            }
            return false;
        }

        #endregion

        #region File In Use Routines
        
        //Check to see if the file is in use by attempting to open or create it. It will throw a (caught) exception if it is
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
        #endregion

        #endregion


        #region Log Tab Functions
        
        private void InsertHyperlink(RichTextBox rtb, TextPointer position)
        {
            string match = string.Empty;
            Regex r = new Regex("(?:^|[\\s\\[\\]\\}\\{\\(\\)\\\'\\\"<>])((?:(?:https?|gopher|ftp|file|irc):\\/\\/|www\\.)[a-zA-Z0-9\\.\\-=;&%\\?]+(?:\\/?[a-zA-Z0-9\\.\\-=;&%\\?]*)*)");

            Hyperlink h;

            while (position != null)
            {
                if (position.CompareTo(rtb.Document.ContentEnd) == 0)
                {
                    break;
                }
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    String text = position.GetTextInRun(LogicalDirection.Forward);
                    Int32 indexInRun = -1;
                    if (r.IsMatch(text))
                    {
                        Match m = r.Match(text);
                        match = m.Groups[1].Value;
                        indexInRun = m.Groups[1].Index;
                    }

                    if (indexInRun >= 0)
                    {
                        position = position.GetPositionAtOffset(indexInRun);
                        h = new Hyperlink(position, position.GetPositionAtOffset(match.Length));
                        h.Tag = match;
                        h.Foreground = Brushes.Blue;
                        h.TextDecorations = TextDecorations.Underline;
                        h.Cursor = System.Windows.Input.Cursors.Hand;
                        //h.MouseLeftButtonDown += new MouseButtonEventHandler(h_MouseLeftButtonDown);
                        position = position.GetPositionAtOffset(match.Length);
                    }
                    else
                    {
                        position = position.GetPositionAtOffset(text.Length);
                    }
                }
                else
                {
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                }
            }
        }
        bool FirstTask = true;
        public void TVScout_Message(string msg, MediaScout.MediaScoutMessage.MessageType mt, int level)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new MediaScout.MediaScoutMessage.Message(TVScout_Message), msg, mt, level);
                return;
            }
            if (msg != "Thread was being aborted.")
            {
                TextRange tr = new TextRange(rtbLog.Document.ContentEnd, rtbLog.Document.ContentEnd);
                Display_Message(tr, msg, mt, level);
                
                rtbLog.ScrollToEnd();
                if(FirstTask)
                    FirstTask = false;
            }
        }
        public void Display_Message(TextRange tr, string msg, MediaScout.MediaScoutMessage.MessageType mt, int level)
        {
            String tab = new String(' ', 4*level);

            switch (mt)
            {
                case MediaScout.MediaScoutMessage.MessageType.Task:
                    tr.Text = (FirstTask ? "" : "\r") + tab + msg;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    break;
                case MediaScout.MediaScoutMessage.MessageType.TaskResult:
                    tr.Text = " : " + msg;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.BlanchedAlmond);
                    break;
                case MediaScout.MediaScoutMessage.MessageType.TaskError:
                    tr.Text = " : " + msg;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                    break;
                case MediaScout.MediaScoutMessage.MessageType.Error:
                    tr.Text =  "\r" + tab + msg;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                    break;                              
            }
        }
        
        #endregion


        #region Images Functions
        
        //TODO : 1 Improve Images Dialog Fucntions
        #region Show Dialog Functions -- Needs Improvement

        private void ShowActorImageDialog(Object obj, Person Actor, String file, String file1)
        {
            //if (!Dispatcher.CheckAccess())
            //{
            //    Dispatcher.Invoke(DispatcherPriority.Normal, new PosterChangedHandler(ShowPosterDialog), obj, id, IsMovie, s, file);
            //    return;
            //}

            Movie m = null;
            TVShow ts = null;
            if (Actor.IsMovieActor)
                m = obj as Movie;
            else
                ts = obj as TVShow;

            ChangeActorImageDialog caid = new ChangeActorImageDialog(Actor.IsMovieActor ? m.ID : ts.ID, Actor.Name, null, Actor.IsMovieActor);
            caid.Owner = this;
            caid.ShowDialog();
            if (caid.Decision == ImageWindowDecisionbType.DownloadAll)
            {
                ShowCancelButtons();

                Thread th = null;
                th = new Thread(delegate()
                {
                    MediaScout.Posters[] posters = null;
                    posters = Actor.IsMovieActor ? tmdb.GetPersonImage(caid.SelectedActor.ID).ToArray() : null;

                    maxvalue += posters.Length;
                    int i = 0;
                    foreach (MediaScout.Posters p in posters)
                    {
                        try
                        {
                            if (Properties.Settings.Default.SaveXBMCMeta)
                            {
                                Message("Saving " + Actor.Name + " Image in \\" + caid.SelectedActor.GetXBMCDirectory() + " as " + file.Substring(file.LastIndexOf("\\") + 1) , MediaScout.MediaScoutMessage.MessageType.Task, 0);                                
                                caid.selected.SavePoster(file);                                
                            }
                            if (Properties.Settings.Default.SaveMyMoviesMeta)
                            {
                                Message("Saving" + Actor.Name + " Image in \\ImagesByName\\" + caid.SelectedActor.GetMyMoviesDirectory() + " as " + file1.Substring(file1.LastIndexOf("\\") + 1), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                                if (Directory.Exists(Actor.MyMoviesFolderPath))
                                    caid.selected.SavePoster(file1); 
                                else
                                    Message("ImagesByName Location Not Defined", MediaScout.MediaScoutMessage.MessageType.TaskError, 0);
                            }                                                    
                        }
                        catch (Exception ex)
                        {
                            Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                        }
                        i++;
                        SetTasbkBarProgressValue(++currentvalue);
                    }
                    //if (app.showballoontip)
                    //    app.notifyIcon.ShowBalloonTip(1, IsMovie ? m.Name : ts.Name , "All Posters Downloaded", System.Windows.Forms.ToolTipIcon.Info);
                    MetadataCompleted(th, "All Images Downloaded", true);
                });
                th.Name = (Actor.Name) + " Image";
                th.SetApartmentState(ApartmentState.STA);
                th.Start();
                tvThreads.Add(th);
            }
            else if (caid.Decision == ImageWindowDecisionbType.PosterSelected)
            {
                Thread th = new Thread(delegate()
                {
                    try
                    {
                        if (Properties.Settings.Default.SaveXBMCMeta)
                        {
                            Message("Saving " + Actor.Name + " Image in \\" + caid.SelectedActor.GetXBMCDirectory() + " as " + file.Substring(file.LastIndexOf("\\") + 1) , MediaScout.MediaScoutMessage.MessageType.Task, 0);                                
                            caid.selected.SavePoster(file);                                
                        }
                        if (Properties.Settings.Default.SaveMyMoviesMeta)
                        {
                            Message("Saving" + Actor.Name + " Image in \\ImagesByName\\" + caid.SelectedActor.GetMyMoviesDirectory() + " as " + file1.Substring(file1.LastIndexOf("\\") + 1), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                            if (Directory.Exists(Actor.MyMoviesFolderPath))
                                caid.selected.SavePoster(file1); 
                            else
                                Message("ImagesByName Location Not Defined", MediaScout.MediaScoutMessage.MessageType.TaskError, 0);
                        } 

                        if (Actor.IsMovieActor)
                            m.LoadActorsThumb(Actor);
                        else
                            ts.LoadActorsThumb(Actor);
                    
                        Message("Done.", MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    }
                    catch (Exception ex)
                    {
                        Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                    }
                });
                th.SetApartmentState(ApartmentState.STA);
                th.Start();
            }
            else if (caid.Decision == ImageWindowDecisionbType.LocalPosterSelected)
            {
                String selfile = caid.selectedLocalPoster.Poster;
                try
                {
                    //if (File.Exists(file))
                    //{
                    //    String renfile = file + "_temp";
                    //    if (selfile != renfile)
                    //    {
                    //        File.Move(selfile, renfile);
                    //        File.Move(file, selfile);
                    //        File.Move(renfile, file);
                    //    }
                    //}
                    //else
                        File.Copy(selfile, file);
                    if (Actor.IsMovieActor)
                        m.LoadActorsThumb(Actor);
                    else
                        ts.LoadActorsThumb(Actor);
                }
                catch (Exception ex)
                {
                    Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                }
            }
        }
        private void ShowPosterDialog(Object obj, String id, bool IsMovie, Season s, String file)
        {
            //if (!Dispatcher.CheckAccess())
            //{
            //    Dispatcher.Invoke(DispatcherPriority.Normal, new PosterChangedHandler(ShowPosterDialog), obj, id, IsMovie, s, file);
            //    return;
            //}

            Movie m = null;
            TVShow ts = null;
            if (IsMovie)
                m = obj as Movie;
            else if (s == null)
                ts = obj as TVShow;

            String Folderpath = (IsMovie ? m.Folderpath : (s == null ? ts.Folderpath : s.Folderpath));
            ChangeImageDialog cmb = new ChangeImageDialog(Folderpath, id, IsMovie, (s != null) ? s.GetNum() : -1, true);
            cmb.Owner = this;
            cmb.ShowDialog();
            if (cmb.Decision == ImageWindowDecisionbType.DownloadAll)
            {
                ShowCancelButtons();

                Thread th = null;                
                th = new Thread(delegate()
                {
                    MediaScout.Posters[] posters = null;
                    if (IsMovie)
                        posters = tmdb.GetPosters(id, MoviePosterType.Poster);
                    else
                        posters = (s != null) ?
                                   tvdb.GetPosters(id, TVShowPosterType.Season_Poster, s.GetNum())
                                   :
                                   tvdb.GetPosters(id, TVShowPosterType.Poster, -1);
                    maxvalue += posters.Length;
                    int i = 0;
                    foreach (MediaScout.Posters p in posters)
                    {
                        try
                        {
                            String posterfile = file.Substring(0, file.LastIndexOf(".")) + i + file.Substring(file.LastIndexOf("."));
                            Message("Saving Poster as " + posterfile.Substring(file.LastIndexOf("\\")+1), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                            Debug.Write("Saving Poster " + p.Poster);
                            p.SavePoster(posterfile);
                        }
                        catch (Exception ex)
                        {
                            Message("Unable to Change Poster : " + ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                        }
                        i++;
                        SetTasbkBarProgressValue(++currentvalue);                        
                    }
                    //if (app.showballoontip)
                    //    app.notifyIcon.ShowBalloonTip(1, IsMovie ? m.Name : ts.Name , "All Posters Downloaded", System.Windows.Forms.ToolTipIcon.Info);
                    MetadataCompleted(th, "All Posters Downloaded", true); 
                });                
                th.Name = (IsMovie ? m.Name : ts.Name) + " Poster";
                th.SetApartmentState(ApartmentState.STA);
                th.Start();
                tvThreads.Add(th);
            }
            else if (cmb.Decision == ImageWindowDecisionbType.PosterSelected)
            {
                if (IsMovie)                    
                    UpdateMoviePoster(m, MoviePosterType.Poster, true);
                else if (s == null)
                    UpdateTVPoster(ts, TVShowPosterType.Poster, true);
                else
                    UpdateSeasonPoster(s, TVShowPosterType.Season_Poster, true);

                Thread th  = new Thread(delegate()
                {
                    try
                    {
                        Message("Saving Poster as " + file.Substring(file.LastIndexOf("\\")+1), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                        Debug.Write("Saving Poster " + cmb.selected.Poster);
                        cmb.selected.SavePoster(file);
                        if (IsMovie)
                            UpdateMoviePoster(m, MoviePosterType.Poster, false);
                        else if (s == null)
                            UpdateTVPoster(ts, TVShowPosterType.Poster, false);
                        else
                            UpdateSeasonPoster(s, TVShowPosterType.Season_Poster, false);
                        Message("Done.", MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    }
                    catch (Exception ex)
                    {
                        Message("Unable to Change Poster : " + ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                    }
                });
                th.SetApartmentState(ApartmentState.STA);
                th.Start();
            }
            else if (cmb.Decision == ImageWindowDecisionbType.LocalPosterSelected)
            {
                String selfile = cmb.selectedLocalPoster.Poster;
                try
                {
                    //if (File.Exists(file))
                    //{
                    //    String renfile = file + "_temp";
                    //    if (selfile != renfile)
                    //    {
                    //        File.Move(selfile, renfile);
                    //        File.Move(file, selfile);
                    //        File.Move(renfile, file);
                    //    }
                    //}
                    //else
                        File.Copy(selfile, file);
                    if (IsMovie)
                        UpdateMoviePoster(m, MoviePosterType.Poster, false);
                    else if (s == null)
                        UpdateTVPoster(ts, TVShowPosterType.Poster, false);
                    else
                        UpdateSeasonPoster(s, TVShowPosterType.Season_Poster, false);
                }
                catch (Exception ex)
                {
                    Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                }
            }
        }
        private void ShowBackdropDialog(Object obj, String id, bool IsMovie, Season s, String file, String file1)
        {
            //if (!Dispatcher.CheckAccess())
            //{
            //    Dispatcher.Invoke(DispatcherPriority.Normal, new BackdropChangedHandler(ShowBackdropDialog), obj, id, IsMovie, s, file, file1);
            //    return;
            //}

            Movie m = null;
            TVShow ts = null;
            if (IsMovie)
                m = obj as Movie;
            else if (s == null)
                ts = obj as TVShow;

            String Folderpath = (IsMovie ? m.Folderpath : (s == null ? ts.Folderpath : s.Folderpath));
            ChangeImageDialog cmb = new ChangeImageDialog(Folderpath, id, IsMovie, (s!=null)? s.GetNum() : -1, false);
            cmb.Owner = this;
            cmb.ShowDialog();
            if (cmb.Decision == ImageWindowDecisionbType.DownloadAll)
            {
                ShowCancelButtons();

                Thread th = null;
                th = new Thread(delegate()
                    {
                        MediaScout.Posters[] posters = null;
                        if(IsMovie)
                            posters = tmdb.GetPosters(id, MoviePosterType.Backdrop);
                        else
                            posters = (s!=null) ?
                                       tvdb.GetPosters(id, TVShowPosterType.Season_Backdrop, s.GetNum())
                                       :
                                       tvdb.GetPosters(id, TVShowPosterType.Backdrop, -1);
                        maxvalue += posters.Length;
                        int i = 0;                        
                        foreach (MediaScout.Posters p in posters)
                        {
                            try
                            {
                                if (Properties.Settings.Default.SaveXBMCMeta)
                                {
                                    String filefanart = file1.Substring(0, file1.LastIndexOf(".")) + i + file1.Substring(file1.LastIndexOf("."));
                                    Message("Saving Backdrop as " + filefanart.Substring(filefanart.LastIndexOf("\\")+1), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                                    Debug.Write("Saving Backdrop " + p.Poster);
                                    p.SavePoster(filefanart);
                                }

                                if (Properties.Settings.Default.SaveMyMoviesMeta)
                                {
                                    String filebackdrop = file.Substring(0, file.LastIndexOf(".")) + i + file.Substring(file.LastIndexOf("."));                                    
                                    Message("Saving Backdrop as " + filebackdrop.Substring(filebackdrop.LastIndexOf("\\")+1), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                                    Debug.Write("Saving Backdrop " +  p.Poster);
                                    p.SavePoster(filebackdrop);
                                }
                            }
                            catch (Exception ex)
                            {
                                Message("Unable to Change Backdrop : " + ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                            }
                            i++;
                            SetTasbkBarProgressValue(++currentvalue);                            
                        }
                        //if (app.showballoontip)
                        //    app.notifyIcon.ShowBalloonTip(1, IsMovie ?  m.Name : ts.Name, "All Backdrops Downloaded", System.Windows.Forms.ToolTipIcon.Info);
                        MetadataCompleted(th, "All Backdrops Downloaded", true);
                    });
                th.Name = (IsMovie ? m.Name : ts.Name) + " Backdrop";;
                th.SetApartmentState(ApartmentState.STA);
                th.Start();
                tvThreads.Add(th);
            }
            else if (cmb.Decision == ImageWindowDecisionbType.PosterSelected)
            {
                if (IsMovie)
                    UpdateMoviePoster(m, MoviePosterType.Backdrop, true);
                else if (s == null)
                    UpdateTVPoster(ts, TVShowPosterType.Backdrop, true);
                else
                    UpdateSeasonPoster(s, TVShowPosterType.Season_Backdrop, true);
                Thread th = new Thread(delegate()
                {
                    try
                    {
                        if (Properties.Settings.Default.SaveXBMCMeta)
                        {
                            Message("Saving Backdrop as " + file1.Substring(file1.LastIndexOf("\\") + 1), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                            Debug.Write("Saving Backdrop " + cmb.selected.Poster);
                            cmb.selected.SavePoster(file1);
                        }
                        if (Properties.Settings.Default.SaveMyMoviesMeta)
                        {
                            Message("Saving Backdrop as " + file.Substring(file.LastIndexOf("\\") + 1), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                            Debug.Write("Saving Backdrop " + cmb.selected.Poster);
                            cmb.selected.SavePoster(file);
                        }
                        if (IsMovie)
                            UpdateMoviePoster(m, MoviePosterType.Backdrop, false);
                        else if (s == null)
                            UpdateTVPoster(ts, TVShowPosterType.Backdrop, false);
                        else
                            UpdateSeasonPoster(s, TVShowPosterType.Season_Backdrop, false);
                        Message("Done.", MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    }
                    catch (Exception ex)
                    {
                        Message("Unable to Change Backdrop : " + ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                    }
                });
                th.SetApartmentState(ApartmentState.STA);
                th.Start();
            }
            else if (cmb.Decision == ImageWindowDecisionbType.LocalPosterSelected)
            {
                try
                {
                    String selfile = cmb.selectedLocalPoster.Poster;

                    if (Properties.Settings.Default.SaveXBMCMeta)
                        File.Copy(selfile, file1, true);
                    
                    if (Properties.Settings.Default.SaveMyMoviesMeta)
                        File.Copy(selfile, file, true);
                                        
                    if (IsMovie)
                        UpdateMoviePoster(m, MoviePosterType.Backdrop, false);
                    else if (s == null)
                        UpdateTVPoster(ts, TVShowPosterType.Backdrop, false);
                    else
                        UpdateSeasonPoster(s, TVShowPosterType.Season_Backdrop, false);
                }
                catch (Exception ex)
                {
                    Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                }
            }
        }
        
        #endregion
        
        #region TV Series Image Functions

        #region Managed Image Display Routines

        private void UpdateTVPoster(TVShow ts, TVShowPosterType type, bool IsLoading)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new TVShowImageChangedHandler(UpdateTVPoster), ts, type, IsLoading);
                return;
            }

            switch (type)
            {
                case TVShowPosterType.Poster:
                    if (IsLoading)
                    {
                        imgTVPoster.Source = null;
                        ts.isPosterLoading = true;
                        imgTVPoster.SetLoading = true;
                    }
                    else
                    {
                        ts.isPosterLoading = false;
                        if (ts == null)
                            imgTVPoster.Source = null;
                        else
                            imgTVPoster.Source = ts.GetImage(type);
                        //imgTVPoster.Visibility = (String.IsNullOrEmpty(ts.Poster) && !IsLoading) ? Visibility.Collapsed : Visibility.Visible;
                    }
                    break;
                case TVShowPosterType.Backdrop:
                    if (IsLoading)
                    {
                        imgTVBackdrop.Source = null;
                        ts.isBackDropLoading = true;
                        imgTVBackdrop.SetLoading = true;
                    }
                    else
                    {
                        ts.isBackDropLoading = false;
                        if (ts == null)
                            imgTVBackdrop.Source = null;
                        else
                            imgTVBackdrop.Source = ts.GetImage(type);
                        //imgTVBackdrop.Visibility = (String.IsNullOrEmpty(ts.Backdrop) && !IsLoading) ? Visibility.Collapsed : Visibility.Visible;
                    }
                    break;
                case TVShowPosterType.Banner:
                    if (IsLoading)
                    {
                        imgTVBanner.Source = null;
                        ts.isBannerLoading = true;
                        imgTVBanner.SetLoading = true;
                    }
                    else
                    {
                        ts.isBannerLoading = false;
                        if (ts == null)
                            imgTVBanner.Source = null;
                        else
                            imgTVBanner.Source = ts.GetImage(type);
                        //imgTVBanner.Visibility = (String.IsNullOrEmpty(ts.Banner) && !IsLoading) ? Visibility.Collapsed : Visibility.Visible;
                    }
                    break;
            }
        }
        private void UpdateSeasonPoster(Season s, TVShowPosterType type,  bool IsLoading)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new SeasonImageChangedHandler(UpdateSeasonPoster), s, type, IsLoading);
                return;
            }
            switch (type)
            {
                case TVShowPosterType.Season_Poster:
                    if (IsLoading)
                    {
                        imgSeasonPoster.Source = null;
                        s.isPosterLoading = true;
                        imgSeasonPoster.SetLoading = true;
                    }
                    else
                    {
                        s.isPosterLoading = false;
                        imgSeasonPoster.Source = s.GetImage(type);
                    }
                    //imgSeasonPoster.Visibility = (String.IsNullOrEmpty(s.Poster) && !IsLoading) ? Visibility.Collapsed : Visibility.Visible;
                    break;
                case TVShowPosterType.Season_Backdrop:
                    if (IsLoading)
                    {
                        imgSeasonBackdrop.Source = null;
                        s.isBackDropLoading = true;
                        imgSeasonBackdrop.SetLoading = true;
                    }
                    else
                    {
                        s.isBackDropLoading = false;
                        imgSeasonBackdrop.Source = s.GetImage(type);
                    }
                    //imgSeasonBackdrop.Visibility = (String.IsNullOrEmpty(s.Backdrop) && !IsLoading) ? Visibility.Collapsed : Visibility.Visible;
                    break;
            }
        }
        private void UpdateEpisodePoster(Episode e, String filename, bool IsLoading)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new EpisodeImageChangedHandler(UpdateEpisodePoster), e, filename, IsLoading);
                return;
            }
            if (IsLoading)
                imgEpisodePoster.SetLoading = true;
            else
                imgEpisodePoster.Source = e.GetImage(filename);
            //imgEpisodePoster.Visibility = (String.IsNullOrEmpty(e.Poster) && !IsLoading) ? Visibility.Collapsed : Visibility.Visible;
        }
        
        #endregion
        
        #region TV And Season Poster Functions
        private void btnChangeSelectedTVPoster_Click(object sender, RoutedEventArgs e)
        {
            TVShow ts = SelectedTVShow;            
            ChangeSelectedTVorSeasonPoster(ts, null, null, false);
        }
        private void btnChangeSelectedSeasonPoster_Click(object sender, RoutedEventArgs e)
        {
            Season s = SelectedSeason;
            TVShow ts = s.TVShow;
            ChangeSelectedTVorSeasonPoster(ts, s, null, false);
        }
        private void ChangeSelectedTVorSeasonPoster(TVShow ts, Season s, String id, bool noDialog)
        {
            bool IsSeason = (s != null) ? true : false;
            String file = (IsSeason ? s.Folderpath : ts.Folderpath) + @"\folder.jpg";

            if (noDialog)
            {
                if ((!File.Exists(file)) || (Properties.Settings.Default.forceUpdate))
                {
                    MediaScout.Posters[] posters = tvdb.GetPosters(id, (IsSeason) ? TVShowPosterType.Season_Poster : TVShowPosterType.Poster, (IsSeason) ? s.GetNum() : -1);
                    if (posters != null)
                    {
                        try
                        {
                            Message("Saving Poster[ ( " + posters[0].Poster + ") as " + file, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                            posters[0].SavePoster(file);
                        }
                        catch (Exception ex)
                        {
                            Message("Unable to Change Poster : " + ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                        }
                    }
                    else
                        Message("No Posters Found", MediaScout.MediaScoutMessage.MessageType.Error, 0);
                }
            }
            else
            {
                //ShowCancelButtons();

                //th = new Thread(delegate()
                //{
                    if (id == null)
                        id = SearchForID(ts, false, ts.Name, null, false, Properties.Settings.Default.forceEnterSearchTerm).SelectedID;
                    if (id != null)
                        ShowPosterDialog(ts, id, false, s, file);
                //});
                //th.SetApartmentState(ApartmentState.STA);
                //th.Start();
                //tvThreads.Add(th);
            }
        }
        #endregion

        #region TV And Season Backdrop Functions
        private void btnChangeSelectedTVBackdrop_Click(object sender, RoutedEventArgs e)
        {
            TVShow ts = SelectedTVShow;
            ChangeSelectedTVorSeasonBackdrop(ts, null, null, false);
        }
        private void btnChangeSelectedSeasonBackdrop_Click(object sender, RoutedEventArgs e)
        {
            Season s = SelectedSeason;
            TVShow ts = s.TVShow;

            ChangeSelectedTVorSeasonBackdrop(ts, s, null, false);
        }
        private void ChangeSelectedTVorSeasonBackdrop(TVShow ts, Season s, String id, bool noDialog)
        {
            bool IsSeason = (s != null) ? true : false;
            String file = (IsSeason ? s.Folderpath : ts.Folderpath) + @"\backdrop.jpg";
            String file1 = (IsSeason ? s.Folderpath : ts.Folderpath) + @"\fanart.jpg";

            if (noDialog)
            {
                if ((!File.Exists(file)) || (Properties.Settings.Default.forceUpdate))
                {
                    MediaScout.Posters[] posters = tvdb.GetPosters(id, (IsSeason) ? TVShowPosterType.Season_Backdrop : TVShowPosterType.Backdrop, (IsSeason) ? s.GetNum() : -1);
                    if (posters != null)
                    {
                        try
                        {
                            Message("Saving Backdrop[ ( " + posters[0].Poster + ") as " + file, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                            posters[0].SavePoster(file);
                            Message("Saving Backdrop[ ( " + posters[0].Poster + ") as " + file1, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                            posters[0].SavePoster(file1);
                        }
                        catch (Exception ex)
                        {
                            Message("Unable to Change Backdrop : " + ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                        }
                    }
                    else
                        Message("No Backdrops Found", MediaScout.MediaScoutMessage.MessageType.Error, 0);
                }
            }
            else
            {
                //ShowCancelButtons();

                //th = new Thread(delegate()
                //{
                    if (id == null)
                        id = SearchForID(ts, false, ts.Name, null, false, Properties.Settings.Default.forceEnterSearchTerm).SelectedID;
                    if (id != null)
                        ShowBackdropDialog(ts, id, false, s, file, file1);
                //});
                //th.SetApartmentState(ApartmentState.STA);
                //th.Start();
                //tvThreads.Add(th);
            }
        }
        #endregion

        #region TV Banner Functions
        private void btnChangeSelectedTVBanner_Click(object sender, RoutedEventArgs e)
        {
            TVShow s = SelectedTVShow;
            ChangeSelectedTVBanner(s, null, false);
        }
        private void ChangeSelectedTVBanner(TVShow ts, String id, bool noDialog)
        {
            if (id == null)
                id = SearchForID(ts, false, ts.Name, null, false, Properties.Settings.Default.forceEnterSearchTerm).SelectedID;

            if (id != null)
            {
                if (noDialog)
                {
                    String file = ts.Folderpath + @"\banner.jpg";
                    if ((!File.Exists(file)) || (Properties.Settings.Default.forceUpdate))
                    {
                        MediaScout.Posters[] posters = tvdb.GetPosters(id, TVShowPosterType.Banner, -1);
                        if (posters != null)
                        {
                            Message("Saving Banner ( " + posters[0].Poster + ") as " + file, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                            try
                            {
                                posters[0].SavePoster(file);
                            }
                            catch (Exception ex)
                            {
                                Message("Unable to Change Banner : " + ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                            }
                        }
                        else
                            Message("No Banners Found", MediaScout.MediaScoutMessage.MessageType.Error, 0);
                    }
                }
                else
                {
                    ChangeTVBanner ctb = new ChangeTVBanner(id, tvdb);
                    ctb.Owner = this;
                    ctb.ShowDialog();
                    if (ctb.DownloadAll)
                    {
                        Thread dallb = new Thread(delegate()
                        {
                            MediaScout.Posters[] posters = tvdb.GetPosters(id, TVShowPosterType.Banner, -1);
                            maxvalue += posters.Length;
                            int i = 0;
                            foreach (MediaScout.Posters p in posters)
                            {
                                String file = ts.Folderpath + @"\banner" + i + ".jpg";
                                Message("Saving Banner (" + p.Poster + ") as " + file, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                                try
                                {
                                    p.SavePoster(file);
                                }
                                catch (Exception ex)
                                {
                                    Message("Unable to Change Banner : " + ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                                }
                                i++;
                                SetTasbkBarProgressValue(++currentvalue);                                
                            }                            
                            Message("All Banners Downloaded", MediaScout.MediaScoutMessage.MessageType.Task, 0);
                            //if(app.showballoontip)
                            //    app.notifyIcon.ShowBalloonTip(1, ts.Name, "All Banners Downloaded", System.Windows.Forms.ToolTipIcon.Info);
                        });
                        dallb.SetApartmentState(ApartmentState.STA);
                        dallb.Start();
                    }
                    else if (ctb.selected != null)
                    {
                        UpdateTVPoster(ts, TVShowPosterType.Banner, true);
                        Thread th = new Thread(delegate()
                        {
                            String file = ts.Folderpath + @"\banner.jpg";
                            Message("Saving Banner (" + ctb.selected.Poster + ") as " + file, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                            try
                            {
                                ctb.selected.SavePoster(file);
                                UpdateTVPoster(ts, TVShowPosterType.Banner, false);
                                Message("Done.", MediaScout.MediaScoutMessage.MessageType.Task, 0);
                            }
                            catch (Exception ex)
                            {
                                Message("Unable to Change Backdrop : " + ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                            }
                        });

                        th.SetApartmentState(ApartmentState.STA);
                        th.Start();
                    }
                }
            }
        }
        #endregion

        #region Episode Poster Functions
        private void btnEpisodePosterFromFile_Click(object sender, RoutedEventArgs e)
        {
            Episode ep = SelectedEpisode;

            String filename = null;
            if (Properties.Settings.Default.SaveXBMCMeta)
            {
                FileInfo fi = new FileInfo(ep.Filepath);
                filename = fi.Name.Replace(fi.Extension, "") + ".tbn";
                Message("Saving Episode Poster as " + filename, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                try
                {
                    MediaScout.VideoInfo.SaveThumb(ep.Filepath, ep.Season.MetadataFolderPath + filename, 0.25);
                    Message("Done.", MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    UpdateEpisodePoster(ep, filename, false);
                }
                catch (Exception ex)
                {
                    Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.TaskError, 0);
                }
            }

            if (Properties.Settings.Default.SaveMyMoviesMeta)
            {
                if (!Directory.Exists(ep.Season.MetadataFolderPath))
                    MediaScout.IOFunctions.CreateHiddenFolder(ep.Season.MetadataFolderPath);

                FileInfo fi = new FileInfo(ep.Filepath);
                filename = ((ep.ID != null) ? ep.ID : fi.Name.Replace(fi.Extension, "")) + ".jpg";
                Message("Saving Episode Poster as " + filename, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                try
                {
                    MediaScout.VideoInfo.SaveThumb(ep.Filepath, ep.Season.MetadataFolderPath + filename, 0.25);
                    Message("Done.", MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    UpdateEpisodePoster(ep, filename, false);
                }
                catch (Exception ex)
                {
                    Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.TaskError, 0);
                }
            }
        }
        #endregion

        #endregion

        #region Movie Image Functions

        #region Managed Image Display Routines
        private void UpdateMoviePoster(Movie m, MoviePosterType type, bool IsLoading)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Send, new MovieImageChangedHandler(UpdateMoviePoster), m, type, IsLoading);
                return;
            }            
            switch (type)
            {
                case MoviePosterType.Poster:
                    if (IsLoading)
                    {
                        imgMoviePoster.Source = null;
                        m.isPosterLoading = true;
                        imgMoviePoster.SetLoading = true;
                    }
                    else
                    {
                        m.isPosterLoading = false;

                        if (m == null)
                            imgMoviePoster.Source = null;
                        else
                            imgMoviePoster.Source = m.GetImage(type);
                    }
                    //imgMoviePoster.Visibility = (String.IsNullOrEmpty(m.Poster) &&!IsLoading) ? Visibility.Collapsed : Visibility.Visible;
                    break;
                case MoviePosterType.Backdrop:
                    if (IsLoading)
                    {
                        imgMovieBackdrop.Source = null;
                        m.isBackDropLoading = true;
                        imgMovieBackdrop.SetLoading = true;
                    }
                    else
                    {
                        m.isBackDropLoading = false;

                        if (m == null)
                            imgMovieBackdrop.Source = null;
                        else
                            imgMovieBackdrop.Source = m.GetImage(type);
                    }
                    //imgMovieBackdrop.Visibility = (String.IsNullOrEmpty(m.Backdrop) && !IsLoading) ? Visibility.Collapsed : Visibility.Visible;
                    break;                
            }
        }
        private void UpdateMovieFilePoster(MovieFile mf, MoviePosterType type, bool IsLoading)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Send, new MovieFileImageChangedHandler(UpdateMovieFilePoster), mf, type, IsLoading);
                return;
            }
            switch (type)
            {
                case MoviePosterType.File_Poster:
                    if (IsLoading)
                    {
                        imgMovieFilePoster.Source = null;
                        mf.isPosterLoading = true;
                        imgMovieFilePoster.SetLoading = true;
                    }
                    else
                    {
                        mf.isPosterLoading = false;
                        imgMovieFilePoster.Source = mf.GetImage(type);
                    }
                    //imgMovieFilePoster.Visibility = (String.IsNullOrEmpty(mf.Poster) && !IsLoading) ? Visibility.Collapsed : Visibility.Visible;
                    break;
                case MoviePosterType.File_Backdrop:
                    if (IsLoading)
                    {
                        imgMovieFileBackdrop.Source = null;
                        mf.isBackDropLoading = true;
                        imgMovieFileBackdrop.SetLoading = true;
                    }
                    else
                    {
                        mf.isBackDropLoading = false;
                        imgMovieFileBackdrop.Source = mf.GetImage(type);
                    }
                    //imgMovieFileBackdrop.Visibility = (String.IsNullOrEmpty(mf.Backdrop) && !IsLoading) ? Visibility.Collapsed : Visibility.Visible;
                    break;
            }
        }
        
        #endregion
        
        #region Movie Poster Functions
        private void btnChangeMoviePoster_Click(object sender, RoutedEventArgs e)
        {
            Movie m = SelectedMovie;
            ChangeSelectedMoviePoster(m, null, null, false);
        }
        private void btnChangeMovieFilePoster_Click(object sender, RoutedEventArgs e)
        {
            MovieFile mf = SelectedMovieFile;
            Movie m = mf.Movie;
            ChangeSelectedMoviePoster(m, null, null, false);
        }
        private void ChangeSelectedMoviePoster(Movie m, MovieFile mf, String id, bool noDialog)
        {
            String file = m.Folderpath + (mf!=null ? mf.StrippedFileName + ".tbn" : @"\folder.jpg");

            if (noDialog)
            {
                if ((!File.Exists(file)) || (Properties.Settings.Default.forceUpdate))
                {
                    MediaScout.Posters[] posters = tmdb.GetPosters(id, MoviePosterType.Poster);
                    if (posters != null)
                    {
                        Message("Saving Poster ( " + posters[0].Poster + ") as " + file, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                        try
                        {
                            posters[0].SavePoster(file);
                        }
                        catch (Exception ex)
                        {
                            Message("Unable to Change Poster : " + ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                        }
                    }
                    else
                        Message("No Posters found", MediaScout.MediaScoutMessage.MessageType.Error, 0);
                }
            }
            else
            {
                //ShowCancelButtons();

                //th = new Thread(delegate()
                //{
                    if (id == null)
                        id = SearchForID(m, true, m.Name, null, false, Properties.Settings.Default.forceEnterSearchTerm).SelectedID;
                    if (id != null)
                        ShowPosterDialog(m, id, true, null, file);
                //});
                //th.SetApartmentState(ApartmentState.STA);
                //th.Start();
                //tvThreads.Add(th);
            }
        }
        #endregion

        #region Movie Backdrop Functions
        private void btnChangeMovieBackdrop_Click(object sender, RoutedEventArgs e)
        {
            Movie m = SelectedMovie;
            ChangeSelectedMovieBackdrop(m, null, m.ID, false);
        }
        private void btnChangeMovieFileBackdrop_Click(object sender, RoutedEventArgs e)
        {
            MovieFile mf = SelectedMovieFile;
            Movie m = mf.Movie;
            ChangeSelectedMovieBackdrop(m, mf, m.ID, false);
        }  
        private void ChangeSelectedMovieBackdrop(Movie m, MovieFile mf, String id, bool noDialog)
        {
            String file = m.Folderpath + @"\backdrop.jpg";
            String file1 = m.Folderpath + (mf != null ? mf.StrippedFileName + ".jpg" : @"\fanart.jpg");

            if (noDialog)
            {
                if ((!File.Exists(file)) || (Properties.Settings.Default.forceUpdate))
                {
                    MediaScout.Posters[] posters = tmdb.GetPosters(id, MoviePosterType.Backdrop);
                    if (posters != null)
                    {
                        Message("Saving Backdrop ( " + posters[0].Poster + ") as " + file, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                        try
                        {
                            posters[0].SavePoster(file);
                        }
                        catch (Exception ex)
                        {
                            Message("Unable to Change Backdrop : " + ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                        }
                    }
                    else
                        Message("No Backdrops found", MediaScout.MediaScoutMessage.MessageType.Error, 0);
                }
            }
            else
            {
                //ShowCancelButtons();

                //th = new Thread(delegate()
                //    {
                        if (id == null)
                            id = SearchForID(m, true, m.Name, null, false, Properties.Settings.Default.forceEnterSearchTerm).SelectedID;
                        if (id != null)
                            ShowBackdropDialog(m, id, true, null, file, file1);
                //    });
                //th.SetApartmentState(ApartmentState.STA);
                //th.Start();
                //tvThreads.Add(th);
            }
        }
        #endregion

        #endregion

        #region Actor Thumb Images Function
        private void btnChangeActorThumb_Click(object sender, RoutedEventArgs e)
        {
            Person p = SelectedPerson;

            String file = p.XBMCFolderPath + "\\" + p.Name.Replace(" " ,"_") + ".jpg";
            String file1 = p.MyMoviesFolderPath + "\\" + p.Name.Replace(" " ,"_") + @"\folder.jpg";

            if (!Directory.Exists(p.XBMCFolderPath))
                MediaScout.IOFunctions.CreateHiddenFolder(p.XBMCFolderPath);

            if (!Directory.Exists(p.MyMoviesFolderPath + "\\" + p.Name.Replace(" ", "_")))
                Directory.CreateDirectory(p.MyMoviesFolderPath + "\\" + p.Name.Replace(" ", "_"));

            if(p.IsMovieActor)
                ShowActorImageDialog(SelectedMovie, p, file, file1);
            else
                ShowActorImageDialog(SelectedTVShow, p, file, file1);

        }
        #endregion
        
        #endregion


        #region Options Routines : 47 

        #region TV Series Options Routines : 19 

        #region Sets the TV Folders
        
        private void btnSetTVFolders_Click(object sender, RoutedEventArgs e)
        {
            FoldersDialog fd = new FoldersDialog(false);
            fd.Owner = this;
            if (fd.ShowDialog() == true)
            {
                if (Properties.Settings.Default.TVFolders == null)
                    Properties.Settings.Default.TVFolders = new System.Collections.Specialized.StringCollection();
                else
                    Properties.Settings.Default.TVFolders.Clear();
                foreach (String dir in fd.lstFolders.Items)
                    Properties.Settings.Default.TVFolders.Add(dir);
                Properties.Settings.Default.Save();
                resetTVfolder = true;
            }
        }
        
        #endregion

        #region TV DropBox Location

        private void ChangeMonitorTVFolder()
        {
            if (Directory.Exists(txtTVDropBox.Text))
                chkTVFSWatcher.IsEnabled = true;
        }
        private void TVDropBoxFolderChanged()
        {
            if (WindowLoaded)
            {
                if (Properties.Settings.Default.TVDropBoxLocation != txtTVDropBox.Text)
                {
                    Properties.Settings.Default.TVDropBoxLocation = txtTVDropBox.Text;
                    Properties.Settings.Default.Save();
                    ChangeMonitorTVFolder();
                }
            }
        }
        private void txtTVDropBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TVDropBoxFolderChanged();
        }
        private void btnBrowserTVDropBox_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog myBrowser = new System.Windows.Forms.FolderBrowserDialog();
            myBrowser.Description = "Select TV Shows DropBox folder";

            if (Directory.Exists(txtTVDropBox.Text))
                myBrowser.SelectedPath = txtTVDropBox.Text;

            if (myBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtTVDropBox.Text = myBrowser.SelectedPath;
                TVDropBoxFolderChanged();
            }
        }
        
        #endregion

        #region TV FS Watcher Checkbox Functions
        private void chkTVFSWatcher_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                if (TVFSWatcher == null)
                {
                    TVFSWatcher = new FileSystemWatcher();
                    TVFSWatcher.Path = Properties.Settings.Default.TVDropBoxLocation;
                    TVFSWatcher.IncludeSubdirectories = true;
                    TVFSWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName;
                    TVFSWatcher.Created += new FileSystemEventHandler(TVFSWatcher_Changed);
                    TVFSWatcher.Changed += new FileSystemEventHandler(TVFSWatcher_Changed);
                }
                TVFSWatcher.EnableRaisingEvents = chkTVFSWatcher.IsChecked.Value;

                Properties.Settings.Default.TVFSWatcher = chkTVFSWatcher.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkTVFSWatcher_Unchecked(object sender, RoutedEventArgs e)
        {
            chkTVFSWatcher_ValueChanged(sender, e);
        }
        private void chkTVFSWatcher_Checked(object sender, RoutedEventArgs e)
        {
            chkTVFSWatcher_ValueChanged(sender, e);
        }
        #endregion

        #region Options 1 Posters and Move TV File Options : 6 

        #region Get Series Posters
        
        private void chkSeriesPosters_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.getSeriesPosters = chkSeriesPosters.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkSeriesPosters_Checked(object sender, RoutedEventArgs e)
        {
            chkSeriesPosters_ValueChanged(sender, e);
        }
        private void chkSeriesPosters_Unchecked(object sender, RoutedEventArgs e)
        {
            chkSeriesPosters_ValueChanged(sender, e);
        }
        
        #endregion
        
        #region Get Season Posters
        
        private void chkSeasonPosters_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.getSeasonPosters = chkSeasonPosters.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkSeasonPosters_Checked(object sender, RoutedEventArgs e)
        {
            chkSeasonPosters_ValueChanged(sender, e);
        }
        private void chkSeasonPosters_Unchecked(object sender, RoutedEventArgs e)
        {
            chkSeasonPosters_ValueChanged(sender, e);
        }
        
        #endregion
        
        #region Get Episode Posters
        
        private void chkEpPosters_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.getEpisodePosters = chkEpPosters.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkEpPosters_Checked(object sender, RoutedEventArgs e)
        {
            chkEpPosters_ValueChanged(sender, e);
        }
        private void chkEpPosters_Unchecked(object sender, RoutedEventArgs e)
        {
            chkEpPosters_ValueChanged(sender, e);
        }
        
        #endregion

        #region Move TV Files
        
        private void chkMoveTVFiles_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.moveTVFiles = chkMoveTVFiles.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkMoveTVFiles_Checked(object sender, RoutedEventArgs e)
        {
            chkMoveTVFiles_ValueChanged(sender, e);
        }
        private void chkMoveTVFiles_Unchecked(object sender, RoutedEventArgs e)
        {
            chkMoveTVFiles_ValueChanged(sender, e);
        }
        
        #endregion

        #region Rename TV Files

        private void chkRenameTVFiles_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.renameTVFiles = chkRenameTVFiles.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkRenameTVFiles_Checked(object sender, RoutedEventArgs e)
        {
            chkRenameTVFiles_ValueChanged(sender, e);
        }
        private void chkRenameTVFiles_Unchecked(object sender, RoutedEventArgs e)
        {
            chkRenameTVFiles_ValueChanged(sender, e);
        }

        #endregion

        #region Save TV Actors Thumb

        private void chkSaveTVActors_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.SaveTVActors = chkSaveTVActors.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkSaveTVActors_Checked(object sender, RoutedEventArgs e)
        {
            chkSaveTVActors_ValueChanged(sender, e);
        }
        private void chkSaveTVActors_Unchecked(object sender, RoutedEventArgs e)
        {
            chkSaveTVActors_ValueChanged(sender, e);
        }

        #endregion
        
        #endregion

        #region Options 2 Season Folder and All Images Options : 8 

        #region Season And Specials Folder Name

        private void txtSeasonFolderName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.SeasonFolderName = txtSeasonFolderName.Text;
                Properties.Settings.Default.Save();
            }
        }
        private void txtSpecialsFolderName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.SpecialsFolderName = txtSpecialsFolderName.Text;
                Properties.Settings.Default.Save();
            }
        }
        
        #endregion

        #region Download All Images
        private void chkdownloadAllTVImages_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                chkdownloadAllTVBackdrops.IsChecked = chkdownloadAllTVImages.IsChecked.Value;
                chkdownloadAllTVBanners.IsChecked = chkdownloadAllTVImages.IsChecked.Value;
                chkdownloadAllTVPosters.IsChecked = chkdownloadAllTVImages.IsChecked.Value;
                chkdownloadAllTVSeasonPosters.IsChecked = chkdownloadAllTVImages.IsChecked.Value;
                chkdownloadAllTVSeasonBackdrops.IsChecked = chkdownloadAllTVImages.IsChecked.Value;

                Properties.Settings.Default.downloadAllTVImages = chkdownloadAllTVImages.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkdownloadAllTVImages_Checked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllTVImages_ValueChanged(sender, e);
        }
        private void chkdownloadAllTVImages_Unchecked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllTVImages_ValueChanged(sender, e);
        }
        #endregion
        
        #region Download All Posters
        private void chkdownloadAllTVPosters_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.downloadAllTVPosters = chkdownloadAllTVPosters.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkdownloadAllTVPosters_Checked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllTVPosters_ValueChanged(sender, e);
        }
        private void chkdownloadAllTVPosters_Unchecked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllTVPosters_ValueChanged(sender, e);
        }
        #endregion

        #region Download All Backdrops
        private void chkdownloadAllTVBackdrops_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.downloadAllTVBackdrops = chkdownloadAllTVBackdrops.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkdownloadAllTVBackdrops_Checked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllTVBackdrops_ValueChanged(sender, e);
        }
        private void chkdownloadAllTVBackdrops_Unchecked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllTVBackdrops_ValueChanged(sender, e);
        }
        #endregion

        #region Download All Banners
        private void chkdownloadAllTVBanners_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.downloadAllTVBanners = chkdownloadAllTVBanners.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkdownloadAllTVBanners_Checked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllTVBanners_ValueChanged(sender, e);
        }
        private void chkdownloadAllTVBanners_Unchecked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllTVBanners_ValueChanged(sender, e);
        }
        #endregion
        
        #region Download All Season Posters
        private void chkdownloadAllTVSeasonPosters_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.downloadAllTVSeasonPosters = chkdownloadAllTVSeasonPosters.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkdownloadAllTVSeasonPosters_Checked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllTVSeasonPosters_ValueChanged(sender, e);
        }
        private void chkdownloadAllTVSeasonPosters_Unchecked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllTVSeasonPosters_ValueChanged(sender, e);
        }
        #endregion

        #region Download All Season Backdrops

        private void chkdownloadAllTVSeasonBackdrops_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.downloadAllTVSeasonBackdrops = chkdownloadAllTVSeasonBackdrops.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkdownloadAllTVSeasonBackdrops_Checked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllTVSeasonBackdrops_ValueChanged(sender, e);
        }
        private void chkdownloadAllTVSeasonBackdrops_Unchecked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllTVSeasonBackdrops_ValueChanged(sender, e);
        }

        #endregion
        
        #endregion

        #region Options 3 Rename TV Files Options : 3 

        #region TV Rename Format
        
        private void txtTVRenameFormat_LostFocus(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.TVfileformat = txtTVRenameFormat.Text;
                Properties.Settings.Default.Save();
            }
        }
        
        #endregion

        #region Season Zero Pad Format
        private void txtSeasonNumZeroPadding_LostFocus(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.SeasonNumZeroPadding = txtSeasonNumZeroPadding.Text;
                Properties.Settings.Default.Save();
            }
        }
        #endregion
        
        #region Episode Zero Pad Format
        private void txtEpisodeNumZeroPadding_LostFocus(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.EpisodeNumZeroPadding = txtEpisodeNumZeroPadding.Text;
                Properties.Settings.Default.Save();
            }
        }
        #endregion
        
        #region Rename FormatText Changed (Not a Saveable Option)
        
        private void txtTVRenameFormat_TextChanged(object sender, TextChangedEventArgs e)
        {
            setTVRenameExample();
        }
        private void txtSeasonNumZeroPad_TextChanged(object sender, TextChangedEventArgs e)
        {
            setTVRenameExample();
        }
        private void txtEpisodeNumZeroPad_TextChanged(object sender, TextChangedEventArgs e)
        {
            setTVRenameExample();
        }
        private void setTVRenameExample()
        {
            //Wrap this in a try because when MediaScout is initially loading the user set options, some of the UI
            // elements do not exist yet and can crash
            try
            {
                String ExampleFormat = makeEpisodeTargetName("Series Name", "1", "Episode Name", "3", "Suffix", ".ext");
                if (ExampleFormat.Length > 0)
                    lblTVRenameExample.Content = ExampleFormat;
                else
                    lblTVRenameExample.Content = "invalid";
            }
            catch { }
        }
        private String makeEpisodeTargetName(String SeriesName, String SeasonNum, String EpisodeName, String EpisodeNum, String Suffix, String FileExtention)
        {
            //Wrap this whole thing in a TRY/CATCH because an invalid format pettern would cause a crash
            try
            {
                //Strip out the suffix from the rename format, alog with any leading characters that are not {varibles}
                String TargetName = Regex.Replace(txtTVRenameFormat.Text, @"([^\}]*?\{4\}[^\{]*)", @"");

                TargetName = String.Format(TargetName + FileExtention,
                    SeriesName,
                    SeasonNum.PadLeft(Int32.Parse(txtSeasonNumZeroPadding.Text), '0'),
                    EpisodeName,
                    EpisodeNum.PadLeft(Int32.Parse(txtEpisodeNumZeroPadding.Text), '0'));


                // Should do sone "file safe" character checking here (or earlier)
                return TargetName;
            }
            catch
            {
                // Could probably do something more graceful
                return null;
            }
        }
        
        #endregion
        
        #endregion

        #endregion

        #region Movies Options Routines : 12 

        #region Sets the Movie Folders

        private void btnSetMovieFolders_Click(object sender, RoutedEventArgs e)
        {
            FoldersDialog fd = new FoldersDialog(true);
            fd.Owner = this;
            if (fd.ShowDialog() == true)
            {
                if (Properties.Settings.Default.MovieFolders == null)
                    Properties.Settings.Default.MovieFolders = new System.Collections.Specialized.StringCollection();
                else
                    Properties.Settings.Default.MovieFolders.Clear();
                foreach (String dir in fd.lstFolders.Items)
                    Properties.Settings.Default.MovieFolders.Add(dir);
                Properties.Settings.Default.Save();
                resetMoviefolder = true;
            }
        }

        #endregion

        #region Movie DropBox Location
        private void ChangeMonitorMovieFolder()
        {
            if (Directory.Exists(txtMovieDropBox.Text))
                chkMovieFSWatcher.IsEnabled = true;
        }
        private void MovieDropBoxFolderChanged()
        {
            if (WindowLoaded)
            {
                if (Properties.Settings.Default.MovieDropBoxLocation != txtMovieDropBox.Text)
                {
                    Properties.Settings.Default.MovieDropBoxLocation = txtMovieDropBox.Text;
                    Properties.Settings.Default.Save();
                    ChangeMonitorMovieFolder();
                }
            }
        }
        private void txtMovieDropBox_LostFocus(object sender, RoutedEventArgs e)
        {
            MovieDropBoxFolderChanged();
        }
        private void btnBrowserMovieDropBox_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog myBrowser = new System.Windows.Forms.FolderBrowserDialog();
            myBrowser.Description = "Select Movies DropBox folder";

            if (Directory.Exists(txtMovieDropBox.Text))
                myBrowser.SelectedPath = txtMovieDropBox.Text;

            if (myBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtMovieDropBox.Text = myBrowser.SelectedPath;
                MovieDropBoxFolderChanged();
            }
        }
        #endregion

        #region Movie FS Watcher Checkbox Functions
        private void chkMovieFSWatcher_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                if (MovieFSWatcher == null)
                {
                    MovieFSWatcher = new FileSystemWatcher();
                    MovieFSWatcher.Path = Properties.Settings.Default.MovieDropBoxLocation;
                    MovieFSWatcher.IncludeSubdirectories = true;
                    MovieFSWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName;
                    MovieFSWatcher.Created += new FileSystemEventHandler(MovieFSWatcher_Changed);
                    MovieFSWatcher.Changed += new FileSystemEventHandler(MovieFSWatcher_Changed);
                }
                MovieFSWatcher.EnableRaisingEvents = chkMovieFSWatcher.IsChecked.Value;

                Properties.Settings.Default.MovieFSWatcher = chkMovieFSWatcher.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkMovieFSWatcher_Unchecked(object sender, RoutedEventArgs e)
        {
            chkMovieFSWatcher_ValueChanged(sender, e);
        }
        private void chkMovieFSWatcher_Checked(object sender, RoutedEventArgs e)
        {
            chkMovieFSWatcher_ValueChanged(sender, e);
        }
        #endregion

        #region Options 1 Posters and Move, Rename Movie File Options : 5

        #region Get Movie Posters

        private void chkMoviesPosters_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.getMoviePosters = chkMoviePosters.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkMoviesPosters_Checked(object sender, RoutedEventArgs e)
        {
            chkMoviesPosters_ValueChanged(sender, e);
        }
        private void chkMoviesPosters_Unchecked(object sender, RoutedEventArgs e)
        {
            chkMoviesPosters_ValueChanged(sender, e);
        }
        
        #endregion

        #region Get Movie File Posters

        private void chkMovieFilePosters_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.getMovieFilePosters = chkMovieFilePosters.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkMovieFilePosters_Checked(object sender, RoutedEventArgs e)
        {
            chkMovieFilePosters_ValueChanged(sender, e);
        }
        private void chkMovieFilePosters_Unchecked(object sender, RoutedEventArgs e)
        {
            chkMovieFilePosters_ValueChanged(sender, e);
        }
        
        #endregion

        #region Move Movie Files

        private void chkMoveMovieFiles_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.moveTVFiles = chkMoveMovieFiles.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkMoveMovieFiles_Checked(object sender, RoutedEventArgs e)
        {
            chkMoveMovieFiles_ValueChanged(sender, e);
        }
        private void chkMoveMovieFiles_Unchecked(object sender, RoutedEventArgs e)
        {
            chkMoveMovieFiles_ValueChanged(sender, e);
        }

        #endregion

        #region Save Movie Actors Thumb

        private void chkSaveMovieActors_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.SaveMovieActors = chkSaveMovieActors.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkSaveMovieActors_Checked(object sender, RoutedEventArgs e)
        {
            chkSaveMovieActors_ValueChanged(sender, e);
        }
        private void chkSaveMovieActors_Unchecked(object sender, RoutedEventArgs e)
        {
            chkSaveMovieActors_ValueChanged(sender, e);
        }

        #endregion

        #region Rename Movie Files And Dir

        private void chkRenameMovieFiles_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.renameMovieFiles = chkRenameMovieFiles.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkRenameMovieFiles_Checked(object sender, RoutedEventArgs e)
        {
            chkRenameMovieFiles_ValueChanged(sender, e);
        }
        private void chkRenameMovieFiles_Unchecked(object sender, RoutedEventArgs e)
        {
            chkRenameMovieFiles_ValueChanged(sender, e);
        }

        #endregion
        
        #endregion

        #region Options 2 All Images Options : 3 

        #region Download All Movie Images
        private void chkdownloadAllMovieImages_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                chkdownloadAllMovieBackdrops.IsChecked = chkdownloadAllMovieImages.IsChecked;
                chkdownloadAllMoviePosters.IsChecked = chkdownloadAllMovieImages.IsChecked;
                Properties.Settings.Default.downloadAllMovieImages = chkdownloadAllMovieImages.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkdownloadAllMovieImages_Checked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllMovieImages_ValueChanged(sender, e);
        }
        private void chkdownloadAllMovieImages_Unchecked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllMovieImages_ValueChanged(sender, e);
        }
        #endregion
        
        #region Download All Movie osters
        private void chkdownloadAllMoviePosters_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.downloadAllMoviePosters = chkdownloadAllMoviePosters.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkdownloadAllMoviePosters_Checked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllMoviePosters_ValueChanged(sender, e);
        }
        private void chkdownloadAllMoviePosters_Unchecked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllMoviePosters_ValueChanged(sender, e);
        }
        #endregion
        
        #region Download All Movie Backdrops
        private void chkdownloadAllMovieBackdrops_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.downloadAllMovieBackdrops = chkdownloadAllMovieBackdrops.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkdownloadAllMovieBackdrops_Checked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllMovieBackdrops_ValueChanged(sender, e);
        }
        private void chkdownloadAllMovieBackdrops_Unchecked(object sender, RoutedEventArgs e)
        {
            chkdownloadAllMovieBackdrops_ValueChanged(sender, e);
        }
        #endregion
        
        #endregion

        #region Options 3 Rename Movie File And Dir Options : 2 
        
        #region Movie File Rename Format
        
        private void txtMovieFileRenameFormat_LostFocus(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.Moviefileformat = txtMovieFileRenameFormat.Text;
                Properties.Settings.Default.Save();
            }
        }
        
        #endregion

        #region Movie Dir Rename Format
        
        private void txtMovieDirRenameFormat_LostFocus(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.MovieDirformat = txtMovieDirRenameFormat.Text;
                Properties.Settings.Default.Save();
            }
        }
        
        #endregion
        
        #region Movie Rename Format Text Changed ( Not a Saveable Option)
        
        private void txtMovieFileRenameFormat_TextChanged(object sender, TextChangedEventArgs e)
        {
            setMovieFileRenameExample();
        }
        private void setMovieFileRenameExample()
        {
            //Wrap this in a try because when MediaScout is initially loading the user set options, some of the UI
            // elements do not exist yet and can crash
            try
            {
                String ExampleFormat = makeMovieTargetName(txtMovieFileRenameFormat.Text, "Movie Name", "2009", ".ext");
                if (ExampleFormat.Length > 0)
                    lblMovieFileRenameExample.Content = ExampleFormat;
                else
                    lblMovieFileRenameExample.Content = "invalid";
            }
            catch { }
        }        
        private void txtMovieDirRenameFormat_TextChanged(object sender, TextChangedEventArgs e)
        {
            setMovieDirRenameExample();
        }
        private void setMovieDirRenameExample()
        {
            //Wrap this in a try because when MediaScout is initially loading the user set options, some of the UI
            // elements do not exist yet and can crash
            try
            {
                String ExampleFormat = makeMovieTargetName(txtMovieDirRenameFormat.Text, "Movie Name", "2009", "");
                if (ExampleFormat.Length > 0)
                    lblMovieDirRenameExample.Content = ExampleFormat;
                else
                    lblMovieDirRenameExample.Content = "invalid";
            }
            catch { }
        }
        private String makeMovieTargetName(String format, String MovieName, String Year, String FileExtention)
        {
            //Wrap this whole thing in a TRY/CATCH because an invalid format pettern would cause a crash
            try
            {
                //Strip out the suffix from the rename format, alog with any leading characters that are not {varibles}
                String TargetName = Regex.Replace(format, @"([^\}]*?\{4\}[^\{]*)", @"");

                TargetName = String.Format(TargetName + FileExtention,
                    MovieName,
                    Year);

                // Should do sone "file safe" character checking here (or earlier)
                return TargetName;
            }
            catch
            {
                // Could probably do something more graceful
                return null;
            }
        }
        
        #endregion

        #endregion

        #endregion

        #region Common Options : 16 

        #region Force Updating of Files
        private void chkForceUpdate_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.forceUpdate = chkForceUpdate.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkForceUpdate_Checked(object sender, RoutedEventArgs e)
        {            
            chkForceUpdate_ValueChanged(sender, e);
        }
        private void chkForceUpdate_Unchecked(object sender, RoutedEventArgs e)
        {
            chkForceUpdate_ValueChanged(sender, e);
        }
        #endregion

        #region Hides Dialog
        private void chkSilentMode_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.SilentMode = chkSilentMode.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkSilentMode_Checked(object sender, RoutedEventArgs e)
        {
            chkSilentMode_ValueChanged(sender, e);            
        }
        private void chkSilentMode_Unchecked(object sender, RoutedEventArgs e)
        {
            chkSilentMode_ValueChanged(sender, e);
        }
        #endregion
        
        #region Forces Search Dailog to Appear
        private void chkforceEnterSearchTerm_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.forceEnterSearchTerm = chkforceEnterSearchTerm.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkforceEnterSearchTerm_Checked(object sender, RoutedEventArgs e)
        {
            chkforceEnterSearchTerm_ValueChanged(sender, e); 
        }
        private void chkforceEnterSearchTerm_Unchecked(object sender, RoutedEventArgs e)
        {
            chkforceEnterSearchTerm_ValueChanged(sender, e);
        }
        #endregion

        #region Allowed Filetypes
        private void txtAllowedFiletypes_LostFocus(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                if (Properties.Settings.Default.allowedFileTypes != txtAllowedFiletypes.Text)
                {
                    Properties.Settings.Default.allowedFileTypes = txtAllowedFiletypes.Text;
                    Properties.Settings.Default.Save();
                    AllowedFileTypes = new List<string>(Properties.Settings.Default.allowedFileTypes.Split(';'));
                    resetTVfolder = true;
                    resetMoviefolder = true;
                }
            }
        }
        #endregion
        
        #region Allowed Subtitles
        private void txtAllowedSubtitles_LostFocus(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.allowedSubtitles = txtAllowedSubtitles.Text;
                Properties.Settings.Default.Save();
                AllowedSubtitleTypes = new List<string>(Properties.Settings.Default.allowedSubtitles.Split(';'));
            }
        }
        #endregion

        #region Search Term Filters
        private void txtSearchTermFilters_LostFocus(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.SearchTermFilters = txtSearchTermFilters.Text;
                Properties.Settings.Default.Save();
            }
        }
       
        #endregion

        #region Enable Glass Frame
        private void chkEnableGlassFrame_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                SetGlassFrame(chkEnableGlassFrame.IsChecked.Value);
                Properties.Settings.Default.EnableGlassFrame = chkEnableGlassFrame.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkEnableGlassFrame_Checked(object sender, RoutedEventArgs e)
        {
            chkEnableGlassFrame_ValueChanged(sender, e);
        }
        private void chkEnableGlassFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            chkEnableGlassFrame_ValueChanged(sender, e);
        }
        #endregion

        #region Save XBMC Meta
        
        private void chkSaveXBMCMeta_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.SaveXBMCMeta = chkSaveXBMCMeta.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkSaveXBMCMeta_Checked(object sender, RoutedEventArgs e)
        {
            chkSaveXBMCMeta_ValueChanged(sender, e);
        }
        private void chkSaveXBMCMeta_Unchecked(object sender, RoutedEventArgs e)
        {
            chkSaveXBMCMeta_ValueChanged(sender, e);
        }

        #endregion

        #region Save MyMovies Meta

        private void chkSaveMMMeta_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.SaveMyMoviesMeta = chkSaveMMMeta.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkSaveMMMeta_Checked(object sender, RoutedEventArgs e)
        {
            chkSaveMMMeta_ValueChanged(sender, e);
        }
        private void chkSaveMMMeta_Unchecked(object sender, RoutedEventArgs e)
        {
            chkSaveMMMeta_ValueChanged(sender, e);
        }
        
        #endregion

        #region Set Sublight Options 4 
        
        private void btnSetSublightOptions_Click(object sender, RoutedEventArgs e)
        {
            SublightOptionsDialog sod = new SublightOptionsDialog();
            sod.Owner = this;
            if (sod.ShowDialog() == true)
            {
                Properties.Settings.Default.SublightUsername = sod.txtSublightUsername.Text;
                Properties.Settings.Default.SublightPassword = sod.txtSublightPassword.Text;
                Properties.Settings.Default.SublightCmd = sod.txtSublightCmd.Text;
                Properties.Settings.Default.Sublight = sod.txtSublight.Text;
                Properties.Settings.Default.Save();
            }
        }
        
        #endregion
        
        #region Overwrite Files
        private void chkOverwrite_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.overwriteFiles = chkOverwrite.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkOverwrite_Unchecked(object sender, RoutedEventArgs e)
        {
            chkOverwrite_ValueChanged(sender, e);
        }
        private void chkOverwrite_Checked(object sender, RoutedEventArgs e)
        {
            chkOverwrite_ValueChanged(sender, e);
        }
        #endregion

        #region ImagesByName Location
        private void btnBrowseIBN_Click(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                System.Windows.Forms.FolderBrowserDialog myBrowser = new System.Windows.Forms.FolderBrowserDialog();
                myBrowser.Description = "Select ImagesByName Folder";

                if (Directory.Exists(txtImagesByName.Text))
                    myBrowser.SelectedPath = txtImagesByName.Text;

                if (myBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtImagesByName.Text = myBrowser.SelectedPath;
                    if (Properties.Settings.Default.ImagesByNameLocation != txtImagesByName.Text)
                    {
                        Properties.Settings.Default.ImagesByNameLocation = txtImagesByName.Text;
                        Properties.Settings.Default.Save();
                    }
                }
            }
        }
        private void txtImagesByName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                if (Properties.Settings.Default.ImagesByNameLocation != txtImagesByName.Text)
                {
                    Properties.Settings.Default.ImagesByNameLocation = txtImagesByName.Text;
                    Properties.Settings.Default.Save();                    
                }
            }
        }
        #endregion

        #region Auto Select Movie Title

        private void chkAutoSelectMovieTitle_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.AutoSelectMovieTitle = chkAutoSelectMovieTitle.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }
        private void chkAutoSelectMovieTitle_Checked(object sender, RoutedEventArgs e)
        {
            chkAutoSelectMovieTitle_ValueChanged(sender, e);
        }
        private void chkAutoSelectMovieTitle_Unchecked(object sender, RoutedEventArgs e)
        {
            chkAutoSelectMovieTitle_ValueChanged(sender, e);
        }

        #endregion

        #region Search Term Filters
        private void txtFilenameReplaceChar_LostFocus(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                Properties.Settings.Default.FilenameReplaceChar = txtFilenameReplaceChar.Text;
                Properties.Settings.Default.Save();
            }
        }

        #endregion

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            About ab = new About(false);
            ab.Owner = this;
            ab.ShowDialog();

        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, "Are You sure you want to reset all settings?", "MediaScout", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.Reset();
                LoadOptions();
            }
        }

        #endregion

        #endregion


        #region TVDb Tab Routines

        private void btnOpenTVDb_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTVShow.ID != null)
                System.Diagnostics.Process.Start("http://thetvdb.com/?tab=series&id=" + SelectedTVShow.ID + "&lid=7");
            else
                System.Diagnostics.Process.Start("http://thetvdb.com/?string=" + SelectedTVShow.Name + "&searchseriesid=&tab=listseries&function=Search");
        }

       
        #endregion


        #region TMDb Tab Routines

        private void btnOpenTMDb_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMovie.ID != null)
                System.Diagnostics.Process.Start("http://www.themoviedb.org/movie/" + SelectedMovie.ID);            
        }
      

        #endregion


        #region Windows Mouse Wheel Routines
                
        //protected override void OnPreviewMouseWheel(MouseWheelEventArgs args)
        //{
        //    base.OnPreviewMouseWheel(args);
        //    if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
        //        Keyboard.IsKeyDown(Key.RightCtrl))
        //        zoomslider.Value += (args.Delta > 0) ? 0.1 : -0.1;
        //}
        //protected override void OnPreviewMouseDown(MouseButtonEventArgs args)
        //{
        //    base.OnPreviewMouseDown(args);
        //    if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
        //        Keyboard.IsKeyDown(Key.RightCtrl))
        //        if (args.MiddleButton == MouseButtonState.Pressed)
        //            zoomslider.ResetZoom();
        //}
        
        #endregion              


        #region Save Routines

        private void btnSaveTVShow_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.SaveMyMoviesMeta)
                SelectedTVShow.XMLBase.SaveXML(SelectedTVShow.Folderpath);
            if (Properties.Settings.Default.SaveXBMCMeta)
                SelectedTVShow.NFOBase.Save(SelectedTVShow.NFOFile);

            SelectedTVShow.MetadataChanged = false;
        }
        private void btnSaveMovie_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.SaveMyMoviesMeta)
                SelectedMovie.XMLBase.SaveXML(SelectedMovie.Folderpath);
            if (Properties.Settings.Default.SaveXBMCMeta)
                SelectedMovie.NFOBase.Save(SelectedMovie.NFOFile);

            SelectedMovie.MetadataChanged = false;
        }
        private void btnSaveEpisode_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.SaveMyMoviesMeta)
                SelectedEpisode.XMLBase.SaveXML(SelectedEpisode.Season.Folderpath, SelectedEpisode.StrippedFileName);
            if (Properties.Settings.Default.SaveXBMCMeta)
                SelectedEpisode.NFOBase.Save(SelectedEpisode.NFOFile);

            SelectedEpisode.MetadataChanged = false;
        }

        #endregion


        #region Search Routines
        public bool TVShowSearchContains(object tvshow)
        {
            TVShow ts = tvshow as TVShow;
            bool success = false;
            //Return members whose Orders have not been filled
            success = (ts.Name.LastIndexOf(txtSearchTVShow.Text, StringComparison.CurrentCultureIgnoreCase) != -1)
                ||
                (ts.Year!=null && ts.Year.LastIndexOf(txtSearchTVShow.Text) != -1)
                ||
                (ts.Genre !=null && ts.Genre.Contains(new MediaScout.Genre() { name = txtSearchTVShow.Text }));
            
            return success;
        }
        private void txtSearchTVShow_Search(object sender, RoutedEventArgs e)
        {
            CollectionView source = CollectionViewSource.GetDefaultView(tvTVShows.ItemsSource) as CollectionView;
            source.Filter += new Predicate<object>(TVShowSearchContains);
        }
        
        public bool MovieSearchContains(object movie)
        {
            Movie m = movie as Movie;
            bool success = false;
            //Return members whose Orders have not been filled
            success = (m.Name.LastIndexOf(txtSearchMovie.Text, StringComparison.CurrentCultureIgnoreCase) != -1)
                ||
                (m.Year != null && m.Year.LastIndexOf(txtSearchMovie.Text) != -1)
                ||
                (m.Genre != null && m.Genre.Contains(new MediaScout.Genre() { name = txtSearchMovie.Text }));

            return success;
        }
        private void txtSearchMovie_Search(object sender, RoutedEventArgs e)
        {
            CollectionView source = CollectionViewSource.GetDefaultView(lbMovies.ItemsSource) as CollectionView;
            source.Filter += new Predicate<object>(MovieSearchContains);
        }
        #endregion #endregion                      
    }    

    public enum ImageWindowDecisionbType
    {
        PosterSelected,
        DownloadAll,
        LocalPosterSelected,
        Cancel
    }
    public enum DecisionType
    {
        Continue,
        Cancel,
        Skip,
        SearchAgain
    }
}