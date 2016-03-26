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
	public class About : Window, IComponentConnector
	{
		private bool closeStoryBoardCompleted;

		internal About Splash;

		internal Image image1;

		internal Button btnOK;

		internal Label lblHomeLink;

		internal Label lblLicenseLink;

		internal TextBlock textBlock1;

		internal TextBlock lblVersion;

		private bool _contentLoaded;

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

		[DebuggerNonUserCode]
		public void InitializeComponent()
		{
			if (this._contentLoaded)
			{
				return;
			}
			this._contentLoaded = true;
			Uri resourceLocator = new Uri("/MediaScoutGUI;component/about.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}

		[EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				this.Splash = (About)target;
				this.Splash.Closing += new CancelEventHandler(this.Splash_Closing);
				return;
			case 2:
				((Storyboard)target).Completed += new EventHandler(this.WindowFadeOut_Completed);
				return;
			case 3:
				this.image1 = (Image)target;
				return;
			case 4:
				this.btnOK = (Button)target;
				this.btnOK.Click += new RoutedEventHandler(this.btnOK_Click);
				return;
			case 5:
				this.lblHomeLink = (Label)target;
				return;
			case 6:
				((Hyperlink)target).RequestNavigate += new RequestNavigateEventHandler(this.HandleLinkClick);
				return;
			case 7:
				this.lblLicenseLink = (Label)target;
				return;
			case 8:
				((Hyperlink)target).RequestNavigate += new RequestNavigateEventHandler(this.HandleLinkClick);
				return;
			case 9:
				this.textBlock1 = (TextBlock)target;
				return;
			case 10:
				this.lblVersion = (TextBlock)target;
				return;
			default:
				this._contentLoaded = true;
				return;
			}
		}
	}
}
