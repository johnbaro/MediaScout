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
	public class NoResultsDialog : Window, IComponentConnector
	{
		private DecisionType _decision = DecisionType.Cancel;

		public string Term;

		public string Year;

		internal Grid gridBox;

		internal TextBlock lblmsg;

		internal Button btnCancel;

		internal Button btnSearch;

		internal Label lblSearchTerm;

		internal Button btnSkip;

		internal TextBox txtTerm;

		internal Label lbItemName;

		internal TextBox txtYear;

		internal Label label1;

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

		[DebuggerNonUserCode]
		public void InitializeComponent()
		{
			if (this._contentLoaded)
			{
				return;
			}
			this._contentLoaded = true;
			Uri resourceLocator = new Uri("/MediaScoutGUI;component/noresultsdialog.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}

		[EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				((NoResultsDialog)target).Loaded += new RoutedEventHandler(this.Window_Loaded);
				return;
			case 2:
				this.gridBox = (Grid)target;
				return;
			case 3:
				this.lblmsg = (TextBlock)target;
				return;
			case 4:
				this.btnCancel = (Button)target;
				this.btnCancel.Click += new RoutedEventHandler(this.btnCancel_Click);
				return;
			case 5:
				this.btnSearch = (Button)target;
				this.btnSearch.Click += new RoutedEventHandler(this.btnSearch_Click);
				return;
			case 6:
				this.lblSearchTerm = (Label)target;
				return;
			case 7:
				this.btnSkip = (Button)target;
				this.btnSkip.Click += new RoutedEventHandler(this.btnSkip_Click);
				return;
			case 8:
				this.txtTerm = (TextBox)target;
				return;
			case 9:
				this.lbItemName = (Label)target;
				return;
			case 10:
				this.txtYear = (TextBox)target;
				return;
			case 11:
				this.label1 = (Label)target;
				return;
			default:
				this._contentLoaded = true;
				return;
			}
		}
	}
}
