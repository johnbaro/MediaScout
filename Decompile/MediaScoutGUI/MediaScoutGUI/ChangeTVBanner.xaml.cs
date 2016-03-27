using MediaScout;
using MediaScout.Providers;
using MediaScoutGUI.Properties;
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
    public partial class ChangeTVBanner : Window, IComponentConnector, IStyleConnector
	{
		private delegate void PostersLoadingHandler();

		private Posters[] posters;

		private ObservableCollection<Posters> Posters = new ObservableCollection<Posters>();

		private DispatchingCollection<ObservableCollection<Posters>, Posters> dispatchPosters;

		private TheTVDBProvider tvdb;

		private string TVShowID;

		public bool DownloadAll;

		public Posters selected;

		internal Grid LayoutRoot;

		internal ListBox lbPosters;

		internal Button btnSelectBanner;

		internal Button btnCancel;

		internal Button btnDownloadAll;

		internal Label lblNoPosters;

		private bool _contentLoaded;

		public ChangeTVBanner(string tvshowid, TheTVDBProvider tvdb)
		{
			this.InitializeComponent();
			this.TVShowID = tvshowid;
			this.tvdb = tvdb;
			this.dispatchPosters = new DispatchingCollection<ObservableCollection<Posters>, Posters>(this.Posters, base.Dispatcher);
		}

		private void lbPosters_Loaded(object sender, RoutedEventArgs e)
		{
			this.LoadPosters();
			this.lbPosters.ItemsSource = this.dispatchPosters;
		}

		public void LoadPosters()
		{
			Thread thread = new Thread(new ThreadStart(delegate
			{
				this.posters = this.tvdb.GetPosters(this.TVShowID, TVShowPosterType.Banner, -1);
				this.AddPosters();
			}));
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
		}

		private void AddPosters()
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new ChangeTVBanner.PostersLoadingHandler(this.AddPosters));
				return;
			}
			if (this.posters.Length == 0)
			{
				this.lblNoPosters.Content = "No Banners found";
			}
			else
			{
				this.lblNoPosters.Visibility = Visibility.Collapsed;
			}
			Posters[] array = this.posters;
			for (int i = 0; i < array.Length; i++)
			{
				Posters item = array[i];
				this.Posters.Add(item);
			}
		}

		private void btnSelectBanner_Click(object sender, RoutedEventArgs e)
		{
			this.selected = (Posters)this.lbPosters.SelectedItem;
			base.DialogResult = new bool?(true);
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			base.DialogResult = new bool?(false);
		}

		private void btnDownloadAll_Click(object sender, RoutedEventArgs e)
		{
			this.DownloadAll = true;
			base.DialogResult = new bool?(false);
		}

		private void HandleLinkClick(object sender, RoutedEventArgs e)
		{
			Hyperlink hyperlink = (Hyperlink)sender;
			string fileName = hyperlink.NavigateUri.ToString();
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

		[DebuggerNonUserCode]
		public void InitializeComponent()
		{
			if (this._contentLoaded)
			{
				return;
			}
			this._contentLoaded = true;
			Uri resourceLocator = new Uri("/MediaScoutGUI/ChangeTVBanner.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}

		[EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				((ChangeTVBanner)target).Loaded += new RoutedEventHandler(this.Window_Loaded);
				return;
			case 3:
				this.LayoutRoot = (Grid)target;
				return;
			case 4:
				this.lbPosters = (ListBox)target;
				this.lbPosters.Loaded += new RoutedEventHandler(this.lbPosters_Loaded);
				return;
			case 5:
				this.btnSelectBanner = (Button)target;
				this.btnSelectBanner.Click += new RoutedEventHandler(this.btnSelectBanner_Click);
				return;
			case 6:
				this.btnCancel = (Button)target;
				this.btnCancel.Click += new RoutedEventHandler(this.btnCancel_Click);
				return;
			case 7:
				this.btnDownloadAll = (Button)target;
				this.btnDownloadAll.Click += new RoutedEventHandler(this.btnDownloadAll_Click);
				return;
			case 8:
				this.lblNoPosters = (Label)target;
				return;
			}
			this._contentLoaded = true;
		}

		[EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
		void IStyleConnector.Connect(int connectionId, object target)
		{
			if (connectionId != 2)
			{
				return;
			}
			((Hyperlink)target).RequestNavigate += new RequestNavigateEventHandler(this.HandleLinkClick);
		}
	}
}
