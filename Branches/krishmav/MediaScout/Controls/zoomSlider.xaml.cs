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
using System.ComponentModel;

namespace MediaScoutGUI.Controls
{
    /// <summary>
    /// Interaction logic for zoomSlider.xaml
    /// </summary>
    public partial class zoomSlider : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        public Double Value
        {
            get { return zoomslider.Value; }
            set
            {
                zoomslider.Value = value;
                NotifyPropertyChanged("Value");
            }
        }

        public void ResetZoom()
        {
            Value = 1.0;
        }

        public zoomSlider()
        {
            InitializeComponent();
            lblZoom.DataContext = this;
        }

        private void zoomslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Value = e.NewValue;
        }
    }
}
