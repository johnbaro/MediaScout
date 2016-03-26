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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace MediaScoutGUI.Wizard
{
    /// <summary>
    /// Interaction logic for Folders.xaml
    /// </summary>
    public partial class Folders : Page
    {
        public Folders()
        {
            InitializeComponent();
        }

        #region Sets the TV Folders

        private void btnSetTVFolders_Click(object sender, RoutedEventArgs e)
        {
            FoldersDialog fd = new FoldersDialog(false);
            fd.Owner = this.Parent as NavigationWindow;
            if (fd.ShowDialog() == true)
            {
                if (Properties.Settings.Default.TVFolders == null)
                    Properties.Settings.Default.TVFolders = new System.Collections.Specialized.StringCollection();
                else
                    Properties.Settings.Default.TVFolders.Clear();
                foreach (String dir in fd.lstFolders.Items)
                    Properties.Settings.Default.TVFolders.Add(dir);
                Properties.Settings.Default.Save();
            }
        }

        #endregion

        #region Sets the Movie Folders

        private void btnSetMovieFolders_Click(object sender, RoutedEventArgs e)
        {
            FoldersDialog fd = new FoldersDialog(true);
            fd.Owner = this.Parent as NavigationWindow;
            if (fd.ShowDialog() == true)
            {
                if (Properties.Settings.Default.MovieFolders == null)
                    Properties.Settings.Default.MovieFolders = new System.Collections.Specialized.StringCollection();
                else
                    Properties.Settings.Default.MovieFolders.Clear();
                foreach (String dir in fd.lstFolders.Items)
                    Properties.Settings.Default.MovieFolders.Add(dir);
                Properties.Settings.Default.Save();
            }
        }

        #endregion

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            Metadata ma = new Metadata();
            NavigationService.Navigate(ma);
        }
    }
}
