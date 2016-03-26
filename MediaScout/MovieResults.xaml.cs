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
	/// Interaction logic for MovieResults.xaml
	/// </summary>
	public partial class MovieResults : Window
	{
        public MediaScout.Movie Selected;

		public MovieResults(MediaScout.Movie[] m)
		{
			this.InitializeComponent();

            lstMovies.ItemsSource = m;
            lstMovies.SelectedIndex = 0;
		}

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            Selected = lstMovies.SelectedItem as MediaScout.Movie;
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Selected = null;
            this.DialogResult = false;
        }

	}
}