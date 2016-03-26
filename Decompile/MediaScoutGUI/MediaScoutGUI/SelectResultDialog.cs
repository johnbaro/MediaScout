using MediaScout;
using MediaScoutGUI.Properties;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;

namespace MediaScoutGUI
{
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public class SelectResultDialog : Window, IComponentConnector
	{
		private DecisionType _decision = DecisionType.Cancel;

		public object Selected;

		internal SelectResultDialog Window;

		internal Grid LayoutRoot;

		internal ListBox lstMovies;

		internal Label lbTitle;

		internal Button btnCancel;

		internal Button btnSelect;

		internal Button btnSearchAgain;

		internal Button btnSkip;

		internal Label label1;

		internal Label lbLoading;

		internal Label lblSearchTerm;

		private bool _contentLoaded;

		public DecisionType Decision
		{
			get
			{
				return this._decision;
			}
			set
			{
				this._decision = value;
			}
		}

		public SelectResultDialog(string SearchObjectName, string SearchTerm, object[] results, bool IsMovieSelection, bool CanUserSkip)
		{
			this.InitializeComponent();
			if (results != null && results.Length > 0)
			{
				this.lbTitle.Content = SearchObjectName;
				this.lblSearchTerm.Content = SearchTerm;
				this.lstMovies.ItemTemplate = (base.FindResource("movieTemplate") as DataTemplate);
				this.lstMovies.ItemsSource = results;
				this.lstMovies.SelectedIndex = 0;
			}
			if (CanUserSkip)
			{
				this.btnSkip.Visibility = Visibility.Visible;
				this._decision = DecisionType.Skip;
			}
		}

		private void btnSearchAgain_Click(object sender, RoutedEventArgs e)
		{
			this._decision = DecisionType.SearchAgain;
			base.DialogResult = new bool?(false);
		}

		private void btnSelect_Click(object sender, RoutedEventArgs e)
		{
			this._decision = DecisionType.Continue;
			base.DialogResult = new bool?(true);
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this._decision = DecisionType.Cancel;
			base.DialogResult = new bool?(false);
		}

		private void btnSkip_Click(object sender, RoutedEventArgs e)
		{
			this._decision = DecisionType.Skip;
			base.DialogResult = new bool?(false);
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
			return VisualTreeHelper.GetContentBounds(this.lstMovies);
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

		private void lstMovies_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.lstMovies.SelectedIndex != -1 && !this.btnSelect.IsEnabled)
			{
				this.btnSelect.IsEnabled = true;
			}
			this.Selected = this.lstMovies.SelectedItem;
		}

		private void lstMovies_Loaded(object sender, RoutedEventArgs e)
		{
			this.lbLoading.Visibility = Visibility.Collapsed;
			this.lstMovies.IsEnabled = true;
		}

		private void btnOpenTVDb_Click(object sender, RoutedEventArgs e)
		{
			TVShowXML tVShowXML = this.Selected as TVShowXML;
			if (tVShowXML.ID != null)
			{
				Process.Start("http://thetvdb.com/?tab=series&id=" + tVShowXML.ID + "&lid=7");
				return;
			}
			Process.Start("http://thetvdb.com/?string=" + tVShowXML.Title + "&searchseriesid=&tab=listseries&function=Search");
		}

		private void btnOpenTMDb_Click(object sender, RoutedEventArgs e)
		{
			MovieXML movieXML = this.Selected as MovieXML;
			if (movieXML.ID != null)
			{
				Process.Start("http://www.themoviedb.org/movie/" + movieXML.ID);
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
			Uri resourceLocator = new Uri("/MediaScoutGUI;component/selectresultdialog.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}

		[EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				this.Window = (SelectResultDialog)target;
				this.Window.Loaded += new RoutedEventHandler(this.Window_Loaded);
				return;
			case 2:
				((MenuItem)target).Click += new RoutedEventHandler(this.btnOpenTVDb_Click);
				return;
			case 3:
				((MenuItem)target).Click += new RoutedEventHandler(this.btnOpenTMDb_Click);
				return;
			case 4:
				this.LayoutRoot = (Grid)target;
				return;
			case 5:
				this.lstMovies = (ListBox)target;
				this.lstMovies.Loaded += new RoutedEventHandler(this.lstMovies_Loaded);
				this.lstMovies.SelectionChanged += new SelectionChangedEventHandler(this.lstMovies_SelectionChanged);
				return;
			case 6:
				this.lbTitle = (Label)target;
				return;
			case 7:
				this.btnCancel = (Button)target;
				this.btnCancel.Click += new RoutedEventHandler(this.btnCancel_Click);
				return;
			case 8:
				this.btnSelect = (Button)target;
				this.btnSelect.Click += new RoutedEventHandler(this.btnSelect_Click);
				return;
			case 9:
				this.btnSearchAgain = (Button)target;
				this.btnSearchAgain.Click += new RoutedEventHandler(this.btnSearchAgain_Click);
				return;
			case 10:
				this.btnSkip = (Button)target;
				this.btnSkip.Click += new RoutedEventHandler(this.btnSkip_Click);
				return;
			case 11:
				this.label1 = (Label)target;
				return;
			case 12:
				this.lbLoading = (Label)target;
				return;
			case 13:
				this.lblSearchTerm = (Label)target;
				return;
			default:
				this._contentLoaded = true;
				return;
			}
		}
	}
}
