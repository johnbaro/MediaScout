using MediaScout;
using MediaScoutGUI.Properties;
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
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
    public partial class TitleSelectionDialog : Window, IComponentConnector
	{
		private Collection<string> Titles;

		public string SelectedTitle;

		public TitleSelectionDialog(MovieXML m)
		{
			this.InitializeComponent();
			this.Titles = new Collection<string>();
			this.Titles.Add(m.Title);
			this.Titles.Add(m.Alt_Title);
			this.SelectedTitle = m.Title;
		}

		private void cbTitles_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.SelectedTitle = (this.cbTitles.SelectedItem as string);
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
			this.cbTitles.ItemsSource = this.Titles;
		}

		public Rect GetBoundsForGlassFrame()
		{
			return VisualTreeHelper.GetContentBounds(this.gridlayout);
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

		private void bntOK_Click(object sender, RoutedEventArgs e)
		{
			base.DialogResult = new bool?(true);
		}
	}
}
