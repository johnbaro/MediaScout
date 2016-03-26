using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MediaScoutGUI
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About(bool IsSplash)
        {
            InitializeComponent();
            lblVersion.Content = "Version : " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (IsSplash)
            {
                btnOK.Visibility = Visibility.Collapsed;
                lblHomeLink.Visibility = Visibility.Collapsed;
                lblLicenseLink.Visibility = Visibility.Collapsed;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private bool closeStoryBoardCompleted = false;
        private void WindowFadeOut_Completed(object sender, EventArgs e)
        {
            closeStoryBoardCompleted = true;
            this.Close();
        }

        private void Splash_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!closeStoryBoardCompleted)
            {
                System.Windows.Media.Animation.Storyboard WindowFadeOut = (System.Windows.Media.Animation.Storyboard)FindResource("WindowFadeOut");
                WindowFadeOut.Begin();
                e.Cancel = true;
            }
        }
        
        #region Hyperlink Click
        private void HandleLinkClick(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            String navigateUri = e.Uri.ToString();
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(navigateUri));
            e.Handled = true;
        }
        #endregion
    }
}
