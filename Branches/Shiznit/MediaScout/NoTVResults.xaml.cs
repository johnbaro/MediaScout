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
    /// Interaction logic for NoTVResults.xaml
    /// </summary>
    public partial class NoTVResults : Window
    {
        public String Term;
        public NoTVResults(String term)
        {
            InitializeComponent();
            txtTerm.Text = term;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Term = txtTerm.Text;
            this.DialogResult = true;
        }
    }
}
