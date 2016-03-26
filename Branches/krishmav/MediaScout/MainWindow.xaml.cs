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
        public delegate void SeasonChangedHandler(TVShow ts, Season s, bool IsRemoved);
        public event SeasonChangedHandler SeasonChanged;
        public delegate void EpisodeChangedHandler(TVShow ts, Season s, Episode e, bool IsRemoved);
        public event EpisodeChangedHandler EpisodeChanged;
        public delegate void MovieChangedHandler(Movie m, bool IsRemoved);
        public event MovieChangedHandler MovieChanged;
        public delegate void MovieFileChangedHandler(Movie m, MovieFile mf, bool IsRemoved);
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
        private bool WindowLoaded;
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
                //Debug.Write(ex.Message);
            }                                   
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Rect bounds = GetBoundsForGlassFrame();
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));
            
            WindowLoaded = true;
            tcTabs.SelectedIndex = SelectedTabIndex;
            LoadOptions();
            
            //Dispatcher.Hooks.OperationPosted += new DispatcherHookEventHandler(Hooks_OperationPosted);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Zoom = zoomslider.Value.ToString();            
            Properties.Settings.Default.Save();

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

            chkForceUpdate.IsChecked = Properties.Settings.Default.forceUpdate;
            chkOverwrite.IsChecked = Properties.Settings.Default.overwriteFiles;
            chkSilentMode.IsChecked = Properties.Settings.Default.SilentMode;
            chkAutoSelectMovieTitle.IsChecked = Properties.Settings.Default.AutoSelectMovieTitle;
            chkforceEnterSearchTerm.IsChecked = Properties.Settings.Default.forceEnterSearchTerm;
            
            chkFullBackdrop.IsChecked = Properties.Settings.Default.FullBackdropView;
            chkEnableGlassFrame.IsChecked = Properties.Settings.Default.EnableGlassFrame;

            chkSaveXBMCMeta.IsChecked = Properties.Settings.Default.SaveXBMCMeta;
            chkSaveMMMeta.IsChecked = Properties.Settings.Default.SaveMyMoviesMeta;

            txtImagesByName.Text = Properties.Settings.Default.ImagesByNameLocation;

            /* Load Variables */
            AllowedFileTypes = new List<string>(Properties.Settings.Default.allowedFileTypes.Split(';'));
            AllowedSubtitleTypes = new List<string>(Properties.Settings.Default.allowedSubtitles.Split(';'));

            zoomslider.Value = Double.Parse(Properties.Settings.Default.Zoom);
        }
        #endregion


        #region Thread Completeion Handlers

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
                    lbShowTitle.Visibility = Visibility.Visible;
                    btnSaveTVShow.Visibility = Visibility.Visible;
                    lbShowDesc.Visibility = Visibility.Visible;
                    lstTVActors.Visibility = Visibility.Visible;
                    SetFocusOnTVShow();
                    gridSeriesView.ContextMenu.Visibility = Visibility.Visible;
                }
                if (th.Name == "Loading Movies")
                {
                    lbMovieTitle.Visibility = Visibility.Visible;
                    btnSaveMovie.Visibility = Visibility.Visible;
                    lbMovieDesc.Visibility = Visibility.Visible;
                    lstMovieActors.Visibility = Visibility.Visible;
                    SetFocusOnMovie();
                    gridMovie.ContextMenu.Visibility = Visibility.Visible;
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
                    ts = new TVShow(ts.Folderpath, ts.Name, ts.IsUnsortedEpisodeCollection);
                    tvshows.Insert(tsindex, ts);

                }
                tvTVShows.SelectedIndex = SelectedTVShowIndex;                            
        }
        private void ResetSeason(TVShow ts, Season s, bool IsRemoved)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new SeasonChangedHandler(ResetSeason), ts, s, IsRemoved);
                return;
            }
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
        private void ResetEpisode(TVShow ts, Season s, Episode e, bool IsRemoved)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new EpisodeChangedHandler(ResetEpisode), ts, s, e, IsRemoved);
                return;
            }
            
            tvEpisodes.SelectedIndex = -1;
            int tsindex = tvshows.IndexOf(ts);
            int sindex = tvshows[tsindex].Seasons.IndexOf(s);
            int eindex = tvshows[tsindex].Seasons[sindex].Episodes.IndexOf(e);
            tvshows[tsindex].Seasons[sindex].Episodes.RemoveAt(eindex);
            if (!IsRemoved)
            {
                e = new Episode(e.Filepath, e.Name, s);
                tvshows[tsindex].Seasons[sindex].Episodes.Insert(eindex, e);
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
                m = new Movie(m.Folderpath, m.Name, m.IsUnsortedFileCollection);
                movies.Insert(mindex, m);
            }
            lbMovies.SelectedIndex = SelectedMovieIndex;
        }
        private void ResetMovieFile(Movie m, MovieFile mf, bool IsRemoved)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new MovieFileChangedHandler(ResetMovieFile), m, mf, IsRemoved);
                return;
            }
            
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

        private void btnReloadSelectedMovie_Click(object sender, RoutedEventArgs e)
        {
            Movie m = SelectedMovie;
            MovieChanged(m, false);
        }

        private void btnReloadAllTV_Click(object sender, RoutedEventArgs e)
        {
            loadTVShows();
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
            }
            else
            {
                _tbPauseButton.Description = "Pause";
                _tbPauseButton.ImageSource = (BitmapImage)Resources["PauseImage"];
                tvThreads[tvThreads.Count - 1].Resume();
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
            if (e.Key == Key.Enter)
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
            if (e.Key == Key.Delete)
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
            if (e.Key == Key.Back)
            {
                if (gridSeasonsView.Visibility == Visibility.Visible)
                    ShowTVShowList();
                else if (gridEpisodesView.Visibility == Visibility.Visible)
                    ShowSeasons();
            }
        }
        private void tabMovies_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (gridMovie.Visibility == Visibility.Visible)
                {
                    if (SelectedMovie != null)
                        ShowMovieFileList();
                }
            }
            if (e.Key == Key.Delete)
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
            if (e.Key == Key.Back)
            {
                if (gridMovieFile.Visibility == Visibility.Visible)
                    ShowMovieList();
            }
        } 
        
        private void tcTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WindowLoaded)
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
                        //Make sure the TVFolder is set before attempting to load in all the TV show data.                    
                        System.Collections.Specialized.StringCollection TVFolders = Properties.Settings.Default.TVFolders;

                        if (TVFolders == null || TVFolders.Count == 0)
                        {
                            if (tvshows.Count > 0)
                            {
                                tvshows.Clear();
                                lbShowTitle.Visibility = Visibility.Collapsed;
                                btnSaveTVShow.Visibility = Visibility.Collapsed;
                                lbShowDesc.Visibility = Visibility.Collapsed;
                                lstTVActors.Visibility = Visibility.Collapsed;
                                gridSeriesView.ContextMenu.Visibility = Visibility.Collapsed;
                                UpdateTVPoster(null, TVShowPosterType.Poster, false);
                                UpdateTVPoster(null, TVShowPosterType.Backdrop, false);
                            }
                            break;
                        }

                        if (tvshows == null || tvshows.Count == 0 || resetTVfolder)
                        {
                            resetTVfolder = false;
                            loadTVShows();
                            gridTVSeries.DataContext = dispatchtvshows;
                        }

                        break;

                    case "tabMovies":

                        //Make sure the MovieFolder is set before attempting to load in all the movies data.
                        System.Collections.Specialized.StringCollection MovieFolders = Properties.Settings.Default.MovieFolders;

                        if (MovieFolders == null || MovieFolders.Count == 0)
                        {
                            if (movies.Count > 0)
                            {
                                movies.Clear();
                                lbMovieTitle.Visibility = Visibility.Collapsed;
                                btnSaveMovie.Visibility = Visibility.Collapsed;
                                lbMovieDesc.Visibility = Visibility.Collapsed;
                                lstMovieActors.Visibility = Visibility.Collapsed;
                                gridMovie.ContextMenu.Visibility = Visibility.Collapsed;
                                UpdateMoviePoster(null, MoviePosterType.Poster, false);
                                UpdateMoviePoster(null, MoviePosterType.Backdrop, false);
                            }
                            break;
                        }

                        if (movies == null || movies.Count == 0 || resetMoviefolder)
                        {
                            resetMoviefolder = false;
                            loadMovies();
                            gridMovies.DataContext = dispatchmovies;
                        }

                        break;

                    default:
                        break;
                }
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
        
        private void loadTVShows()
        {
            tvTVShows.SelectedIndex = -1;
            tvshows.Clear();
            //Load it on a seperate thread (hence the use of the DispatchingCollection) so as not to lag UI
            ShowCancelButtons();
            Thread th = null;
            th = new Thread(delegate()
            {
                foreach (String dir in Properties.Settings.Default.TVFolders)
                {
                    DirectoryInfo TVShows = new DirectoryInfo(dir);
                    maxvalue += TVShows.GetDirectories().Length;
                    try
                    {
                        foreach (DirectoryInfo di in TVShows.GetDirectories())
                        {
                            TVShow t = new TVShow(di.FullName, di.Name, false);
                            tvshows.Add(t);
                            SetTasbkBarProgressValue(++currentvalue);
                        }
                        bool containsUnsortedFiles = false;
                        foreach (FileInfo fi in TVShows.GetFiles())
                        {
                            if (AllowedFileTypes.Contains(fi.Extension))
                                containsUnsortedFiles = true;
                        }
                        if (containsUnsortedFiles)
                        {
                            TVShow t = new TVShow(TVShows.FullName, "Unsorted Episodes", true);
                            UnsortedEpisodes = t;
                            tvshows.Add(t);
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
                MetadataCompleted(th, tvshows.Count.ToString(), true);
            });

            th.Name = "Loading TV Shows";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();            
            tvThreads.Add(th);
        }

        private void loadMovies()
        {
            lbMovies.SelectedIndex = -1;
            movies.Clear();
            ShowCancelButtons();
            Thread th = null;
            th = new Thread(delegate()
            {
                foreach (String dir in Properties.Settings.Default.MovieFolders)
                {
                    DirectoryInfo Movies = new DirectoryInfo(dir);
                    maxvalue += Movies.GetDirectories().Length;
                    try
                    {
                        foreach (DirectoryInfo di in Movies.GetDirectories())
                        {
                            Movie m = new Movie(di.FullName, di.Name, false);
                            movies.Add(m);
                            SetTasbkBarProgressValue(++currentvalue);
                        }
                        bool containsUnsortedFiles = false;
                        foreach (FileInfo fi in Movies.GetFiles())
                        {
                            if (AllowedFileTypes.Contains(fi.Extension))
                                containsUnsortedFiles = true;
                        }
                        if (containsUnsortedFiles)
                        {
                            Movie m = new Movie(Movies.FullName, "Unsorted Files", true);
                            UnsortedFiles = m;
                            movies.Add(m);
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
                MetadataCompleted(th, movies.Count.ToString(), true);
            });

            th.Name = "Loading Movies";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);
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
            if (tvTVShows.SelectedIndex != -1)
            {               
                SelectedTVShow = (TVShow)tvTVShows.SelectedItem;
                SelectedTVShowIndex = tvTVShows.SelectedIndex;
                SelectedTVShow.Load();
                UpdateTVPoster(SelectedTVShow, TVShowPosterType.Poster, false);
                UpdateTVPoster(SelectedTVShow, TVShowPosterType.Backdrop, false);                
                UpdateTVPoster(SelectedTVShow, TVShowPosterType.Banner, false);                
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
        private void ShowSeasons()
        {
            TVShow ts = SelectedTVShow;
            if (ts.IsUnsortedEpisodeCollection)
            {
                if (gridEpisodesView.Visibility == Visibility.Visible)
                    ShowTVShowList();
                else
                {
                    SelectedSeason = ts.UnsortedEpisodes[0].Season;
                    ShowEpisodeList();
                }
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
            if (tvSeasons.SelectedIndex != -1)
            {
                SelectedSeason = (Season)tvSeasons.SelectedItem;
                SelectedSeasonIndex = tvSeasons.SelectedIndex;
                UpdateSeasonPoster(SelectedSeason, TVShowPosterType.Season_Poster, false);
                UpdateSeasonPoster(SelectedSeason, TVShowPosterType.Season_Backdrop, false);
            }
        }        
        private void SeasonItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                ShowEpisodeList();
        }
        private void ShowEpisodeList()
        {
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
            if (tvEpisodes.SelectedIndex != -1)
            {
                SelectedEpisode = tvEpisodes.SelectedItem as Episode;
                SelectedEpisodeIndex = tvEpisodes.SelectedIndex;
                SelectedEpisode.Load();
                UpdateEpisodePoster(SelectedEpisode, SelectedEpisode.PosterFilename, false);
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
            if (lbMovies.SelectedIndex != -1)
            {
                SelectedMovie = (Movie)lbMovies.SelectedItem;
                SelectedMovieIndex = lbMovies.SelectedIndex;
                SelectedMovie.Load();                
                UpdateMoviePoster(SelectedMovie, MoviePosterType.Poster, false);
                UpdateMoviePoster(SelectedMovie, MoviePosterType.Backdrop, false);
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
            gridMovie.Visibility = Visibility.Hidden;
            gridMovieFile.Visibility = Visibility.Visible;
            SetFocusOnMovieFile();
        }
        #endregion

        #region Movie File ListBox Routines

        private void lbMovieFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbMovieFiles.SelectedIndex != -1)
            {
                SelectedMovieFile = (MovieFile)lbMovieFiles.SelectedItem;
                SelectedMovieFileIndex = lbMovieFiles.SelectedIndex;
                UpdateMovieFilePoster(SelectedMovieFile, MoviePosterType.File_Poster, false);
                UpdateMovieFilePoster(SelectedMovieFile, MoviePosterType.File_Backdrop, false);
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
                                SortedByYear.Add(i);
                            }
                        }
                        else
                            SortedByYear.Add(i);
                        i++;
                    }
                    if (!found)
                    {
                        List<int> SortedByName = new List<int>();
                        foreach (int index in SortedByYear)
                        {
                            if ((results[index] as MediaScout.MovieXML).Title == SearchTerm)
                            {
                                foundindex = index;
                                found = true;
                                break;
                            }
                            if( (results[index] as MediaScout.MovieXML).Year != null)
                                SortedByName.Add(index);
                        }
                        if (!found)
                        {
                            foreach (int index in SortedByName)
                            {
                                if ((results[index] as MediaScout.MovieXML).Title.Contains(SearchTerm))
                                {
                                    foundindex = index;
                                    found = true;
                                    break;
                                }
                            }
                            if (SortedByName.Count != 0 && !found)
                            {
                                foundindex = SortedByName[0];
                                found = true;
                            }
                        }
                        if (SortedByYear.Count != 0 && !found)
                        {
                            foundindex = SortedByYear[0];
                            found = true;
                        }
                    }
                }
                else
                {
                    List<int> SortedByName = new List<int>();
                    foreach (MediaScout.MovieXML m in results)
                    {
                        if (m.Title == SearchTerm)
                        {
                            foundindex = i;
                            found = true;
                            break;
                        }
                        SortedByName.Add(i);
                        i++;
                    }
                    if (!found)
                    {
                        foreach (int index in SortedByName)
                        {
                            if ((results[index] as MediaScout.MovieXML).Title.Contains(SearchTerm))
                            {
                                foundindex = index;
                                found = true;
                                break;
                            }
                        }
                        if (SortedByName.Count != 0 && !found)
                        {
                            foundindex = SortedByName[0];
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
                foreach (MediaScout.TVShowXML ts in results)
                {
                    if (tvshow.ID == ts.ID)
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
                    foreach (MediaScout.TVShowXML ts in results)
                    {
                        if (ts.Year != null)
                        {
                            if (ts.Year == tvshow.Year)
                            {
                                if (ts.Title == SearchTerm)
                                {
                                    foundindex = i;
                                    found = true;
                                    break;
                                }
                                SortedByYear.Add(i);
                            }
                        }
                        else
                            SortedByYear.Add(i);
                        i++;
                    }
                    if (!found)
                    {
                        List<int> SortedByName = new List<int>();
                        foreach (int index in SortedByYear)
                        {
                            if ((results[index] as MediaScout.TVShowXML).Title == SearchTerm)
                            {
                                foundindex = index;
                                found = true;
                                break;
                            }
                            if ((results[index] as MediaScout.TVShowXML).Year != null)
                                SortedByName.Add(index);
                        }
                        if (!found)
                        {
                            foreach (int index in SortedByName)
                            {
                                if ((results[index] as MediaScout.TVShowXML).Title.Contains(SearchTerm))
                                {
                                    foundindex = index;
                                    found = true;
                                    break;
                                }
                            }
                            if (SortedByName.Count != 0 && !found)
                            {
                                foundindex = SortedByName[0];
                                found = true;
                            }
                        }
                        if (SortedByYear.Count != 0 && !found)
                        {
                            foundindex = SortedByYear[0];
                            found = true;
                        }
                    }
                }
                else
                {
                    List<int> SortedByName = new List<int>();
                    foreach (MediaScout.TVShowXML ts in results)
                    {
                        if (ts.Title == SearchTerm)
                        {
                            foundindex = i;
                            found = true;
                            break;
                        }
                        SortedByName.Add(i);
                        i++;
                    }
                    if (!found)
                    {
                        foreach (int index in SortedByName)
                        {
                            if ((results[index] as MediaScout.TVShowXML).Title.Contains(SearchTerm))
                            {
                                foundindex = index;
                                found = true;
                                break;
                            }
                        }
                        if (SortedByName.Count != 0 && !found)
                        {
                            foundindex = SortedByName[0];
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
        
        private SearchResultsDecision PromptForSearchTerm(String SearchTerm, bool IsMovie, bool forced, bool CanUserSkip)
        {
            SearchResultsDecision SearchDecision = new SearchResultsDecision();
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        while ((SearchDecision.results == null || SearchDecision.results.Length == 0) && (SearchDecision.Decision == DecisionType.Continue))
                        {
                            NoResultsDialog nrd = new NoResultsDialog(SearchTerm, forced, CanUserSkip, IsMovie);
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
        private SearchResultsDecision PromptForSelection(SearchResultsDecision SearchDecision, object item, bool IsMovie, bool Skip)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        SelectResultDialog rsd = new SelectResultDialog(SearchDecision.results, IsMovie, Skip);
                        rsd.Owner = this;
                        rsd.ShowDialog();
                        SearchDecision.Decision = rsd.Decision;
                        if (SearchDecision.Decision == DecisionType.SearchAgain)
                        {
                            SearchDecision = PromptForSearchTerm(SearchDecision.SearchTerm, IsMovie, true, Skip);
                            if (SearchDecision.results != null)
                            {
                                if (SearchDecision.results.Length == 0)
                                    SearchDecision = PromptForSearchTerm(SearchDecision.SearchTerm, IsMovie, false, Skip);

                                if (SearchDecision.results.Length == 1)
                                    SearchDecision = GetSelectedIDAndName(SearchDecision, IsMovie, 0);
                                else if (Properties.Settings.Default.SilentMode)
                                {
                                    if (IsMovie)
                                        SetBestMatchOnTopInMovieResults(SearchDecision.results, item as Movie, SearchDecision.SearchTerm);
                                    else
                                        SetBestMatchOnTopInTVShowResults(SearchDecision.results, item as TVShow, SearchDecision.SearchTerm);
                                    SearchDecision = GetSelectedIDAndName(SearchDecision, IsMovie, 0);
                                }
                                else if (SearchDecision.results.Length > 1)
                                {
                                    if (IsMovie)
                                        SetBestMatchOnTopInMovieResults(SearchDecision.results, item as Movie, SearchDecision.SearchTerm);
                                    else
                                        SetBestMatchOnTopInTVShowResults(SearchDecision.results, item as TVShow, SearchDecision.SearchTerm);
                                    SearchDecision = PromptForSelection(SearchDecision, item, IsMovie, Skip);
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
        private SearchResultsDecision SearchForID(object item, bool IsMovie, String SearchTerm, bool CanUserSkip, bool forceEnterSearchTerm)
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
                    SearchDecision = PromptForSearchTerm(SearchDecision.SearchTerm, IsMovie, true, CanUserSkip);
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
                        SearchDecision = PromptForSearchTerm(SearchDecision.SearchTerm, IsMovie, false, CanUserSkip);

                    if (SearchDecision.Decision == DecisionType.Continue)
                    {
                        if (SearchDecision.results.Length == 1)
                            SearchDecision = GetSelectedIDAndName(SearchDecision, IsMovie, 0);
                        else if (Properties.Settings.Default.SilentMode)
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
                            SearchDecision = PromptForSelection(SearchDecision, item, IsMovie, CanUserSkip);
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


        #region  Fetch List Items( TV Series, Movie And Episode) Routines

        private void btnFetchSelectedTV_Click(object sender, RoutedEventArgs e)
        {
            ShowCancelButtons();

            TVShow ts = SelectedTVShow;
            if (ts.IsUnsortedEpisodeCollection)
            {
                MessageBox.Show("Not Implemented");
            }
            else
            {
                Thread th = null;
                th = new Thread(delegate()
                {
                    SearchResultsDecision SearchDecision = SearchForID(ts, false, null, false, Properties.Settings.Default.forceEnterSearchTerm);
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

                        TVShowChanged(ts, false);
                        MetadataCompleted(th, "Done.", true);
                    }
                    else
                        MetadataCompleted(th, "Canceled.", true);
                });
                th.Name = "Fetching " + ts.Name;
                th.SetApartmentState(ApartmentState.STA);
                th.Start();

                tvThreads.Add(th);
            }
        }
        private void btnFetchSelectedMovie_Click(object sender, RoutedEventArgs e)
        {
            ShowCancelButtons();

            Movie m = SelectedMovie;

            if (m.IsUnsortedFileCollection)
            {
                MessageBox.Show("Not Implemented");
            }
            else
            {
                Thread th = null;
                th = new Thread(delegate()
                {
                    SearchResultsDecision SearchDecision = SearchForID(m, true, null, false, Properties.Settings.Default.forceEnterSearchTerm);
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

                        MovieChanged(m, false);
                        MetadataCompleted(th, "Done.", true);
                    }
                    else
                        MetadataCompleted(th, "Canceled.", true);
                });
                th.Name = "Fetching " + m.Name;
                th.SetApartmentState(ApartmentState.STA);
                th.Start();

                tvThreads.Add(th);
            }
        }
        private void btnFetchSelectedEpisode_Click(object sender, RoutedEventArgs e)
        {
            ShowCancelButtons();
            
            Episode Episode = SelectedEpisode;
            Season s = Episode.Season;
            TVShow ts = Episode.Season.TVShow;

            String eid = Episode.GetNum();
            String sid = (ts.IsUnsortedEpisodeCollection) ? Episode.GetSeasonNum() : s.GetNum();

            if (eid == null || sid == null)
            // Should prompt for Dialog box asking episode/season number instead of Messagebox
                MessageBox.Show("Unable to Get Episode/Season Number from File"); 
            else
            {
                Thread th = null;
                th = new Thread(delegate()
                {
                    String SearchTerm = ts.IsUnsortedEpisodeCollection ? ts.GetSearchTerm(Episode.StrippedFileName) : null;
                    SearchResultsDecision SearchDecision = SearchForID(ts, false, SearchTerm, false, Properties.Settings.Default.forceEnterSearchTerm);
                    if (SearchDecision.Decision == DecisionType.Continue)
                    {
                        String id = SearchDecision.SelectedID;
                        if (id != null)
                        {
                            MediaScout.EpisodeXML selected = tvdb.GetEpisode(id, sid, eid);

                            #region Save Metadata
                            if (Properties.Settings.Default.SaveXBMCMeta)
                            {
                                Message("Saving Metadata as " + selected.GetNFOFile(s.Folderpath, Episode.StrippedFileName), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                                try
                                {
                                    selected.SaveNFO(s.Folderpath, Episode.StrippedFileName);
                                }
                                catch (Exception ex)
                                {
                                    Message(ex.Message, MediaScout.MediaScoutMessage.MessageType.Error, 0);
                                }
                                Message("Done", MediaScout.MediaScoutMessage.MessageType.TaskResult, 0);
                            }
                            if (Properties.Settings.Default.SaveMyMoviesMeta)
                            {
                                Message("Saving Metadata as " + selected.GetXMLFile(s.Folderpath, Episode.StrippedFileName), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                                try
                                {
                                    selected.SaveXML(s.Folderpath, Episode.StrippedFileName);
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
                                        Message("Saving Episode Poster as " + selected.GetXBMCThumbFilename(Episode.StrippedFileName), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                                        String filename = selected.GetXBMCThumbFile(Episode.Season.Folderpath, Episode.StrippedFileName);
                                        p.SavePoster(filename);
                                        Message("Done", MediaScout.MediaScoutMessage.MessageType.TaskResult, 0);
                                    }
                                    if (Properties.Settings.Default.SaveMyMoviesMeta)
                                    {
                                        Message("Saving Episode Poster as " + selected.GetMyMoviesThumbFilename(), MediaScout.MediaScoutMessage.MessageType.Task, 0);
                                        String filename = selected.GetMyMoviesThumbFile(Episode.Season.MetadataFolderPath);
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

                            EpisodeChanged(ts, s, Episode, false);
                            MetadataCompleted(th, "Done.", true);
                        }
                    }
                    else
                        MetadataCompleted(th, "Canceled.", true);
                });

                th.Name = "Fetcing " + Episode.Name;
                th.SetApartmentState(ApartmentState.STA);
                th.Start();
                tvThreads.Add(th);
            }
            
        }

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
                            {
                                MediaScout.DirFunc df = new MediaScout.DirFunc();
                                df.CreateHiddenFolder(ActorsDir);
                            }

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


        #region Strip (TV, Sesaons, And Episode) Routines

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
            else
                Debug.WriteLine("Leaving Metadata alone.");
        }
        private void StripSelectedTV(TVShow ts)
        {
            if (!ts.IsUnsortedEpisodeCollection)
            {
                Message("Delete all of the Metadata for series " + ts.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                String file;
                file = ts.Folderpath + @"\folder.jpg";
                if (File.Exists(file))
                {
                    Message("Deleting folder.jpg", MediaScout.MediaScoutMessage.MessageType.Task, 1);
                    File.Delete(file);
                }
                file = ts.Folderpath + @"\banner.jpg";
                if (File.Exists(file))
                {
                    Message("Deleting banner.jpg", MediaScout.MediaScoutMessage.MessageType.Task, 1);
                    File.Delete(file);
                }
                if (Properties.Settings.Default.SaveMyMoviesMeta)
                {
                    file = ts.Folderpath + @"\backdrop.jpg";
                    if (File.Exists(file))
                    {
                        Message("Deleting backdrop.jpg", MediaScout.MediaScoutMessage.MessageType.Task, 1);
                        File.Delete(file);
                    }
                    
                    file = ts.Folderpath + @"\series.xml";
                    if (File.Exists(file))
                    {
                        Message("Deleting series.xml", MediaScout.MediaScoutMessage.MessageType.Task, 1);
                        File.Delete(file);
                    }
                }
                if (Properties.Settings.Default.SaveXBMCMeta)
                {
                    file = ts.Folderpath + @"\fanart.jpg";
                    if (File.Exists(file))
                    {
                        Message("Deleting fanart.jpg", MediaScout.MediaScoutMessage.MessageType.Task, 1);
                        File.Delete(file);
                    }
                    file = ts.Folderpath + @"\tvshow.nfo";
                    if (File.Exists(file))
                    {
                        Message("Deleting tvshow.nfo", MediaScout.MediaScoutMessage.MessageType.Task, 1);
                        File.Delete(file);
                    }
                    String dir = ts.Folderpath + @"\.actors";
                    if (Directory.Exists(dir))
                    {
                        Message("Deleting .actors", MediaScout.MediaScoutMessage.MessageType.Task, 1);
                        Directory.Delete(dir, true);
                    }
                }
                
                if (ts.Actors != null)
                    for (int i = 0; i < ts.Actors.Count; i++)
                        StripSelectedActorThumb(ts.Actors[i]);

                if (ts.Seasons != null)
                    for (int i = 0; i < ts.Seasons.Count; i++)
                        StripSelectedSeason(ts, ts.Seasons[i]);
            }

            TVShowChanged(ts, false);
        }
        private void btnStripAllTV_Click(object sender, RoutedEventArgs e)
        {
            String msg = "Are you sure you want to delete all Metadata and images for all TV Shows?";
            if (MessageBox.Show(msg, "TV Shows", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
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
                th.Name = "Stripping All TV Shows";
                th.SetApartmentState(ApartmentState.STA);
                th.Start();

                tvThreads.Add(th);
            }
        }
        #endregion
        
        #region Strip Season Routines
        private void btnStripSelectedSeason_Click(object sender, RoutedEventArgs e)
        {
            TVShow ts = SelectedTVShow;
            Season s = SelectedSeason;

            String msg = "Are you sure you want to delete all Metadata and images for this season?";
            if (MessageBox.Show(msg, s.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                StripSelectedSeason(ts, s);
                Message("Done.", MediaScout.MediaScoutMessage.MessageType.Task, 1);
            }          
        }
        private void StripSelectedSeason(TVShow ts, Season s)
        {
            Message("Deleting Metadata for " + s.Name, MediaScout.MediaScoutMessage.MessageType.Task, 1);
            String file;
            file = s.Folderpath + @"\folder.jpg";
            if (File.Exists(file))
            {
                Message("Deleting folder.jpg", MediaScout.MediaScoutMessage.MessageType.Task, 2);
                File.Delete(file);
            }
            if (Properties.Settings.Default.SaveMyMoviesMeta)
            {
                file = s.Folderpath + @"\backdrop.jpg";
                if (File.Exists(file))
                {
                    Message("Deleting backdrop.jpg", MediaScout.MediaScoutMessage.MessageType.Task, 2);
                    File.Delete(file);
                }
                String dir = s.Folderpath + @"\metadata";
                if (Directory.Exists(dir))
                {
                    Message("Deleting Metadata folder", MediaScout.MediaScoutMessage.MessageType.Task, 2);
                    Directory.Delete(dir, true);
                }
            }
            if (Properties.Settings.Default.SaveXBMCMeta)
            {
                file = s.Folderpath + @"\fanart.jpg";
                if (File.Exists(file))
                {
                    Message("Deleting fanart.jpg", MediaScout.MediaScoutMessage.MessageType.Task, 2);
                    File.Delete(file);
                }

                String dir = s.Folderpath + @"\.actors";
                if (Directory.Exists(dir))
                {
                    Message("Deleting .actors", MediaScout.MediaScoutMessage.MessageType.Task, 2);
                    Directory.Delete(dir, true);
                }
            }

            if(s.Episodes != null)
                for (int i = 0; i < s.Episodes.Count; i++)
                    StripSelectedEpisode(ts, s, s.Episodes[i]);

            SeasonChanged(ts, s, false);
        }
        private void btnStripAllSeasons_Click(object sender, RoutedEventArgs e)
        {
            TVShow ts = SelectedTVShow;
            String msg = "Are you sure you want to delete all Metadata and images fo all seasons?";
            if (MessageBox.Show(msg, ts.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                tcTabs.SelectedIndex = LogTabIndex;

                ShowCancelButtons();

                
                Thread th = null;
                th = new Thread(delegate()
                {
                    maxvalue += ts.Seasons.Count;
                    for (int i = 0; i < ts.Seasons.Count; i++)
                    {
                        StripSelectedSeason(ts, ts.Seasons[i]);
                        SetTasbkBarProgressValue(++currentvalue);
                    }

                    MetadataCompleted(th, "Done.", true);
                });

                th.Name = "Stripping All Seasons";
                th.SetApartmentState(ApartmentState.STA);
                th.Start();

                tvThreads.Add(th);
            }
        }
        #endregion
        
        #region Strip Episode Routines
        private void btnStripSelectedEpisode_Click(object sender, RoutedEventArgs e)
        {
            Episode Episode = SelectedEpisode;
            Season s = Episode.Season;
            TVShow ts = s.TVShow;

            String msg = "Are you sure you want to delete all Metadata and images for this episode?";
            if (MessageBox.Show(msg, s.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                StripSelectedEpisode(ts, s, Episode);
                Message("Done.", MediaScout.MediaScoutMessage.MessageType.Task, 2);
            }
            else
                Debug.WriteLine("Leaving Metadata alone.");
        }
        private void StripSelectedEpisode(TVShow ts, Season s, Episode e)
        {
            FileInfo fi = new FileInfo(e.Filepath);
            String file;
            if (Properties.Settings.Default.SaveMyMoviesMeta)
            {
                file = fi.Directory + @"\metadata\" + fi.Name.Replace(fi.Extension, ".xml");
                if (File.Exists(file))
                {
                    Message("Deleting " + fi.Name.Replace(fi.Extension, ".xml"), MediaScout.MediaScoutMessage.MessageType.Task, 3);
                    File.Delete(file);
                }
                file = fi.Directory + @"\metadata\" + e.PosterFilename;
                if (File.Exists(file))
                {
                    Message("Deleting " + e.PosterFilename, MediaScout.MediaScoutMessage.MessageType.Task, 3);
                    File.Delete(file);
                }
            }
            if (Properties.Settings.Default.SaveXBMCMeta)
            {
                file = fi.Directory + "\\" + fi.Name.Replace(fi.Extension, ".nfo");
                if (File.Exists(file))
                {
                    Message("Deleting " + fi.Name.Replace(fi.Extension, ".nfo"), MediaScout.MediaScoutMessage.MessageType.Task, 3);
                    File.Delete(file);
                }
                file = fi.Directory + "\\" + fi.Name.Replace(fi.Extension, ".tbn");
                if (File.Exists(file))
                {
                    Message("Deleting " + fi.Name.Replace(fi.Extension, ".tbn"), MediaScout.MediaScoutMessage.MessageType.Task, 3);
                    File.Delete(file);
                }
            }
            EpisodeChanged(ts, s, e, false);
        }
        private void btnStripAllEpisodes_Click(object sender, RoutedEventArgs e)
        {
            Season s = SelectedSeason;
            TVShow ts = s.TVShow;

            String msg = "Are you sure you want to delete all Metadata and images for all episode?";
            if (MessageBox.Show(msg, s.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                tcTabs.SelectedIndex = LogTabIndex;

                ShowCancelButtons();

                Thread th = null;
                th = new Thread(delegate()
                {
                    maxvalue += s.Episodes.Count;
                    for (int i = 0; i < s.Episodes.Count; i++)
                    {
                        StripSelectedEpisode(ts, s, s.Episodes[i]);
                        SetTasbkBarProgressValue(++currentvalue);
                    }

                    MetadataCompleted(th, "Done.", true);
                });

                th.Name = "Stripping All Episodes";
                th.SetApartmentState(ApartmentState.STA);
                th.Start();

                tvThreads.Add(th);
            }
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
            if (!m.IsUnsortedFileCollection)
            {
                Message("Delete all of the Metadata for movie " + m.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
                String file;
                file = m.Folderpath + @"\folder.jpg";
                if (File.Exists(file))
                {
                    Message("Deleting folder.jpg", MediaScout.MediaScoutMessage.MessageType.Task, 1);
                    File.Delete(file);
                }
                if (Properties.Settings.Default.SaveMyMoviesMeta)
                {
                    file = m.Folderpath + @"\backdrop.jpg";
                    if (File.Exists(file))
                    {
                        Message("Deleting backdrop.jpg", MediaScout.MediaScoutMessage.MessageType.Task, 1);
                        File.Delete(file);
                    }
                    file = m.Folderpath + @"\mymovies.xml";
                    if (File.Exists(file))
                    {
                        Message("Deleting mymovies.xml", MediaScout.MediaScoutMessage.MessageType.Task, 1);
                        File.Delete(file);
                    }
                }
                if (Properties.Settings.Default.SaveXBMCMeta)
                {
                    file = m.Folderpath + @"\fanart.jpg";
                    if (File.Exists(file))
                    {
                        Message("Deleting fanart.jpg", MediaScout.MediaScoutMessage.MessageType.Task, 1);
                        File.Delete(file);
                    }
                    file = m.Folderpath + @"\movie.nfo";
                    if (File.Exists(file))
                    {
                        Message("Deleting movie.nfo", MediaScout.MediaScoutMessage.MessageType.Task, 1);
                        File.Delete(file);
                    }
                    String dir = m.Folderpath + @"\.actors";
                    if (Directory.Exists(dir))
                    {
                        Message("Deleting .actors", MediaScout.MediaScoutMessage.MessageType.Task, 1);
                        Directory.Delete(dir, true);
                    }
                }

                if (m.Actors != null)
                    for (int i = 0; i < m.Actors.Count; i++)
                        StripSelectedActorThumb(m.Actors[i]);

                if (m.Files != null)
                    for (int i = 0; i < m.Files.Count; i++)
                        StripSelectedMovieFile(m, m.Files[i]);
            }
            MovieChanged(m, false);
        }
        private void btnStripAllMovie_Click(object sender, RoutedEventArgs e)
        {
            String msg = "Are you sure you want to delete all Metadata and images for all Movies?";
            if (MessageBox.Show(msg, "Movies", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
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

                th.Name = "Stripping All Movies";
                th.SetApartmentState(ApartmentState.STA);
                th.Start();

                tvThreads.Add(th);
            }
        }
        #endregion
        
        #region Strip Movie File Routines
        private void btnStripSelectedMovieFile_Click(object sender, RoutedEventArgs e)
        {
            MovieFile mf = SelectedMovieFile;
            Movie m = mf.Movie;

            String msg = "Are you sure you want to delete all Metadata and images for this Movie File?";
            if (MessageBox.Show(msg, m.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                StripSelectedMovieFile(m, mf);
                Message("Done.", MediaScout.MediaScoutMessage.MessageType.Task, 0);
            }
            else
                Debug.WriteLine("Leaving Metadata alone.");
        }
        private void StripSelectedMovieFile(Movie m, MovieFile mf)
        {
            FileInfo fi = new FileInfo(mf.Filepath);
            String file;
            if (Properties.Settings.Default.SaveXBMCMeta)
            {
                file = fi.Directory + fi.Name.Replace(fi.Extension, ".nfo");
                if (File.Exists(file))
                {
                    Message("Deleting " + fi.Name.Replace(fi.Extension, ".nfo"), MediaScout.MediaScoutMessage.MessageType.Task, 2);
                    File.Delete(file);
                }
                file = fi.Directory + "\\" + fi.Name.Replace(fi.Extension, ".nfo");
                if (File.Exists(file))
                {
                    Message("Deleting " + fi.Name.Replace(fi.Extension, ".nfo"), MediaScout.MediaScoutMessage.MessageType.Task, 2);
                    File.Delete(file);
                }
                file = fi.Directory + "\\" + fi.Name.Replace(fi.Extension, ".tbn");
                if (File.Exists(file))
                {
                    Message("Deleting " + fi.Name.Replace(fi.Extension, ".tbn"), MediaScout.MediaScoutMessage.MessageType.Task, 2);
                    File.Delete(file);
                }
            }
            MovieFileChanged(m, mf, false);
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
            String file;
            if (Properties.Settings.Default.SaveMyMoviesMeta)
            {
                file = p.MyMoviesFolderPath + "\\" + p.Name + "\\folder.jpg";
                if (File.Exists(file))
                {
                    Message("Deleting ImagesByName\\" + p.Name + "\\folder.jpg", MediaScout.MediaScoutMessage.MessageType.Task, 2);
                    File.Delete(file);
                }
                String dir = p.MyMoviesFolderPath + "\\" + p.Name;
                if (File.Exists(file))
                {
                    Message("Deleting ImagesByName\\" + p.Name, MediaScout.MediaScoutMessage.MessageType.Task, 2);
                    Directory.Delete(dir, true);
                }
                
            }
            if (Properties.Settings.Default.SaveXBMCMeta)
            {
                file = p.XBMCFolderPath + "\\" + p.Name.Replace(" ", "_") + ".jpg";
                if (File.Exists(file))
                {
                    Message("Deleting " + p.Name.Replace(" ", "_") + ".jpg", MediaScout.MediaScoutMessage.MessageType.Task, 1);
                    File.Delete(file);
                }
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
                SaveMyMoviesMeta = Properties.Settings.Default.SaveMyMoviesMeta
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
                SaveMyMoviesMeta = Properties.Settings.Default.SaveMyMoviesMeta
            };

            MovieScout = new MediaScout.MovieScout(Options, Message, Properties.Settings.Default.ImagesByNameLocation);
            return MovieScout;
        }
        
        #endregion

        #region Process TV Routines

        private void btnProcessSelectedTV_Click(object sender, RoutedEventArgs e)
        {            
            TVShow ts = SelectedTVShow;
            if (ts.IsUnsortedEpisodeCollection)
            {
                SelectedSeason = ts.Seasons[0];
                btnProcessAllEpisodes_Click(sender, e);
            }
            else
            {
                tcTabs.SelectedIndex = LogTabIndex;

                ShowCancelButtons();

                TVScout = SetTVScout();

                Thread th = null;
                th = new Thread(delegate()
                {
                    if (ProcessingSelectedTV(ts, null, false) == DecisionType.Cancel)
                        MetadataCompleted(th, "Canceled.", true);
                    else
                        MetadataCompleted(th, "Done.", true);
                });
                
                th.Name = "Processing " + ts.Name;
                th.SetApartmentState(ApartmentState.STA);
                th.Start();
                tvThreads.Add(th);
            }
        }
        private DecisionType ProcessingSelectedTV(TVShow ts, Season s, bool CanUserSkip)
        {            
            Message("Scanning folder " + ((s != null) ? s.Name : ts.Name), MediaScout.MediaScoutMessage.MessageType.Task, 0);

            SearchResultsDecision SearchDecision = SearchForID(ts, false, null, CanUserSkip, Properties.Settings.Default.forceEnterSearchTerm);
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
                    String name;
                    if (s == null)
                    {
                        name = TVScout.ProcessDirectory(ts.Folderpath);
                        if (name != null)
                        {
                            if (name.Substring(0, 2) != "d:")
                            {
                                ts.Name = name;
                                ts.Folderpath = new DirectoryInfo(ts.Folderpath).Parent.FullName + "\\" + name;
                            }
                            else
                                ts.IsDeleted = true;

                            TVShowChanged(ts, false);
                        }
                    }
                    else
                    {
                        name = TVScout.ProcessSeasonDirectory(ts.Folderpath, new DirectoryInfo(s.Folderpath), -1);
                        if (name != null)
                        {
                            if (name.Substring(0, 2) != "d:")
                            {
                                s.Name = name;
                                s.Folderpath = ts.Folderpath + "\\" + name;
                                SeasonChanged(ts, s, false);
                            }
                            else
                                SeasonChanged(ts, s, true);
                        }

                    }
                }
            }
            return SearchDecision.Decision;
        }
        private void btnProcessAllTV_Click(object sender, RoutedEventArgs e)
        {
            tcTabs.SelectedIndex = LogTabIndex;

            ShowCancelButtons();

            TVScout = SetTVScout();

            Thread th = null;            
            th = new Thread(delegate()
            {
                maxvalue += tvshows.Count;
                for (int i = 0; i < tvshows.Count; i++)
                {
                    if (tvshows[i].IsUnsortedEpisodeCollection)
                        continue;

                    DecisionType Decision = ProcessingSelectedTV(tvshows[i], null, true);
                    if (Decision == DecisionType.Skip)
                        Message("Skipped", MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    else if (Decision == DecisionType.Cancel)
                        MetadataCompleted(th, "Canceled.", true);

                    SetTasbkBarProgressValue(++currentvalue);
                }

                for (int i = 0; i < tvshows.Count; i++)
                {
                    if (tvshows[i].IsDeleted)
                        TVShowChanged(tvshows[i], true);
                }

                MetadataCompleted(th, "Done.", true);
            });
            
            th.Name = "Processing All TVShows";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);
        }

        #endregion        

        #region Process Season Routines

        private void btnProcessSelectedSeason_Click(object sender, RoutedEventArgs e)
        {
            tcTabs.SelectedIndex = LogTabIndex;

            ShowCancelButtons();

            TVScout = SetTVScout();
            
            Season s = SelectedSeason;
            TVShow ts = s.TVShow;

            Thread th = null;            
            th = new Thread(delegate()
            {
               if(ProcessingSelectedTV(ts, s, false) == DecisionType.Cancel)
                   MetadataCompleted(th, "Canceled.", true);
               else
                   MetadataCompleted(th, "Done.", true);
            });
            
            th.Name = "Processing " + s.Name;
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);
        }
        
        #endregion

        #region Process Episode Routines

        private void btnProcessSelectedEpisode_Click(object sender, RoutedEventArgs e)
        {
            tcTabs.SelectedIndex = LogTabIndex;

            ShowCancelButtons();

            TVScout = SetTVScout();

            Episode Episode = SelectedEpisode;
            Season s = Episode.Season;
            TVShow ts = Episode.Season.TVShow;

            Thread th = null;            
            th = new Thread(delegate()
            {
                if(ProcessEpisode(ts, s, Episode, false) == DecisionType.Cancel)
                    MetadataCompleted(th, "Canceled.", true);
                
                if (ts.IsUnsortedEpisodeCollection)
                {
                    for (int i = 0; i < s.Episodes.Count; i++)
                    {
                        if (s.Episodes[i].IsDeleted)
                            EpisodeChanged(ts, s, s.Episodes[i], true);
                    }

                    if (s.Episodes.Count == 0)
                        TVShowChanged(ts, true);
                }

                MetadataCompleted(th, "Done.", true);
            });
            
            th.Name = "Processing " + Episode.Name;
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);
        }
        private DecisionType ProcessEpisode(TVShow ts, Season s, Episode e, bool CanUserSkip)
        {
            Message("Scanning file " + e.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);

            String SearchTerm = ts.IsUnsortedEpisodeCollection ? ts.GetSearchTerm(e.StrippedFileName) : null;
            SearchResultsDecision SearchDecision = SearchForID(ts, false, SearchTerm, CanUserSkip, Properties.Settings.Default.forceEnterSearchTerm);

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
                        int sid = (ts.IsUnsortedEpisodeCollection) ? -1 : int.Parse(s.GetNum()); 
                        TVScout.series = selected;
                        FileInfo fi = new FileInfo(e.Filepath);
                        String name = TVScout.ProcessEpisode(ts.Folderpath, fi, sid, !ts.IsUnsortedEpisodeCollection, -1);
                        if (name != null)
                        {
                            if (ts.IsUnsortedEpisodeCollection)
                                e.IsDeleted = true;                                
                            else
                            {
                                e.Name = name;
                                e.Filepath = e.Season.Folderpath + "\\" + e.Name;
                                EpisodeChanged(ts, s, e, false);
                            }
                        }
                    }
                }
            }
            return SearchDecision.Decision;
        }
        private void btnProcessAllEpisodes_Click(object sender, RoutedEventArgs e)
        {
            tcTabs.SelectedIndex = LogTabIndex;

            ShowCancelButtons();

            TVScout = SetTVScout();
            
            Season s = SelectedSeason;
            TVShow ts = s.TVShow;

            Thread th = null;            
            th = new Thread(delegate()
            {
                maxvalue += s.Episodes.Count;
                for (int i = 0; i < s.Episodes.Count; i++)
                {
                    DecisionType Decision = ProcessEpisode(ts, s, s.Episodes[i], true);
                    if (Decision == DecisionType.Skip)
                        Message("Skipped", MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    else if (Decision == DecisionType.Cancel)
                        MetadataCompleted(th, "Canceled.", true);

                    SetTasbkBarProgressValue(++currentvalue);
                }

                for (int i = 0; i < s.Episodes.Count; i++)
                {
                    if (s.Episodes[i].IsDeleted)
                        EpisodeChanged(ts, s, s.Episodes[i], true);
                }

                if (ts.IsUnsortedEpisodeCollection)
                    if (s.Episodes.Count == 0)
                        TVShowChanged(ts, true);

                MetadataCompleted(th, "Done.", true);
            });
            
            th.Name = "Processing All Episodes";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);
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
            Movie m = SelectedMovie;

            if (m.IsUnsortedFileCollection)
                btnProcessAllMovieFile_Click(sender, e);
            else
            {
                tcTabs.SelectedIndex = LogTabIndex;

                ShowCancelButtons();

                MovieScout = SetMovieScout();

                Thread th = null;
                th = new Thread(delegate()
                {
                    if (ProcessingSelectedMovie(m, null, null, false) == DecisionType.Cancel)
                        MetadataCompleted(th, "Canceled.", true);
                    else
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

            SearchResultsDecision SearchDecision = SearchForID(m, true, null, CanUserSkip, Properties.Settings.Default.forceEnterSearchTerm);
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
                            MovieChanged(m, false);
                        }
                        else
                            MovieChanged(m, true);
                    }
                }
            }
            return SearchDecision.Decision;
        }
        private void btnProcessAllMovie_Click(object sender, RoutedEventArgs e)
        {
            ProcessAllMoviesDialog pam = new ProcessAllMoviesDialog(movies, this);
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

        private void btnProcessSelectedMovieFile_Click(object sender, RoutedEventArgs e)
        {
            tcTabs.SelectedIndex = LogTabIndex;

            ShowCancelButtons();

            MovieScout = SetMovieScout();

            MovieFile mf = SelectedMovieFile;
            Movie m = mf.Movie;

            Thread th = null;            
            th = new Thread(delegate()
            {
                if (ProcessingSelectedMovieFile(m, mf, false) == DecisionType.Cancel)
                    MetadataCompleted(th, "Canceled.", true);

                if (m.IsUnsortedFileCollection)
                {
                    for (int i = 0; i < m.Files.Count; i++)
                    {
                        if (m.Files[i].IsDeleted)
                            MovieFileChanged(m, m.Files[i], true);
                    }

                    if (m.Files.Count == 0)
                        MovieChanged(m, true);
                }
                MetadataCompleted(th, "Done.", true);
            });
            
            th.Name = "Processing " + mf.Name;
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
            tvThreads.Add(th);
        }
        private DecisionType ProcessingSelectedMovieFile(Movie m, MovieFile mf, bool CanUserSkip)
        {
            Message("Processing File " + m.Name, MediaScout.MediaScoutMessage.MessageType.Task, 0);
            
            String SearchTerm = m.IsUnsortedFileCollection ? m.GetSearchTerm(mf.StrippedFileName) : null;
            SearchResultsDecision SearchDecision = SearchForID(m, true, SearchTerm, CanUserSkip, Properties.Settings.Default.forceEnterSearchTerm);
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
                    FileInfo fi = new FileInfo(mf.Filepath);
                    String name = MovieScout.ProcessFile(m.Folderpath, fi, !m.IsUnsortedFileCollection, -1);
                    if (name != null)
                    {
                        if(m.IsUnsortedFileCollection)                        
                            mf.IsDeleted = true;                                                    
                        else
                        {
                            m.Name = name;
                            m.Folderpath = Properties.Settings.Default.MovieFolders + "\\" + name;
                            MovieChanged(m, false);
                        }
                    }
                }
            }
            return SearchDecision.Decision;
        }
        private void btnProcessAllMovieFile_Click(object sender, RoutedEventArgs e)
        {
            tcTabs.SelectedIndex = LogTabIndex;

            ShowCancelButtons();

            MovieScout = SetMovieScout();

            Movie m = SelectedMovie;

            Thread th = null;
            th = new Thread(delegate()
            {
                maxvalue += m.Files.Count;
                for (int i = 0; i < m.Files.Count; i++)
                {
                    DecisionType Decision = ProcessingSelectedMovieFile(m, m.Files[i], true);
                    if (Decision == DecisionType.Skip)
                        Message("Skipped", MediaScout.MediaScoutMessage.MessageType.Task, 0);
                    else if (Decision == DecisionType.Cancel)
                        MetadataCompleted(th, "Canceled.", true);

                    SetTasbkBarProgressValue(++currentvalue);
                }

                for (int i = 0; i < m.Files.Count; i++)
                {
                    if (m.Files[i].IsDeleted)
                        MovieFileChanged(m, m.Files[i], true);
                }

                if(m.IsUnsortedFileCollection)
                    if (m.Files.Count == 0)
                        MovieChanged(m, true);

                MetadataCompleted(th, "Done.", true);
            });

            th.Name = "Processing All Movie Files";
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            tvThreads.Add(th);
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
            SearchResultsDecision SearchDecision = SearchForID(ts, false, SearchTerm, false, (ShowName != null) ? Properties.Settings.Default.forceEnterSearchTerm : true);
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

            SearchResultsDecision SearchDecision = SearchForID(mo, true, SearchTerm, false, Properties.Settings.Default.forceEnterSearchTerm);
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
            ChangeImageDialog cmb = new ChangeImageDialog(Folderpath, id, IsMovie, (s != null) ? s.GetNum() : null, true);
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
                                   tvdb.GetPosters(id, TVShowPosterType.Poster, null);
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
            ChangeImageDialog cmb = new ChangeImageDialog(Folderpath, id, IsMovie, (s!=null)? s.GetNum() : null, false);
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
                                       tvdb.GetPosters(id, TVShowPosterType.Backdrop, null);
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
                        imgTVPoster.SetLoading = true;
                    else
                    {
                        if (ts == null)
                            imgTVPoster.Source = null;
                        else
                            imgTVPoster.Source = ts.GetImage(type);
                        //imgTVPoster.Visibility = (String.IsNullOrEmpty(ts.Poster) && !IsLoading) ? Visibility.Collapsed : Visibility.Visible;
                    }
                    break;
                case TVShowPosterType.Backdrop:
                    if (IsLoading)
                        imgTVBackdrop.SetLoading = true;
                    else
                    {
                        if (ts == null)
                            imgTVBackdrop.Source = null;
                        else
                            imgTVBackdrop.Source = ts.GetImage(type);
                        //imgTVBackdrop.Visibility = (String.IsNullOrEmpty(ts.Backdrop) && !IsLoading) ? Visibility.Collapsed : Visibility.Visible;
                    }
                    break;
                case TVShowPosterType.Banner:
                    if (IsLoading)
                        imgTVBanner.SetLoading = true;
                    else
                    {
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
                        imgSeasonPoster.SetLoading = true;
                    else
                        imgSeasonPoster.Source = s.GetImage(type);
                    //imgSeasonPoster.Visibility = (String.IsNullOrEmpty(s.Poster) && !IsLoading) ? Visibility.Collapsed : Visibility.Visible;

                    break;
                case TVShowPosterType.Season_Backdrop:
                    if (IsLoading)
                        imgSeasonBackdrop.SetLoading = true;
                    else
                        imgSeasonBackdrop.Source = s.GetImage(type);
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
                    MediaScout.Posters[] posters = tvdb.GetPosters(id, (IsSeason) ? TVShowPosterType.Season_Poster : TVShowPosterType.Poster, (IsSeason) ? s.GetNum() : null);
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
                        id = SearchForID(ts, false, null, false, Properties.Settings.Default.forceEnterSearchTerm).SelectedID;
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
                    MediaScout.Posters[] posters = tvdb.GetPosters(id, (IsSeason) ? TVShowPosterType.Season_Backdrop : TVShowPosterType.Backdrop, (IsSeason) ? s.GetNum() : null);
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
                        id = SearchForID(ts, false, null, false, Properties.Settings.Default.forceEnterSearchTerm).SelectedID;
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
                id = SearchForID(ts, false, null, false, Properties.Settings.Default.forceEnterSearchTerm).SelectedID;

            if (id != null)
            {
                if (noDialog)
                {
                    String file = ts.Folderpath + @"\banner.jpg";
                    if ((!File.Exists(file)) || (Properties.Settings.Default.forceUpdate))
                    {
                        MediaScout.Posters[] posters = tvdb.GetPosters(id, TVShowPosterType.Banner, null);
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
                            MediaScout.Posters[] posters = tvdb.GetPosters(id, TVShowPosterType.Banner, null);
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
                {
                    MediaScout.DirFunc df = new MediaScout.DirFunc();
                    df.CreateHiddenFolder(ep.Season.MetadataFolderPath);
                }

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
                        imgMoviePoster.SetLoading = true;
                    else
                    {
                        if (m == null)
                            imgMoviePoster.Source = null;
                        else
                            imgMoviePoster.Source = m.GetImage(type);
                        //imgMoviePoster.Visibility = (String.IsNullOrEmpty(m.Poster) &&!IsLoading) ? Visibility.Collapsed : Visibility.Visible;
                    }
                    break;
                case MoviePosterType.Backdrop:
                    if (IsLoading)
                        imgMovieBackdrop.SetLoading = true;
                    else
                    {
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
                        imgMovieFilePoster.SetLoading = true;
                    else
                        imgMovieFilePoster.Source = mf.GetImage(type);
                    //imgMovieFilePoster.Visibility = (String.IsNullOrEmpty(mf.Poster) && !IsLoading) ? Visibility.Collapsed : Visibility.Visible;
                    break;
                case MoviePosterType.File_Backdrop:
                    if (IsLoading)
                        imgMovieFileBackdrop.SetLoading = true;
                    else
                        imgMovieFileBackdrop.Source = mf.GetImage(type);
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
                        id = SearchForID(m, true, null, false, Properties.Settings.Default.forceEnterSearchTerm).SelectedID;
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
                            id = SearchForID(m, true, null, false, Properties.Settings.Default.forceEnterSearchTerm).SelectedID;
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
            {
                MediaScout.DirFunc df = new MediaScout.DirFunc();
                df.CreateHiddenFolder(p.XBMCFolderPath);
            }
            if (!Directory.Exists(p.MyMoviesFolderPath + "\\" + p.Name.Replace(" ", "_")))
            {
                MediaScout.DirFunc df = new MediaScout.DirFunc();
                df.CreateHiddenFolder(p.MyMoviesFolderPath + "\\" + p.Name.Replace(" ", "_"));
            }

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
                String ExampleFormat = makeEpisodeTargetName("Series Name", "1", "Episode Name", "3", "Suffix", ".mkv");
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

        #region Options 1 Posters and Move Movie File Options : 5

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
                String ExampleFormat = makeMovieTargetName(txtMovieFileRenameFormat.Text, "Movie Name", "2009", ".mkv");
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

        #region Full Backdrop View
        private void chkFullBackdrop_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                if (chkFullBackdrop.IsChecked == true)
                {
                    Grid.SetColumn(imgTVBackdrop, 0);
                    Grid.SetRow(imgTVBackdrop, 0);
                    Grid.SetColumnSpan(imgTVBackdrop, 7);
                    Grid.SetRowSpan(imgTVBackdrop, 7);
                    imgTVBackdrop.Stretch = Stretch.UniformToFill;
                    imgTVBackdrop.SetValue(Grid.ZIndexProperty, -1);

                    Grid.SetColumn(imgSeasonBackdrop, 0);
                    Grid.SetRow(imgSeasonBackdrop, 0);
                    Grid.SetColumnSpan(imgSeasonBackdrop, 8);
                    Grid.SetRowSpan(imgSeasonBackdrop, 7);
                    imgSeasonBackdrop.Stretch = Stretch.UniformToFill;
                    imgSeasonBackdrop.SetValue(Grid.ZIndexProperty, -1);

                    Grid.SetColumn(imgMovieBackdrop, 0);
                    Grid.SetRow(imgMovieBackdrop, 0);
                    Grid.SetColumnSpan(imgMovieBackdrop, 7);
                    Grid.SetRowSpan(imgMovieBackdrop, 7);
                    imgMovieBackdrop.Stretch = Stretch.UniformToFill;
                    imgMovieBackdrop.SetValue(Grid.ZIndexProperty, -1);

                    Grid.SetColumn(imgMovieFileBackdrop, 0);
                    Grid.SetRow(imgMovieFileBackdrop, 0);
                    Grid.SetColumnSpan(imgMovieFileBackdrop, 8);
                    Grid.SetRowSpan(imgMovieFileBackdrop, 7);
                    imgMovieFileBackdrop.Stretch = Stretch.UniformToFill;
                    imgMovieFileBackdrop.SetValue(Grid.ZIndexProperty, -1);
                }
                else
                {
                    Grid.SetColumn(imgTVBackdrop, 3);
                    Grid.SetRow(imgTVBackdrop, 4);
                    Grid.SetColumnSpan(imgTVBackdrop, 1);
                    Grid.SetRowSpan(imgTVBackdrop, 1);
                    imgTVBackdrop.Stretch = Stretch.Uniform;

                    Grid.SetColumn(imgSeasonBackdrop, 4);
                    Grid.SetRow(imgSeasonBackdrop, 4);
                    Grid.SetColumnSpan(imgSeasonBackdrop, 1);
                    Grid.SetRowSpan(imgSeasonBackdrop, 1);
                    imgSeasonBackdrop.Stretch = Stretch.Uniform;

                    Grid.SetColumn(imgMovieBackdrop, 3);
                    Grid.SetRow(imgMovieBackdrop, 4);
                    Grid.SetColumnSpan(imgMovieBackdrop, 1);
                    Grid.SetRowSpan(imgMovieBackdrop, 1);
                    imgMovieBackdrop.Stretch = Stretch.Uniform;

                    Grid.SetColumn(imgMovieFileBackdrop, 4);
                    Grid.SetRow(imgMovieFileBackdrop, 4);
                    Grid.SetColumnSpan(imgMovieFileBackdrop, 1);
                    Grid.SetRowSpan(imgMovieFileBackdrop, 1);
                    imgMovieFileBackdrop.Stretch = Stretch.Uniform;
                }
            }
        }
        private void chkFullBackdrop_Checked(object sender, RoutedEventArgs e)
        {
            chkFullBackdrop_ValueChanged(sender, e);
        }
        private void chkFullBackdrop_Unchecked(object sender, RoutedEventArgs e)
        {
            chkFullBackdrop_ValueChanged(sender, e);
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
                Properties.Settings.Default.SublightPassword = sod.txtSublightPassword.Password;
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
                
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs args)
        {
            base.OnPreviewMouseWheel(args);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightCtrl))
                zoomslider.Value += (args.Delta > 0) ? 0.1 : -0.1;
        }
        protected override void OnPreviewMouseDown(MouseButtonEventArgs args)
        {
            base.OnPreviewMouseDown(args);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightCtrl))
                if (args.MiddleButton == MouseButtonState.Pressed)
                    zoomslider.ResetZoom();
        }
        
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

        #endregion

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