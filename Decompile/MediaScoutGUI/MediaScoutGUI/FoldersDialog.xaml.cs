using MediaScoutGUI.Properties;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
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
    public partial class FoldersDialog : Window, IComponentConnector
	{
		private bool IsMovieFoldersDialog;

		public FoldersDialog(bool IsMovieFoldersDialog)
		{
			this.InitializeComponent();
			this.IsMovieFoldersDialog = IsMovieFoldersDialog;
			StringCollection stringCollection;
			if (IsMovieFoldersDialog)
			{
				stringCollection = Settings.Default.MovieFolders;
			}
			else
			{
				stringCollection = Settings.Default.TVFolders;
			}
			if (stringCollection != null)
			{
				foreach (string current in stringCollection)
				{
					this.lstFolders.Items.Add(current);
				}
			}
			if (this.lstFolders.Items.Count > 0)
			{
				this.btnDeleteAll.Visibility = Visibility.Visible;
			}
		}

		private void btnBrowseFolder_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.Description = "Select folder";
			if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				this.lstFolders.Items.Add(folderBrowserDialog.SelectedPath);
			}
			if (this.lstFolders.Items.Count > 0)
			{
				this.btnDeleteAll.Visibility = Visibility.Visible;
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

		private void btnOk_Click(object sender, RoutedEventArgs e)
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + (this.IsMovieFoldersDialog ? "\\MediaScout\\Moviefolders.set" : "\\MediaScout\\TVfolders.set");
			StreamWriter streamWriter = new StreamWriter(path, false);
			foreach (string value in ((IEnumerable)this.lstFolders.Items))
			{
				streamWriter.WriteLine(value);
			}
			streamWriter.Close();
			base.DialogResult = new bool?(true);
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			base.DialogResult = new bool?(false);
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			this.lstFolders.Items.Remove(this.lstFolders.SelectedItem);
			if (this.lstFolders.Items.Count > 0)
			{
				this.btnDeleteAll.Visibility = Visibility.Visible;
				return;
			}
			this.btnDeleteAll.Visibility = Visibility.Collapsed;
		}

		private void lstFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.lstFolders.SelectedIndex != -1)
			{
				if (this.btnDelete.Visibility == Visibility.Collapsed)
				{
					this.btnDelete.Visibility = Visibility.Visible;
					return;
				}
			}
			else
			{
				this.btnDelete.Visibility = Visibility.Collapsed;
			}
		}

		private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
		{
			this.lstFolders.Items.Clear();
			this.btnDeleteAll.Visibility = Visibility.Collapsed;
		}
	}
}
