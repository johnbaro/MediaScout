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

namespace MediaScoutGUI
{
	/// <summary>
	/// Interaction logic for TVShowResults.xaml
	/// </summary>
	public partial class TVShowResults : Window
	{
        public MediaScout.TVShow Selected;

        public TVShowResults(MediaScout.TVShow[] t)
		{
			this.InitializeComponent();

            lstShows.ItemsSource = t;
            lstShows.SelectedIndex = 0;
		}

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            Selected = lstShows.SelectedItem as MediaScout.TVShow;
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Selected = null;
            this.DialogResult = false;
        }
	}
}