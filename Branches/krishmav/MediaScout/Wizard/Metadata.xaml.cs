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

namespace MediaScoutGUI.Wizard
{
    /// <summary>
    /// Interaction logic for Metadata.xaml
    /// </summary>
    public partial class Metadata : Page
    {
        public Metadata()
        {
            InitializeComponent();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SaveXBMCMeta = chkSaveXBMCMeta.IsChecked.Value;
            Properties.Settings.Default.SaveMyMoviesMeta = chkSaveMMMeta.IsChecked.Value;
            Properties.Settings.Default.Save();
            (this.Parent as Window).DialogResult = true;
        }
    }
}
