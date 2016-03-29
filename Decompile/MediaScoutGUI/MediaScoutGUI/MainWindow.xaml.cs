using MediaScout;
using MediaScout.GUI.Controls;
using MediaScout.Providers;
using MediaScoutGUI.Controls;
using MediaScoutGUI.GUITypes;
using MediaScoutGUI.Properties;
using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using System.Windows.Threading;
using System.Xml;

namespace MediaScoutGUI
{
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public partial class MainWindow : Window, IComponentConnector, IStyleConnector
	{
		public delegate void MetadataCompletedHandler(Thread th, string reason, bool Reset);

		public delegate void TVShowChangedHandler(TVShow ts, bool IsRemoved);

		public delegate void SeasonChangedHandler(MediaScoutGUI.GUITypes.Season s, bool IsRemoved);

		public delegate void EpisodeChangedHandler(Episode e, bool IsRemoved);

		public delegate void MovieChangedHandler(Movie m, bool IsRemoved);

		public delegate void MovieFileChangedHandler(MovieFile mf, bool IsRemoved);

		public delegate void ActorThumbChangedHandler(MediaScoutGUI.GUITypes.Person p);

		public delegate void FocusChangedHandler();

		public delegate void TaskbarProgressValueChangeHandler(int value);

		public delegate void TaskbarProgressStatusChangeHandler(TaskbarItemProgressState state);

		public delegate void TVShowImageChangedHandler(TVShow ts, TVShowPosterType type, bool IsLoading);

		public delegate void SeasonImageChangedHandler(MediaScoutGUI.GUITypes.Season s, TVShowPosterType type, bool IsLoading);

		public delegate void EpisodeImageChangedHandler(Episode e, string filename, bool IsLoading);

		public delegate void MovieImageChangedHandler(Movie m, MoviePosterType type, bool IsLoading);

		public delegate void MovieFileImageChangedHandler(MovieFile mf, MoviePosterType type, bool IsLoading);

		public delegate void PosterChangedHandler(object obj, string id, bool IsMovie, MediaScoutGUI.GUITypes.Season s, string file);

		public delegate void BackdropChangedHandler(object obj, string id, bool IsMovie, MediaScoutGUI.GUITypes.Season s, string file, string file1);

		private class SearchResultsDecision
		{
			public string SelectedID;

			public string SelectedName;

			public bool SelectedHasMultipleTitles;

			public string SearchID;

			public string SearchTerm;

			public string SearchYear;

			public object[] results;

			public DecisionType Decision;
		}

		public MediaScoutMessage.Message Message;

		private ObservableCollection<TVShow> tvshows = new ObservableCollection<TVShow>();

		private ObservableCollection<Movie> movies = new ObservableCollection<Movie>();

		private DispatchingCollection<ObservableCollection<TVShow>, TVShow> dispatchtvshows;

		private DispatchingCollection<ObservableCollection<Movie>, Movie> dispatchmovies;

		private List<string> ignoredFiles = new List<string>();

		private List<string> AllowedFileTypes;

		private List<string> AllowedSubtitleTypes;

		private new string Language;

		private MediaScoutApp app;

		private bool WindowLoaded;

		private bool WindowRendered;

		private JumpList jumplist;

		private JumpTask jumpCancel;

		private JumpTask jumpCancelAll;

		private JumpTask jumpOperationsSeparator;

		private string CacheDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MediaScout\\Cache";

		private TVScout TVScout;

		private MovieScout MovieScout;

		private FileSystemWatcher TVFSWatcher;

		private FileSystemWatcher MovieFSWatcher;

		private TheTVDBProvider tvdb;

		private TheMovieDBProvider tmdb;

		private Collection<Thread> tvThreads = new Collection<Thread>();

		private bool resetTVfolder;

		private bool resetMoviefolder;

		private int currentvalue;

		private int maxvalue;

		private TVShow SelectedTVShow;

		private int SelectedTVShowIndex;

		private TVShow UnsortedEpisodes;

		private MediaScoutGUI.GUITypes.Season SelectedSeason;

		private int SelectedSeasonIndex;

		private Episode SelectedEpisode;

		private int SelectedEpisodeIndex;

		private Movie SelectedMovie;

		private int SelectedMovieIndex;

		private Movie UnsortedFiles;

		private MovieFile SelectedMovieFile;

		private int SelectedMovieFileIndex;

		private MediaScoutGUI.GUITypes.Person SelectedPerson;

		private int LogTabIndex = 1;

		private int SelectedTabIndex;

		private bool LoadingTVShows;

		private bool LoadingMovies;

		private bool FirstTask = true;

		public event MainWindow.MetadataCompletedHandler MetadataCompleted;

		public event MainWindow.TVShowChangedHandler TVShowChanged;

		public event MainWindow.SeasonChangedHandler SeasonChanged;

		public event MainWindow.EpisodeChangedHandler EpisodeChanged;

		public event MainWindow.MovieChangedHandler MovieChanged;

		public event MainWindow.MovieFileChangedHandler MovieFileChanged;

		public event MainWindow.ActorThumbChangedHandler ActorThumbChanged;

		public Rect GetBoundsForGlassFrame()
		{
			return VisualTreeHelper.GetContentBounds(this.tcTabs);
		}

		public bool SetGlassFrame(bool ExtendGlass)
		{
			bool result;
			if (ExtendGlass)
			{
				Rect boundsForGlassFrame = this.GetBoundsForGlassFrame();
				result = GlassHelper.ExtendGlassFrame(this, new Thickness(boundsForGlassFrame.Left, boundsForGlassFrame.Top, boundsForGlassFrame.Right, boundsForGlassFrame.Bottom));
			}
			else
			{
				GlassHelper.DisableGlassFrame(this);
				result = true;
			}
			return result;
		}

		public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			int num = 798;
			if (msg == num)
			{
				this.SetGlassFrame(GlassHelper.IsGlassEnabled);
				handled = true;
			}
			return IntPtr.Zero;
		}

