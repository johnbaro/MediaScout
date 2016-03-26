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

namespace MediaScoutGUI.Controls
{
    /// <summary>
    /// Interaction logic for Image.xaml
    /// </summary>
    public partial class ImageControl : UserControl
    {
        private bool setLoading = false;
        public bool SetLoading
        {
            get { return setLoading; }
            set
            {
                setLoading = value;
                if (setLoading)
                    LoadingPB.Visibility = Visibility.Visible;
                else
                    LoadingPB.Visibility = Visibility.Collapsed;
            }
        }

        public ImageSource source = null;
        public ImageSource Source
        {
            get { return source; }
            set
            {                
                myImage.Source = source = value;
                SetLoading = false;
            }
        }

        public String filename = null;
        public String Filename
        {
            get { return filename; }
            set { filename = value; }
        }

        public Stretch stretch = Stretch.Fill;
        public Stretch Stretch
        {
            get { return stretch; }
            set { myImage.Stretch = stretch = value; }
        }

        public ImageControl()
        {
            InitializeComponent();
        }
    }
}
