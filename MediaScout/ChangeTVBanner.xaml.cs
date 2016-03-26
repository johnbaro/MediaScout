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

using System.Diagnostics;

namespace MediaScoutGUI
{
    /// <summary>
    /// Interaction logic for ChangeTVBanner.xaml
    /// </summary>
    public partial class ChangeTVBanner : Window
    {
        public System.Drawing.Image Selected;

        public ChangeTVBanner(MediaScout.Posters[] posters)
        {
            InitializeComponent();
            lbPosters.ItemsSource = posters;
        }

        private void btnSelectBanner_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Selected item #" + lbPosters.SelectedIndex);
            MediaScout.Posters selectedImage = (MediaScout.Posters)lbPosters.SelectedItem;

            WebRequest requestPic = WebRequest.Create(selectedImage.Poster);
            WebResponse responsePic = requestPic.GetResponse();
            Selected = System.Drawing.Image.FromStream(responsePic.GetResponseStream());

            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