		public MainWindow(int SelectedTabIndex)
		{
			try
			{
				this.InitializeComponent();
				this.app = (MediaScoutApp)System.Windows.Application.Current;
				this.jumplist = this.app.jumplist;
				this.SelectedTabIndex = SelectedTabIndex;
				this.Message = new MediaScoutMessage.Message(this.TVScout_Message);
				this.tvdb = new TheTVDBProvider(this.Message);
				this.tmdb = new TheMovieDBProvider(this.Message);
				this.MetadataCompleted += new MainWindow.MetadataCompletedHandler(this.ResetUI);
				this.TVShowChanged += new MainWindow.TVShowChangedHandler(this.ResetTVShow);
				this.SeasonChanged += new MainWindow.SeasonChangedHandler(this.ResetSeason);
				this.EpisodeChanged += new MainWindow.EpisodeChangedHandler(this.ResetEpisode);
				this.MovieChanged += new MainWindow.MovieChangedHandler(this.ResetMovie);
				this.MovieFileChanged += new MainWindow.MovieFileChangedHandler(this.ResetMovieFile);
				this.ActorThumbChanged += new MainWindow.ActorThumbChangedHandler(this.ResetActorThumb);
				string path = this.CacheDir + "\\TVCache";
				string path2 = this.CacheDir + "\\MovieCache";
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				if (!Directory.Exists(path2))
				{
					Directory.CreateDirectory(path2);
				}
				this.SetCancelButtons();
				this.dispatchtvshows = new DispatchingCollection<ObservableCollection<TVShow>, TVShow>(this.tvshows, base.Dispatcher);
				this.dispatchmovies = new DispatchingCollection<ObservableCollection<Movie>, Movie>(this.movies, base.Dispatcher);
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show(ex.Message);
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.GetBoundsForGlassFrame();
			HwndSource hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
			hwndSource.AddHook(new HwndSourceHook(this.WndProc));
			this.WindowLoaded = true;
			this.LoadOptions();
			this.tcTabs.SelectedIndex = this.SelectedTabIndex;
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
		}

		private void Window_StateChanged(object sender, EventArgs e)
		{
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			this.AbortAllThreads();
			this.app.Shutdown();
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			this.WindowRendered = true;
			this.Load_Post_Rendering_Options();
			this.StartLoadingItems();
		}

		private void LoadOptions()
		{
			this.txtTVDropBox.Text = Settings.Default.TVDropBoxLocation;
			this.ChangeMonitorTVFolder();
			if (this.chkTVFSWatcher.IsEnabled)
			{
				this.chkTVFSWatcher.IsChecked = new bool?(Settings.Default.TVFSWatcher);
			}
			this.chkSeriesPosters.IsChecked = new bool?(Settings.Default.getSeriesPosters);
			this.chkSeasonPosters.IsChecked = new bool?(Settings.Default.getSeasonPosters);
			this.chkEpPosters.IsChecked = new bool?(Settings.Default.getEpisodePosters);
			this.chkMoveTVFiles.IsChecked = new bool?(Settings.Default.moveTVFiles);
			this.chkSaveTVActors.IsChecked = new bool?(Settings.Default.SaveTVActors);
			this.txtSeasonFolderName.Text = Settings.Default.SeasonFolderName;
			this.txtSpecialsFolderName.Text = Settings.Default.SpecialsFolderName;
			this.chkdownloadAllTVImages.IsChecked = new bool?(Settings.Default.downloadAllTVImages);
			this.chkdownloadAllTVPosters.IsChecked = new bool?(Settings.Default.downloadAllTVPosters);
			this.chkdownloadAllTVBackdrops.IsChecked = new bool?(Settings.Default.downloadAllTVBackdrops);
			this.chkdownloadAllTVBanners.IsChecked = new bool?(Settings.Default.downloadAllTVBanners);
			this.chkdownloadAllTVSeasonPosters.IsChecked = new bool?(Settings.Default.downloadAllTVSeasonPosters);
			this.chkdownloadAllTVSeasonBackdrops.IsChecked = new bool?(Settings.Default.downloadAllTVSeasonBackdrops);
			this.chkRenameTVFiles.IsChecked = new bool?(Settings.Default.renameTVFiles);
			this.txtTVRenameFormat.Text = Settings.Default.TVfileformat;
			this.txtSeasonNumZeroPadding.Text = Settings.Default.SeasonNumZeroPadding;
			this.txtEpisodeNumZeroPadding.Text = Settings.Default.EpisodeNumZeroPadding;
			this.txtMovieDropBox.Text = Settings.Default.MovieDropBoxLocation;
			this.ChangeMonitorMovieFolder();
			if (this.chkMovieFSWatcher.IsEnabled)
			{
				this.chkMovieFSWatcher.IsChecked = new bool?(Settings.Default.MovieFSWatcher);
			}
			this.chkMoviePosters.IsChecked = new bool?(Settings.Default.getMoviePosters);
			this.chkMovieFilePosters.IsChecked = new bool?(Settings.Default.getMovieFilePosters);
			this.chkMoveMovieFiles.IsChecked = new bool?(Settings.Default.moveMovieFiles);
			this.chkdownloadAllMovieImages.IsChecked = new bool?(Settings.Default.downloadAllMovieImages);
			this.chkdownloadAllMoviePosters.IsChecked = new bool?(Settings.Default.downloadAllMoviePosters);
			this.chkdownloadAllMovieBackdrops.IsChecked = new bool?(Settings.Default.downloadAllMovieBackdrops);
			this.chkSaveMovieActors.IsChecked = new bool?(Settings.Default.SaveMovieActors);
			this.chkRenameMovieFiles.IsChecked = new bool?(Settings.Default.renameMovieFiles);
			this.txtMovieFileRenameFormat.Text = Settings.Default.Moviefileformat;
			this.txtMovieDirRenameFormat.Text = Settings.Default.MovieDirformat;
			this.txtAllowedFiletypes.Text = Settings.Default.allowedFileTypes;
			this.txtAllowedSubtitles.Text = Settings.Default.allowedSubtitles;
			this.txtSearchTermFilters.Text = Settings.Default.SearchTermFilters;
			this.chkForceUpdate.IsChecked = new bool?(Settings.Default.forceUpdate);
			this.chkSilentMode.IsChecked = new bool?(Settings.Default.SilentMode);
			this.chkAutoSelectMatch.IsChecked = new bool?(Settings.Default.AutoSelectMatch);
			this.chkOverwrite.IsChecked = new bool?(Settings.Default.overwriteFiles);
			this.chkAutoSelectMovieTitle.IsChecked = new bool?(Settings.Default.AutoSelectMovieTitle);
			this.chkforceEnterSearchTerm.IsChecked = new bool?(Settings.Default.forceEnterSearchTerm);
			this.chkEnableGlassFrame.IsChecked = new bool?(Settings.Default.EnableGlassFrame);
			this.chkSaveXBMCMeta.IsChecked = new bool?(Settings.Default.SaveXBMCMeta);
			this.chkSaveMMMeta.IsChecked = new bool?(Settings.Default.SaveMyMoviesMeta);
			this.txtImagesByName.Text = Settings.Default.ImagesByNameLocation;
			this.AllowedFileTypes = new List<string>(Settings.Default.allowedFileTypes.Split(new char[]
			{
				';'
			}));
			this.AllowedSubtitleTypes = new List<string>(Settings.Default.allowedSubtitles.Split(new char[]
			{
				';'
			}));
		}

		private void Load_Post_Rendering_Options()
		{
			this.lstLanguages.SelectedIndex = Settings.Default.language;
			this.Language = ((XmlNode)this.lstLanguages.SelectedItem).ParentNode.SelectSingleNode("abbreviation").InnerText;
		}

		private void SetTVShowTabItemsVisibility(Visibility v)
		{
			this.gridSeriesView.Visibility = v;
			this.gridSeasonsView.Visibility = v;
			this.gridEpisodesView.Visibility = v;
		}

		private void SetMovieTabItemsVisibility(Visibility v)
		{
			this.gridMovie.Visibility = v;
			this.gridMovieFile.Visibility = v;
		}

		public void AbortAllThreads()
		{
			if (this.tvThreads.Count > 0)
			{
				foreach (Thread current in this.tvThreads)
				{
					current.Abort();
				}
				this.tvThreads.Clear();
				this.MetadataCompleted(null, "All Operations aborted", true);
			}
		}

		private void ResetUI(Thread th, string reason, bool Reset)
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new MainWindow.MetadataCompletedHandler(this.ResetUI), th, new object[]
				{
					reason,
					Reset
				});
				return;
			}
			if (th != null)
			{
				th.Abort();
				this.tvThreads.Remove(th);
				if (th.Name == "Loading TV Shows")
				{
					this.LoadingTVShows = false;
					if (this.tvshows.Count > 0)
					{
						this.txtSearchTVShow.Focus();
					}
				}
				if (th.Name == "Loading Movies")
				{
					this.LoadingMovies = false;
					if (this.movies.Count > 0)
					{
						this.txtSearchMovie.Focus();
					}
				}
			}
			if (Reset && this.tvThreads.Count == 0)
			{
				this.HideCancelButtons();
				this.maxvalue = 0;
				this.currentvalue = 0;
				base.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
			}
			if (reason != null)
			{
				this.Message(((th != null && th.Name != null) ? (th.Name + " : ") : "") + reason, MediaScoutMessage.MessageType.Task, 0);
			}
		}

		private void ResetTVShow(TVShow ts, bool IsRemoved)
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new MainWindow.TVShowChangedHandler(this.ResetTVShow), ts, new object[]
				{
					IsRemoved
				});
				return;
			}
			this.tvTVShows.SelectedIndex = -1;
			int index = this.tvshows.IndexOf(ts);
			this.tvshows.RemoveAt(index);
			if (!IsRemoved && Directory.Exists(ts.Folderpath))
			{
				ts = new TVShow(ts.Folderpath, ts.Foldername, ts.IsUnsortedEpisodeCollection);
				this.tvshows.Insert(index, ts);
			}
			this.tvTVShows.SelectedIndex = this.SelectedTVShowIndex;
		}

		private void ResetSeason(MediaScoutGUI.GUITypes.Season s, bool IsRemoved)
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new MainWindow.SeasonChangedHandler(this.ResetSeason), s, new object[]
				{
					IsRemoved
				});
				return;
			}
			TVShow tVShow = s.TVShow;
			this.tvSeasons.SelectedIndex = -1;
			int index = this.tvshows.IndexOf(tVShow);
			int index2 = this.tvshows[index].Seasons.IndexOf(s);
			this.tvshows[index].Seasons.RemoveAt(index2);
			if (!IsRemoved && Directory.Exists(s.Folderpath))
			{
				s = new MediaScoutGUI.GUITypes.Season(s.Folderpath, s.Name, tVShow, s.IsUnsortedEpisodeCollection);
				this.tvshows[index].Seasons.Insert(index2, s);
			}
			this.tvSeasons.SelectedIndex = this.SelectedSeasonIndex;
		}

		private void ResetEpisode(Episode e, bool IsRemoved)
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new MainWindow.EpisodeChangedHandler(this.ResetEpisode), e, new object[]
				{
					IsRemoved
				});
				return;
			}
			MediaScoutGUI.GUITypes.Season season = e.Season;
			this.tvEpisodes.SelectedIndex = -1;
			int index = e.Season.Episodes.IndexOf(e);
			e.Season.Episodes.RemoveAt(index);
			if (!IsRemoved && File.Exists(e.Filepath))
			{
				e = new Episode(e.Filepath, e.Name, season);
				season.Episodes.Insert(index, e);
			}
			this.tvEpisodes.SelectedIndex = this.SelectedEpisodeIndex;
		}

		private void ResetMovie(Movie m, bool IsRemoved)
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new MainWindow.MovieChangedHandler(this.ResetMovie), m, new object[]
				{
					IsRemoved
				});
				return;
			}
			this.lbMovies.SelectedIndex = -1;
			int index = this.movies.IndexOf(m);
			this.movies.RemoveAt(index);
			if (!IsRemoved && Directory.Exists(m.Folderpath))
			{
				m = new Movie(m.Folderpath, m.Foldername, m.IsUnsortedFileCollection);
				this.movies.Insert(index, m);
			}
			this.lbMovies.SelectedIndex = this.SelectedMovieIndex;
		}

		private void ResetMovieFile(MovieFile mf, bool IsRemoved)
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new MainWindow.MovieFileChangedHandler(this.ResetMovieFile), mf, new object[]
				{
					IsRemoved
				});
				return;
			}
			Movie movie = mf.Movie;
			this.lbMovieFiles.SelectedIndex = -1;
			int index = this.movies.IndexOf(movie);
			int index2 = this.movies[index].Files.IndexOf(mf);
			this.movies[index].Files.RemoveAt(index2);
			if (!IsRemoved && File.Exists(mf.Filepath))
			{
				mf = new MovieFile(mf.Filepath, mf.Name, movie);
				this.movies[index].Files.Insert(index2, mf);
			}
			this.lbMovieFiles.SelectedIndex = this.SelectedMovieFileIndex;
		}

		private void ResetActorThumb(MediaScoutGUI.GUITypes.Person p)
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new MainWindow.ActorThumbChangedHandler(this.ResetActorThumb), p);
				return;
			}
			p.Thumb = p.GetImage(p.XBMCFolderPath);
		}

		private void btnReloadSelectedTV_Click(object sender, RoutedEventArgs e)
		{
			TVShow selectedTVShow = this.SelectedTVShow;
			this.TVShowChanged(selectedTVShow, false);
		}

		private void btnReloadSelectedTVs_Click(object sender, RoutedEventArgs e)
		{
			foreach (TVShow current in this.tvshows)
			{
				this.TVShowChanged(current, false);
			}
		}

		private void btnReloadAllTV_Click(object sender, RoutedEventArgs e)
		{
			this.loadTVShows();
		}

		private void btnReloadSelectedMovie_Click(object sender, RoutedEventArgs e)
		{
			Movie selectedMovie = this.SelectedMovie;
			this.MovieChanged(selectedMovie, false);
		}

		private void btnReloadSelectedMovies_Click(object sender, RoutedEventArgs e)
		{
			foreach (Movie current in this.movies)
			{
				this.MovieChanged(current, false);
			}
		}

		private void btnReloadAllMovie_Click(object sender, RoutedEventArgs e)
		{
			this.loadMovies();
		}

		private void SetCancelButtons()
		{
			this.jumpCancel = new JumpTask();
			this.jumpCancel.ApplicationPath = Assembly.GetExecutingAssembly().Location;
			this.jumpCancel.Arguments = "/Cancel:";
			this.jumpCancel.IconResourcePath = Assembly.GetExecutingAssembly().Location;
			this.jumpCancel.IconResourceIndex = 4;
			this.jumpCancel.Title = "Cancel";
			this.jumpCancel.Description = "Cancels Last Operation";
			this.jumpCancelAll = new JumpTask();
			this.jumpCancelAll.ApplicationPath = Assembly.GetExecutingAssembly().Location;
			this.jumpCancelAll.Arguments = "/CancelAll:";
			this.jumpCancelAll.IconResourcePath = Assembly.GetExecutingAssembly().Location;
			this.jumpCancelAll.IconResourceIndex = 4;
			this.jumpCancelAll.Title = "Cancel All";
			this.jumpCancelAll.Description = "Cancels All Operation";
			this.jumpOperationsSeparator = new JumpTask();
		}

		private void ShowCancelButtons()
		{
			this.mnuCancelBar.Visibility = Visibility.Visible;
			base.TaskbarItemInfo.Overlay = (BitmapSource)base.FindResource("imgoverlay");
			if (!this.jumplist.JumpItems.Contains(this.jumpOperationsSeparator))
			{
				List<JumpItem> list = new List<JumpItem>();
				list.Add(this.jumpCancel);
				list.Add(this.jumpCancelAll);
				list.Add(this.jumpOperationsSeparator);
				this.jumplist.JumpItems.InsertRange(0, list);
				this.jumplist.Apply();
			}
		}

		private void HideCancelButtons()
		{
			this.mnuCancelBar.Visibility = Visibility.Collapsed;
			base.TaskbarItemInfo.Overlay = null;
			if (this.jumplist.JumpItems.Contains(this.jumpOperationsSeparator))
			{
				this.jumplist.JumpItems.Remove(this.jumpOperationsSeparator);
				this.jumplist.JumpItems.Remove(this.jumpCancel);
				this.jumplist.JumpItems.Remove(this.jumpCancelAll);
				this.jumplist.Apply();
			}
		}

		private void btnCancelAll_Click(object sender, RoutedEventArgs e)
		{
			this.AbortAllThreads();
		}

		private void _tbCancelAllButton_Click(object sender, EventArgs e)
		{
			this.AbortAllThreads();
		}

		public void CancelOperation(Thread th)
		{
			if (th == null)
			{
				th = this.tvThreads[this.tvThreads.Count - 1];
			}
			this.MetadataCompleted(th, "Operation Aborted", true);
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.CancelOperation(null);
		}

		private void _tbCancelButton_Click(object sender, EventArgs e)
		{
			this.CancelOperation(null);
		}

		private void _tbPauseButton_Click(object sender, EventArgs e)
		{
			if (this._tbPauseButton.Description == "Pause")
			{
				this._tbPauseButton.Description = "Play";
				this._tbPauseButton.ImageSource = (BitmapImage)base.Resources["PlayImage"];
				this.tvThreads[this.tvThreads.Count - 1].Suspend();
				this.SetTasbkBarProgressStatus(TaskbarItemProgressState.Paused);
				return;
			}
			this._tbPauseButton.Description = "Pause";
			this._tbPauseButton.ImageSource = (BitmapImage)base.Resources["PauseImage"];
			this.tvThreads[this.tvThreads.Count - 1].Resume();
			this.SetTasbkBarProgressStatus(TaskbarItemProgressState.Normal);
		}

		private void SetFocusOnTVShow()
		{
			if (this.tvTVShows.SelectedIndex == -1)
			{
				this.tvTVShows.SelectedIndex = 0;
			}
			if (this.tvTVShows.Items.Count > 0 && this.tvTVShows.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
			{
				ListBoxItem listBoxItem = this.tvTVShows.ItemContainerGenerator.ContainerFromIndex(this.SelectedTVShowIndex) as ListBoxItem;
				if (listBoxItem != null)
				{
					listBoxItem.IsSelected = true;
					listBoxItem.Focus();
				}
			}
		}

		private void SetFocusOnSeason()
		{
			if (this.tvSeasons.SelectedIndex == -1)
			{
				this.tvSeasons.SelectedIndex = 0;
			}
			if (this.tvSeasons.Items.Count > 0 && this.tvSeasons.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
			{
				ListBoxItem listBoxItem = this.tvSeasons.ItemContainerGenerator.ContainerFromIndex(this.SelectedSeasonIndex) as ListBoxItem;
				if (listBoxItem != null)
				{
					listBoxItem.IsSelected = true;
					listBoxItem.Focus();
				}
			}
		}

		private void SetFocusOnEpisode()
		{
			if (this.tvEpisodes.SelectedIndex == -1)
			{
				this.tvEpisodes.SelectedIndex = 0;
			}
			if (this.tvEpisodes.Items.Count > 0 && this.tvEpisodes.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
			{
				ListBoxItem listBoxItem = this.tvEpisodes.ItemContainerGenerator.ContainerFromIndex(this.SelectedEpisodeIndex) as ListBoxItem;
				if (listBoxItem != null)
				{
					listBoxItem.IsSelected = true;
					listBoxItem.Focus();
				}
			}
		}

		private void SetFocusOnMovie()
		{
			if (this.lbMovies.SelectedIndex == -1)
			{
				this.lbMovies.SelectedIndex = 0;
			}
			if (this.lbMovies.Items.Count > 0 && this.lbMovies.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
			{
				ListBoxItem listBoxItem = this.lbMovies.ItemContainerGenerator.ContainerFromIndex(this.SelectedMovieIndex) as ListBoxItem;
				if (listBoxItem != null)
				{
					listBoxItem.IsSelected = true;
					listBoxItem.Focus();
				}
			}
		}

		private void SetFocusOnMovieFile()
		{
			if (this.lbMovieFiles.SelectedIndex == -1)
			{
				this.lbMovieFiles.SelectedIndex = 0;
			}
			if (this.lbMovieFiles.Items.Count > 0 && this.lbMovieFiles.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
			{
				ListBoxItem listBoxItem = this.lbMovieFiles.ItemContainerGenerator.ContainerFromIndex(this.SelectedMovieFileIndex) as ListBoxItem;
				if (listBoxItem != null)
				{
					listBoxItem.IsSelected = true;
					listBoxItem.Focus();
				}
			}
		}

		private void SetTVTabFocus()
		{
			if (this.gridSeriesView.Visibility == Visibility.Visible)
			{
				this.SetFocusOnTVShow();
				return;
			}
			if (this.gridSeasonsView.Visibility == Visibility.Visible)
			{
				this.SetFocusOnSeason();
				return;
			}
			if (this.gridEpisodesView.Visibility == Visibility.Visible)
			{
				this.SetFocusOnEpisode();
			}
		}

		private void SetMovieTabFocus()
		{
			if (this.gridMovie.Visibility == Visibility.Visible)
			{
				this.SetFocusOnMovie();
				return;
			}
			if (this.gridMovieFile.Visibility == Visibility.Visible)
			{
				this.SetFocusOnMovieFile();
			}
		}

		private void tabTVSeries_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
			{
				this.txtSearchTVShow.Focus();
				return;
			}
			if (e.Key == Key.Return)
			{
				if (this.gridSeriesView.Visibility == Visibility.Visible)
				{
					if (this.SelectedTVShow != null)
					{
						this.ShowSeasons();
						return;
					}
				}
				else if (this.gridSeasonsView.Visibility == Visibility.Visible && this.SelectedSeason != null)
				{
					this.ShowEpisodeList();
					return;
				}
			}
			else if (e.Key == Key.Delete)
			{
				if (this.gridSeriesView.Visibility == Visibility.Visible)
				{
					if (this.tvTVShows.SelectedItem != null)
					{
						this.btnStripSelectedTV_Click(null, null);
						return;
					}
				}
				else if (this.gridSeasonsView.Visibility == Visibility.Visible && this.tvSeasons.SelectedItem != null)
				{
					this.btnStripSelectedSeason_Click(null, null);
					return;
				}
			}
			else if (e.Key == Key.Back)
			{
				if (this.gridSeasonsView.Visibility == Visibility.Visible)
				{
					this.ShowTVShowList();
					return;
				}
				if (this.gridEpisodesView.Visibility == Visibility.Visible)
				{
					this.ShowSeasons();
				}
			}
		}

		private void tabMovies_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
			{
				this.txtSearchMovie.Focus();
				return;
			}
			if (e.Key == Key.Return)
			{
				if (this.gridMovie.Visibility == Visibility.Visible && this.SelectedMovie != null)
				{
					this.ShowMovieFileList();
					return;
				}
			}
			else if (e.Key == Key.Delete)
			{
				if (this.gridMovie.Visibility == Visibility.Visible)
				{
					if (this.SelectedMovie != null)
					{
						this.btnStripSelectedMovie_Click(null, null);
						return;
					}
				}
				else if (this.gridMovieFile.Visibility == Visibility.Visible && this.SelectedMovie != null)
				{
					this.btnStripSelectedMovieFile_Click(null, null);
					return;
				}
			}
			else if (e.Key == Key.Back && this.gridMovieFile.Visibility == Visibility.Visible)
			{
				this.ShowMovieList();
			}
		}

		private void tcTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.WindowRendered)
			{
				this.StartLoadingItems();
			}
		}

		private void StartLoadingItems()
		{
			TabItem tabItem = this.tcTabs.SelectedItem as TabItem;
			string text = "";
			if (tabItem != null)
			{
				text = tabItem.Name;
			}
			string a;
			if ((a = text) != null)
			{
				if (a == "tabTVSeries")
				{
					this.StartLoadingTVShows();
					return;
				}
				if (!(a == "tabMovies"))
				{
					return;
				}
				this.StartLoadingMovies();
			}
		}

		private void SetTasbkBarProgressStatus(TaskbarItemProgressState state)
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new MainWindow.TaskbarProgressStatusChangeHandler(this.SetTasbkBarProgressStatus), state);
				return;
			}
			base.TaskbarItemInfo.ProgressState = state;
		}

		private void SetTasbkBarProgressValue(int value)
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Send, new MainWindow.TaskbarProgressValueChangeHandler(this.SetTasbkBarProgressValue), value);
				return;
			}
			if (value != this.maxvalue)
			{
				if (base.TaskbarItemInfo.ProgressState != TaskbarItemProgressState.Normal)
				{
					this.SetTasbkBarProgressStatus(TaskbarItemProgressState.Normal);
				}
				base.TaskbarItemInfo.ProgressValue = (double)value / (double)this.maxvalue;
				return;
			}
			this.SetTasbkBarProgressStatus(TaskbarItemProgressState.None);
		}

		private void StartLoadingTVShows()
		{
			StringCollection tVFolders = Settings.Default.TVFolders;
			if (tVFolders == null || tVFolders.Count == 0)
			{
				if (this.tvshows.Count > 0)
				{
					this.tvshows.Clear();
					this.SetTVShowTabItemsVisibility(Visibility.Collapsed);
				}
				return;
			}
			if ((this.tvshows == null || this.tvshows.Count == 0 || this.resetTVfolder) && this.WindowRendered)
			{
				this.resetTVfolder = false;
				this.loadTVShows();
				this.gridTVSeries.DataContext = this.dispatchtvshows;
			}
		}

		private void loadTVShows()
		{
			if (!this.LoadingTVShows)
			{
				this.LoadingTVShows = true;
				this.tvTVShows.SelectedIndex = -1;
				this.tvshows.Clear();
				this.ShowCancelButtons();
				Thread th = null;
				th = new Thread(new ThreadStart(delegate
                {
					foreach (string current in Settings.Default.TVFolders)
					{
						if (Directory.Exists(current))
						{
							DirectoryInfo directoryInfo = new DirectoryInfo(current);
							this.maxvalue += directoryInfo.GetDirectories().Length;
							try
							{
								int num = 0;
								bool flag = false;
								FileInfo[] files = directoryInfo.GetFiles();
								for (int i = 0; i < files.Length; i++)
								{
									FileInfo fileInfo = files[i];
									if (this.AllowedFileTypes.Contains(fileInfo.Extension))
									{
										flag = true;
										num++;
									}
								}
								if (flag)
								{
									this.maxvalue += num;
									TVShow tVShow = new TVShow(directoryInfo.FullName, "Unsorted Episodes", true);
									this.UnsortedEpisodes = tVShow;
									this.tvshows.Add(tVShow);
								}
								DirectoryInfo[] directories = directoryInfo.GetDirectories();
								for (int j = 0; j < directories.Length; j++)
								{
									DirectoryInfo directoryInfo2 = directories[j];
									TVShow item = new TVShow(directoryInfo2.FullName, directoryInfo2.Name, false);
									this.tvshows.Add(item);
									this.SetTasbkBarProgressValue(++this.currentvalue);
								}
							}
							catch (Exception ex)
							{
								if (th.ThreadState != System.Threading.ThreadState.AbortRequested)
								{
									this.SetTasbkBarProgressStatus(TaskbarItemProgressState.Error);
									System.Windows.MessageBox.Show(ex.Message);
								}
							}
						}
					}
					this.MetadataCompleted(th, this.tvshows.Count.ToString(), true);
				}));
				th.Name = "Loading TV Shows";
				th.SetApartmentState(ApartmentState.STA);
				th.Start();
				this.tvThreads.Add(th);
			}
		}

		private void StartLoadingMovies()
		{
			StringCollection movieFolders = Settings.Default.MovieFolders;
			if (movieFolders == null || movieFolders.Count == 0)
			{
				if (this.movies.Count > 0)
				{
					this.movies.Clear();
					this.SetMovieTabItemsVisibility(Visibility.Collapsed);
				}
				return;
			}
			if (this.movies == null || this.movies.Count == 0 || this.resetMoviefolder)
			{
				this.resetMoviefolder = false;
				this.loadMovies();
				this.gridMovies.DataContext = this.dispatchmovies;
			}
		}

		private void loadMovies()
		{
			if (!this.LoadingMovies)
			{
				this.LoadingMovies = true;
				this.lbMovies.SelectedIndex = -1;
				this.movies.Clear();
				this.ShowCancelButtons();
				Thread th = null;
				th = new Thread(new ThreadStart(delegate
                {
					foreach (string current in Settings.Default.MovieFolders)
					{
						if (Directory.Exists(current))
						{
							DirectoryInfo directoryInfo = new DirectoryInfo(current);
							this.maxvalue += directoryInfo.GetDirectories().Length;
							try
							{
								int num = 0;
								bool flag = false;
								FileInfo[] files = directoryInfo.GetFiles();
								for (int i = 0; i < files.Length; i++)
								{
									FileInfo fileInfo = files[i];
									if (this.AllowedFileTypes.Contains(fileInfo.Extension))
									{
										flag = true;
										num++;
									}
								}
								this.maxvalue += num;
								if (flag)
								{
									Movie movie = new Movie(directoryInfo.FullName, "Unsorted Files", true);
									this.UnsortedFiles = movie;
									this.movies.Add(movie);
								}
								DirectoryInfo[] directories = directoryInfo.GetDirectories();
								for (int j = 0; j < directories.Length; j++)
								{
									DirectoryInfo directoryInfo2 = directories[j];
									Movie item = new Movie(directoryInfo2.FullName, directoryInfo2.Name, false);
									this.movies.Add(item);
									this.SetTasbkBarProgressValue(++this.currentvalue);
								}
							}
							catch (Exception ex)
							{
								if (th.ThreadState != System.Threading.ThreadState.AbortRequested)
								{
									this.SetTasbkBarProgressStatus(TaskbarItemProgressState.Error);
									System.Windows.MessageBox.Show(ex.Message);
								}
							}
						}
					}
					this.MetadataCompleted(th, this.movies.Count.ToString(), true);
				}));
				th.Name = "Loading Movies";
				th.SetApartmentState(ApartmentState.STA);
				th.Start();
				this.tvThreads.Add(th);
			}
		}

		private void btnOpenTVShow_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("explorer.exe", "/select," + this.SelectedTVShow.Folderpath);
		}

		private void btnOpenSeason_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("explorer.exe", "/select," + this.SelectedSeason.Folderpath);
		}

		private void btnOpenSeasonMetadata_Click(object sender, RoutedEventArgs e)
		{
			string metadataFolderPath = this.SelectedSeason.MetadataFolderPath;
			if (Directory.Exists(metadataFolderPath))
			{
				Process.Start("explorer.exe", metadataFolderPath);
				return;
			}
			System.Windows.MessageBox.Show("Directory doesn't Exist");
		}

		private void btnOpenEpisode_Click(object sender, RoutedEventArgs e)
		{
			string filepath = this.SelectedEpisode.Filepath;
			if (File.Exists(filepath))
			{
				Process.Start("explorer.exe", "/select," + filepath);
				return;
			}
			System.Windows.MessageBox.Show("File doesn't Exist");
		}

		private void btnOpenEpisodeXMLMetadata_Click(object sender, RoutedEventArgs e)
		{
			string xMLFile = this.SelectedEpisode.XMLFile;
			if (File.Exists(xMLFile))
			{
				Process.Start("explorer.exe", "/select," + xMLFile);
				return;
			}
			System.Windows.MessageBox.Show("File doesn't Exist");
		}

		private void btnOpenMovie_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("explorer.exe", "/select," + this.SelectedMovie.Folderpath);
		}

		private void btnOpenMovieFile_Click(object sender, RoutedEventArgs e)
		{
			string filepath = this.SelectedMovieFile.Filepath;
			if (File.Exists(filepath))
			{
				Process.Start("explorer.exe", "/select," + filepath);
				return;
			}
			System.Windows.MessageBox.Show("File doesn't Exist");
		}

		private void btnOpenActorThumb_Click(object sender, RoutedEventArgs e)
		{
			if (Settings.Default.SaveXBMCMeta)
			{
				string text = this.SelectedPerson.XBMCFolderPath + "\\" + this.SelectedPerson.Name.Replace(" ", "_") + ".jpg";
				if (File.Exists(text))
				{
					Process.Start("explorer.exe", "/select," + text);
				}
				else
				{
					System.Windows.MessageBox.Show("File doesn't Exist");
				}
			}
			if (Settings.Default.SaveMyMoviesMeta)
			{
				string text = this.SelectedPerson.MyMoviesFolderPath + "\\" + this.SelectedPerson.Name.Replace(" ", "_") + "\\folder.jpg";
				if (File.Exists(text))
				{
					Process.Start("explorer.exe", "/select," + text);
					return;
				}
				System.Windows.MessageBox.Show("File doesn't Exist");
			}
		}

		private void tvTVShows_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.tvTVShows.SelectedItems.Count > 1)
			{
				this.tvTVShows.ItemTemplate = (base.FindResource("dtMultipleTVShows") as DataTemplate);
				return;
			}
			if (this.tvTVShows.SelectedIndex == -1)
			{
				this.SetTVShowTabItemsVisibility(Visibility.Collapsed);
				return;
			}
			this.SelectedTVShow = (TVShow)this.tvTVShows.SelectedItem;
			this.SelectedTVShowIndex = this.tvTVShows.SelectedIndex;
			if (!this.SelectedTVShow.IsUnsortedEpisodeCollection)
			{
				this.tvTVShows.ItemTemplate = (base.FindResource("dtTVShows") as DataTemplate);
				this.ShowTVShowList();
				this.SelectedTVShow.Load();
				this.UpdateTVPoster(this.SelectedTVShow, TVShowPosterType.Poster, this.SelectedTVShow.isPosterLoading);
				this.UpdateTVPoster(this.SelectedTVShow, TVShowPosterType.Backdrop, this.SelectedTVShow.isBackDropLoading);
				this.UpdateTVPoster(this.SelectedTVShow, TVShowPosterType.Banner, this.SelectedTVShow.isBannerLoading);
				return;
			}
			this.tvTVShows.ItemTemplate = (base.FindResource("dtUnsortedEpisodesCollection") as DataTemplate);
			this.ShowUnsortedEpisodeCollection();
		}

		private void ShowTVShowList()
		{
			this.gridEpisodesView.Visibility = Visibility.Hidden;
			this.gridSeasonsView.Visibility = Visibility.Hidden;
			this.gridSeriesView.Visibility = Visibility.Visible;
			this.SetFocusOnTVShow();
		}

		private void TVShowItem_DoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				this.ShowSeasons();
			}
		}

		private void ShowUnsortedEpisodeCollection()
		{
			this.SelectedSeason = this.SelectedTVShow.UnsortedEpisodes[0].Season;
			this.ShowEpisodeList();
		}

		private void ShowSeasons()
		{
			TVShow selectedTVShow = this.SelectedTVShow;
			if (!selectedTVShow.IsUnsortedEpisodeCollection)
			{
				this.ShowSeasonList();
				return;
			}
			if (this.gridEpisodesView.Visibility == Visibility.Visible)
			{
				this.ShowTVShowList();
				return;
			}
			this.ShowUnsortedEpisodeCollection();
		}

		private void ShowSeasonList()
		{
			this.gridSeriesView.Visibility = Visibility.Hidden;
			this.gridEpisodesView.Visibility = Visibility.Hidden;
			this.gridSeasonsView.Visibility = Visibility.Visible;
			this.gridSeasonsView.DataContext = this.SelectedTVShow;
			this.SetFocusOnSeason();
		}

		private void tvSeasons_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.tvSeasons.SelectedItems.Count > 1)
			{
				this.tvSeasons.ItemTemplate = (base.FindResource("dtMultipleSeasons") as DataTemplate);
				return;
			}
			if (this.tvSeasons.SelectedIndex != -1)
			{
				this.SelectedSeason = (MediaScoutGUI.GUITypes.Season)this.tvSeasons.SelectedItem;
				this.SelectedSeasonIndex = this.tvSeasons.SelectedIndex;
				if (!this.SelectedSeason.IsUnsortedEpisodeCollection)
				{
					this.tvSeasons.ItemTemplate = (base.FindResource("dtSeasons") as DataTemplate);
					this.UpdateSeasonPoster(this.SelectedSeason, TVShowPosterType.Season_Poster, this.SelectedSeason.isPosterLoading);
					this.UpdateSeasonPoster(this.SelectedSeason, TVShowPosterType.Season_Backdrop, this.SelectedSeason.isBackDropLoading);
					return;
				}
				this.tvSeasons.ItemTemplate = (base.FindResource("dtSeasonUnsortedEpisodes") as DataTemplate);
			}
		}

		private void SeasonItem_DoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				this.ShowEpisodeList();
			}
		}

		private void ShowEpisodeList()
		{
			if (this.SelectedSeason.TVShow.IsUnsortedEpisodeCollection)
			{
				this.btnEpisodesBack.Visibility = Visibility.Collapsed;
			}
			else
			{
				this.btnEpisodesBack.Visibility = Visibility.Visible;
			}
			this.gridSeriesView.Visibility = Visibility.Hidden;
			this.gridSeasonsView.Visibility = Visibility.Hidden;
			this.gridEpisodesView.Visibility = Visibility.Visible;
			this.gridEpisodesView.DataContext = this.SelectedSeason;
			this.SetFocusOnEpisode();
		}

		private void btnSeasonBack_Click(object sender, RoutedEventArgs e)
		{
			this.ShowTVShowList();
		}

		private void tvEpisodes_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.tvEpisodes.SelectedItems.Count > 1)
			{
				this.tvEpisodes.ItemTemplate = (base.FindResource("dtMultipleEpisodes") as DataTemplate);
				return;
			}
			this.tvEpisodes.ItemTemplate = (base.FindResource("dtEpisodes") as DataTemplate);
			if (this.tvEpisodes.SelectedIndex != -1)
			{
				this.SelectedEpisode = (this.tvEpisodes.SelectedItem as Episode);
				this.SelectedEpisodeIndex = this.tvEpisodes.SelectedIndex;
				this.SelectedEpisode.Load();
				this.UpdateEpisodePoster(this.SelectedEpisode, this.SelectedEpisode.PosterFilename, false);
			}
		}

		private void btnEpisodesBack_Click(object sender, RoutedEventArgs e)
		{
			this.ShowSeasons();
		}

		private void lbMovies_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.lbMovies.SelectedItems.Count > 1)
			{
				this.lbMovies.ItemTemplate = (base.FindResource("dtMultipleMovies") as DataTemplate);
				return;
			}
			this.lbMovies.ItemTemplate = (base.FindResource("dtMovies") as DataTemplate);
			if (this.lbMovies.SelectedIndex == -1)
			{
				this.SetMovieTabItemsVisibility(Visibility.Collapsed);
				return;
			}
			this.SelectedMovie = (Movie)this.lbMovies.SelectedItem;
			this.SelectedMovieIndex = this.lbMovies.SelectedIndex;
			if (!this.SelectedMovie.IsUnsortedFileCollection)
			{
				this.ShowMovieList();
				this.SelectedMovie.Load();
				this.UpdateMoviePoster(this.SelectedMovie, MoviePosterType.Poster, this.SelectedMovie.isPosterLoading);
				this.UpdateMoviePoster(this.SelectedMovie, MoviePosterType.Backdrop, this.SelectedMovie.isBackDropLoading);
				return;
			}
			this.lbMovies.ItemTemplate = (base.FindResource("dtUnsortedFilesCollection") as DataTemplate);
			this.ShowMovieFileList();
		}

		private void ShowMovieList()
		{
			this.gridMovieFile.Visibility = Visibility.Hidden;
			this.gridMovie.Visibility = Visibility.Visible;
			this.SetFocusOnMovie();
		}

		private void MovieItem_DoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				this.ShowMovieFileList();
			}
		}

		private void ShowMovieFileList()
		{
			if (this.SelectedMovie.IsUnsortedFileCollection)
			{
				this.btnMovieFileBack.Visibility = Visibility.Collapsed;
			}
			else
			{
				this.btnMovieFileBack.Visibility = Visibility.Visible;
			}
			this.gridMovie.Visibility = Visibility.Hidden;
			this.gridMovieFile.Visibility = Visibility.Visible;
			this.SetFocusOnMovieFile();
		}

		private void lbMovieFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.lbMovieFiles.SelectedItems.Count > 1)
			{
				this.lbMovieFiles.ItemTemplate = (base.FindResource("dtMultipleMovieFiles") as DataTemplate);
				return;
			}
			this.lbMovieFiles.ItemTemplate = (base.FindResource("dtMovieFiles") as DataTemplate);
			if (this.lbMovieFiles.SelectedIndex != -1)
			{
				this.SelectedMovieFile = (MovieFile)this.lbMovieFiles.SelectedItem;
				this.SelectedMovieFileIndex = this.lbMovieFiles.SelectedIndex;
				this.UpdateMovieFilePoster(this.SelectedMovieFile, MoviePosterType.File_Poster, this.SelectedMovieFile.isPosterLoading);
				this.UpdateMovieFilePoster(this.SelectedMovieFile, MoviePosterType.File_Backdrop, this.SelectedMovieFile.isBackDropLoading);
			}
		}

		private void btnMovieFileBack_Click(object sender, RoutedEventArgs e)
		{
			this.ShowMovieList();
		}

		private void lstMovieActors_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.lstMovieActors.SelectedIndex != -1)
			{
				this.SelectedPerson = (this.lstMovieActors.SelectedItem as MediaScoutGUI.GUITypes.Person);
			}
		}

		private void lstTVActors_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.lstTVActors.SelectedIndex != -1)
			{
				this.SelectedPerson = (this.lstTVActors.SelectedItem as MediaScoutGUI.GUITypes.Person);
			}
		}

		private void SetBestMatchOnTopInResults(object[] results, string SearchID, string SearchTerm, string SearchYear, bool IsMovie)
		{
			int num = 0;
			bool flag = false;
			int num2 = 0;
			if (SearchID != null)
			{
				for (int i = 0; i < results.Length; i++)
				{
					object obj = results[i];
					string b = IsMovie ? (obj as MovieXML).ID : (obj as TVShowXML).ID;
					if (SearchID == b)
					{
						num2 = num;
						flag = true;
						break;
					}
					num++;
				}
			}
			else if (SearchYear != null)
			{
				List<int> list = new List<int>();
				for (int j = 0; j < results.Length; j++)
				{
					object obj2 = results[j];
					string a = IsMovie ? (obj2 as MovieXML).Title : (obj2 as TVShowXML).Title;
					string text = IsMovie ? (obj2 as MovieXML).Year : (obj2 as TVShowXML).Year;
					if (!string.IsNullOrEmpty(text))
					{
						if (text == SearchYear)
						{
							if (a == SearchTerm)
							{
								num2 = num;
								flag = true;
								break;
							}
							list.Add(num);
						}
					}
					else if (a == SearchTerm)
					{
						num2 = num;
						flag = true;
						list.Add(num);
					}
					num++;
				}
				if (!flag)
				{
					foreach (int current in list)
					{
						string text2 = IsMovie ? (results[current] as MovieXML).Title : (results[current] as TVShowXML).Title;
						if (text2.Contains(SearchTerm))
						{
							num2 = current;
							flag = true;
							break;
						}
					}
					if (!flag && list.Count > 0)
					{
						num2 = list[0];
						flag = true;
					}
				}
			}
			else
			{
				for (int k = 0; k < results.Length; k++)
				{
					object obj3 = results[k];
					string text3 = IsMovie ? (obj3 as MovieXML).Title : (obj3 as TVShowXML).Title;
					if (text3 == SearchTerm)
					{
						num2 = num;
						flag = true;
						break;
					}
					if (text3.Contains(SearchTerm))
					{
						num2 = num;
						flag = true;
					}
				}
			}
			if (flag)
			{
				object obj4 = results[0];
				results[0] = results[num2];
				results[num2] = obj4;
			}
		}

		private MainWindow.SearchResultsDecision PromptForSearchTerm(string SearchObjectName, string SearchTerm, string SearchYear, bool IsMovie, bool forced, bool CanUserSkip)
		{
			MainWindow.SearchResultsDecision SearchDecision = new MainWindow.SearchResultsDecision();
			base.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				while ((SearchDecision.results == null || SearchDecision.results.Length == 0) && SearchDecision.Decision == DecisionType.Continue)
				{
					NoResultsDialog noResultsDialog = new NoResultsDialog(SearchObjectName, SearchTerm, SearchYear, forced, CanUserSkip, IsMovie);
					forced = false;
					noResultsDialog.Owner = this;
					noResultsDialog.ShowDialog();
					SearchTerm = noResultsDialog.Term;
					SearchYear = noResultsDialog.Year;
					SearchDecision.Decision = noResultsDialog.Decision;
					if (SearchDecision.Decision == DecisionType.Continue)
					{
						if (IsMovie)
						{
							SearchDecision.results = this.tmdb.Search(SearchTerm, SearchYear, this.Language);
						}
						else
						{
							SearchDecision.results = this.tvdb.Search(SearchTerm, SearchYear, this.Language);
						}
					}
				}
			}));
			SearchDecision.SearchTerm = SearchTerm;
			SearchDecision.SearchYear = SearchYear;
			return SearchDecision;
		}

		private MainWindow.SearchResultsDecision PromptForSelection(string SearchObjectName, MainWindow.SearchResultsDecision SearchDecision, object item, bool IsMovie, bool Skip)
		{
			base.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				SelectResultDialog selectResultDialog = new SelectResultDialog(SearchObjectName, SearchDecision.SearchTerm, SearchDecision.results, IsMovie, Skip);
				selectResultDialog.Owner = this;
				selectResultDialog.ShowDialog();
				SearchDecision.Decision = selectResultDialog.Decision;
				if (SearchDecision.Decision == DecisionType.SearchAgain)
				{
					SearchDecision = this.PromptForSearchTerm(SearchObjectName, SearchDecision.SearchTerm, SearchDecision.SearchYear, IsMovie, true, Skip);
					if (SearchDecision.results != null)
					{
						if (SearchDecision.results.Length == 0)
						{
							SearchDecision = this.PromptForSearchTerm(SearchObjectName, SearchDecision.SearchTerm, SearchDecision.SearchYear, IsMovie, false, Skip);
						}
						if (SearchDecision.Decision == DecisionType.Continue)
						{
							if (Settings.Default.AutoSelectMatch)
							{
								this.SetBestMatchOnTopInResults(SearchDecision.results, SearchDecision.SearchID, SearchDecision.SearchTerm, SearchDecision.SearchYear, IsMovie);
								SearchDecision = this.GetSelectedIDAndName(SearchDecision, IsMovie, 0);
							}
							else
							{
								this.SetBestMatchOnTopInResults(SearchDecision.results, SearchDecision.SearchID, SearchDecision.SearchTerm, SearchDecision.SearchYear, IsMovie);
								SearchDecision = this.PromptForSelection(SearchObjectName, SearchDecision, item, IsMovie, Skip);
							}
						}
					}
				}
				if (selectResultDialog.Decision == DecisionType.Continue)
				{
					string selectedName = IsMovie ? (selectResultDialog.Selected as MovieXML).Title : (selectResultDialog.Selected as TVShowXML).Title;
					string selectedID = IsMovie ? (selectResultDialog.Selected as MovieXML).ID : (selectResultDialog.Selected as TVShowXML).ID;
					SearchDecision.SelectedName = selectedName;
					SearchDecision.SelectedID = selectedID;
				}
			}));
			return SearchDecision;
		}

		private MainWindow.SearchResultsDecision SearchForID(object item, bool IsMovie, string SearchObjectName, string SearchTerm, string SearchYear, bool CanUserSkip, bool forceEnterSearchTerm)
		{
			MainWindow.SearchResultsDecision searchResultsDecision = new MainWindow.SearchResultsDecision();
			Movie movie = null;
			TVShow tVShow = null;
			if (IsMovie)
			{
				movie = (item as Movie);
			}
			else
			{
				tVShow = (item as TVShow);
			}
			if (!forceEnterSearchTerm)
			{
				if (IsMovie)
				{
					if (movie.ID == null)
					{
						movie.Load();
					}
					if (movie.HasMetadata || movie.ID != null)
					{
						searchResultsDecision.SelectedName = movie.Name;
						searchResultsDecision.SearchID = (searchResultsDecision.SelectedID = movie.ID);
					}
				}
				else
				{
					if (tVShow.ID == null)
					{
						tVShow.Load();
					}
					if (tVShow.HasMetadata || tVShow.ID != null)
					{
						searchResultsDecision.SelectedName = tVShow.Name;
						searchResultsDecision.SearchID = (searchResultsDecision.SelectedID = tVShow.ID);
					}
				}
			}
			if (searchResultsDecision.SelectedID == null)
			{
				if (SearchTerm == null)
				{
					if (IsMovie)
					{
						SearchTerm = (movie.HasMetadata ? movie.SearchTerm : movie.GetSearchTerm());
					}
					else
					{
						SearchTerm = (tVShow.HasMetadata ? tVShow.SearchTerm : tVShow.GetSearchTerm());
					}
				}
				if (SearchYear == null)
				{
					if (IsMovie)
					{
						SearchYear = movie.GetYear();
					}
					else
					{
						SearchYear = tVShow.GetYear();
					}
				}
				searchResultsDecision.SearchTerm = SearchTerm;
				searchResultsDecision.SearchYear = SearchYear;
				if (forceEnterSearchTerm)
				{
					searchResultsDecision = this.PromptForSearchTerm(SearchObjectName, searchResultsDecision.SearchTerm, searchResultsDecision.SearchYear, IsMovie, true, CanUserSkip);
				}
				else if (IsMovie)
				{
					searchResultsDecision.results = this.tmdb.Search(searchResultsDecision.SearchTerm, searchResultsDecision.SearchYear, this.Language);
				}
				else
				{
					searchResultsDecision.results = this.tvdb.Search(searchResultsDecision.SearchTerm, searchResultsDecision.SearchYear, this.Language);
				}
				if (searchResultsDecision.Decision == DecisionType.Continue)
				{
					if (searchResultsDecision.results == null)
					{
						searchResultsDecision = this.PromptForSearchTerm(SearchObjectName, searchResultsDecision.SearchTerm, searchResultsDecision.SearchYear, IsMovie, false, CanUserSkip);
					}
					if (searchResultsDecision.Decision == DecisionType.Continue)
					{
						if (Settings.Default.AutoSelectMatch)
						{
							this.SetBestMatchOnTopInResults(searchResultsDecision.results, searchResultsDecision.SearchID, searchResultsDecision.SearchTerm, searchResultsDecision.SearchYear, IsMovie);
							searchResultsDecision = this.GetSelectedIDAndName(searchResultsDecision, IsMovie, 0);
						}
						else
						{
							this.SetBestMatchOnTopInResults(searchResultsDecision.results, searchResultsDecision.SearchID, searchResultsDecision.SearchTerm, searchResultsDecision.SearchYear, IsMovie);
							searchResultsDecision = this.PromptForSelection(SearchObjectName, searchResultsDecision, item, IsMovie, CanUserSkip);
						}
					}
					if (searchResultsDecision.SelectedName != null)
					{
						this.Message("Selected " + searchResultsDecision.SelectedName, MediaScoutMessage.MessageType.Task, 0);
					}
				}
			}
			return searchResultsDecision;
		}

		private MainWindow.SearchResultsDecision GetSelectedIDAndName(MainWindow.SearchResultsDecision SearchDecision, bool IsMovie, int index)
		{
			if (IsMovie)
			{
				MovieXML movieXML = SearchDecision.results[index] as MovieXML;
				SearchDecision.SelectedName = movieXML.Title;
				SearchDecision.SelectedID = movieXML.ID;
			}
			else
			{
				TVShowXML tVShowXML = SearchDecision.results[index] as TVShowXML;
				SearchDecision.SelectedName = tVShowXML.Title;
				SearchDecision.SelectedID = tVShowXML.ID;
			}
			return SearchDecision;
		}

		private ObservableCollection<TVShow> BuildSelectedShowList()
		{
			ObservableCollection<TVShow> observableCollection = null;
			if (this.tvTVShows.SelectedItems.Count > 0)
			{
				observableCollection = new ObservableCollection<TVShow>();
				foreach (TVShow item in this.tvTVShows.SelectedItems)
				{
					observableCollection.Add(item);
				}
			}
			return observableCollection;
		}

		private ObservableCollection<MediaScoutGUI.GUITypes.Season> BuildSelectedSeasonList()
		{
			ObservableCollection<MediaScoutGUI.GUITypes.Season> observableCollection = null;
			if (this.tvSeasons.SelectedItems.Count > 0)
			{
				observableCollection = new ObservableCollection<MediaScoutGUI.GUITypes.Season>();
				foreach (MediaScoutGUI.GUITypes.Season item in this.tvSeasons.SelectedItems)
				{
					observableCollection.Add(item);
				}
			}
			return observableCollection;
		}

		private ObservableCollection<Episode> BuildSelectedEpisodeList()
		{
			ObservableCollection<Episode> observableCollection = null;
			if (this.tvEpisodes.SelectedItems.Count > 0)
			{
				observableCollection = new ObservableCollection<Episode>();
				foreach (Episode item in this.tvEpisodes.SelectedItems)
				{
					observableCollection.Add(item);
				}
			}
			return observableCollection;
		}

		private ObservableCollection<Movie> BuildSelectedMovieList()
		{
			ObservableCollection<Movie> observableCollection = null;
			if (this.lbMovies.SelectedItems.Count > 0)
			{
				observableCollection = new ObservableCollection<Movie>();
				foreach (Movie item in this.lbMovies.SelectedItems)
				{
					observableCollection.Add(item);
				}
			}
			return observableCollection;
		}

		private ObservableCollection<MovieFile> BuildSelectedMovieFileList()
		{
			ObservableCollection<MovieFile> observableCollection = null;
			if (this.lbMovieFiles.SelectedItems.Count > 0)
			{
				observableCollection = new ObservableCollection<MovieFile>();
				foreach (MovieFile item in this.lbMovieFiles.SelectedItems)
				{
					observableCollection.Add(item);
				}
			}
			return observableCollection;
		}

		private void btnFetchSelectedTV_Click(object sender, RoutedEventArgs e)
		{
			this.ShowCancelButtons();
			TVShow ts = this.SelectedTVShow;
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				if (ts.IsUnsortedEpisodeCollection)
				{
					this.FetchUnsortedEpisodeCollection(th, ts.UnsortedEpisodes);
				}
				else
				{
					DecisionType decisionType = this.FetchSelectedTV(ts, false);
					if (decisionType == DecisionType.Cancel)
					{
						this.MetadataCompleted(th, "Canceled.", true);
					}
					else
					{
						this.TVShowChanged(ts, false);
					}
				}
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Fetching " + ts.Name;
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private DecisionType FetchSelectedTV(TVShow ts, bool CanUserSkip)
		{
			MainWindow.SearchResultsDecision searchResultsDecision = this.SearchForID(ts, false, ts.Name, null, null, CanUserSkip, Settings.Default.forceEnterSearchTerm);
			if (searchResultsDecision.Decision == DecisionType.Continue)
			{
				TVShowXML tVShow = this.tvdb.GetTVShow(searchResultsDecision.SelectedID, this.Language);
				if (Settings.Default.SaveXBMCMeta)
				{
					this.Message("Saving Metadata as " + tVShow.GetNFOFile(ts.Folderpath), MediaScoutMessage.MessageType.Task, 0);
					try
					{
						tVShow.SaveNFO(ts.Folderpath);
					}
					catch (Exception ex)
					{
						this.Message(ex.Message, MediaScoutMessage.MessageType.Error, 0);
					}
					this.Message("Done", MediaScoutMessage.MessageType.TaskResult, 0);
				}
				if (Settings.Default.SaveMyMoviesMeta)
				{
					this.Message("Saving Metadata as " + tVShow.GetXMLFile(ts.Folderpath), MediaScoutMessage.MessageType.Task, 0);
					try
					{
						tVShow.SaveXML(ts.Folderpath);
					}
					catch (Exception ex2)
					{
						this.Message(ex2.Message, MediaScoutMessage.MessageType.Error, 0);
					}
					this.Message("Done", MediaScoutMessage.MessageType.TaskResult, 0);
				}
			}
			return searchResultsDecision.Decision;
		}

		private void btnFetchSelectedTVs_Click(object sender, RoutedEventArgs e)
		{
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			ObservableCollection<TVShow> selectedtvshows = this.BuildSelectedShowList();
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
			{
				foreach (TVShow current in selectedtvshows)
				{
					if (current.IsUnsortedEpisodeCollection)
					{
						this.FetchUnsortedEpisodeCollection(th, current.UnsortedEpisodes);
					}
					else
					{
						DecisionType decisionType = this.FetchSelectedTV(current, true);
						if (decisionType == DecisionType.Skip)
						{
							this.Message("Skipped " + current.Name, MediaScoutMessage.MessageType.Task, 0);
						}
						else if (decisionType == DecisionType.Cancel)
						{
							this.MetadataCompleted(th, "Canceled.", true);
						}
						else
						{
							this.TVShowChanged(current, false);
						}
					}
				}
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Fetching Selected TVShows";
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private void btnFetchSelectedMovie_Click(object sender, RoutedEventArgs e)
		{
			this.ShowCancelButtons();
			Movie m = this.SelectedMovie;
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				DecisionType decisionType = this.FetchSelectedMovie(m, false);
				if (decisionType == DecisionType.Cancel)
				{
					this.MetadataCompleted(th, "Canceled.", true);
					return;
				}
				this.MovieChanged(m, false);
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Fetching " + m.Name;
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private DecisionType FetchSelectedMovie(Movie m, bool CanUserSkip)
		{
			if (m.IsUnsortedFileCollection)
			{
				return DecisionType.Skip;
			}
			MainWindow.SearchResultsDecision searchResultsDecision = this.SearchForID(m, true, m.Name, null, null, CanUserSkip, Settings.Default.forceEnterSearchTerm);
			if (searchResultsDecision.Decision == DecisionType.Continue)
			{
				MovieXML movieXML = this.tmdb.Get(searchResultsDecision.SelectedID, this.Language);
				if (Settings.Default.SaveXBMCMeta)
				{
					this.Message("Saving Metadata as " + movieXML.GetNFOFile(m.Folderpath), MediaScoutMessage.MessageType.Task, 0);
					try
					{
						movieXML.SaveNFO(m.Folderpath);
					}
					catch (Exception ex)
					{
						this.Message(ex.Message, MediaScoutMessage.MessageType.Error, 0);
					}
					this.Message("Done", MediaScoutMessage.MessageType.TaskResult, 0);
				}
				if (Settings.Default.SaveMyMoviesMeta)
				{
					this.Message("Saving Metadata as " + movieXML.GetXMLFile(m.Folderpath), MediaScoutMessage.MessageType.Task, 0);
					try
					{
						movieXML.SaveXML(m.Folderpath);
					}
					catch (Exception ex2)
					{
						this.Message(ex2.Message, MediaScoutMessage.MessageType.Error, 0);
					}
					this.Message("Done", MediaScoutMessage.MessageType.TaskResult, 0);
				}
			}
			return searchResultsDecision.Decision;
		}

		private void btnFetchSelectedMovies_Click(object sender, RoutedEventArgs e)
		{
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			ObservableCollection<Movie> selectedmovies = this.BuildSelectedMovieList();
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				foreach (Movie current in selectedmovies)
				{
					DecisionType decisionType = this.FetchSelectedMovie(current, true);
					if (decisionType == DecisionType.Skip)
					{
						this.Message("Skipped " + current.Name, MediaScoutMessage.MessageType.Task, 0);
					}
					else if (decisionType == DecisionType.Cancel)
					{
						this.MetadataCompleted(th, "Canceled.", true);
					}
					else
					{
						this.MovieChanged(current, false);
					}
				}
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Fetching Selected Movies";
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private void FetchUnsortedEpisodeCollection(Thread th, ObservableCollection<Episode> Episodes)
		{
			foreach (Episode current in Episodes)
			{
				DecisionType decisionType = this.FetchSelectedEpisode(current, true);
				if (decisionType == DecisionType.Skip)
				{
					this.Message("Skipped " + current.Name, MediaScoutMessage.MessageType.Task, 0);
				}
				else if (decisionType == DecisionType.Cancel)
				{
					this.MetadataCompleted(th, "Canceled.", true);
				}
				else
				{
					this.EpisodeChanged(current, false);
				}
			}
		}

		private void btnFetchSelectedEpisode_Click(object sender, RoutedEventArgs e)
		{
			this.ShowCancelButtons();
			Episode episode = this.SelectedEpisode;
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				DecisionType decisionType = this.FetchSelectedEpisode(episode, false);
				if (decisionType == DecisionType.Cancel)
				{
					this.MetadataCompleted(th, "Canceled.", true);
					return;
				}
				this.EpisodeChanged(episode, false);
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Fetcing " + episode.Name;
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private DecisionType FetchSelectedEpisode(Episode e, bool CanUserSkip)
		{
			MediaScoutGUI.GUITypes.Season season = e.Season;
			TVShow tVShow = season.TVShow;
			int episodeID = GetID.GetSeasonAndEpisodeIDFromFile(e.StrippedFileName).EpisodeID;
			int num = tVShow.IsUnsortedEpisodeCollection ? GetID.GetSeasonAndEpisodeIDFromFile(e.StrippedFileName).SeasonID : season.GetNum();
			if (episodeID == -1 || num == -1)
			{
				System.Windows.MessageBox.Show("Unable to Get Episode/Season Number from File");
				return DecisionType.Skip;
			}
			string searchTerm = tVShow.IsUnsortedEpisodeCollection ? tVShow.GetSearchTerm(e.StrippedFileName) : null;
			string searchObjectName = tVShow.IsUnsortedEpisodeCollection ? e.Name : tVShow.Name;
			MainWindow.SearchResultsDecision searchResultsDecision = this.SearchForID(tVShow, false, searchObjectName, searchTerm, null, false, Settings.Default.forceEnterSearchTerm);
			if (searchResultsDecision.Decision == DecisionType.Continue)
			{
				string selectedID = searchResultsDecision.SelectedID;
				if (!string.IsNullOrEmpty(selectedID))
				{
					EpisodeXML episode = this.tvdb.GetEpisode(selectedID, num.ToString(), episodeID.ToString());
					if (Settings.Default.SaveXBMCMeta)
					{
						this.Message("Saving Metadata as " + episode.GetNFOFile(season.Folderpath, e.StrippedFileName), MediaScoutMessage.MessageType.Task, 0);
						try
						{
							episode.SaveNFO(season.Folderpath, e.StrippedFileName);
						}
						catch (Exception ex)
						{
							this.Message(ex.Message, MediaScoutMessage.MessageType.Error, 0);
						}
						this.Message("Done", MediaScoutMessage.MessageType.TaskResult, 0);
					}
					if (Settings.Default.SaveMyMoviesMeta)
					{
						if (!Directory.Exists(season.MetadataFolderPath))
						{
							IOFunctions.CreateHiddenFolder(season.MetadataFolderPath);
						}
						this.Message("Saving Metadata as " + episode.GetXMLFile(season.Folderpath, e.StrippedFileName), MediaScoutMessage.MessageType.Task, 0);
						try
						{
							episode.SaveXML(season.Folderpath, e.StrippedFileName);
						}
						catch (Exception ex2)
						{
							this.Message(ex2.Message, MediaScoutMessage.MessageType.Error, 0);
						}
						this.Message("Done", MediaScoutMessage.MessageType.TaskResult, 0);
					}
					if (!string.IsNullOrEmpty(episode.PosterUrl))
					{
						Posters posters = new Posters();
						posters.Poster = episode.PosterUrl;
						try
						{
							if (Settings.Default.SaveXBMCMeta)
							{
								this.Message("Saving Episode Poster as " + episode.GetXBMCThumbFilename(e.StrippedFileName), MediaScoutMessage.MessageType.Task, 0);
								string xBMCThumbFile = episode.GetXBMCThumbFile(e.Season.Folderpath, e.StrippedFileName);
								posters.SavePoster(xBMCThumbFile);
								this.Message("Done", MediaScoutMessage.MessageType.TaskResult, 0);
							}
							if (Settings.Default.SaveMyMoviesMeta)
							{
								this.Message("Saving Episode Poster as " + episode.GetMyMoviesThumbFilename(), MediaScoutMessage.MessageType.Task, 0);
								string myMoviesThumbFile = episode.GetMyMoviesThumbFile(e.Season.Folderpath);
								posters.SavePoster(myMoviesThumbFile);
								this.Message("Done", MediaScoutMessage.MessageType.TaskResult, 0);
							}
						}
						catch (Exception ex3)
						{
							this.Message(ex3.Message, MediaScoutMessage.MessageType.TaskError, 0);
						}
					}
				}
			}
			return searchResultsDecision.Decision;
		}

		private void btnFetchSelectedEpisodes_Click(object sender, RoutedEventArgs e)
		{
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			ObservableCollection<Episode> selectedepisodes = this.BuildSelectedEpisodeList();
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				foreach (Episode current in selectedepisodes)
				{
					DecisionType decisionType = this.FetchSelectedEpisode(current, true);
					if (decisionType == DecisionType.Skip)
					{
						this.Message("Skipped " + current.Name, MediaScoutMessage.MessageType.Task, 0);
					}
					else if (decisionType == DecisionType.Cancel)
					{
						this.MetadataCompleted(th, "Canceled.", true);
					}
					else
					{
						this.EpisodeChanged(current, false);
					}
				}
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Fetcing Selected Episodes";
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private void btnFetchSelectedActorThumb_Click(object sender, RoutedEventArgs e)
		{
			List<MediaScout.Person> list = null;
			try
			{
				list = (this.SelectedPerson.IsMovieActor ? this.tmdb.GetActors(this.SelectedMovie.ID) : this.tvdb.GetActors(this.SelectedTVShow.ID));
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show(ex.Message);
			}
			if (list != null & list.Count != 0)
			{
				foreach (MediaScout.Person current in list)
				{
					if (this.SelectedPerson.Name == current.Name)
					{
						if (Settings.Default.SaveXBMCMeta)
						{
							this.Message("Saving " + current.Name + " Image in \\" + current.GetXBMCDirectory(), MediaScoutMessage.MessageType.Task, 0);
							string xBMCFolderPath = this.SelectedPerson.XBMCFolderPath;
							if (!Directory.Exists(xBMCFolderPath))
							{
								IOFunctions.CreateHiddenFolder(xBMCFolderPath);
							}
							if (!string.IsNullOrEmpty(current.Thumb))
							{
								string xBMCFilename = current.GetXBMCFilename();
								string filepath = xBMCFolderPath + "\\" + xBMCFilename;
								current.SaveThumb(filepath);
								this.Message("Done", MediaScoutMessage.MessageType.TaskResult, 0);
							}
							else
							{
								this.Message("Image Not Found", MediaScoutMessage.MessageType.TaskError, 0);
							}
						}
						if (Settings.Default.SaveMyMoviesMeta)
						{
							this.Message("Saving" + current.Name + " Image in \\ImagesByName\\" + current.GetMyMoviesDirectory(), MediaScoutMessage.MessageType.Task, 0);
							if (Directory.Exists(Settings.Default.ImagesByNameLocation))
							{
								if (!string.IsNullOrEmpty(current.Thumb))
								{
									string text = this.SelectedPerson.MyMoviesFolderPath + "\\" + current.GetMyMoviesDirectory();
									string filepath2 = text + "\\" + current.GetMyMoviesFilename();
									if (!Directory.Exists(text))
									{
										Directory.CreateDirectory(text);
									}
									current.SaveThumb(filepath2);
									this.Message("Done", MediaScoutMessage.MessageType.TaskResult, 0);
								}
								else
								{
									this.Message("Image Not Found", MediaScoutMessage.MessageType.TaskError, 0);
								}
							}
							else
							{
								this.Message("ImagesByName Location Not Defined", MediaScoutMessage.MessageType.TaskError, 0);
							}
						}
						if (this.SelectedPerson.IsMovieActor)
						{
							this.SelectedMovie.LoadActorsThumb(this.SelectedPerson);
							break;
						}
						this.SelectedTVShow.LoadActorsThumb(this.SelectedPerson);
						break;
					}
				}
			}
		}

		private void StripFile(string name, string path, int level)
		{
			if (File.Exists(path))
			{
				this.Message("Deleting " + name, MediaScoutMessage.MessageType.Task, level);
				try
				{
					IOFunctions.DeleteFile(path, true);
				}
				catch (Exception ex)
				{
					this.Message(ex.Message, MediaScoutMessage.MessageType.TaskError, level);
				}
				this.Message("Done", MediaScoutMessage.MessageType.TaskResult, level);
			}
		}

		private void StripDir(string name, string path, int level)
		{
			if (Directory.Exists(path))
			{
				this.Message("Deleting " + name, MediaScoutMessage.MessageType.Task, level);
				try
				{
					IOFunctions.DeleteDirectory(path, true);
				}
				catch (Exception ex)
				{
					this.Message(ex.Message, MediaScoutMessage.MessageType.TaskError, level);
				}
				this.Message("Done", MediaScoutMessage.MessageType.TaskResult, level);
			}
		}

		private void btnStripSelectedTV_Click(object sender, RoutedEventArgs e)
		{
			TVShow selectedTVShow = this.SelectedTVShow;
			string messageBoxText = "Are you sure you want to delete all Metadata and images for this series?";
			if (System.Windows.MessageBox.Show(messageBoxText, selectedTVShow.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				this.StripSelectedTV(selectedTVShow);
				this.Message("Done.", MediaScoutMessage.MessageType.Task, 0);
			}
		}

		private void StripSelectedTV(TVShow ts)
		{
			if (ts.IsUnsortedEpisodeCollection)
			{
				this.btnStripAllEpisodes_Click(null, null);
				return;
			}
			this.Message("Delete all of the Metadata for series " + ts.Name, MediaScoutMessage.MessageType.Task, 0);
			string text = "folder.jpg";
			string path = ts.Folderpath + "\\" + text;
			this.StripFile(text, path, 1);
			text = "banner.jpg";
			path = ts.Folderpath + "\\" + text;
			this.StripFile(text, path, 1);
			if (Settings.Default.SaveMyMoviesMeta)
			{
				text = "backdrop.jpg";
				path = ts.Folderpath + "\\" + text;
				this.StripFile(text, path, 1);
				text = "series.xml";
				path = ts.Folderpath + "\\" + text;
				this.StripFile(text, path, 1);
			}
			if (Settings.Default.SaveXBMCMeta)
			{
				text = "fanart.jpg";
				path = ts.Folderpath + "\\" + text;
				this.StripFile(text, path, 1);
				text = "tvshow.nfo";
				path = ts.Folderpath + "\\" + text;
				this.StripFile(text, path, 1);
				text = ".actors";
				path = Path.Combine(ts.Folderpath, text);
				this.StripDir(text, path, 1);
			}
			if (ts.Actors != null)
			{
				for (int i = 0; i < ts.Actors.Count; i++)
				{
					this.StripSelectedActorThumb(ts.Actors[i]);
				}
			}
			if (ts.Seasons != null)
			{
				for (int j = 0; j < ts.Seasons.Count; j++)
				{
					this.StripSelectedSeason(ts.Seasons[j]);
				}
			}
			this.TVShowChanged(ts, false);
		}

		private void btnStripSelectedTVs_Click(object sender, RoutedEventArgs e)
		{
			string messageBoxText = "Are you sure you want to delete all Metadata and images for selected TV Shows?";
			if (System.Windows.MessageBox.Show(messageBoxText, "TV Shows", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				this.StripMultipleTV(this.BuildSelectedShowList());
			}
		}

		private void StripMultipleTV(ObservableCollection<TVShow> tvshows)
		{
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				this.maxvalue += tvshows.Count;
				for (int i = 0; i < tvshows.Count; i++)
				{
					this.StripSelectedTV(tvshows[i]);
					this.SetTasbkBarProgressValue(++this.currentvalue);
				}
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Stripping TV Shows";
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private void btnStripAllTV_Click(object sender, RoutedEventArgs e)
		{
			string messageBoxText = "Are you sure you want to delete all Metadata and images for all TV Shows?";
			if (System.Windows.MessageBox.Show(messageBoxText, "TV Shows", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				this.StripMultipleTV(this.tvshows);
			}
		}

		private void btnStripSelectedSeason_Click(object sender, RoutedEventArgs e)
		{
			MediaScoutGUI.GUITypes.Season selectedSeason = this.SelectedSeason;
			string messageBoxText = "Are you sure you want to delete all Metadata and images for this season?";
			if (System.Windows.MessageBox.Show(messageBoxText, selectedSeason.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				this.StripSelectedSeason(selectedSeason);
				this.Message("Done.", MediaScoutMessage.MessageType.Task, 1);
			}
		}

		private void StripSelectedSeason(MediaScoutGUI.GUITypes.Season s)
		{
			this.Message("Deleting Metadata for " + s.Name, MediaScoutMessage.MessageType.Task, 1);
			string text = "folder.jpg";
			string path = s.Folderpath + "\\" + text;
			this.StripFile(text, path, 2);
			if (Settings.Default.SaveMyMoviesMeta)
			{
				text = "backdrop.jpg";
				path = s.Folderpath + "\\" + text;
				this.StripFile(text, path, 2);
				text = "metadata";
				path = Path.Combine(s.Folderpath, text);
				this.StripDir(text, path, 2);
			}
			if (Settings.Default.SaveXBMCMeta)
			{
				text = "fanart.jpg";
				path = s.Folderpath + "\\" + text;
				this.StripFile(text, path, 2);
				text = ".actors";
				path = Path.Combine(s.Folderpath, text);
				this.StripDir(text, path, 2);
			}
			if (s.Episodes != null)
			{
				for (int i = 0; i < s.Episodes.Count; i++)
				{
					this.StripSelectedEpisode(s.Episodes[i]);
				}
			}
			this.SeasonChanged(s, false);
		}

		private void btnStripSelectedSeasons_Click(object sender, RoutedEventArgs e)
		{
			TVShow selectedTVShow = this.SelectedTVShow;
			string messageBoxText = "Are you sure you want to delete all Metadata and images fo selected seasons?";
			if (System.Windows.MessageBox.Show(messageBoxText, selectedTVShow.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				this.StripMultipleSeason(this.BuildSelectedSeasonList());
			}
		}

		private void StripMultipleSeason(ObservableCollection<MediaScoutGUI.GUITypes.Season> seasons)
		{
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				this.maxvalue += seasons.Count;
				for (int i = 0; i < seasons.Count; i++)
				{
					this.StripSelectedSeason(seasons[i]);
					this.SetTasbkBarProgressValue(++this.currentvalue);
				}
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Stripping Seasons";
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private void btnStripAllSeasons_Click(object sender, RoutedEventArgs e)
		{
			TVShow selectedTVShow = this.SelectedTVShow;
			string messageBoxText = "Are you sure you want to delete all Metadata and images fo all seasons?";
			if (System.Windows.MessageBox.Show(messageBoxText, selectedTVShow.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				this.StripMultipleSeason(selectedTVShow.Seasons);
			}
		}

		private void btnStripSelectedEpisode_Click(object sender, RoutedEventArgs e)
		{
			Episode selectedEpisode = this.SelectedEpisode;
			string messageBoxText = "Are you sure you want to delete all Metadata and images for this episode?";
			if (System.Windows.MessageBox.Show(messageBoxText, selectedEpisode.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				this.StripSelectedEpisode(selectedEpisode);
				this.Message("Done.", MediaScoutMessage.MessageType.Task, 2);
			}
		}

		private void StripSelectedEpisode(Episode e)
		{
			this.Message("Stripping Metadata for " + e.Name, MediaScoutMessage.MessageType.Task, 2);
			if (Settings.Default.SaveMyMoviesMeta)
			{
				string text = e.StrippedFileName + ".xml";
				string path = e.Season.MetadataFolderPath + "\\" + text;
				this.StripFile(text, path, 3);
				text = e.PosterFilename;
				path = e.Season.MetadataFolderPath + "\\" + text;
				this.StripFile(text, path, 3);
			}
			if (Settings.Default.SaveXBMCMeta)
			{
				string text = e.StrippedFileName + ".nfo";
				string path = e.Season.Folderpath + "\\" + text;
				this.StripFile(text, path, 3);
				text = e.StrippedFileName + ".tbn";
				path = e.Season.Folderpath + "\\" + text;
				this.StripFile(text, path, 3);
			}
			this.EpisodeChanged(e, false);
		}

		private void btnStripSelectedEpisodes_Click(object sender, RoutedEventArgs e)
		{
			MediaScoutGUI.GUITypes.Season selectedSeason = this.SelectedSeason;
			string messageBoxText = "Are you sure you want to delete all Metadata and images for all episode?";
			if (System.Windows.MessageBox.Show(messageBoxText, selectedSeason.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				this.StripMultipleEpisodes(this.BuildSelectedEpisodeList());
			}
		}

		private void StripMultipleEpisodes(ObservableCollection<Episode> episodes)
		{
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				this.maxvalue += episodes.Count;
				for (int i = 0; i < episodes.Count; i++)
				{
					this.StripSelectedEpisode(episodes[i]);
					this.SetTasbkBarProgressValue(++this.currentvalue);
				}
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Stripping Episodes";
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private void btnStripAllEpisodes_Click(object sender, RoutedEventArgs e)
		{
			MediaScoutGUI.GUITypes.Season selectedSeason = this.SelectedSeason;
			string messageBoxText = "Are you sure you want to delete all Metadata and images for all episode?";
			if (System.Windows.MessageBox.Show(messageBoxText, selectedSeason.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				this.StripMultipleEpisodes(selectedSeason.Episodes);
			}
		}

		private void btnStripSelectedMovie_Click(object sender, RoutedEventArgs e)
		{
			Movie selectedMovie = this.SelectedMovie;
			string messageBoxText = "Are you sure you want to delete all Metadata and images for this Movie?";
			if (System.Windows.MessageBox.Show(messageBoxText, selectedMovie.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				this.StripSelectedMovie(selectedMovie);
				this.Message("Done.", MediaScoutMessage.MessageType.Task, 0);
				return;
			}
			Debug.WriteLine("Leaving Metadata alone.");
		}

		private void StripSelectedMovie(Movie m)
		{
			if (m.IsUnsortedFileCollection)
			{
				this.btnStripAllMovieFiles_Click(null, null);
				return;
			}
			this.Message("Deleting all of the Metadata for movie " + m.Name, MediaScoutMessage.MessageType.Task, 0);
			string text = "folder.jpg";
			string path = m.Folderpath + "\\" + text;
			this.StripFile(text, path, 1);
			if (Settings.Default.SaveMyMoviesMeta)
			{
				text = "backdrop.jpg";
				path = m.Folderpath + "\\" + text;
				this.StripFile(text, path, 1);
				text = "mymovies.xml";
				path = m.Folderpath + "\\" + text;
				this.StripFile(text, path, 1);
			}
			if (Settings.Default.SaveXBMCMeta)
			{
				text = "fanart.jpg";
				path = m.Folderpath + "\\" + text;
				this.StripFile(text, path, 1);
				text = "movie.nfo";
				path = m.Folderpath + "\\" + text;
				this.StripFile(text, path, 1);
				text = ".actors";
				path = Path.Combine(m.Folderpath, text);
				this.StripDir(text, path, 1);
			}
			if (m.Actors != null)
			{
				for (int i = 0; i < m.Actors.Count; i++)
				{
					this.StripSelectedActorThumb(m.Actors[i]);
				}
			}
			if (m.Files != null)
			{
				for (int j = 0; j < m.Files.Count; j++)
				{
					this.StripSelectedMovieFile(m.Files[j]);
				}
			}
			this.MovieChanged(m, false);
		}

		private void btnStripSelectedMovies_Click(object sender, RoutedEventArgs e)
		{
			string messageBoxText = "Are you sure you want to delete all Metadata and images for selected Movies?";
			if (System.Windows.MessageBox.Show(messageBoxText, "Movies", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				this.StripMultipleMovies(this.BuildSelectedMovieList());
			}
		}

		private void StripMultipleMovies(ObservableCollection<Movie> movies)
		{
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				this.maxvalue += movies.Count;
				for (int i = 0; i < movies.Count; i++)
				{
					this.StripSelectedMovie(movies[i]);
					this.SetTasbkBarProgressValue(++this.currentvalue);
				}
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Stripping Movies";
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private void btnStripAllMovie_Click(object sender, RoutedEventArgs e)
		{
			string messageBoxText = "Are you sure you want to delete all Metadata and images for all Movies?";
			if (System.Windows.MessageBox.Show(messageBoxText, "Movies", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				this.StripMultipleMovies(this.movies);
			}
		}

		private void btnStripSelectedMovieFile_Click(object sender, RoutedEventArgs e)
		{
			MovieFile selectedMovieFile = this.SelectedMovieFile;
			string messageBoxText = "Are you sure you want to delete all Metadata and images for this Movie File?";
			if (System.Windows.MessageBox.Show(messageBoxText, selectedMovieFile.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				this.StripSelectedMovieFile(selectedMovieFile);
				this.Message("Done.", MediaScoutMessage.MessageType.Task, 0);
			}
		}

		private void StripSelectedMovieFile(MovieFile mf)
		{
			if (Settings.Default.SaveXBMCMeta)
			{
				string text = mf.StrippedFileName + ".nfo";
				string path = mf.Movie.Folderpath + "\\" + text;
				this.StripFile(text, path, 2);
				text = mf.StrippedFileName + ".tbn";
				path = mf.Movie.Folderpath + "\\" + text;
				this.StripFile(text, path, 2);
			}
			this.MovieFileChanged(mf, false);
		}

		private void btnStripSelectedMovieFiles_Click(object sender, RoutedEventArgs e)
		{
			Movie selectedMovie = this.SelectedMovie;
			string messageBoxText = "Are you sure you want to delete all Metadata and images for selected Movie Files?";
			if (System.Windows.MessageBox.Show(messageBoxText, selectedMovie.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				this.StripMultipleMovieFiles(this.BuildSelectedMovieFileList());
			}
		}

		private void StripMultipleMovieFiles(ObservableCollection<MovieFile> files)
		{
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				this.maxvalue += this.movies.Count;
				for (int i = 0; i < files.Count; i++)
				{
					this.StripSelectedMovieFile(files[i]);
					this.SetTasbkBarProgressValue(++this.currentvalue);
				}
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Stripping Movie Files";
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private void btnStripAllMovieFiles_Click(object sender, RoutedEventArgs e)
		{
			Movie selectedMovie = this.SelectedMovie;
			string messageBoxText = "Are you sure you want to delete all Metadata and images for all Movie Files?";
			if (System.Windows.MessageBox.Show(messageBoxText, selectedMovie.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				this.StripMultipleMovieFiles(selectedMovie.Files);
			}
		}

		private void btnStripSelectedActorThumb_Click(object sender, RoutedEventArgs e)
		{
			MediaScoutGUI.GUITypes.Person selectedPerson = this.SelectedPerson;
			string messageBoxText = "Are you sure you want to delete images for this actor?";
			if (System.Windows.MessageBox.Show(messageBoxText, selectedPerson.Name, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				this.StripSelectedActorThumb(selectedPerson);
				this.Message("Done.", MediaScoutMessage.MessageType.Task, 0);
			}
		}

		private void StripSelectedActorThumb(MediaScoutGUI.GUITypes.Person p)
		{
			if (Settings.Default.SaveMyMoviesMeta)
			{
				string text = p.Name + "\\folder.jpg";
				string path = p.MyMoviesFolderPath + "\\" + text;
				this.StripFile(text, path, 2);
				text = p.Name;
				path = Path.Combine(p.MyMoviesFolderPath, text);
				this.StripDir(text, path, 2);
			}
			if (Settings.Default.SaveXBMCMeta)
			{
				string text = p.Name.Replace(" ", "_") + ".jpg";
				string path = p.XBMCFolderPath + "\\" + text;
				this.StripFile(text, path, 2);
			}
			this.ActorThumbChanged(p);
		}

		private TVScout SetTVScout()
		{
			TVScoutOptions options = new TVScoutOptions
			{
				GetSeriesPosters = Settings.Default.getSeriesPosters,
				GetSeasonPosters = Settings.Default.getSeasonPosters,
				GetEpisodePosters = Settings.Default.getEpisodePosters,
				MoveFiles = Settings.Default.moveTVFiles,
				SeasonFolderName = Settings.Default.SeasonFolderName,
				SpecialsFolderName = Settings.Default.SpecialsFolderName,
				DownloadAllPosters = Settings.Default.downloadAllTVPosters,
				DownloadAllBackdrops = Settings.Default.downloadAllTVBackdrops,
				DownloadAllBanners = Settings.Default.downloadAllTVBanners,
				DownloadAllSeasonPosters = Settings.Default.downloadAllTVSeasonPosters,
				DownloadAllSeasonBackdrops = Settings.Default.downloadAllTVSeasonBackdrops,
				SaveActors = Settings.Default.SaveTVActors,
				RenameFiles = Settings.Default.renameTVFiles,
				RenameFormat = Settings.Default.TVfileformat,
				SeasonNumZeroPadding = int.Parse(Settings.Default.SeasonNumZeroPadding),
				EpisodeNumZeroPadding = int.Parse(Settings.Default.EpisodeNumZeroPadding),
				AllowedFileTypes = this.AllowedFileTypes.ToArray(),
				AllowedSubtitles = this.AllowedSubtitleTypes.ToArray(),
				ForceUpdate = Settings.Default.forceUpdate,
				overwrite = Settings.Default.overwriteFiles,
				SaveXBMCMeta = Settings.Default.SaveXBMCMeta,
				SaveMyMoviesMeta = Settings.Default.SaveMyMoviesMeta,
				FilenameReplaceChar = Settings.Default.FilenameReplaceChar
			};
			this.TVScout = new TVScout(options, this.Message, Settings.Default.ImagesByNameLocation);
			return this.TVScout;
		}

		private MovieScout SetMovieScout()
		{
			MovieScoutOptions options = new MovieScoutOptions
			{
				GetMoviePosters = Settings.Default.getMoviePosters,
				GetMovieFilePosters = Settings.Default.getMovieFilePosters,
				MoveFiles = Settings.Default.moveMovieFiles,
				DownloadAllPosters = Settings.Default.downloadAllMoviePosters,
				DownloadAllBackdrops = Settings.Default.downloadAllMovieBackdrops,
				SaveActors = Settings.Default.SaveMovieActors,
				RenameFiles = Settings.Default.renameMovieFiles,
				FileRenameFormat = Settings.Default.Moviefileformat,
				DirRenameFormat = Settings.Default.MovieDirformat,
				AllowedFileTypes = this.AllowedFileTypes.ToArray(),
				AllowedSubtitles = this.AllowedSubtitleTypes.ToArray(),
				ForceUpdate = Settings.Default.forceUpdate,
				overwrite = Settings.Default.overwriteFiles,
				SaveXBMCMeta = Settings.Default.SaveXBMCMeta,
				SaveMyMoviesMeta = Settings.Default.SaveMyMoviesMeta,
				FilenameReplaceChar = Settings.Default.FilenameReplaceChar
			};
			this.MovieScout = new MovieScout(options, this.Message, Settings.Default.ImagesByNameLocation);
			return this.MovieScout;
		}

		private void btnProcessSelectedTV_Click(object sender, RoutedEventArgs e)
		{
			TVShow ts = this.SelectedTVShow;
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			this.TVScout = this.SetTVScout();
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				if (ts.IsUnsortedEpisodeCollection)
				{
					this.ProcessMultipleEpisodes(th, true, ts.Seasons[0].Episodes);
				}
				else
				{
					DecisionType decisionType = this.ProcessingSelectedTV(ts, false);
					if (decisionType == DecisionType.Cancel)
					{
						this.MetadataCompleted(th, "Canceled.", true);
					}
					else
					{
						this.TVShowChanged(ts, ts.IsDeleted);
					}
				}
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Processing " + ts.Name;
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private DecisionType ProcessingSelectedTV(TVShow ts, bool CanUserSkip)
		{
			this.Message("Scanning folder " + ts.Name, MediaScoutMessage.MessageType.Task, 0);
			MainWindow.SearchResultsDecision searchResultsDecision = this.SearchForID(ts, false, ts.Name, null, null, CanUserSkip, Settings.Default.forceEnterSearchTerm);
			if (searchResultsDecision.Decision == DecisionType.Continue)
			{
				TVShowXML tVShowXML = null;
				try
				{
					tVShowXML = this.tvdb.GetTVShow(searchResultsDecision.SelectedID, this.Language);
				}
				catch (Exception ex)
				{
					this.Message(ex.Message, MediaScoutMessage.MessageType.Error, 0);
				}
				if (tVShowXML != null)
				{
					this.TVScout.series = tVShowXML;
					string text = this.TVScout.ProcessDirectory(ts.Folderpath);
					if (text != null && text.Length > 2)
					{
						if (text.Substring(0, 2) != "d:")
						{
							ts.Foldername = text;
							ts.Folderpath = Path.GetDirectoryName(ts.Folderpath) + "\\" + ts.Foldername;
						}
						else
						{
							ts.IsDeleted = true;
						}
					}
				}
			}
			return searchResultsDecision.Decision;
		}

		private void btnProcessSelectedTVs_Click(object sender, RoutedEventArgs e)
		{
			this.ProcessMultipleTVs(this.BuildSelectedShowList());
		}

		private void ProcessMultipleTVs(ObservableCollection<TVShow> tvshows)
		{
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			this.TVScout = this.SetTVScout();
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				this.maxvalue += tvshows.Count;
				for (int i = 0; i < tvshows.Count; i++)
				{
					TVShow tVShow = tvshows[i];
					if (tVShow.IsUnsortedEpisodeCollection)
					{
						this.ProcessMultipleEpisodes(th, false, tVShow.Seasons[0].Episodes);
					}
					else
					{
						DecisionType decisionType = this.ProcessingSelectedTV(tVShow, true);
						if (decisionType == DecisionType.Skip)
						{
							this.Message("Skipped " + tVShow.Name, MediaScoutMessage.MessageType.Task, 0);
						}
						else if (decisionType == DecisionType.Cancel)
						{
							this.MetadataCompleted(th, "Canceled.", true);
						}
						else
						{
							this.TVShowChanged(tVShow, false);
						}
					}
					this.SetTasbkBarProgressValue(++this.currentvalue);
				}
				for (int j = 0; j < tvshows.Count; j++)
				{
					if (tvshows[j].IsDeleted)
					{
						this.TVShowChanged(tvshows[j], true);
						j--;
					}
				}
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Processing TVShows";
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private void btnProcessAllTV_Click(object sender, RoutedEventArgs e)
		{
			this.ProcessMultipleTVs(this.tvshows);
		}

		private void btnProcessSelectedSeason_Click(object sender, RoutedEventArgs e)
		{
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			this.TVScout = this.SetTVScout();
			MediaScoutGUI.GUITypes.Season s = this.SelectedSeason;
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				if (s.IsUnsortedEpisodeCollection)
				{
					this.ProcessMultipleEpisodes(th, true, s.Episodes);
				}
				else
				{
					DecisionType decisionType = this.ProcessingSelectedSeason(s, false);
					if (decisionType == DecisionType.Cancel)
					{
						this.MetadataCompleted(th, "Canceled.", true);
					}
					else
					{
						this.SeasonChanged(s, s.IsDeleted);
					}
				}
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Processing " + s.Name;
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private DecisionType ProcessingSelectedSeason(MediaScoutGUI.GUITypes.Season s, bool CanUserSkip)
		{
			TVShow tVShow = s.TVShow;
			this.Message("Scanning folder " + s.Name, MediaScoutMessage.MessageType.Task, 0);
			MainWindow.SearchResultsDecision searchResultsDecision = this.SearchForID(tVShow, false, tVShow.Name, null, null, CanUserSkip, Settings.Default.forceEnterSearchTerm);
			if (searchResultsDecision.Decision == DecisionType.Continue)
			{
				TVShowXML tVShowXML = null;
				try
				{
					tVShowXML = this.tvdb.GetTVShow(searchResultsDecision.SelectedID, this.Language);
				}
				catch (Exception ex)
				{
					this.Message(ex.Message, MediaScoutMessage.MessageType.Error, 0);
				}
				if (tVShowXML != null)
				{
					this.TVScout.series = tVShowXML;
					string text = this.TVScout.ProcessSeasonDirectory(tVShow.Folderpath, new DirectoryInfo(s.Folderpath), -1);
					if (text != null && text.Length > 2)
					{
						if (text.Substring(0, 2) != "d:")
						{
							s.Name = text;
							s.Folderpath = tVShow.Folderpath + "\\" + text;
						}
						else
						{
							s.IsDeleted = true;
						}
					}
				}
			}
			return searchResultsDecision.Decision;
		}

		private void btnProcessSelectedSeasons_Click(object sender, RoutedEventArgs e)
		{
			this.ProcessMultipleSeasons(this.BuildSelectedSeasonList());
		}

		private void ProcessMultipleSeasons(ObservableCollection<MediaScoutGUI.GUITypes.Season> seasons)
		{
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			this.TVScout = this.SetTVScout();
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				this.maxvalue += seasons.Count;
				foreach (MediaScoutGUI.GUITypes.Season current in seasons)
				{
					DecisionType decisionType = this.ProcessingSelectedSeason(current, true);
					if (decisionType == DecisionType.Skip)
					{
						this.Message("Skipped " + current.Name, MediaScoutMessage.MessageType.Task, 0);
					}
					else if (decisionType == DecisionType.Cancel)
					{
						this.MetadataCompleted(th, "Canceled.", true);
					}
					else
					{
						this.SeasonChanged(current, false);
					}
					this.SetTasbkBarProgressValue(++this.currentvalue);
				}
				for (int i = 0; i < seasons.Count; i++)
				{
					if (seasons[i].IsDeleted)
					{
						this.SeasonChanged(seasons[i], true);
					}
				}
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Processing Seasons";
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private void btnProcessAllSeasons_Click(object sender, RoutedEventArgs e)
		{
			TVShow selectedTVShow = this.SelectedTVShow;
			this.ProcessMultipleSeasons(selectedTVShow.Seasons);
		}

		private void btnProcessSelectedEpisode_Click(object sender, RoutedEventArgs e)
		{
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			this.TVScout = this.SetTVScout();
			Episode Episode = this.SelectedEpisode;
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				DecisionType decisionType = this.ProcessingSelectedEpisode(Episode, false);
				if (decisionType == DecisionType.Cancel)
				{
					this.MetadataCompleted(th, "Canceled.", true);
				}
				else
				{
					this.EpisodeChanged(Episode, Episode.IsDeleted);
				}
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Processing " + Episode.Name;
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private DecisionType ProcessingSelectedEpisode(Episode e, bool CanUserSkip)
		{
			this.Message("Scanning file " + e.Name, MediaScoutMessage.MessageType.Task, 0);
			MediaScoutGUI.GUITypes.Season season = e.Season;
			TVShow tVShow = season.TVShow;
			int seasonNum = (tVShow.IsUnsortedEpisodeCollection || season.IsUnsortedEpisodeCollection) ? -1 : season.GetNum();
			string searchTerm = tVShow.IsUnsortedEpisodeCollection ? tVShow.GetSearchTerm(e.StrippedFileName) : null;
			string searchObjectName = tVShow.IsUnsortedEpisodeCollection ? e.Name : tVShow.Name;
			MainWindow.SearchResultsDecision searchResultsDecision = this.SearchForID(tVShow, false, searchObjectName, searchTerm, null, CanUserSkip, Settings.Default.forceEnterSearchTerm);
			if (searchResultsDecision.Decision == DecisionType.Continue)
			{
				string selectedID = searchResultsDecision.SelectedID;
				if (!string.IsNullOrEmpty(selectedID))
				{
					TVShowXML tVShowXML = null;
					try
					{
						tVShowXML = this.tvdb.GetTVShow(selectedID, this.Language);
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.Message);
						this.Message(ex.Message, MediaScoutMessage.MessageType.Error, 0);
					}
					if (tVShowXML != null)
					{
						this.TVScout.series = tVShowXML;
						string text = this.TVScout.ProcessEpisode(tVShow.Folderpath, new FileInfo(e.Filepath), seasonNum, !tVShow.IsUnsortedEpisodeCollection, -1);
						if (text != null)
						{
							if (tVShow.IsUnsortedEpisodeCollection || season.IsUnsortedEpisodeCollection)
							{
								e.IsDeleted = true;
							}
							else
							{
								e.Name = text;
								e.Filepath = e.Season.Folderpath + "\\" + e.Name;
							}
						}
					}
				}
			}
			return searchResultsDecision.Decision;
		}

		private void btnProcessSelectedEpisodes_Click(object sender, RoutedEventArgs e)
		{
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			this.TVScout = this.SetTVScout();
			ObservableCollection<Episode> Episodes = this.BuildSelectedEpisodeList();
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				this.ProcessMultipleEpisodes(th, true, Episodes);
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Processing Episodes";
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private void ProcessMultipleEpisodes(Thread th, bool RemoveTVShoworSeason, ObservableCollection<Episode> episodes)
		{
			MediaScoutGUI.GUITypes.Season season = episodes[0].Season;
			TVShow tVShow = season.TVShow;
			this.maxvalue += episodes.Count;
			foreach (Episode current in episodes)
			{
				DecisionType decisionType = this.ProcessingSelectedEpisode(current, true);
				if (decisionType == DecisionType.Skip)
				{
					this.Message("Skipped " + current.Name, MediaScoutMessage.MessageType.Task, 0);
				}
				else if (decisionType == DecisionType.Cancel)
				{
					this.MetadataCompleted(th, "Canceled.", true);
				}
				this.SetTasbkBarProgressValue(++this.currentvalue);
			}
			for (int i = 0; i < episodes.Count; i++)
			{
				if (episodes[i].IsDeleted)
				{
					this.EpisodeChanged(episodes[i], true);
					i--;
				}
			}
			if (RemoveTVShoworSeason && episodes.Count == 0)
			{
				if (tVShow.IsUnsortedEpisodeCollection)
				{
					this.TVShowChanged(tVShow, true);
					return;
				}
				if (season.IsUnsortedEpisodeCollection)
				{
					this.SeasonChanged(season, true);
				}
			}
		}

		private void btnProcessAllEpisodes_Click(object sender, RoutedEventArgs e)
		{
			MediaScoutGUI.GUITypes.Season s = this.SelectedSeason;
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			this.TVScout = this.SetTVScout();
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				this.ProcessMultipleEpisodes(th, true, s.Episodes);
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Processing Episodes";
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private string PromptForTitleSelectionDialog(MovieXML selected)
		{
			string Title = null;
			base.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				TitleSelectionDialog titleSelectionDialog = new TitleSelectionDialog(selected);
				titleSelectionDialog.Owner = this;
				titleSelectionDialog.ShowDialog();
				Title = titleSelectionDialog.SelectedTitle;
			}));
			return Title;
		}

		private void btnProcessSelectedMovie_Click(object sender, RoutedEventArgs e)
		{
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			this.MovieScout = this.SetMovieScout();
			Movie m = this.SelectedMovie;
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				if (m.IsUnsortedFileCollection)
				{
					this.ProcessMultipleMovieFiles(th, true, m.Files);
				}
				else
				{
					DecisionType decisionType = this.ProcessingSelectedMovie(m, null, null, false);
					if (decisionType == DecisionType.Cancel)
					{
						this.MetadataCompleted(th, "Canceled.", true);
					}
					else
					{
						this.MovieChanged(m, m.IsDeleted);
					}
				}
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Processing " + m.Name;
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private DecisionType ProcessingSelectedMovie(Movie m, string Title, string Year, bool CanUserSkip)
		{
			this.Message("Processing Movie " + m.Name, MediaScoutMessage.MessageType.Task, 0);
			MainWindow.SearchResultsDecision searchResultsDecision = this.SearchForID(m, true, m.Name, null, null, CanUserSkip, Settings.Default.forceEnterSearchTerm);
			if (searchResultsDecision.Decision == DecisionType.Continue)
			{
				MovieXML movieXML = null;
				try
				{
					movieXML = this.tmdb.Get(searchResultsDecision.SelectedID, this.Language);
				}
				catch (Exception ex)
				{
					this.Message(ex.Message, MediaScoutMessage.MessageType.Error, 0);
				}
				if (movieXML != null)
				{
					if (!string.IsNullOrEmpty(movieXML.Alt_Title) && movieXML.Alt_Title != movieXML.Title)
					{
						searchResultsDecision.SelectedHasMultipleTitles = true;
					}
					if (!Settings.Default.AutoSelectMovieTitle && Title == null && searchResultsDecision.SelectedHasMultipleTitles)
					{
						Title = this.PromptForTitleSelectionDialog(movieXML);
					}
					if (Title != null)
					{
						movieXML.Title = Title;
					}
					if (Year != null)
					{
						movieXML.Year = Year;
					}
					this.MovieScout.m = movieXML;
					string text = this.MovieScout.ProcessDirectory(m.Folderpath);
					if (text != null && text.Length > 2)
					{
						if (text.Substring(0, 2) != "d:")
						{
							m.Name = text;
							m.Folderpath = new DirectoryInfo(m.Folderpath).Parent.FullName + "\\" + text;
						}
						else
						{
							m.IsDeleted = true;
						}
					}
				}
			}
			return searchResultsDecision.Decision;
		}

		private void btnProcessSelectedMovies_Click(object sender, RoutedEventArgs e)
		{
			this.ProcessMultipleMovies(this.BuildSelectedMovieList());
		}

		private void ProcessMultipleMovies(ObservableCollection<Movie> movies)
		{
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			this.MovieScout = this.SetMovieScout();
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				this.maxvalue += movies.Count;
				foreach (Movie current in movies)
				{
					if (current.IsUnsortedFileCollection)
					{
						this.ProcessMultipleMovieFiles(th, true, current.Files);
					}
					else
					{
						DecisionType decisionType = this.ProcessingSelectedMovie(current, null, null, true);
						if (decisionType == DecisionType.Skip)
						{
							this.Message("Skipped " + current.Name, MediaScoutMessage.MessageType.Task, 0);
						}
						else if (decisionType == DecisionType.Cancel)
						{
							this.MetadataCompleted(th, "Canceled.", true);
						}
						else
						{
							this.MovieChanged(current, false);
						}
					}
					this.SetTasbkBarProgressValue(++this.currentvalue);
				}
				for (int i = 0; i < movies.Count; i++)
				{
					if (movies[i].IsDeleted)
					{
						this.MovieChanged(movies[i], true);
						i--;
					}
				}
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Processing Movies";
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private void btnProcessAllMovie_Click(object sender, RoutedEventArgs e)
		{
			ProcessAllMoviesDialog pam = new ProcessAllMoviesDialog(this.movies, this.UnsortedFiles, this.Language);
			pam.Owner = this;
			base.Hide();
			pam.ShowDialog();
			base.Show();
			if (pam.DialogResult == true)
			{
				this.tcTabs.SelectedIndex = this.LogTabIndex;
				this.ShowCancelButtons();
				this.MovieScout = this.SetMovieScout();
				Thread th = null;
				th = new Thread(new ThreadStart(delegate
                {
					this.maxvalue += pam.mslist.Count;
					foreach (MoviesSearch current in pam.mslist)
					{
						if (current.Skip)
						{
							this.Message("Skipped " + current.Movie.Name, MediaScoutMessage.MessageType.Task, 0);
						}
						else
						{
							current.Movie.ID = current.SelectedMovie.ID;
							this.ProcessingSelectedMovie(current.Movie, current.SelectedMovie.Title, current.SelectedMovie.Year, false);
						}
						this.SetTasbkBarProgressValue(++this.currentvalue);
					}
					this.MetadataCompleted(th, "Done.", true);
				}));
				th.Name = "Processing All Movies";
				th.SetApartmentState(ApartmentState.STA);
				th.Start();
				this.tvThreads.Add(th);
			}
		}

		private void btnProcessSelectedMovieFile_Click(object sender, RoutedEventArgs e)
		{
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			this.MovieScout = this.SetMovieScout();
			MovieFile mf = this.SelectedMovieFile;
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				DecisionType decisionType = this.ProcessingSelectedMovieFile(mf, false);
				if (decisionType == DecisionType.Cancel)
				{
					this.MetadataCompleted(th, "Canceled.", true);
				}
				else
				{
					this.MovieFileChanged(mf, mf.IsDeleted);
				}
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Processing " + mf.Name;
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private DecisionType ProcessingSelectedMovieFile(MovieFile mf, bool CanUserSkip)
		{
			Movie movie = mf.Movie;
			this.Message("Processing File " + movie.Name, MediaScoutMessage.MessageType.Task, 0);
			string searchTerm = movie.IsUnsortedFileCollection ? movie.GetSearchTerm(mf.StrippedFileName) : null;
			string searchObjectName = movie.IsUnsortedFileCollection ? mf.Name : movie.Name;
			MainWindow.SearchResultsDecision searchResultsDecision = this.SearchForID(movie, true, searchObjectName, searchTerm, null, CanUserSkip, Settings.Default.forceEnterSearchTerm);
			if (searchResultsDecision.Decision == DecisionType.Continue)
			{
				MovieXML movieXML = null;
				try
				{
					movieXML = this.tmdb.Get(searchResultsDecision.SelectedID, this.Language);
				}
				catch (Exception ex)
				{
					this.Message(ex.Message, MediaScoutMessage.MessageType.Error, 0);
				}
				if (movieXML != null)
				{
					if (!string.IsNullOrEmpty(movieXML.LocalTitle) && movieXML.LocalTitle != movieXML.Title)
					{
						searchResultsDecision.SelectedHasMultipleTitles = true;
					}
					if (!Settings.Default.AutoSelectMovieTitle && searchResultsDecision.SelectedHasMultipleTitles)
					{
						movieXML.Title = this.PromptForTitleSelectionDialog(movieXML);
					}
					this.MovieScout.m = movieXML;
					string text = this.MovieScout.ProcessFile(movie.Folderpath, new FileInfo(mf.Filepath), !movie.IsUnsortedFileCollection, -1);
					if (text != null)
					{
						if (movie.IsUnsortedFileCollection)
						{
							mf.IsDeleted = true;
						}
						else
						{
							movie.Name = text;
							movie.Folderpath = Settings.Default.MovieFolders + "\\" + text;
						}
					}
				}
			}
			return searchResultsDecision.Decision;
		}

		private void btnProcessSelectedMovieFiles_Click(object sender, RoutedEventArgs e)
		{
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			this.MovieScout = this.SetMovieScout();
			ObservableCollection<MovieFile> Files = this.BuildSelectedMovieFileList();
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				this.ProcessMultipleMovieFiles(th, true, Files);
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Processing Movie Files";
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private void ProcessMultipleMovieFiles(Thread th, bool RemoveMovie, ObservableCollection<MovieFile> files)
		{
			Movie movie = files[0].Movie;
			this.maxvalue += files.Count;
			foreach (MovieFile current in files)
			{
				DecisionType decisionType = this.ProcessingSelectedMovieFile(current, true);
				if (decisionType == DecisionType.Skip)
				{
					this.Message("Skipped " + current.Name, MediaScoutMessage.MessageType.Task, 0);
				}
				else if (decisionType == DecisionType.Cancel)
				{
					this.MetadataCompleted(th, "Canceled.", true);
				}
				this.SetTasbkBarProgressValue(++this.currentvalue);
			}
			for (int i = 0; i < files.Count; i++)
			{
				if (files[i].IsDeleted)
				{
					this.MovieFileChanged(files[i], true);
					i--;
				}
			}
			if (RemoveMovie && movie.IsUnsortedFileCollection && movie.Files.Count == 0)
			{
				this.MovieChanged(movie, true);
			}
		}

		private void btnProcessAllMovieFiles_Click(object sender, RoutedEventArgs e)
		{
			Movie m = this.SelectedMovie;
			this.tcTabs.SelectedIndex = this.LogTabIndex;
			this.ShowCancelButtons();
			this.MovieScout = this.SetMovieScout();
			this.BuildSelectedMovieFileList();
			Thread th = null;
			th = new Thread(new ThreadStart(delegate
            {
				this.ProcessMultipleMovieFiles(th, true, m.Files);
				this.MetadataCompleted(th, "Done.", true);
			}));
			th.Name = "Processing Movie Files";
			th.SetApartmentState(ApartmentState.STA);
			th.Start();
			this.tvThreads.Add(th);
		}

		private static byte[] GetLastBytes(byte[] arr, int n)
		{
			List<byte> list = new List<byte>();
			for (int i = 0; i < n; i++)
			{
				list.Add(arr[arr.Length - 1 - i]);
			}
			return list.ToArray();
		}

		private static byte[] Invert(byte[] arr)
		{
			Array.Reverse(arr);
			return arr;
		}

		private static byte[] ComputeMD5FromFile(string fileName, int nbytes)
		{
			MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
			byte[] array = Encoding.UTF8.GetBytes(fileName);
			array = mD5CryptoServiceProvider.ComputeHash(array);
			List<byte> list = new List<byte>();
			for (int i = 0; i < nbytes; i++)
			{
				list.Add(array[i]);
			}
			return list.ToArray();
		}

		public static string ComputeSublightVideoHash(string filePath)
		{
			string result;
			try
			{
				if (!File.Exists(filePath))
				{
					result = null;
				}
				else
				{
					List<byte> list = new List<byte>(26);
					list.Insert(0, 0);
					short value = (short)VideoInfo.GetRuntime(filePath);
					if (BitConverter.IsLittleEndian)
					{
						list.InsertRange(1, MainWindow.Invert(BitConverter.GetBytes(value)));
					}
					else
					{
						list.InsertRange(1, BitConverter.GetBytes(value));
					}
					long length = new FileInfo(filePath).Length;
					if (BitConverter.IsLittleEndian)
					{
						list.InsertRange(3, MainWindow.GetLastBytes(MainWindow.Invert(BitConverter.GetBytes(length)), 6));
					}
					else
					{
						list.InsertRange(3, MainWindow.GetLastBytes(BitConverter.GetBytes(length), 6));
					}
					list.InsertRange(9, MainWindow.ComputeMD5FromFile(filePath, 5242880));
					int num = 0;
					for (int i = 0; i < 25; i++)
					{
						num += (int)list[i];
					}
					list.Insert(25, Convert.ToByte(num % 256));
					StringBuilder stringBuilder = new StringBuilder();
					for (int j = 0; j < list.Count; j++)
					{
						stringBuilder.AppendFormat("{0:x2}", list[j]);
					}
					result = stringBuilder.ToString();
				}
			}
			catch
			{
				result = null;
			}
			return result;
		}

		private string GetSublightCmdOptions(string Folderpath)
		{
			if (!string.IsNullOrEmpty(Settings.Default.SublightUsername))
			{
				string text = string.Concat(new object[]
				{
					"downloadbatch ",
					'"',
					Folderpath,
					'"'
				});
				string text2 = " \"en\" ";
				string text3 = " \"" + Settings.Default.allowedFileTypes.Replace(".", "*.") + "\" ";
				string text4 = Settings.Default.overwriteFiles ? "" : "/smartDownload ";
				string text5 = "/recursive:true";
				string text6 = (string.IsNullOrEmpty(Settings.Default.SublightUsername) ? "" : (" /username:\"" + Settings.Default.SublightUsername + "\"")) + (string.IsNullOrEmpty(Settings.Default.SublightPassword) ? "" : (" /password:\"" + Settings.Default.SublightPassword + "\""));
				return string.Concat(new string[]
				{
					text,
					text3,
					text2,
					text4,
					text5,
					text6
				});
			}
			System.Windows.MessageBox.Show("Sublight Username is not defined");
			this.btnSetSublightOptions_Click(null, null);
			return null;
		}

		private string GetSublightCmdFileOptions(string Filepath)
		{
			if (!string.IsNullOrEmpty(Settings.Default.SublightUsername))
			{
				string str = string.Concat(new object[]
				{
					"download ",
					'"',
					Filepath,
					'"'
				});
				string str2 = " \"en\"";
				string str3 = string.IsNullOrEmpty(Settings.Default.SublightUsername) ? "" : (" /username:\"" + Settings.Default.SublightUsername + "\"");
				return str + str2 + str3;
			}
			System.Windows.MessageBox.Show("Sublight Username is not defined");
			this.btnSetSublightOptions_Click(null, null);
			return null;
		}

		private ClientInfo GetSublightClientInfo()
		{
			string clientId = "MediaScout";
			string apiKey = "6FE1A80F-8874-45ED-B5FB-E979B70DD6E6";
			return new ClientInfo
			{
				ApiKey = apiKey,
				ClientId = clientId
			};
		}

		private void btnDownloadSelectedTVSubtitles_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists(Settings.Default.SublightCmd))
			{
				string sublightCmdOptions = this.GetSublightCmdOptions(this.SelectedTVShow.Folderpath);
				if (sublightCmdOptions != null)
				{
					Process.Start(Settings.Default.SublightCmd, sublightCmdOptions);
				}
			}
		}

		private void btnDownloadAllTVSubtitles_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists(Settings.Default.SublightCmd))
			{
				foreach (string current in Settings.Default.TVFolders)
				{
					string sublightCmdOptions = this.GetSublightCmdOptions(current);
					if (sublightCmdOptions != null)
					{
						Process.Start(Settings.Default.SublightCmd, sublightCmdOptions);
					}
				}
			}
		}

		private void btnDownloadSelectedSeasonSubtitles_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists(Settings.Default.SublightCmd))
			{
				string sublightCmdOptions = this.GetSublightCmdOptions(this.SelectedSeason.Folderpath);
				if (sublightCmdOptions != null)
				{
					Process.Start(Settings.Default.SublightCmd, sublightCmdOptions);
				}
			}
		}

		private void btnDownloadSelectedEpisodeSubtitles_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists(Settings.Default.SublightCmd))
			{
				string sublightCmdFileOptions = this.GetSublightCmdFileOptions(this.SelectedEpisode.Filepath);
				if (sublightCmdFileOptions != null)
				{
					Process.Start(Settings.Default.SublightCmd, sublightCmdFileOptions);
				}
			}
		}

		private void btnFindSelectedEpisodeSubtitles_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists(Settings.Default.Sublight))
			{
				string arguments = "file=\"" + this.SelectedEpisode.Filepath + "\"";
				Process.Start(Settings.Default.Sublight, arguments);
			}
		}

		private void btnDownloadSelectedMovieSubtitles_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists(Settings.Default.SublightCmd))
			{
				string sublightCmdOptions = this.GetSublightCmdOptions(this.SelectedMovie.Folderpath);
				if (sublightCmdOptions != null)
				{
					Process.Start(Settings.Default.SublightCmd, sublightCmdOptions);
				}
			}
		}

		private void btnDownloadAllMovieSubtitles_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists(Settings.Default.SublightCmd))
			{
				foreach (string current in Settings.Default.MovieFolders)
				{
					string sublightCmdOptions = this.GetSublightCmdOptions(current);
					if (sublightCmdOptions != null)
					{
						Process.Start(Settings.Default.SublightCmd, sublightCmdOptions);
					}
				}
			}
		}

		private void btnDownloadSelectedMovieFileSubtitles_Click(object sender, RoutedEventArgs e)
		{
			WcfSublightClient wcfSublightClient = new WcfSublightClient();
			wcfSublightClient.Open();
			string[] args = null;
			string[] array = null;
			string text = null;
			Guid session = wcfSublightClient.LogInAnonymous4(out array, out text, this.GetSublightClientInfo(), args);
			List<SubtitleLanguage> list = new List<SubtitleLanguage>();
			list.Add(SubtitleLanguage.English);
			global::Genre[] genres = null;
			string videoHash = MainWindow.ComputeSublightVideoHash(this.SelectedMovieFile.Filepath);
			Subtitle[] array2;
			Release[] array3;
			bool flag;
			wcfSublightClient.SearchSubtitles3(session, videoHash, this.SelectedMovie.Name, new int?(int.Parse(this.SelectedMovieFile.GetYear())), new byte?(0), new int?(0), list.ToArray(), genres, "MediaScout", new float?(0f), out array2, out array3, out flag, out text);
			string plugin = null;
			string text2;
			short num;
			wcfSublightClient.GetDownloadTicket(out text2, out num, out text, session, plugin, array2[0].SubtitleID.ToString());
		}

		private void btnFindSelectedMovieFileSubtitles_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists(Settings.Default.Sublight))
			{
				string arguments = "file=\"" + this.SelectedMovieFile.Filepath + "\"";
				Process.Start(Settings.Default.Sublight, arguments);
			}
		}

		private void TVFSWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			if (!Directory.Exists(e.FullPath))
			{
				FileInfo fileInfo = new FileInfo(e.FullPath);
				if (this.AllowedFileTypes.Contains(fileInfo.Extension) && !this.FileInUse(e.FullPath))
				{
					this.Message("Autotron : " + e.FullPath, MediaScoutMessage.MessageType.Task, 0);
					int seasonNum = -1;
					DirectoryInfo directoryInfo = fileInfo.Directory;
					if (directoryInfo.Name.Contains(Settings.Default.SeasonFolderName) || directoryInfo.Name.Contains(Settings.Default.SpecialsFolderName))
					{
						if (directoryInfo.Name == Settings.Default.SpecialsFolderName)
						{
							seasonNum = 0;
						}
						else
						{
							seasonNum = int.Parse(directoryInfo.Name.Replace(Settings.Default.SeasonFolderName, ""));
						}
						directoryInfo = directoryInfo.Parent;
					}
					string showName = (directoryInfo.FullName != Settings.Default.TVDropBoxLocation) ? directoryInfo.Name : null;
					this.StartTVShowProcess(directoryInfo, fileInfo, showName, seasonNum);
				}
			}
		}

		private bool StartTVShowProcess(DirectoryInfo SeriesFolder, FileInfo EpisodeFile, string ShowName, int SeasonNum)
		{
			string text = (ShowName != null) ? ShowName : EpisodeFile.Name.Replace(EpisodeFile.Extension, "");
			TVShow item = new TVShow(SeriesFolder.FullName, SeriesFolder.Name, false);
			MainWindow.SearchResultsDecision searchResultsDecision = this.SearchForID(item, false, text, text, null, false, ShowName == null || Settings.Default.forceEnterSearchTerm);
			if (searchResultsDecision.Decision == DecisionType.Continue)
			{
				TVShowXML tVShow = this.tvdb.GetTVShow(searchResultsDecision.SelectedID, this.Language);
				this.TVScout = this.SetTVScout();
				this.TVScout.series = tVShow;
				this.TVFSWatcher.EnableRaisingEvents = false;
				this.TVScout.ProcessEpisode(SeriesFolder.FullName, EpisodeFile, SeasonNum, ShowName != null, -1);
				this.TVFSWatcher.EnableRaisingEvents = true;
				return true;
			}
			return false;
		}

		private void MovieFSWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			if (!Directory.Exists(e.FullPath))
			{
				FileInfo fileInfo = new FileInfo(e.FullPath);
				if (this.AllowedFileTypes.Contains(fileInfo.Extension) && !this.FileInUse(e.FullPath))
				{
					this.Message("Autotron : " + e.FullPath, MediaScoutMessage.MessageType.Task, 0);
					string movieName = (fileInfo.Directory.FullName != Settings.Default.MovieDropBoxLocation) ? fileInfo.Directory.Name : null;
					this.StartMovieProcess(fileInfo.Directory, fileInfo, movieName);
					return;
				}
			}
			else
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(e.FullPath);
				if (directoryInfo.Parent.FullName == Settings.Default.MovieDropBoxLocation)
				{
					this.Message("Autotron : " + e.FullPath, MediaScoutMessage.MessageType.Task, 0);
					this.StartMovieProcess(directoryInfo, null, directoryInfo.Name);
				}
			}
		}

		private bool StartMovieProcess(DirectoryInfo MovieFolder, FileInfo MovieFile, string MovieName)
		{
			Movie movie;
			if (MovieFile == null)
			{
				movie = new Movie(MovieFolder.FullName, MovieFolder.Name, false);
			}
			else
			{
				movie = new Movie(MovieFolder.FullName, MovieFolder.Name, MovieFile);
			}
			string text = (MovieFile == null) ? null : ((MovieName != null) ? movie.GetSearchTerm() : movie.Files[0].GetSearchTerm());
			MainWindow.SearchResultsDecision searchResultsDecision = this.SearchForID(movie, true, text, text, null, false, Settings.Default.forceEnterSearchTerm);
			if (searchResultsDecision.Decision == DecisionType.Continue)
			{
				MovieXML m = this.tmdb.Get(searchResultsDecision.SelectedID, this.Language);
				this.MovieScout = this.SetMovieScout();
				this.MovieScout.m = m;
				this.MovieFSWatcher.EnableRaisingEvents = false;
				if (MovieFile != null)
				{
					this.MovieScout.ProcessFile(MovieFolder.FullName, MovieFile, MovieName != null, -1);
				}
				else
				{
					this.MovieScout.ProcessDirectory(MovieFolder.FullName);
				}
				this.MovieFSWatcher.EnableRaisingEvents = true;
				return true;
			}
			return false;
		}

		private bool FileInUse(string file)
		{
			bool result;
			try
			{
				using (new FileStream(file, FileMode.OpenOrCreate))
				{
				}
				result = false;
			}
			catch
			{
				result = true;
			}
			return result;
		}

		private void InsertHyperlink(System.Windows.Controls.RichTextBox rtb, TextPointer position)
		{
			string text = string.Empty;
			Regex regex = new Regex("(?:^|[\\s\\[\\]\\}\\{\\(\\)\\'\\\"<>])((?:(?:https?|gopher|ftp|file|irc):\\/\\/|www\\.)[a-zA-Z0-9\\.\\-=;&%\\?]+(?:\\/?[a-zA-Z0-9\\.\\-=;&%\\?]*)*)");
			while (position != null)
			{
				if (position.CompareTo(rtb.Document.ContentEnd) == 0)
				{
					return;
				}
				if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
				{
					string textInRun = position.GetTextInRun(LogicalDirection.Forward);
					int num = -1;
					if (regex.IsMatch(textInRun))
					{
						Match match = regex.Match(textInRun);
						text = match.Groups[1].Value;
						num = match.Groups[1].Index;
					}
					if (num >= 0)
					{
						position = position.GetPositionAtOffset(num);
						Hyperlink hyperlink = new Hyperlink(position, position.GetPositionAtOffset(text.Length));
						hyperlink.Tag = text;
						hyperlink.Foreground = Brushes.Blue;
						hyperlink.TextDecorations = TextDecorations.Underline;
						hyperlink.Cursor = System.Windows.Input.Cursors.Hand;
						position = position.GetPositionAtOffset(text.Length);
					}
					else
					{
						position = position.GetPositionAtOffset(textInRun.Length);
					}
				}
				else
				{
					position = position.GetNextContextPosition(LogicalDirection.Forward);
				}
			}
		}

		public void TVScout_Message(string msg, MediaScoutMessage.MessageType mt, int level)
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new MediaScoutMessage.Message(this.TVScout_Message), msg, new object[]
				{
					mt,
					level
				});
				return;
			}
			if (msg != "Thread was being aborted.")
			{
				TextRange tr = new TextRange(this.rtbLog.Document.ContentEnd, this.rtbLog.Document.ContentEnd);
				this.Display_Message(tr, msg, mt, level);
				this.rtbLog.ScrollToEnd();
				if (this.FirstTask)
				{
					this.FirstTask = false;
				}
			}
		}

		public void Display_Message(TextRange tr, string msg, MediaScoutMessage.MessageType mt, int level)
		{
			string str = new string(' ', 4 * level);
			switch (mt)
			{
			case MediaScoutMessage.MessageType.Task:
				tr.Text = (this.FirstTask ? "" : "\r") + str + msg;
				tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
				return;
			case MediaScoutMessage.MessageType.TaskResult:
				tr.Text = " : " + msg;
				tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.BlanchedAlmond);
				return;
			case MediaScoutMessage.MessageType.TaskError:
				tr.Text = " : " + msg;
				tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
				return;
			case MediaScoutMessage.MessageType.Error:
				tr.Text = "\r" + str + msg;
				tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
				return;
			default:
				return;
			}
		}

		private void btnSaveLog_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
			saveFileDialog.DefaultExt = ".txt";
			saveFileDialog.Filter = "Text documents (.txt)|*.txt";
			if (saveFileDialog.ShowDialog() == true)
			{
				TextRange textRange = new TextRange(this.rtbLog.Document.ContentStart, this.rtbLog.Document.ContentEnd);
				File.WriteAllText(saveFileDialog.FileName, textRange.Text.Replace("\r", Environment.NewLine), Encoding.UTF8);
			}
		}

		private void ShowActorImageDialog(object obj, MediaScoutGUI.GUITypes.Person Actor, string file, string file1)
		{
			Movie m = null;
			TVShow ts = null;
			if (Actor.IsMovieActor)
			{
				m = (obj as Movie);
			}
			else
			{
				ts = (obj as TVShow);
			}
			ChangeActorImageDialog caid = new ChangeActorImageDialog(Actor.IsMovieActor ? m.ID : ts.ID, Actor.Name, null, Actor.IsMovieActor);
			caid.Owner = this;
			caid.ShowDialog();
			if (caid.Decision == ImageWindowDecisionbType.DownloadAll)
			{
				this.ShowCancelButtons();
				Thread th = null;
				th = new Thread(new ThreadStart(delegate
                {
					Posters[] array = Actor.IsMovieActor ? this.tmdb.GetPersonImage(caid.SelectedActor.ID).ToArray() : null;
					this.maxvalue += array.Length;
					int num = 0;
					Posters[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						Posters arg_71_0 = array2[i];
						try
						{
							if (Settings.Default.SaveXBMCMeta)
							{
								this.Message(string.Concat(new string[]
								{
									"Saving ",
									Actor.Name,
									" Image in \\",
									caid.SelectedActor.GetXBMCDirectory(),
									" as ",
									file.Substring(file.LastIndexOf("\\") + 1)
								}), MediaScoutMessage.MessageType.Task, 0);
								caid.selected.SavePoster(file);
							}
							if (Settings.Default.SaveMyMoviesMeta)
							{
								this.Message(string.Concat(new string[]
								{
									"Saving",
									Actor.Name,
									" Image in \\ImagesByName\\",
									caid.SelectedActor.GetMyMoviesDirectory(),
									" as ",
									file1.Substring(file1.LastIndexOf("\\") + 1)
								}), MediaScoutMessage.MessageType.Task, 0);
								if (Directory.Exists(Actor.MyMoviesFolderPath))
								{
									caid.selected.SavePoster(file1);
								}
								else
								{
									this.Message("ImagesByName Location Not Defined", MediaScoutMessage.MessageType.TaskError, 0);
								}
							}
						}
						catch (Exception ex2)
						{
							this.Message(ex2.Message, MediaScoutMessage.MessageType.Error, 0);
						}
						num++;
						this.SetTasbkBarProgressValue(++this.currentvalue);
					}
					this.MetadataCompleted(th, "All Images Downloaded", true);
				}));
				th.Name = Actor.Name + " Image";
				th.SetApartmentState(ApartmentState.STA);
				th.Start();
				this.tvThreads.Add(th);
				return;
			}
			if (caid.Decision == ImageWindowDecisionbType.PosterSelected)
			{
				Thread thread = new Thread(new ThreadStart(delegate
                {
					try
					{
						if (Settings.Default.SaveXBMCMeta)
						{
							this.Message(string.Concat(new string[]
							{
								"Saving ",
								Actor.Name,
								" Image in \\",
								caid.SelectedActor.GetXBMCDirectory(),
								" as ",
								file.Substring(file.LastIndexOf("\\") + 1)
							}), MediaScoutMessage.MessageType.Task, 0);
							caid.selected.SavePoster(file);
						}
						if (Settings.Default.SaveMyMoviesMeta)
						{
							this.Message(string.Concat(new string[]
							{
								"Saving",
								Actor.Name,
								" Image in \\ImagesByName\\",
								caid.SelectedActor.GetMyMoviesDirectory(),
								" as ",
								file1.Substring(file1.LastIndexOf("\\") + 1)
							}), MediaScoutMessage.MessageType.Task, 0);
							if (Directory.Exists(Actor.MyMoviesFolderPath))
							{
								caid.selected.SavePoster(file1);
							}
							else
							{
								this.Message("ImagesByName Location Not Defined", MediaScoutMessage.MessageType.TaskError, 0);
							}
						}
						if (Actor.IsMovieActor)
						{
							m.LoadActorsThumb(Actor);
						}
						else
						{
							ts.LoadActorsThumb(Actor);
						}
						this.Message("Done.", MediaScoutMessage.MessageType.Task, 0);
					}
					catch (Exception ex2)
					{
						this.Message(ex2.Message, MediaScoutMessage.MessageType.Error, 0);
					}
				}));
				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
				return;
			}
			if (caid.Decision == ImageWindowDecisionbType.LocalPosterSelected)
			{
				string poster = caid.selectedLocalPoster.Poster;
				try
				{
					File.Copy(poster, file);
					if (Actor.IsMovieActor)
					{
						m.LoadActorsThumb(Actor);
					}
					else
					{
						ts.LoadActorsThumb(Actor);
					}
				}
				catch (Exception ex)
				{
					this.Message(ex.Message, MediaScoutMessage.MessageType.Error, 0);
				}
			}
		}

		private void ShowPosterDialog(object obj, string id, bool IsMovie, MediaScoutGUI.GUITypes.Season s, string file)
		{
			Movie m = null;
			TVShow ts = null;
			if (IsMovie)
			{
				m = (obj as Movie);
			}
			else if (s == null)
			{
				ts = (obj as TVShow);
			}
			string directory = IsMovie ? m.Folderpath : ((s == null) ? ts.Folderpath : s.Folderpath);
			ChangeImageDialog cmb = new ChangeImageDialog(directory, id, IsMovie, (s != null) ? s.GetNum() : -1, true);
			cmb.Owner = this;
			cmb.ShowDialog();
			if (cmb.Decision == ImageWindowDecisionbType.DownloadAll)
			{
				this.ShowCancelButtons();
				Thread th = null;
				th = new Thread(new ThreadStart(delegate
                {
					Posters[] array;
					if (IsMovie)
					{
						array = this.tmdb.GetPosters(id, MoviePosterType.Poster);
					}
					else
					{
						array = ((s != null) ? this.tvdb.GetPosters(id, TVShowPosterType.Season_Poster, s.GetNum()) : this.tvdb.GetPosters(id, TVShowPosterType.Poster, -1));
					}
					this.maxvalue += array.Length;
					int num = 0;
					Posters[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						Posters posters = array2[i];
						try
						{
							string text = file.Substring(0, file.LastIndexOf(".")) + num + file.Substring(file.LastIndexOf("."));
							this.Message("Saving Poster as " + text.Substring(file.LastIndexOf("\\") + 1), MediaScoutMessage.MessageType.Task, 0);
							Debug.Write("Saving Poster " + posters.Poster);
							posters.SavePoster(text);
						}
						catch (Exception ex2)
						{
							this.Message("Unable to Change Poster : " + ex2.Message, MediaScoutMessage.MessageType.Error, 0);
						}
						num++;
						this.SetTasbkBarProgressValue(++this.currentvalue);
					}
					this.MetadataCompleted(th, "All Posters Downloaded", true);
				}));
				th.Name = (IsMovie ? m.Name : ts.Name) + " Poster";
				th.SetApartmentState(ApartmentState.STA);
				th.Start();
				this.tvThreads.Add(th);
				return;
			}
			if (cmb.Decision == ImageWindowDecisionbType.PosterSelected)
			{
				if (IsMovie)
				{
					this.UpdateMoviePoster(m, MoviePosterType.Poster, true);
				}
				else if (s == null)
				{
					this.UpdateTVPoster(ts, TVShowPosterType.Poster, true);
				}
				else
				{
					this.UpdateSeasonPoster(s, TVShowPosterType.Season_Poster, true);
				}
				Thread thread = new Thread(new ThreadStart(delegate
                {
					try
					{
						this.Message("Saving Poster as " + file.Substring(file.LastIndexOf("\\") + 1), MediaScoutMessage.MessageType.Task, 0);
						Debug.Write("Saving Poster " + cmb.selected.Poster);
						cmb.selected.SavePoster(file);
						if (IsMovie)
						{
							this.UpdateMoviePoster(m, MoviePosterType.Poster, false);
						}
						else if (s == null)
						{
							this.UpdateTVPoster(ts, TVShowPosterType.Poster, false);
						}
						else
						{
							this.UpdateSeasonPoster(s, TVShowPosterType.Season_Poster, false);
						}
						this.Message("Done.", MediaScoutMessage.MessageType.Task, 0);
					}
					catch (Exception ex2)
					{
						this.Message("Unable to Change Poster : " + ex2.Message, MediaScoutMessage.MessageType.Error, 0);
					}
				}));
				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
				return;
			}
			if (cmb.Decision == ImageWindowDecisionbType.LocalPosterSelected)
			{
				string poster = cmb.selectedLocalPoster.Poster;
				try
				{
					File.Copy(poster, file);
					if (IsMovie)
					{
						this.UpdateMoviePoster(m, MoviePosterType.Poster, false);
					}
					else if (s == null)
					{
						this.UpdateTVPoster(ts, TVShowPosterType.Poster, false);
					}
					else
					{
						this.UpdateSeasonPoster(s, TVShowPosterType.Season_Poster, false);
					}
				}
				catch (Exception ex)
				{
					this.Message(ex.Message, MediaScoutMessage.MessageType.Error, 0);
				}
			}
		}

		private void ShowBackdropDialog(object obj, string id, bool IsMovie, MediaScoutGUI.GUITypes.Season s, string file, string file1)
		{
			Movie m = null;
			TVShow ts = null;
			if (IsMovie)
			{
				m = (obj as Movie);
			}
			else if (s == null)
			{
				ts = (obj as TVShow);
			}
			string directory = IsMovie ? m.Folderpath : ((s == null) ? ts.Folderpath : s.Folderpath);
			ChangeImageDialog cmb = new ChangeImageDialog(directory, id, IsMovie, (s != null) ? s.GetNum() : -1, false);
			cmb.Owner = this;
			cmb.ShowDialog();
			if (cmb.Decision == ImageWindowDecisionbType.DownloadAll)
			{
				this.ShowCancelButtons();
				Thread th = null;
				th = new Thread(new ThreadStart(delegate
                {
					Posters[] array;
					if (IsMovie)
					{
						array = this.tmdb.GetPosters(id, MoviePosterType.Backdrop);
					}
					else
					{
						array = ((s != null) ? this.tvdb.GetPosters(id, TVShowPosterType.Season_Backdrop, s.GetNum()) : this.tvdb.GetPosters(id, TVShowPosterType.Backdrop, -1));
					}
					this.maxvalue += array.Length;
					int num = 0;
					Posters[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						Posters posters = array2[i];
						try
						{
							if (Settings.Default.SaveXBMCMeta)
							{
								string text = file1.Substring(0, file1.LastIndexOf(".")) + num + file1.Substring(file1.LastIndexOf("."));
								this.Message("Saving Backdrop as " + text.Substring(text.LastIndexOf("\\") + 1), MediaScoutMessage.MessageType.Task, 0);
								Debug.Write("Saving Backdrop " + posters.Poster);
								posters.SavePoster(text);
							}
							if (Settings.Default.SaveMyMoviesMeta)
							{
								string text2 = file.Substring(0, file.LastIndexOf(".")) + num + file.Substring(file.LastIndexOf("."));
								this.Message("Saving Backdrop as " + text2.Substring(text2.LastIndexOf("\\") + 1), MediaScoutMessage.MessageType.Task, 0);
								Debug.Write("Saving Backdrop " + posters.Poster);
								posters.SavePoster(text2);
							}
						}
						catch (Exception ex2)
						{
							this.Message("Unable to Change Backdrop : " + ex2.Message, MediaScoutMessage.MessageType.Error, 0);
						}
						num++;
						this.SetTasbkBarProgressValue(++this.currentvalue);
					}
					this.MetadataCompleted(th, "All Backdrops Downloaded", true);
				}));
				th.Name = (IsMovie ? m.Name : ts.Name) + " Backdrop";
				th.SetApartmentState(ApartmentState.STA);
				th.Start();
				this.tvThreads.Add(th);
				return;
			}
			if (cmb.Decision == ImageWindowDecisionbType.PosterSelected)
			{
				if (IsMovie)
				{
					this.UpdateMoviePoster(m, MoviePosterType.Backdrop, true);
				}
				else if (s == null)
				{
					this.UpdateTVPoster(ts, TVShowPosterType.Backdrop, true);
				}
				else
				{
					this.UpdateSeasonPoster(s, TVShowPosterType.Season_Backdrop, true);
				}
				Thread thread = new Thread(new ThreadStart(delegate
                {
					try
					{
						if (Settings.Default.SaveXBMCMeta)
						{
							this.Message("Saving Backdrop as " + file1.Substring(file1.LastIndexOf("\\") + 1), MediaScoutMessage.MessageType.Task, 0);
							Debug.Write("Saving Backdrop " + cmb.selected.Poster);
							cmb.selected.SavePoster(file1);
						}
						if (Settings.Default.SaveMyMoviesMeta)
						{
							this.Message("Saving Backdrop as " + file.Substring(file.LastIndexOf("\\") + 1), MediaScoutMessage.MessageType.Task, 0);
							Debug.Write("Saving Backdrop " + cmb.selected.Poster);
							cmb.selected.SavePoster(file);
						}
						if (IsMovie)
						{
							this.UpdateMoviePoster(m, MoviePosterType.Backdrop, false);
						}
						else if (s == null)
						{
							this.UpdateTVPoster(ts, TVShowPosterType.Backdrop, false);
						}
						else
						{
							this.UpdateSeasonPoster(s, TVShowPosterType.Season_Backdrop, false);
						}
						this.Message("Done.", MediaScoutMessage.MessageType.Task, 0);
					}
					catch (Exception ex2)
					{
						this.Message("Unable to Change Backdrop : " + ex2.Message, MediaScoutMessage.MessageType.Error, 0);
					}
				}));
				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
				return;
			}
			if (cmb.Decision == ImageWindowDecisionbType.LocalPosterSelected)
			{
				try
				{
					string poster = cmb.selectedLocalPoster.Poster;
					if (Settings.Default.SaveXBMCMeta)
					{
						File.Copy(poster, file1, true);
					}
					if (Settings.Default.SaveMyMoviesMeta)
					{
						File.Copy(poster, file, true);
					}
					if (IsMovie)
					{
						this.UpdateMoviePoster(m, MoviePosterType.Backdrop, false);
					}
					else if (s == null)
					{
						this.UpdateTVPoster(ts, TVShowPosterType.Backdrop, false);
					}
					else
					{
						this.UpdateSeasonPoster(s, TVShowPosterType.Season_Backdrop, false);
					}
				}
				catch (Exception ex)
				{
					this.Message(ex.Message, MediaScoutMessage.MessageType.Error, 0);
				}
			}
		}

		private void UpdateTVPoster(TVShow ts, TVShowPosterType type, bool IsLoading)
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new MainWindow.TVShowImageChangedHandler(this.UpdateTVPoster), ts, new object[]
				{
					type,
					IsLoading
				});
				return;
			}
			switch (type)
			{
			case TVShowPosterType.Poster:
			{
				if (IsLoading)
				{
					this.imgTVPoster.Source = null;
					ts.isPosterLoading = true;
					this.imgTVPoster.SetLoading = true;
					return;
				}
				if (ts == null)
				{
					this.imgTVPoster.Source = null;
					return;
				}
				ts.isPosterLoading = false;
				BitmapImage image = ts.GetImage(type);
				this.imgTVPoster.Source = ((image == null) ? (base.FindResource("imgaddposter") as BitmapImage) : image);
				return;
			}
			case TVShowPosterType.Backdrop:
			{
				if (IsLoading)
				{
					this.imgTVBackdrop.Source = null;
					ts.isBackDropLoading = true;
					this.imgTVBackdrop.SetLoading = true;
					return;
				}
				if (ts == null)
				{
					this.imgTVBackdrop.Source = null;
					return;
				}
				ts.isBackDropLoading = false;
				BitmapImage image2 = ts.GetImage(type);
				this.imgTVBackdrop.Source = ((image2 == null) ? (base.FindResource("imgaddbackdrop") as BitmapImage) : image2);
				return;
			}
			case TVShowPosterType.Banner:
			{
				if (IsLoading)
				{
					this.imgTVBanner.Source = null;
					ts.isBannerLoading = true;
					this.imgTVBanner.SetLoading = true;
					return;
				}
				if (ts == null)
				{
					this.imgTVBanner.Source = null;
					return;
				}
				ts.isBannerLoading = false;
				BitmapImage image3 = ts.GetImage(type);
				this.imgTVBanner.Source = ((image3 == null) ? (base.FindResource("imgaddbanner") as BitmapImage) : image3);
				return;
			}
			default:
				return;
			}
		}

		private void UpdateSeasonPoster(MediaScoutGUI.GUITypes.Season s, TVShowPosterType type, bool IsLoading)
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new MainWindow.SeasonImageChangedHandler(this.UpdateSeasonPoster), s, new object[]
				{
					type,
					IsLoading
				});
				return;
			}
			switch (type)
			{
			case TVShowPosterType.Season_Poster:
				if (IsLoading)
				{
					this.imgSeasonPoster.Source = null;
					s.isPosterLoading = true;
					this.imgSeasonPoster.SetLoading = true;
					return;
				}
				s.isPosterLoading = false;
				this.imgSeasonPoster.Source = s.GetImage(type);
				return;
			case TVShowPosterType.Season_Backdrop:
				if (IsLoading)
				{
					this.imgSeasonBackdrop.Source = null;
					s.isBackDropLoading = true;
					this.imgSeasonBackdrop.SetLoading = true;
					return;
				}
				s.isBackDropLoading = false;
				this.imgSeasonBackdrop.Source = s.GetImage(type);
				return;
			default:
				return;
			}
		}

		private void UpdateEpisodePoster(Episode e, string filename, bool IsLoading)
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new MainWindow.EpisodeImageChangedHandler(this.UpdateEpisodePoster), e, new object[]
				{
					filename,
					IsLoading
				});
				return;
			}
			if (IsLoading)
			{
				this.imgEpisodePoster.SetLoading = true;
				return;
			}
			this.imgEpisodePoster.Source = e.GetImage(filename);
		}

		private void btnChangeSelectedTVPoster_Click(object sender, RoutedEventArgs e)
		{
			TVShow selectedTVShow = this.SelectedTVShow;
			this.ChangeSelectedTVorSeasonPoster(selectedTVShow, null, null, false);
		}

		private void btnChangeSelectedSeasonPoster_Click(object sender, RoutedEventArgs e)
		{
			MediaScoutGUI.GUITypes.Season selectedSeason = this.SelectedSeason;
			TVShow tVShow = selectedSeason.TVShow;
			this.ChangeSelectedTVorSeasonPoster(tVShow, selectedSeason, null, false);
		}

		private void ChangeSelectedTVorSeasonPoster(TVShow ts, MediaScoutGUI.GUITypes.Season s, string id, bool noDialog)
		{
			bool flag = s != null;
			string text = (flag ? s.Folderpath : ts.Folderpath) + "\\folder.jpg";
			if (noDialog)
			{
				if (!File.Exists(text) || Settings.Default.forceUpdate)
				{
					Posters[] posters = this.tvdb.GetPosters(id, flag ? TVShowPosterType.Season_Poster : TVShowPosterType.Poster, flag ? s.GetNum() : -1);
					if (posters != null)
					{
						try
						{
							this.Message("Saving Poster[ ( " + posters[0].Poster + ") as " + text, MediaScoutMessage.MessageType.Task, 0);
							posters[0].SavePoster(text);
							return;
						}
						catch (Exception ex)
						{
							this.Message("Unable to Change Poster : " + ex.Message, MediaScoutMessage.MessageType.Error, 0);
							return;
						}
					}
					this.Message("No Posters Found", MediaScoutMessage.MessageType.Error, 0);
					return;
				}
			}
			else
			{
				if (id == null)
				{
					id = this.SearchForID(ts, false, ts.Name, null, null, false, Settings.Default.forceEnterSearchTerm).SelectedID;
				}
				if (id != null)
				{
					this.ShowPosterDialog(ts, id, false, s, text);
				}
			}
		}

		private void btnChangeSelectedTVBackdrop_Click(object sender, RoutedEventArgs e)
		{
			TVShow selectedTVShow = this.SelectedTVShow;
			this.ChangeSelectedTVorSeasonBackdrop(selectedTVShow, null, null, false);
		}

		private void btnChangeSelectedSeasonBackdrop_Click(object sender, RoutedEventArgs e)
		{
			MediaScoutGUI.GUITypes.Season selectedSeason = this.SelectedSeason;
			TVShow tVShow = selectedSeason.TVShow;
			this.ChangeSelectedTVorSeasonBackdrop(tVShow, selectedSeason, null, false);
		}

		private void ChangeSelectedTVorSeasonBackdrop(TVShow ts, MediaScoutGUI.GUITypes.Season s, string id, bool noDialog)
		{
			bool flag = s != null;
			string text = (flag ? s.Folderpath : ts.Folderpath) + "\\backdrop.jpg";
			string text2 = (flag ? s.Folderpath : ts.Folderpath) + "\\fanart.jpg";
			if (noDialog)
			{
				if (!File.Exists(text) || Settings.Default.forceUpdate)
				{
					Posters[] posters = this.tvdb.GetPosters(id, flag ? TVShowPosterType.Season_Backdrop : TVShowPosterType.Backdrop, flag ? s.GetNum() : -1);
					if (posters != null)
					{
						try
						{
							this.Message("Saving Backdrop[ ( " + posters[0].Poster + ") as " + text, MediaScoutMessage.MessageType.Task, 0);
							posters[0].SavePoster(text);
							this.Message("Saving Backdrop[ ( " + posters[0].Poster + ") as " + text2, MediaScoutMessage.MessageType.Task, 0);
							posters[0].SavePoster(text2);
							return;
						}
						catch (Exception ex)
						{
							this.Message("Unable to Change Backdrop : " + ex.Message, MediaScoutMessage.MessageType.Error, 0);
							return;
						}
					}
					this.Message("No Backdrops Found", MediaScoutMessage.MessageType.Error, 0);
					return;
				}
			}
			else
			{
				if (id == null)
				{
					id = this.SearchForID(ts, false, ts.Name, null, null, false, Settings.Default.forceEnterSearchTerm).SelectedID;
				}
				if (id != null)
				{
					this.ShowBackdropDialog(ts, id, false, s, text, text2);
				}
			}
		}

		private void btnChangeSelectedTVBanner_Click(object sender, RoutedEventArgs e)
		{
			TVShow selectedTVShow = this.SelectedTVShow;
			this.ChangeSelectedTVBanner(selectedTVShow, null, false);
		}

		private void ChangeSelectedTVBanner(TVShow ts, string id, bool noDialog)
		{
			if (id == null)
			{
				id = this.SearchForID(ts, false, ts.Name, null, null, false, Settings.Default.forceEnterSearchTerm).SelectedID;
			}
			if (id != null)
			{
				if (noDialog)
				{
					string text = ts.Folderpath + "\\banner.jpg";
					if (!File.Exists(text) || Settings.Default.forceUpdate)
					{
						Posters[] posters = this.tvdb.GetPosters(id, TVShowPosterType.Banner, -1);
						if (posters != null)
						{
							this.Message("Saving Banner ( " + posters[0].Poster + ") as " + text, MediaScoutMessage.MessageType.Task, 0);
							try
							{
								posters[0].SavePoster(text);
								return;
							}
							catch (Exception ex)
							{
								this.Message("Unable to Change Banner : " + ex.Message, MediaScoutMessage.MessageType.Error, 0);
								return;
							}
						}
						this.Message("No Banners Found", MediaScoutMessage.MessageType.Error, 0);
						return;
					}
				}
				else
				{
					ChangeTVBanner ctb = new ChangeTVBanner(id, this.tvdb);
					ctb.Owner = this;
					ctb.ShowDialog();
					if (ctb.DownloadAll)
					{
						Thread thread = new Thread(new ThreadStart(delegate
                        {
							Posters[] posters2 = this.tvdb.GetPosters(id, TVShowPosterType.Banner, -1);
							this.maxvalue += posters2.Length;
							int num = 0;
							Posters[] array = posters2;
							for (int i = 0; i < array.Length; i++)
							{
								Posters posters3 = array[i];
								string text2 = string.Concat(new object[]
								{
									ts.Folderpath,
									"\\banner",
									num,
									".jpg"
								});
								this.Message("Saving Banner (" + posters3.Poster + ") as " + text2, MediaScoutMessage.MessageType.Task, 0);
								try
								{
									posters3.SavePoster(text2);
								}
								catch (Exception ex2)
								{
									this.Message("Unable to Change Banner : " + ex2.Message, MediaScoutMessage.MessageType.Error, 0);
								}
								num++;
								this.SetTasbkBarProgressValue(++this.currentvalue);
							}
							this.Message("All Banners Downloaded", MediaScoutMessage.MessageType.Task, 0);
						}));
						thread.SetApartmentState(ApartmentState.STA);
						thread.Start();
						return;
					}
					if (ctb.selected != null)
					{
						this.UpdateTVPoster(ts, TVShowPosterType.Banner, true);
						Thread thread2 = new Thread(new ThreadStart(delegate
                        {
							string text2 = ts.Folderpath + "\\banner.jpg";
							this.Message("Saving Banner (" + ctb.selected.Poster + ") as " + text2, MediaScoutMessage.MessageType.Task, 0);
							try
							{
								ctb.selected.SavePoster(text2);
								this.UpdateTVPoster(ts, TVShowPosterType.Banner, false);
								this.Message("Done.", MediaScoutMessage.MessageType.Task, 0);
							}
							catch (Exception ex2)
							{
								this.Message("Unable to Change Backdrop : " + ex2.Message, MediaScoutMessage.MessageType.Error, 0);
							}
						}));
						thread2.SetApartmentState(ApartmentState.STA);
						thread2.Start();
					}
				}
			}
		}

		private void btnEpisodePosterFromFile_Click(object sender, RoutedEventArgs e)
		{
			Episode selectedEpisode = this.SelectedEpisode;
			if (Settings.Default.SaveXBMCMeta)
			{
				FileInfo fileInfo = new FileInfo(selectedEpisode.Filepath);
				string text = fileInfo.Name.Replace(fileInfo.Extension, "") + ".tbn";
				this.Message("Saving Episode Poster as " + text, MediaScoutMessage.MessageType.Task, 0);
				try
				{
					VideoInfo.SaveThumb(selectedEpisode.Filepath, selectedEpisode.Season.MetadataFolderPath + text, 0.25);
					this.Message("Done.", MediaScoutMessage.MessageType.Task, 0);
					this.UpdateEpisodePoster(selectedEpisode, text, false);
				}
				catch (Exception ex)
				{
					this.Message(ex.Message, MediaScoutMessage.MessageType.TaskError, 0);
				}
			}
			if (Settings.Default.SaveMyMoviesMeta)
			{
				if (!Directory.Exists(selectedEpisode.Season.MetadataFolderPath))
				{
					IOFunctions.CreateHiddenFolder(selectedEpisode.Season.MetadataFolderPath);
				}
				FileInfo fileInfo2 = new FileInfo(selectedEpisode.Filepath);
				string text = ((selectedEpisode.ID != null) ? selectedEpisode.ID : fileInfo2.Name.Replace(fileInfo2.Extension, "")) + ".jpg";
				this.Message("Saving Episode Poster as " + text, MediaScoutMessage.MessageType.Task, 0);
				try
				{
					VideoInfo.SaveThumb(selectedEpisode.Filepath, selectedEpisode.Season.MetadataFolderPath + text, 0.25);
					this.Message("Done.", MediaScoutMessage.MessageType.Task, 0);
					this.UpdateEpisodePoster(selectedEpisode, text, false);
				}
				catch (Exception ex2)
				{
					this.Message(ex2.Message, MediaScoutMessage.MessageType.TaskError, 0);
				}
			}
		}

		private void UpdateMoviePoster(Movie m, MoviePosterType type, bool IsLoading)
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Send, new MainWindow.MovieImageChangedHandler(this.UpdateMoviePoster), m, new object[]
				{
					type,
					IsLoading
				});
				return;
			}
			switch (type)
			{
			case MoviePosterType.Poster:
			{
				if (IsLoading)
				{
					this.imgMoviePoster.Source = null;
					m.isPosterLoading = true;
					this.imgMoviePoster.SetLoading = true;
					return;
				}
				if (m == null)
				{
					this.imgMoviePoster.Source = null;
					return;
				}
				m.isPosterLoading = false;
				BitmapImage image = m.GetImage(type);
				this.imgMoviePoster.Source = ((image == null) ? (base.FindResource("imgaddposter") as BitmapImage) : image);
				return;
			}
			case MoviePosterType.Backdrop:
			{
				if (IsLoading)
				{
					this.imgMovieBackdrop.Source = null;
					m.isBackDropLoading = true;
					this.imgMovieBackdrop.SetLoading = true;
					return;
				}
				if (m == null)
				{
					this.imgMovieBackdrop.Source = null;
					return;
				}
				m.isBackDropLoading = false;
				BitmapImage image2 = m.GetImage(type);
				this.imgMovieBackdrop.Source = ((image2 == null) ? (base.FindResource("imgaddbackdrop") as BitmapImage) : image2);
				return;
			}
			default:
				return;
			}
		}

		private void UpdateMovieFilePoster(MovieFile mf, MoviePosterType type, bool IsLoading)
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Send, new MainWindow.MovieFileImageChangedHandler(this.UpdateMovieFilePoster), mf, new object[]
				{
					type,
					IsLoading
				});
				return;
			}
			switch (type)
			{
			case MoviePosterType.File_Poster:
				if (IsLoading)
				{
					this.imgMovieFilePoster.Source = null;
					mf.isPosterLoading = true;
					this.imgMovieFilePoster.SetLoading = true;
					return;
				}
				mf.isPosterLoading = false;
				this.imgMovieFilePoster.Source = mf.GetImage(type);
				return;
			case MoviePosterType.File_Backdrop:
				if (IsLoading)
				{
					this.imgMovieFileBackdrop.Source = null;
					mf.isBackDropLoading = true;
					this.imgMovieFileBackdrop.SetLoading = true;
					return;
				}
				mf.isBackDropLoading = false;
				this.imgMovieFileBackdrop.Source = mf.GetImage(type);
				return;
			default:
				return;
			}
		}

		private void btnChangeMoviePoster_Click(object sender, RoutedEventArgs e)
		{
			Movie selectedMovie = this.SelectedMovie;
			this.ChangeSelectedMoviePoster(selectedMovie, null, null, false);
		}

		private void btnChangeMovieFilePoster_Click(object sender, RoutedEventArgs e)
		{
			MovieFile selectedMovieFile = this.SelectedMovieFile;
			Movie movie = selectedMovieFile.Movie;
			this.ChangeSelectedMoviePoster(movie, null, null, false);
		}

		private void ChangeSelectedMoviePoster(Movie m, MovieFile mf, string id, bool noDialog)
		{
			string text = m.Folderpath + ((mf != null) ? (mf.StrippedFileName + ".tbn") : "\\folder.jpg");
			if (noDialog)
			{
				if (!File.Exists(text) || Settings.Default.forceUpdate)
				{
					Posters[] posters = this.tmdb.GetPosters(id, MoviePosterType.Poster);
					if (posters != null)
					{
						this.Message("Saving Poster ( " + posters[0].Poster + ") as " + text, MediaScoutMessage.MessageType.Task, 0);
						try
						{
							posters[0].SavePoster(text);
							return;
						}
						catch (Exception ex)
						{
							this.Message("Unable to Change Poster : " + ex.Message, MediaScoutMessage.MessageType.Error, 0);
							return;
						}
					}
					this.Message("No Posters found", MediaScoutMessage.MessageType.Error, 0);
					return;
				}
			}
			else
			{
				if (id == null)
				{
					id = this.SearchForID(m, true, m.Name, null, null, false, Settings.Default.forceEnterSearchTerm).SelectedID;
				}
				if (id != null)
				{
					this.ShowPosterDialog(m, id, true, null, text);
				}
			}
		}

		private void btnChangeMovieBackdrop_Click(object sender, RoutedEventArgs e)
		{
			Movie selectedMovie = this.SelectedMovie;
			this.ChangeSelectedMovieBackdrop(selectedMovie, null, selectedMovie.ID, false);
		}

		private void btnChangeMovieFileBackdrop_Click(object sender, RoutedEventArgs e)
		{
			MovieFile selectedMovieFile = this.SelectedMovieFile;
			Movie movie = selectedMovieFile.Movie;
			this.ChangeSelectedMovieBackdrop(movie, selectedMovieFile, movie.ID, false);
		}

		private void ChangeSelectedMovieBackdrop(Movie m, MovieFile mf, string id, bool noDialog)
		{
			string text = m.Folderpath + "\\backdrop.jpg";
			string file = m.Folderpath + ((mf != null) ? (mf.StrippedFileName + ".jpg") : "\\fanart.jpg");
			if (noDialog)
			{
				if (!File.Exists(text) || Settings.Default.forceUpdate)
				{
					Posters[] posters = this.tmdb.GetPosters(id, MoviePosterType.Backdrop);
					if (posters != null)
					{
						this.Message("Saving Backdrop ( " + posters[0].Poster + ") as " + text, MediaScoutMessage.MessageType.Task, 0);
						try
						{
							posters[0].SavePoster(text);
							return;
						}
						catch (Exception ex)
						{
							this.Message("Unable to Change Backdrop : " + ex.Message, MediaScoutMessage.MessageType.Error, 0);
							return;
						}
					}
					this.Message("No Backdrops found", MediaScoutMessage.MessageType.Error, 0);
					return;
				}
			}
			else
			{
				if (id == null)
				{
					id = this.SearchForID(m, true, m.Name, null, null, false, Settings.Default.forceEnterSearchTerm).SelectedID;
				}
				if (id != null)
				{
					this.ShowBackdropDialog(m, id, true, null, text, file);
				}
			}
		}

		private void btnChangeActorThumb_Click(object sender, RoutedEventArgs e)
		{
			MediaScoutGUI.GUITypes.Person selectedPerson = this.SelectedPerson;
			string file = selectedPerson.XBMCFolderPath + "\\" + selectedPerson.Name.Replace(" ", "_") + ".jpg";
			string file2 = selectedPerson.MyMoviesFolderPath + "\\" + selectedPerson.Name.Replace(" ", "_") + "\\folder.jpg";
			if (!Directory.Exists(selectedPerson.XBMCFolderPath))
			{
				IOFunctions.CreateHiddenFolder(selectedPerson.XBMCFolderPath);
			}
			if (!Directory.Exists(selectedPerson.MyMoviesFolderPath + "\\" + selectedPerson.Name.Replace(" ", "_")))
			{
				Directory.CreateDirectory(selectedPerson.MyMoviesFolderPath + "\\" + selectedPerson.Name.Replace(" ", "_"));
			}
			if (selectedPerson.IsMovieActor)
			{
				this.ShowActorImageDialog(this.SelectedMovie, selectedPerson, file, file2);
				return;
			}
			this.ShowActorImageDialog(this.SelectedTVShow, selectedPerson, file, file2);
		}

		private void btnSetTVFolders_Click(object sender, RoutedEventArgs e)
		{
			FoldersDialog foldersDialog = new FoldersDialog(false);
			foldersDialog.Owner = this;
			if (foldersDialog.ShowDialog() == true)
			{
				if (Settings.Default.TVFolders == null)
				{
					Settings.Default.TVFolders = new StringCollection();
				}
				else
				{
					Settings.Default.TVFolders.Clear();
				}
				foreach (string value in ((IEnumerable)foldersDialog.lstFolders.Items))
				{
					Settings.Default.TVFolders.Add(value);
				}
				Settings.Default.Save();
				this.resetTVfolder = true;
			}
		}

		private void ChangeMonitorTVFolder()
		{
			if (Directory.Exists(this.txtTVDropBox.Text))
			{
				this.chkTVFSWatcher.IsEnabled = true;
			}
		}

		private void TVDropBoxFolderChanged()
		{
			if (this.WindowLoaded && Settings.Default.TVDropBoxLocation != this.txtTVDropBox.Text)
			{
				Settings.Default.TVDropBoxLocation = this.txtTVDropBox.Text;
				Settings.Default.Save();
				this.ChangeMonitorTVFolder();
			}
		}

		private void txtTVDropBox_LostFocus(object sender, RoutedEventArgs e)
		{
			this.TVDropBoxFolderChanged();
		}

		private void btnBrowserTVDropBox_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.Description = "Select TV Shows DropBox folder";
			if (Directory.Exists(this.txtTVDropBox.Text))
			{
				folderBrowserDialog.SelectedPath = this.txtTVDropBox.Text;
			}
			if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				this.txtTVDropBox.Text = folderBrowserDialog.SelectedPath;
				this.TVDropBoxFolderChanged();
			}
		}

		private void chkTVFSWatcher_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				if (this.TVFSWatcher == null)
				{
					this.TVFSWatcher = new FileSystemWatcher();
					this.TVFSWatcher.Path = Settings.Default.TVDropBoxLocation;
					this.TVFSWatcher.IncludeSubdirectories = true;
					this.TVFSWatcher.NotifyFilter = (NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite);
					this.TVFSWatcher.Created += new FileSystemEventHandler(this.TVFSWatcher_Changed);
					this.TVFSWatcher.Changed += new FileSystemEventHandler(this.TVFSWatcher_Changed);
				}
				this.TVFSWatcher.EnableRaisingEvents = this.chkTVFSWatcher.IsChecked.Value;
				Settings.Default.TVFSWatcher = this.chkTVFSWatcher.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkTVFSWatcher_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkTVFSWatcher_ValueChanged(sender, e);
		}

		private void chkTVFSWatcher_Checked(object sender, RoutedEventArgs e)
		{
			this.chkTVFSWatcher_ValueChanged(sender, e);
		}

		private void chkSeriesPosters_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.getSeriesPosters = this.chkSeriesPosters.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkSeriesPosters_Checked(object sender, RoutedEventArgs e)
		{
			this.chkSeriesPosters_ValueChanged(sender, e);
		}

		private void chkSeriesPosters_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkSeriesPosters_ValueChanged(sender, e);
		}

		private void chkSeasonPosters_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.getSeasonPosters = this.chkSeasonPosters.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkSeasonPosters_Checked(object sender, RoutedEventArgs e)
		{
			this.chkSeasonPosters_ValueChanged(sender, e);
		}

		private void chkSeasonPosters_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkSeasonPosters_ValueChanged(sender, e);
		}

		private void chkEpPosters_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.getEpisodePosters = this.chkEpPosters.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkEpPosters_Checked(object sender, RoutedEventArgs e)
		{
			this.chkEpPosters_ValueChanged(sender, e);
		}

		private void chkEpPosters_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkEpPosters_ValueChanged(sender, e);
		}

		private void chkMoveTVFiles_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.moveTVFiles = this.chkMoveTVFiles.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkMoveTVFiles_Checked(object sender, RoutedEventArgs e)
		{
			this.chkMoveTVFiles_ValueChanged(sender, e);
		}

		private void chkMoveTVFiles_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkMoveTVFiles_ValueChanged(sender, e);
		}

		private void chkRenameTVFiles_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.renameTVFiles = this.chkRenameTVFiles.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkRenameTVFiles_Checked(object sender, RoutedEventArgs e)
		{
			this.chkRenameTVFiles_ValueChanged(sender, e);
		}

		private void chkRenameTVFiles_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkRenameTVFiles_ValueChanged(sender, e);
		}

		private void chkSaveTVActors_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.SaveTVActors = this.chkSaveTVActors.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkSaveTVActors_Checked(object sender, RoutedEventArgs e)
		{
			this.chkSaveTVActors_ValueChanged(sender, e);
		}

		private void chkSaveTVActors_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkSaveTVActors_ValueChanged(sender, e);
		}

		private void txtSeasonFolderName_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.SeasonFolderName = this.txtSeasonFolderName.Text;
				Settings.Default.Save();
			}
		}

		private void txtSpecialsFolderName_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.SpecialsFolderName = this.txtSpecialsFolderName.Text;
				Settings.Default.Save();
			}
		}

		private void chkdownloadAllTVImages_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				this.chkdownloadAllTVBackdrops.IsChecked = new bool?(this.chkdownloadAllTVImages.IsChecked.Value);
				this.chkdownloadAllTVBanners.IsChecked = new bool?(this.chkdownloadAllTVImages.IsChecked.Value);
				this.chkdownloadAllTVPosters.IsChecked = new bool?(this.chkdownloadAllTVImages.IsChecked.Value);
				this.chkdownloadAllTVSeasonPosters.IsChecked = new bool?(this.chkdownloadAllTVImages.IsChecked.Value);
				this.chkdownloadAllTVSeasonBackdrops.IsChecked = new bool?(this.chkdownloadAllTVImages.IsChecked.Value);
				Settings.Default.downloadAllTVImages = this.chkdownloadAllTVImages.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkdownloadAllTVImages_Checked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllTVImages_ValueChanged(sender, e);
		}

		private void chkdownloadAllTVImages_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllTVImages_ValueChanged(sender, e);
		}

		private void chkdownloadAllTVPosters_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.downloadAllTVPosters = this.chkdownloadAllTVPosters.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkdownloadAllTVPosters_Checked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllTVPosters_ValueChanged(sender, e);
		}

		private void chkdownloadAllTVPosters_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllTVPosters_ValueChanged(sender, e);
		}

		private void chkdownloadAllTVBackdrops_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.downloadAllTVBackdrops = this.chkdownloadAllTVBackdrops.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkdownloadAllTVBackdrops_Checked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllTVBackdrops_ValueChanged(sender, e);
		}

		private void chkdownloadAllTVBackdrops_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllTVBackdrops_ValueChanged(sender, e);
		}

		private void chkdownloadAllTVBanners_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.downloadAllTVBanners = this.chkdownloadAllTVBanners.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkdownloadAllTVBanners_Checked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllTVBanners_ValueChanged(sender, e);
		}

		private void chkdownloadAllTVBanners_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllTVBanners_ValueChanged(sender, e);
		}

		private void chkdownloadAllTVSeasonPosters_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.downloadAllTVSeasonPosters = this.chkdownloadAllTVSeasonPosters.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkdownloadAllTVSeasonPosters_Checked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllTVSeasonPosters_ValueChanged(sender, e);
		}

		private void chkdownloadAllTVSeasonPosters_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllTVSeasonPosters_ValueChanged(sender, e);
		}

		private void chkdownloadAllTVSeasonBackdrops_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.downloadAllTVSeasonBackdrops = this.chkdownloadAllTVSeasonBackdrops.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkdownloadAllTVSeasonBackdrops_Checked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllTVSeasonBackdrops_ValueChanged(sender, e);
		}

		private void chkdownloadAllTVSeasonBackdrops_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllTVSeasonBackdrops_ValueChanged(sender, e);
		}

		private void txtTVRenameFormat_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.TVfileformat = this.txtTVRenameFormat.Text;
				Settings.Default.Save();
			}
		}

		private void txtSeasonNumZeroPadding_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.SeasonNumZeroPadding = this.txtSeasonNumZeroPadding.Text;
				Settings.Default.Save();
			}
		}

		private void txtEpisodeNumZeroPadding_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.EpisodeNumZeroPadding = this.txtEpisodeNumZeroPadding.Text;
				Settings.Default.Save();
			}
		}

		private void txtTVRenameFormat_TextChanged(object sender, TextChangedEventArgs e)
		{
			this.setTVRenameExample();
		}

		private void txtSeasonNumZeroPad_TextChanged(object sender, TextChangedEventArgs e)
		{
			this.setTVRenameExample();
		}

		private void txtEpisodeNumZeroPad_TextChanged(object sender, TextChangedEventArgs e)
		{
			this.setTVRenameExample();
		}

		private void setTVRenameExample()
		{
			try
			{
				string text = this.makeEpisodeTargetName("Series Name", "1", "Episode Name", "3", "Suffix", ".ext");
				if (text.Length > 0)
				{
					this.lblTVRenameExample.Content = text;
				}
				else
				{
					this.lblTVRenameExample.Content = "invalid";
				}
			}
			catch
			{
			}
		}

		private string makeEpisodeTargetName(string SeriesName, string SeasonNum, string EpisodeName, string EpisodeNum, string Suffix, string FileExtention)
		{
			string result;
			try
			{
				string text = Regex.Replace(this.txtTVRenameFormat.Text, "([^\\}]*?\\{4\\}[^\\{]*)", "");
				text = string.Format(text + FileExtention, new object[]
				{
					SeriesName,
					SeasonNum.PadLeft(int.Parse(this.txtSeasonNumZeroPadding.Text), '0'),
					EpisodeName,
					EpisodeNum.PadLeft(int.Parse(this.txtEpisodeNumZeroPadding.Text), '0')
				});
				result = text;
			}
			catch
			{
				result = null;
			}
			return result;
		}

		private void btnSetMovieFolders_Click(object sender, RoutedEventArgs e)
		{
			FoldersDialog foldersDialog = new FoldersDialog(true);
			foldersDialog.Owner = this;
			if (foldersDialog.ShowDialog() == true)
			{
				if (Settings.Default.MovieFolders == null)
				{
					Settings.Default.MovieFolders = new StringCollection();
				}
				else
				{
					Settings.Default.MovieFolders.Clear();
				}
				foreach (string value in ((IEnumerable)foldersDialog.lstFolders.Items))
				{
					Settings.Default.MovieFolders.Add(value);
				}
				Settings.Default.Save();
				this.resetMoviefolder = true;
			}
		}

		private void ChangeMonitorMovieFolder()
		{
			if (Directory.Exists(this.txtMovieDropBox.Text))
			{
				this.chkMovieFSWatcher.IsEnabled = true;
			}
		}

		private void MovieDropBoxFolderChanged()
		{
			if (this.WindowLoaded && Settings.Default.MovieDropBoxLocation != this.txtMovieDropBox.Text)
			{
				Settings.Default.MovieDropBoxLocation = this.txtMovieDropBox.Text;
				Settings.Default.Save();
				this.ChangeMonitorMovieFolder();
			}
		}

		private void txtMovieDropBox_LostFocus(object sender, RoutedEventArgs e)
		{
			this.MovieDropBoxFolderChanged();
		}

		private void btnBrowserMovieDropBox_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.Description = "Select Movies DropBox folder";
			if (Directory.Exists(this.txtMovieDropBox.Text))
			{
				folderBrowserDialog.SelectedPath = this.txtMovieDropBox.Text;
			}
			if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				this.txtMovieDropBox.Text = folderBrowserDialog.SelectedPath;
				this.MovieDropBoxFolderChanged();
			}
		}

		private void chkMovieFSWatcher_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				if (this.MovieFSWatcher == null)
				{
					this.MovieFSWatcher = new FileSystemWatcher();
					this.MovieFSWatcher.Path = Settings.Default.MovieDropBoxLocation;
					this.MovieFSWatcher.IncludeSubdirectories = true;
					this.MovieFSWatcher.NotifyFilter = (NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite);
					this.MovieFSWatcher.Created += new FileSystemEventHandler(this.MovieFSWatcher_Changed);
					this.MovieFSWatcher.Changed += new FileSystemEventHandler(this.MovieFSWatcher_Changed);
				}
				this.MovieFSWatcher.EnableRaisingEvents = this.chkMovieFSWatcher.IsChecked.Value;
				Settings.Default.MovieFSWatcher = this.chkMovieFSWatcher.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkMovieFSWatcher_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkMovieFSWatcher_ValueChanged(sender, e);
		}

		private void chkMovieFSWatcher_Checked(object sender, RoutedEventArgs e)
		{
			this.chkMovieFSWatcher_ValueChanged(sender, e);
		}

		private void chkMoviesPosters_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.getMoviePosters = this.chkMoviePosters.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkMoviesPosters_Checked(object sender, RoutedEventArgs e)
		{
			this.chkMoviesPosters_ValueChanged(sender, e);
		}

		private void chkMoviesPosters_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkMoviesPosters_ValueChanged(sender, e);
		}

		private void chkMovieFilePosters_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.getMovieFilePosters = this.chkMovieFilePosters.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkMovieFilePosters_Checked(object sender, RoutedEventArgs e)
		{
			this.chkMovieFilePosters_ValueChanged(sender, e);
		}

		private void chkMovieFilePosters_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkMovieFilePosters_ValueChanged(sender, e);
		}

		private void chkMoveMovieFiles_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.moveTVFiles = this.chkMoveMovieFiles.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkMoveMovieFiles_Checked(object sender, RoutedEventArgs e)
		{
			this.chkMoveMovieFiles_ValueChanged(sender, e);
		}

		private void chkMoveMovieFiles_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkMoveMovieFiles_ValueChanged(sender, e);
		}

		private void chkSaveMovieActors_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.SaveMovieActors = this.chkSaveMovieActors.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkSaveMovieActors_Checked(object sender, RoutedEventArgs e)
		{
			this.chkSaveMovieActors_ValueChanged(sender, e);
		}

		private void chkSaveMovieActors_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkSaveMovieActors_ValueChanged(sender, e);
		}

		private void chkRenameMovieFiles_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.renameMovieFiles = this.chkRenameMovieFiles.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkRenameMovieFiles_Checked(object sender, RoutedEventArgs e)
		{
			this.chkRenameMovieFiles_ValueChanged(sender, e);
		}

		private void chkRenameMovieFiles_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkRenameMovieFiles_ValueChanged(sender, e);
		}

		private void chkdownloadAllMovieImages_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				this.chkdownloadAllMovieBackdrops.IsChecked = this.chkdownloadAllMovieImages.IsChecked;
				this.chkdownloadAllMoviePosters.IsChecked = this.chkdownloadAllMovieImages.IsChecked;
				Settings.Default.downloadAllMovieImages = this.chkdownloadAllMovieImages.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkdownloadAllMovieImages_Checked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllMovieImages_ValueChanged(sender, e);
		}

		private void chkdownloadAllMovieImages_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllMovieImages_ValueChanged(sender, e);
		}

		private void chkdownloadAllMoviePosters_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.downloadAllMoviePosters = this.chkdownloadAllMoviePosters.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkdownloadAllMoviePosters_Checked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllMoviePosters_ValueChanged(sender, e);
		}

		private void chkdownloadAllMoviePosters_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllMoviePosters_ValueChanged(sender, e);
		}

		private void chkdownloadAllMovieBackdrops_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.downloadAllMovieBackdrops = this.chkdownloadAllMovieBackdrops.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkdownloadAllMovieBackdrops_Checked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllMovieBackdrops_ValueChanged(sender, e);
		}

		private void chkdownloadAllMovieBackdrops_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkdownloadAllMovieBackdrops_ValueChanged(sender, e);
		}

		private void txtMovieFileRenameFormat_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.Moviefileformat = this.txtMovieFileRenameFormat.Text;
				Settings.Default.Save();
			}
		}

		private void txtMovieDirRenameFormat_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.MovieDirformat = this.txtMovieDirRenameFormat.Text;
				Settings.Default.Save();
			}
		}

		private void txtMovieFileRenameFormat_TextChanged(object sender, TextChangedEventArgs e)
		{
			this.setMovieFileRenameExample();
		}

		private void setMovieFileRenameExample()
		{
			try
			{
				string text = this.makeMovieTargetName(this.txtMovieFileRenameFormat.Text, "Movie Name", "2009", ".ext");
				if (text.Length > 0)
				{
					this.lblMovieFileRenameExample.Content = text;
				}
				else
				{
					this.lblMovieFileRenameExample.Content = "invalid";
				}
			}
			catch
			{
			}
		}

		private void txtMovieDirRenameFormat_TextChanged(object sender, TextChangedEventArgs e)
		{
			this.setMovieDirRenameExample();
		}

		private void setMovieDirRenameExample()
		{
			try
			{
				string text = this.makeMovieTargetName(this.txtMovieDirRenameFormat.Text, "Movie Name", "2009", "");
				if (text.Length > 0)
				{
					this.lblMovieDirRenameExample.Content = text;
				}
				else
				{
					this.lblMovieDirRenameExample.Content = "invalid";
				}
			}
			catch
			{
			}
		}

		private string makeMovieTargetName(string format, string MovieName, string Year, string FileExtention)
		{
			string result;
			try
			{
				string text = Regex.Replace(format, "([^\\}]*?\\{4\\}[^\\{]*)", "");
				text = string.Format(text + FileExtention, MovieName, Year);
				result = text;
			}
			catch
			{
				result = null;
			}
			return result;
		}

		private void chkForceUpdate_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.forceUpdate = this.chkForceUpdate.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkForceUpdate_Checked(object sender, RoutedEventArgs e)
		{
			this.chkForceUpdate_ValueChanged(sender, e);
		}

		private void chkForceUpdate_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkForceUpdate_ValueChanged(sender, e);
		}

		private void txtAllowedFiletypes_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded && Settings.Default.allowedFileTypes != this.txtAllowedFiletypes.Text)
			{
				Settings.Default.allowedFileTypes = this.txtAllowedFiletypes.Text;
				Settings.Default.Save();
				this.AllowedFileTypes = new List<string>(Settings.Default.allowedFileTypes.Split(new char[]
				{
					';'
				}));
				this.resetTVfolder = true;
				this.resetMoviefolder = true;
			}
		}

		private void txtAllowedSubtitles_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.allowedSubtitles = this.txtAllowedSubtitles.Text;
				Settings.Default.Save();
				this.AllowedSubtitleTypes = new List<string>(Settings.Default.allowedSubtitles.Split(new char[]
				{
					';'
				}));
			}
		}

		private void txtSearchTermFilters_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.SearchTermFilters = this.txtSearchTermFilters.Text;
				Settings.Default.Save();
			}
		}

		private void chkEnableGlassFrame_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				this.SetGlassFrame(this.chkEnableGlassFrame.IsChecked.Value);
				Settings.Default.EnableGlassFrame = this.chkEnableGlassFrame.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkEnableGlassFrame_Checked(object sender, RoutedEventArgs e)
		{
			this.chkEnableGlassFrame_ValueChanged(sender, e);
		}

		private void chkEnableGlassFrame_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkEnableGlassFrame_ValueChanged(sender, e);
		}

		private void chkSaveXBMCMeta_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.SaveXBMCMeta = this.chkSaveXBMCMeta.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkSaveXBMCMeta_Checked(object sender, RoutedEventArgs e)
		{
			this.chkSaveXBMCMeta_ValueChanged(sender, e);
		}

		private void chkSaveXBMCMeta_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkSaveXBMCMeta_ValueChanged(sender, e);
		}

		private void chkSaveMMMeta_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.SaveMyMoviesMeta = this.chkSaveMMMeta.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkSaveMMMeta_Checked(object sender, RoutedEventArgs e)
		{
			this.chkSaveMMMeta_ValueChanged(sender, e);
		}

		private void chkSaveMMMeta_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkSaveMMMeta_ValueChanged(sender, e);
		}

		private void btnSetSublightOptions_Click(object sender, RoutedEventArgs e)
		{
			SublightOptionsDialog sublightOptionsDialog = new SublightOptionsDialog();
			sublightOptionsDialog.Owner = this;
			if (sublightOptionsDialog.ShowDialog() == true)
			{
				Settings.Default.SublightUsername = sublightOptionsDialog.txtSublightUsername.Text;
				Settings.Default.SublightPassword = sublightOptionsDialog.txtSublightPassword.Text;
				Settings.Default.SublightCmd = sublightOptionsDialog.txtSublightCmd.Text;
				Settings.Default.Sublight = sublightOptionsDialog.txtSublight.Text;
				Settings.Default.Save();
			}
		}

		private void chkSilentMode_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				this.chkAutoSelectMatch.IsChecked = new bool?(this.chkSilentMode.IsChecked.Value);
				this.chkAutoSelectMovieTitle.IsChecked = new bool?(this.chkSilentMode.IsChecked.Value);
				this.chkOverwrite.IsChecked = new bool?(this.chkSilentMode.IsChecked.Value);
				this.chkforceEnterSearchTerm.IsChecked = new bool?(!this.chkSilentMode.IsChecked.Value);
				Settings.Default.SilentMode = this.chkSilentMode.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkSilentMode_Checked(object sender, RoutedEventArgs e)
		{
			this.chkSilentMode_ValueChanged(sender, e);
		}

		private void chkSilentMode_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkSilentMode_ValueChanged(sender, e);
		}

		private void chkAutoSelectMatch_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.AutoSelectMatch = this.chkAutoSelectMatch.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkAutoSelectMatch_Checked(object sender, RoutedEventArgs e)
		{
			this.chkAutoSelectMatch_ValueChanged(sender, e);
		}

		private void chkAutoSelectMatch_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkAutoSelectMatch_ValueChanged(sender, e);
		}

		private void chkAutoSelectMovieTitle_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.AutoSelectMovieTitle = this.chkAutoSelectMovieTitle.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkAutoSelectMovieTitle_Checked(object sender, RoutedEventArgs e)
		{
			this.chkAutoSelectMovieTitle_ValueChanged(sender, e);
		}

		private void chkAutoSelectMovieTitle_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkAutoSelectMovieTitle_ValueChanged(sender, e);
		}

		private void chkOverwrite_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.overwriteFiles = this.chkOverwrite.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkOverwrite_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkOverwrite_ValueChanged(sender, e);
		}

		private void chkOverwrite_Checked(object sender, RoutedEventArgs e)
		{
			this.chkOverwrite_ValueChanged(sender, e);
		}

		private void chkforceEnterSearchTerm_ValueChanged(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.forceEnterSearchTerm = this.chkforceEnterSearchTerm.IsChecked.Value;
				Settings.Default.Save();
			}
		}

		private void chkforceEnterSearchTerm_Checked(object sender, RoutedEventArgs e)
		{
			this.chkforceEnterSearchTerm_ValueChanged(sender, e);
		}

		private void chkforceEnterSearchTerm_Unchecked(object sender, RoutedEventArgs e)
		{
			this.chkforceEnterSearchTerm_ValueChanged(sender, e);
		}

		private void btnBrowseIBN_Click(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
				folderBrowserDialog.Description = "Select ImagesByName Folder";
				if (Directory.Exists(this.txtImagesByName.Text))
				{
					folderBrowserDialog.SelectedPath = this.txtImagesByName.Text;
				}
				if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					this.txtImagesByName.Text = folderBrowserDialog.SelectedPath;
					if (Settings.Default.ImagesByNameLocation != this.txtImagesByName.Text)
					{
						Settings.Default.ImagesByNameLocation = this.txtImagesByName.Text;
						Settings.Default.Save();
					}
				}
			}
		}

		private void txtImagesByName_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded && Settings.Default.ImagesByNameLocation != this.txtImagesByName.Text)
			{
				Settings.Default.ImagesByNameLocation = this.txtImagesByName.Text;
				Settings.Default.Save();
			}
		}

		private void txtFilenameReplaceChar_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.WindowLoaded)
			{
				Settings.Default.FilenameReplaceChar = this.txtFilenameReplaceChar.Text;
				Settings.Default.Save();
			}
		}

		private void lstLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.WindowRendered)
			{
				Settings.Default.language = this.lstLanguages.SelectedIndex;
				Settings.Default.Save();
			}
		}

		private void btnEmptyCache_Click(object sender, RoutedEventArgs e)
		{
			string messageBoxText = "Are you sure you want to clear the cache?";
			if (System.Windows.MessageBox.Show(messageBoxText, "MediaScout", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				this.Message("Directory : " + this.CacheDir, MediaScoutMessage.MessageType.Task, 0);
				DirectoryInfo directoryInfo = new DirectoryInfo(this.CacheDir);
				DirectoryInfo[] directories = directoryInfo.GetDirectories();
				for (int i = 0; i < directories.Length; i++)
				{
					DirectoryInfo directoryInfo2 = directories[i];
					this.StripDir(directoryInfo2.Name, directoryInfo2.FullName, 1);
				}
				this.Message("Done.", MediaScoutMessage.MessageType.Task, 0);
			}
		}

		private void btnOpenCache_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("explorer.exe", "/select," + this.CacheDir);
		}

		private void btnAbout_Click(object sender, RoutedEventArgs e)
		{
			new About(false)
			{
				Owner = this
			}.ShowDialog();
		}

		private void btnReset_Click(object sender, RoutedEventArgs e)
		{
			if (System.Windows.MessageBox.Show(this, "Are You sure you want to reset all settings?", "MediaScout", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
			{
				StringCollection tVFolders = Settings.Default.TVFolders;
				StringCollection movieFolders = Settings.Default.MovieFolders;
				Settings.Default.Reset();
				Settings.Default.TVFolders = tVFolders;
				Settings.Default.MovieFolders = movieFolders;
				this.LoadOptions();
				this.Load_Post_Rendering_Options();
			}
		}

		private void btnOpenTVDb_Click(object sender, RoutedEventArgs e)
		{
			if (this.SelectedTVShow.ID != null)
			{
				Process.Start("http://thetvdb.com/?tab=series&id=" + this.SelectedTVShow.ID + "&lid=7");
				return;
			}
			Process.Start("http://thetvdb.com/?string=" + this.SelectedTVShow.Name + "&searchseriesid=&tab=listseries&function=Search");
		}

		private void btnOpenTMDb_Click(object sender, RoutedEventArgs e)
		{
			if (this.SelectedMovie.ID != null)
			{
				Process.Start("http://www.themoviedb.org/movie/" + this.SelectedMovie.ID);
			}
		}

		private void btnSaveTVShow_Click(object sender, RoutedEventArgs e)
		{
			if (Settings.Default.SaveMyMoviesMeta)
			{
				this.SelectedTVShow.XMLBase.SaveXML(this.SelectedTVShow.Folderpath);
			}
			if (Settings.Default.SaveXBMCMeta)
			{
				this.SelectedTVShow.NFOBase.Save(this.SelectedTVShow.NFOFile);
			}
			this.SelectedTVShow.MetadataChanged = false;
		}

		private void btnSaveMovie_Click(object sender, RoutedEventArgs e)
		{
			if (Settings.Default.SaveMyMoviesMeta)
			{
				this.SelectedMovie.XMLBase.SaveXML(this.SelectedMovie.Folderpath);
			}
			if (Settings.Default.SaveXBMCMeta)
			{
				this.SelectedMovie.NFOBase.Save(this.SelectedMovie.NFOFile);
			}
			this.SelectedMovie.MetadataChanged = false;
		}

		private void btnSaveEpisode_Click(object sender, RoutedEventArgs e)
		{
			if (Settings.Default.SaveMyMoviesMeta)
			{
				this.SelectedEpisode.XMLBase.SaveXML(this.SelectedEpisode.Season.Folderpath, this.SelectedEpisode.StrippedFileName);
			}
			if (Settings.Default.SaveXBMCMeta)
			{
				this.SelectedEpisode.NFOBase.Save(this.SelectedEpisode.NFOFile);
			}
			this.SelectedEpisode.MetadataChanged = false;
		}

		public bool TVShowSearchContains(object tvshow)
		{
			TVShow tVShow = tvshow as TVShow;
			return tVShow.Name.LastIndexOf(this.txtSearchTVShow.Text, StringComparison.CurrentCultureIgnoreCase) != -1 || (tVShow.Year != null && tVShow.Year.LastIndexOf(this.txtSearchTVShow.Text) != -1) || (tVShow.Genre != null && tVShow.Genre.Contains(new MediaScout.Genre
			{
				name = this.txtSearchTVShow.Text
			}));
		}

		private void txtSearchTVShow_Search(object sender, RoutedEventArgs e)
		{
			CollectionView collectionView = CollectionViewSource.GetDefaultView(this.tvTVShows.ItemsSource) as CollectionView;
			CollectionView expr_17 = collectionView;
			expr_17.Filter = (Predicate<object>)Delegate.Combine(expr_17.Filter, new Predicate<object>(this.TVShowSearchContains));
		}

		public bool MovieSearchContains(object movie)
		{
			Movie movie2 = movie as Movie;
			return movie2.Name.LastIndexOf(this.txtSearchMovie.Text, StringComparison.CurrentCultureIgnoreCase) != -1 || (movie2.Year != null && movie2.Year.LastIndexOf(this.txtSearchMovie.Text) != -1) || (movie2.Genre != null && movie2.Genre.Contains(new MediaScout.Genre
			{
				name = this.txtSearchMovie.Text
			}));
		}

		private void txtSearchMovie_Search(object sender, RoutedEventArgs e)
		{
			CollectionView collectionView = CollectionViewSource.GetDefaultView(this.lbMovies.ItemsSource) as CollectionView;
			CollectionView expr_17 = collectionView;
			expr_17.Filter = (Predicate<object>)Delegate.Combine(expr_17.Filter, new Predicate<object>(this.MovieSearchContains));
		}

		private void button1_Click(object sender, RoutedEventArgs e)
		{
			string text = "MediaScout";
			char arg_15_0 = text[text.Length];
		}

		[DebuggerNonUserCode]
		internal Delegate _CreateDelegate(Type delegateType, string handler)
		{
			return Delegate.CreateDelegate(delegateType, this, handler);
		}

		[EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
		void IStyleConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 69:
				((TextBlock)target).MouseDown += new MouseButtonEventHandler(this.TVShowItem_DoubleClick);
				return;
			case 70:
				((TextBlock)target).MouseDown += new MouseButtonEventHandler(this.TVShowItem_DoubleClick);
				return;
			case 71:
				((TextBlock)target).MouseDown += new MouseButtonEventHandler(this.TVShowItem_DoubleClick);
				return;
			case 72:
				((Grid)target).MouseDown += new MouseButtonEventHandler(this.SeasonItem_DoubleClick);
				return;
			case 73:
				((Grid)target).MouseDown += new MouseButtonEventHandler(this.SeasonItem_DoubleClick);
				return;
			case 74:
				((Grid)target).MouseDown += new MouseButtonEventHandler(this.SeasonItem_DoubleClick);
				return;
			case 75:
				((TextBlock)target).MouseDown += new MouseButtonEventHandler(this.MovieItem_DoubleClick);
				return;
			case 76:
				((TextBlock)target).MouseDown += new MouseButtonEventHandler(this.MovieItem_DoubleClick);
				return;
			case 77:
				((TextBlock)target).MouseDown += new MouseButtonEventHandler(this.MovieItem_DoubleClick);
				return;
			default:
				return;
			}
		}
	}
}
