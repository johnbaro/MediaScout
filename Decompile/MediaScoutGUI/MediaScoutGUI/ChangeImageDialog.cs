using MediaScout;
using MediaScout.Providers;
using MediaScoutGUI.Properties;
using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace MediaScoutGUI
{
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public class ChangeImageDialog : Window, IComponentConnector, IStyleConnector
	{
		private delegate void PostersLoadingHandler();

		private Posters[] posters;

		private ObservableCollection<LocalPosters> localPosters = new ObservableCollection<LocalPosters>();

		private ObservableCollection<Posters> Posters = new ObservableCollection<Posters>();

		private DispatchingCollection<ObservableCollection<Posters>, Posters> dispatchPosters;

		private TheMovieDBProvider tmdb;

		private TheTVDBProvider tvdb;

		private string ID;

		private string Folderpath;

		private bool IsMovie;

		private int SeasonNum = -1;

		private bool IsPosterDialog = true;

		public Posters selected;

		public LocalPosters selectedLocalPoster;

		public ImageWindowDecisionbType Decision = ImageWindowDecisionbType.Cancel;

		private bool jump = true;

		internal Grid LayoutRoot;

		internal ListBox lbPosters;

		internal Button btnSelectPoster;

		internal Button btnCancel;

		internal Button btnDownloadAll;

		internal Label lblNoPosters;

		internal ListBox lblocalPosters;

		internal TextBlock lbNoLocals;

		private bool _contentLoaded;

		public ChangeImageDialog(string directory, string id, bool IsMovie, int SeasonNum, bool IsPosterDialog)
		{
			this.InitializeComponent();
			this.Folderpath = directory;
			this.ID = id;
			this.IsMovie = IsMovie;
			this.IsPosterDialog = IsPosterDialog;
			this.lblocalPosters.ItemTemplate = (this.lbPosters.ItemTemplate = (DataTemplate)(IsPosterDialog ? base.Resources["dtPosters"] : base.Resources["dtBackdrops"]));
			this.btnSelectPoster.Content = "Select " + (IsPosterDialog ? "Poster" : "Backdrop");
			base.Title = "Select " + (IsPosterDialog ? "Poster" : "Backdrop");
			if (IsMovie)
			{
				this.tmdb = new TheMovieDBProvider(null);
			}
			else
			{
				this.SeasonNum = SeasonNum;
				this.tvdb = new TheTVDBProvider(null);
			}
			this.dispatchPosters = new DispatchingCollection<ObservableCollection<Posters>, Posters>(this.Posters, base.Dispatcher);
		}

		public void LoadPosters()
		{
			Thread thread = new Thread(delegate
			{
				if (this.IsMovie)
				{
					this.posters = this.tmdb.GetPosters(this.ID, this.IsPosterDialog ? MoviePosterType.Poster : MoviePosterType.Backdrop);
				}
				else
				{
					this.posters = ((this.SeasonNum != -1) ? this.tvdb.GetPosters(this.ID, this.IsPosterDialog ? TVShowPosterType.Season_Poster : TVShowPosterType.Season_Backdrop, this.SeasonNum) : this.tvdb.GetPosters(this.ID, this.IsPosterDialog ? TVShowPosterType.Poster : TVShowPosterType.Backdrop, -1));
				}
				this.AddPosters();
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
		}

		public void LoadLocalPosters(string FilePrefix)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(this.Folderpath);
			FileInfo[] files = directoryInfo.GetFiles();
			FileInfo fi;
			for (int i = 0; i < files.Length; i++)
			{
				fi = files[i];
				if (Regex.Match(fi.Name, FilePrefix + "([0-9])*\\.jpg").Success)
				{
					try
					{
						base.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(delegate
						{
							this.localPosters.Add(new LocalPosters(fi.FullName));
						}));
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);
					}
				}
			}
			this.AddLocalPosters();
		}

		private void AddPosters()
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new ChangeImageDialog.PostersLoadingHandler(this.AddPosters));
				return;
			}
			if (this.posters == null || this.posters.Length == 0)
			{
				this.lblNoPosters.Content = "No Posters found";
				return;
			}
			if (!this.btnDownloadAll.IsEnabled)
			{
				this.btnDownloadAll.IsEnabled = true;
			}
			if (!this.btnSelectPoster.IsEnabled)
			{
				this.btnSelectPoster.IsEnabled = true;
			}
			this.lblNoPosters.Visibility = Visibility.Collapsed;
			Posters[] array = this.posters;
			for (int i = 0; i < array.Length; i++)
			{
				Posters item = array[i];
				this.Posters.Add(item);
			}
		}

		private void AddLocalPosters()
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new ChangeImageDialog.PostersLoadingHandler(this.AddLocalPosters));
				return;
			}
			if (this.localPosters.Count == 0)
			{
				this.lbNoLocals.Visibility = Visibility.Visible;
				this.lbNoLocals.Text = "No Local Posters found. Right Click to add images";
				return;
			}
			if (!this.btnSelectPoster.IsEnabled)
			{
				this.btnSelectPoster.IsEnabled = true;
			}
			this.lbNoLocals.Visibility = Visibility.Collapsed;
		}

		private void btnSelectPoster_Click(object sender, RoutedEventArgs e)
		{
			if (this.lbPosters.SelectedIndex != -1)
			{
				this.Decision = ImageWindowDecisionbType.PosterSelected;
				this.selected = (Posters)this.lbPosters.SelectedItem;
			}
			else
			{
				this.Decision = ImageWindowDecisionbType.LocalPosterSelected;
				this.selectedLocalPoster = (LocalPosters)this.lblocalPosters.SelectedItem;
			}
			base.DialogResult = new bool?(true);
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.Decision = ImageWindowDecisionbType.Cancel;
			base.DialogResult = new bool?(false);
		}

		private void btnDownloadAll_Click(object sender, RoutedEventArgs e)
		{
			this.Decision = ImageWindowDecisionbType.DownloadAll;
			base.DialogResult = new bool?(true);
		}

		private void HandleLinkClick(object sender, RequestNavigateEventArgs e)
		{
			string fileName = e.Uri.ToString();
			Process.Start(new ProcessStartInfo(fileName));
			e.Handled = true;
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
			if (this.IsPosterDialog)
			{
				this.LoadLocalPosters("folder");
			}
			else
			{
				this.LoadLocalPosters("fanart");
				this.LoadLocalPosters("backdrop");
			}
			this.lblocalPosters.ItemsSource = this.localPosters;
			this.LoadPosters();
			this.lbPosters.ItemsSource = this.dispatchPosters;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			this.localPosters.Clear();
		}

		public Rect GetBoundsForGlassFrame()
		{
			return VisualTreeHelper.GetContentBounds(this.lbPosters);
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

		private void lbPosters_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.lblocalPosters.SelectedIndex != -1 && this.jump)
			{
				this.jump = false;
				this.lblocalPosters.UnselectAll();
				return;
			}
			if (!this.jump)
			{
				this.jump = true;
			}
		}

		private void lblocalPosters_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.lbPosters.SelectedIndex != -1 && this.jump)
			{
				this.jump = false;
				this.lbPosters.UnselectAll();
				return;
			}
			if (!this.jump)
			{
				this.jump = true;
			}
		}

		private void btnDeleteAllPoster_Click(object sender, RoutedEventArgs e)
		{
			foreach (LocalPosters current in this.localPosters)
			{
				this.DeleteSelectedPoster(current, false);
			}
			this.localPosters.Clear();
			this.AddLocalPosters();
		}

		private void DeleteSelectedPoster(LocalPosters lp, bool remove)
		{
			string poster = lp.Poster;
			if (File.Exists(poster))
			{
				File.Delete(poster);
			}
			if (remove)
			{
				int index = this.localPosters.IndexOf(lp);
				this.localPosters.RemoveAt(index);
				if (this.localPosters.Count == 0)
				{
					this.AddLocalPosters();
				}
			}
		}

		private void btnDeletePoster_Click(object sender, RoutedEventArgs e)
		{
			this.DeleteSelectedPoster(this.lblocalPosters.SelectedItem as LocalPosters, true);
		}

		private void btnAddPoster_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.CheckFileExists = true;
			openFileDialog.CheckPathExists = true;
			openFileDialog.Filter = "Image Files|*.jpg;*.tbn";
			if (openFileDialog.ShowDialog(this) == true)
			{
				string[] fileNames = openFileDialog.FileNames;
				string Filename;
				for (int i = 0; i < fileNames.Length; i++)
				{
					Filename = fileNames[i];
					try
					{
						base.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(delegate
						{
							this.localPosters.Add(new LocalPosters(Filename));
							this.AddLocalPosters();
						}));
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);
					}
				}
			}
		}

		[DebuggerNonUserCode]
		public void InitializeComponent()
		{
			if (this._contentLoaded)
			{
				return;
			}
			this._contentLoaded = true;
			Uri resourceLocator = new Uri("/MediaScoutGUI;component/changeimagedialog.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}

		[EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				((ChangeImageDialog)target).Loaded += new RoutedEventHandler(this.Window_Loaded);
				((ChangeImageDialog)target).Closed += new EventHandler(this.Window_Closed);
				return;
			case 2:
				((MenuItem)target).Click += new RoutedEventHandler(this.btnAddPoster_Click);
				return;
			case 3:
				((MenuItem)target).Click += new RoutedEventHandler(this.btnDeleteAllPoster_Click);
				return;
			case 4:
				((MenuItem)target).Click += new RoutedEventHandler(this.btnDeletePoster_Click);
				return;
			case 7:
				this.LayoutRoot = (Grid)target;
				return;
			case 8:
				this.lbPosters = (ListBox)target;
				this.lbPosters.SelectionChanged += new SelectionChangedEventHandler(this.lbPosters_SelectionChanged);
				return;
			case 9:
				this.btnSelectPoster = (Button)target;
				this.btnSelectPoster.Click += new RoutedEventHandler(this.btnSelectPoster_Click);
				return;
			case 10:
				this.btnCancel = (Button)target;
				this.btnCancel.Click += new RoutedEventHandler(this.btnCancel_Click);
				return;
			case 11:
				this.btnDownloadAll = (Button)target;
				this.btnDownloadAll.Click += new RoutedEventHandler(this.btnDownloadAll_Click);
				return;
			case 12:
				this.lblNoPosters = (Label)target;
				return;
			case 13:
				this.lblocalPosters = (ListBox)target;
				this.lblocalPosters.SelectionChanged += new SelectionChangedEventHandler(this.lblocalPosters_SelectionChanged);
				return;
			case 14:
				this.lbNoLocals = (TextBlock)target;
				return;
			}
			this._contentLoaded = true;
		}

		[EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
		void IStyleConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 5:
				((Hyperlink)target).RequestNavigate += new RequestNavigateEventHandler(this.HandleLinkClick);
				return;
			case 6:
				((Hyperlink)target).RequestNavigate += new RequestNavigateEventHandler(this.HandleLinkClick);
				return;
			default:
				return;
			}
		}
	}
}
