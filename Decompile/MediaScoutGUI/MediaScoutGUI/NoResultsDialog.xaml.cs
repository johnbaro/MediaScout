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
    public partial class NoResultsDialog : Window, IComponentConnector
	{
		private DecisionType _decision = DecisionType.Cancel;

		public string Term;

		public string Year;


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

		public NoResultsDialog(string SearchObjectName, string term, string year, bool Forced, bool Skip, bool IsMovieSearch)
		{
			this.InitializeComponent();
			this.Term = term;
			this.Year = year;
			string str = IsMovieSearch ? "Movie" : "TV Show";
			this.lbItemName.Content = SearchObjectName;
			this.lblmsg.Text = (Forced ? "" : ("No " + str + "s were found..."));
			this.lblSearchTerm.Content = "Enter " + str + " Name : ";
			base.Title = "Search " + str;
			if (Skip)
			{
				this.btnSkip.Visibility = Visibility.Visible;
				this._decision = DecisionType.Skip;
			}
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

		private void btnSearch_Click(object sender, RoutedEventArgs e)
		{
			this._decision = DecisionType.Continue;
			this.Term = this.txtTerm.Text;
			this.Year = this.txtYear.Text;
			base.DialogResult = new bool?(true);
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
			this.txtYear.Text = this.Year;
			this.txtTerm.Focus();
			this.txtTerm.Text = this.Term;
			this.txtTerm.CaretIndex = this.txtTerm.Text.Length;
		}

		public Rect GetBoundsForGlassFrame()
		{
			return VisualTreeHelper.GetContentBounds(this.gridBox);
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
	}
}
