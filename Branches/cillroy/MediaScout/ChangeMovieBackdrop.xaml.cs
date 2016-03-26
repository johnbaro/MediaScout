using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Net;
using System.Drawing.Imaging;
namespace MediaScoutGUI
{
    /// <summary>
    /// Interaction logic for ChangeMovieBackdrop.xaml
    /// </summary>
    public partial class ChangeMovieBackdrop : Window
    {
        public System.Drawing.Image Selected;

        public ChangeMovieBackdrop(MediaScout.Posters[] posters)
        {
            InitializeComponent();

            if (posters.Length == 0)
            {
                lblNoPosters.Visibility = Visibility.Visible;
                btnSelectPoster.IsEnabled = false;
            }
            else
                lbPosters.ItemsSource = posters;
        }
        private void btnSelectPoster_Click(object sender, RoutedEventArgs e)
        {
            String selectedImage = ((MediaScout.Posters)lbPosters.SelectedItem).Poster;

            WebRequest requestPic = WebRequest.Create(selectedImage);
            WebResponse responsePic = requestPic.GetResponse();
            Selected = System.Drawing.Image.FromStream(responsePic.GetResponseStream());

            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
