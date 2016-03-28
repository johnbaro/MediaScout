using MediaScout;
using MediaScout.Providers;
using MediaScoutGUI.Controls;
using MediaScoutGUI.GUITypes;
using MediaScoutGUI.Properties;
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using System.Windows.Threading;
using MediaScout.GUI.Controls;


namespace MediaScoutGUI
{
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public partial class ProcessAllMoviesDialog : Window, IComponentConnector, IStyleConnector
	{
		public delegate void LoadingCompletedHandler();

		public ObservableCollection<Movie> movies = new ObservableCollection<Movie>();

		public ObservableCollection<MoviesSearch> mslist = new ObservableCollection<MoviesSearch>();

		private DispatchingCollection<ObservableCollection<MoviesSearch>, MoviesSearch> dispatchmslist;

		private Thread LoadingThread;

		private int currentvalue;

		private int maxvalue;

		private TheMovieDBProvider tmdb;

		private new string Language;

		public ProcessAllMoviesDialog(ObservableCollection<Movie> movies, Movie UnsortedFiles, string language)
		{
			this.InitializeComponent();
			this.movies = movies;
			this.Language = language;
			this.maxvalue += movies.Count;
			this.tmdb = new TheMovieDBProvider(null);
			this.dispatchmslist = new DispatchingCollection<ObservableCollection<MoviesSearch>, MoviesSearch>(this.mslist, base.Dispatcher);
		}

		private void SetTasbkBarProgressValue(int value)
		{
			if (value != this.maxvalue)
			{
				if (base.TaskbarItemInfo.ProgressState != TaskbarItemProgressState.Normal)
				{
					base.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
				}
				base.TaskbarItemInfo.ProgressValue = (double)value / (double)this.maxvalue;
				return;
			}
			base.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
		}

		public void AbortThread()
		{
			if (this.LoadingThread != null)
			{
				this.LoadingThread.Abort();
				this.LoadingThread = null;
			}
		}

		private void UpdateProgress()
		{
			base.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				this.txtLoading.Text = string.Concat(new object[]
				{
					"Loading (",
					this.currentvalue,
					"/",
					this.maxvalue,
					")"
				});
				base.TaskbarItemInfo.ProgressValue = (double)this.currentvalue;
			}));
		}

		private void LoadingCompleted()
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new ProcessAllMoviesDialog.LoadingCompletedHandler(this.LoadingCompleted));
				return;
			}
			if (this.mslist.Count > 0)
			{
				this.btnProcess.Visibility = Visibility.Visible;
			}
			this.btnStop.Visibility = Visibility.Collapsed;
			this.gdLoading.Visibility = Visibility.Collapsed;
			this.dataGrid1.IsEnabled = true;
		}

		private void LoadMovies()
		{
            this.LoadingThread = new Thread(new ThreadStart(delegate
            {
                foreach (Movie current in this.movies)
                {
                    if (!current.IsUnsortedFileCollection)
                    {
                        this.mslist.Add(new MoviesSearch(current, this.tmdb.Search(current.GetSearchTerm(), current.GetYear(), this.Language)));
                        this.currentvalue++;
                    }
                    else
                    {
                        this.currentvalue++;
                    }
                    this.UpdateProgress();
                }
                this.LoadingCompleted();
            }));
			this.LoadingThread.SetApartmentState(ApartmentState.STA);
			this.LoadingThread.Start();
		}

		private void btnProcess_Click(object sender, RoutedEventArgs e)
		{
			base.DialogResult = new bool?(true);
		}

		private void btnStop_Click(object sender, RoutedEventArgs e)
		{
			this.AbortThread();
			this.LoadingCompleted();
		}

		private void _tbCancelButton_Click(object sender, EventArgs e)
		{
			this.AbortThread();
			this.LoadingCompleted();
		}

		private void _tbPauseButton_Click(object sender, EventArgs e)
		{
			if (this._tbPauseButton.Description == "Pause")
			{
				this._tbPauseButton.Description = "Play";
				this._tbPauseButton.ImageSource = (BitmapImage)base.Resources["PlayImage"];
				this.LoadingThread.Suspend();
				base.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Paused;
				return;
			}
			this._tbPauseButton.Description = "Pause";
			this._tbPauseButton.ImageSource = (BitmapImage)base.Resources["PauseImage"];
			this.LoadingThread.Resume();
			base.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (Settings.Default.EnableGlassFrame)
			{
				this.GetBoundsForGlassFrame();
				HwndSource hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
				hwndSource.AddHook(new HwndSourceHook(this.WndProc));
				this.SetGlassFrame(true);
			}
			this.LoadMovies();
			this.dataGrid1.ItemsSource = this.dispatchmslist;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			this.AbortThread();
		}

		public Rect GetBoundsForGlassFrame()
		{
			return VisualTreeHelper.GetContentBounds(this.GridLayout);
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

		private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			Binding binding = new Binding("Movie.SearchTerm");
			binding.Mode = BindingMode.TwoWay;
			SearchTextBox searchTextBox = sender as SearchTextBox;
			searchTextBox.SetBinding(TextBox.TextProperty, binding);
		}

		private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			Binding binding = new Binding("Movie.Name");
			SearchTextBox searchTextBox = sender as SearchTextBox;
			searchTextBox.SetBinding(TextBox.TextProperty, binding);
		}

		private void SearchTextBox_Search(object sender, RoutedEventArgs e)
		{
			SearchTextBox searchTextBox = sender as SearchTextBox;
			MoviesSearch moviesSearch = searchTextBox.Tag as MoviesSearch;
			moviesSearch.SearchResults = this.tmdb.Search(searchTextBox.Text, moviesSearch.Movie.Year, this.Language);
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBox comboBox = sender as ComboBox;
			MoviesSearch moviesSearch = comboBox.Tag as MoviesSearch;
			moviesSearch.SelectedMovie = (comboBox.SelectedItem as MovieXML);
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox textBox = sender as TextBox;
			MoviesSearch moviesSearch = textBox.Tag as MoviesSearch;
			moviesSearch.SetAttentionAndSkip();
		}

		private void ComboBox_LostFocus(object sender, RoutedEventArgs e)
		{
			ComboBox comboBox = sender as ComboBox;
			MoviesSearch moviesSearch = comboBox.Tag as MoviesSearch;
			moviesSearch.SetAttentionAndSkip();
		}

		[EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
		void IStyleConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 6:
				((ComboBox)target).SelectionChanged += new SelectionChangedEventHandler(this.ComboBox_SelectionChanged);
				return;
			case 7:
				((ComboBox)target).LostFocus += new RoutedEventHandler(this.ComboBox_LostFocus);
				return;
			case 8:
				((TextBox)target).TextChanged += new TextChangedEventHandler(this.TextBox_TextChanged);
				return;
			default:
				return;
			}
		}
	}
}
