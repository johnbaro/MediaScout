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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using MediaScout.Interfaces;
using System.Collections.ObjectModel;

namespace MediaScoutGUI
{
    /// <summary>
    /// Interaction logic for AppHost.xaml
    /// </summary>
    public partial class AppHost : Window
    {


        public AppHost()
        {
            InitializeComponent();

            tbTabs.ItemsSource = ((App)Application.Current).Tabs;
        }
    }
}
