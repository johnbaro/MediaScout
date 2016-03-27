using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace MediaScoutGUI
{
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public partial class About : Window, IComponentConnector
	{
		private bool closeStoryBoardCompleted;

		
		public About(bool IsSplash)
		{
			this.InitializeComponent();
			this.lblVersion.Text = "Version : " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
			if (IsSplash)
			{
				this.btnOK.Visibility = Visibility.Collapsed;
				this.lblHomeLink.Visibility = Visibility.Collapsed;
				this.lblLicenseLink.Visibility = Visibility.Collapsed;
			}
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			base.DialogResult = new bool?(true);
		}

		private void WindowFadeOut_Completed(object sender, EventArgs e)
		{
			this.closeStoryBoardCompleted = true;
			base.Close();
		}

		private void Splash_Closing(object sender, CancelEventArgs e)
		{
			if (!this.closeStoryBoardCompleted)
			{
				Storyboard storyboard = (Storyboard)base.FindResource("WindowFadeOut");
				storyboard.Begin();
				e.Cancel = true;
			}
		}

		private void HandleLinkClick(object sender, RequestNavigateEventArgs e)
		{
			string fileName = e.Uri.ToString();
			Process.Start(new ProcessStartInfo(fileName));
			e.Handled = true;
		}
	}
}
