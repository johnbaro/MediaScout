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
using System.ComponentModel.Composition;
using System.Collections.ObjectModel;
using MediaScout.Interfaces;

namespace MediaScoutGUI.Views
{
    /// <summary>
    /// Interaction logic for BulkTVView.xaml
    /// </summary>
    public partial class BulkTVView : UserControl
    {
        public BulkTVView()
        {
            InitializeComponent();
            //cbSource.ItemsSource = TVProviders;
        }

        private void btnFetch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
