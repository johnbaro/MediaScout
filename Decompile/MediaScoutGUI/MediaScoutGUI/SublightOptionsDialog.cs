using MediaScoutGUI.Properties;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;

namespace MediaScoutGUI
{
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public class SublightOptionsDialog : Window, IComponentConnector
	{
		internal Grid GridLayout;

		internal System.Windows.Controls.TextBox txtSublightUsername;

		internal System.Windows.Controls.TextBox txtSublightPassword;

		internal System.Windows.Controls.TextBox txtSublightCmd;

		internal System.Windows.Controls.Button btnBrowseSublightCmd;

		internal System.Windows.Controls.Label label1;

		internal System.Windows.Controls.Label label2;

		internal System.Windows.Controls.Label label3;

		internal System.Windows.Controls.Button btnOK;

		internal System.Windows.Controls.Button btnCancel;

		internal System.Windows.Controls.Button btnRegister;

		internal System.Windows.Controls.TextBox txtSublight;

		internal System.Windows.Controls.Button btnBrowseSublight;

		internal System.Windows.Controls.Label label4;

		private bool _contentLoaded;

		public SublightOptionsDialog()
		{
			this.InitializeComponent();
			this.txtSublight.Text = Settings.Default.Sublight;
			this.txtSublightCmd.Text = Settings.Default.SublightCmd;
			this.txtSublightUsername.Text = Settings.Default.SublightUsername;
			this.txtSublightPassword.Text = Settings.Default.SublightPassword;
		}

		private void txtSublightUsername_TextChanged(object sender, TextChangedEventArgs e)
		{
			this.btnRegister.IsEnabled = !string.IsNullOrEmpty(this.txtSublightUsername.Text);
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			base.DialogResult = new bool?(true);
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			base.DialogResult = new bool?(false);
		}

		private void btnRegister_Click(object sender, RoutedEventArgs e)
		{
		}

		private void btnBrowseSublightCmd_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.CheckFileExists = true;
			openFileDialog.CustomPlaces.Add("C:\\Program Files\\Sublight");
			openFileDialog.Multiselect = false;
			openFileDialog.Title = "Select SublightCmd.exe Locaion";
			openFileDialog.Filter = "SublightCmd.exe|SublightCmd.exe";
			if (File.Exists(this.txtSublightCmd.Text))
			{
				openFileDialog.FileName = this.txtSublightCmd.Text;
			}
			if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				this.txtSublightCmd.Text = openFileDialog.FileName;
			}
		}

		private void btnBrowseSublight_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.CheckFileExists = true;
			openFileDialog.CustomPlaces.Add("C:\\Program Files\\Sublight");
			openFileDialog.Multiselect = false;
			openFileDialog.Title = "Select Sublight.exe Locaion";
			openFileDialog.Filter = "Sublight.exe|Sublight.exe";
			if (File.Exists(this.txtSublight.Text))
			{
				openFileDialog.FileName = this.txtSublightCmd.Text;
			}
			if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				this.txtSublight.Text = openFileDialog.FileName;
			}
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

		[DebuggerNonUserCode]
		public void InitializeComponent()
		{
			if (this._contentLoaded)
			{
				return;
			}
			this._contentLoaded = true;
			Uri resourceLocator = new Uri("/MediaScoutGUI;component/sublightoptionsdialog.xaml", UriKind.Relative);
			System.Windows.Application.LoadComponent(this, resourceLocator);
		}

		[EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				((SublightOptionsDialog)target).Loaded += new RoutedEventHandler(this.Window_Loaded);
				return;
			case 2:
				this.GridLayout = (Grid)target;
				return;
			case 3:
				this.txtSublightUsername = (System.Windows.Controls.TextBox)target;
				this.txtSublightUsername.TextChanged += new TextChangedEventHandler(this.txtSublightUsername_TextChanged);
				return;
			case 4:
				this.txtSublightPassword = (System.Windows.Controls.TextBox)target;
				return;
			case 5:
				this.txtSublightCmd = (System.Windows.Controls.TextBox)target;
				return;
			case 6:
				this.btnBrowseSublightCmd = (System.Windows.Controls.Button)target;
				this.btnBrowseSublightCmd.Click += new RoutedEventHandler(this.btnBrowseSublightCmd_Click);
				return;
			case 7:
				this.label1 = (System.Windows.Controls.Label)target;
				return;
			case 8:
				this.label2 = (System.Windows.Controls.Label)target;
				return;
			case 9:
				this.label3 = (System.Windows.Controls.Label)target;
				return;
			case 10:
				this.btnOK = (System.Windows.Controls.Button)target;
				this.btnOK.Click += new RoutedEventHandler(this.btnOK_Click);
				return;
			case 11:
				this.btnCancel = (System.Windows.Controls.Button)target;
				this.btnCancel.Click += new RoutedEventHandler(this.btnCancel_Click);
				return;
			case 12:
				this.btnRegister = (System.Windows.Controls.Button)target;
				this.btnRegister.Click += new RoutedEventHandler(this.btnRegister_Click);
				return;
			case 13:
				this.txtSublight = (System.Windows.Controls.TextBox)target;
				return;
			case 14:
				this.btnBrowseSublight = (System.Windows.Controls.Button)target;
				this.btnBrowseSublight.Click += new RoutedEventHandler(this.btnBrowseSublight_Click);
				return;
			case 15:
				this.label4 = (System.Windows.Controls.Label)target;
				return;
			default:
				this._contentLoaded = true;
				return;
			}
		}
	}
}
