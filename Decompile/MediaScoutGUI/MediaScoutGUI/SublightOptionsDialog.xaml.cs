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
    public partial class SublightOptionsDialog : Window, IComponentConnector
	{

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



	}
}
