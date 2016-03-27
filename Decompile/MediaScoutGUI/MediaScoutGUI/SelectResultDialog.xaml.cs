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
    public partial class SelectResultDialog : Window, IComponentConnector
	{
		private DecisionType _decision = DecisionType.Cancel;

		public object Selected;

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
	}
}
