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
using System.Collections.ObjectModel;
using System.Windows.Interop;

namespace MediaScoutGUI
{
    /// <summary>
    /// Interaction logic for TitleSelectionDialog.xaml
    /// </summary>
    public partial class TitleSelectionDialog : Window
    {
        Collection<String> Titles;
        public String SelectedTitle;
        public TitleSelectionDialog(MediaScout.MovieXML m)
        {
            InitializeComponent();
            Titles = new Collection<string>();
            Titles.Add(m.OriginalTitle);
            Titles.Add(m.LocalTitle);
            SelectedTitle = m.OriginalTitle;
        }

        private void cbTitles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedTitle = cbTitles.SelectedItem as String;
        }

        #region Window Routines

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (MediaScoutGUI.Properties.Settings.Default.EnableGlassFrame == true)
            {
                Rect bounds = GetBoundsForGlassFrame();
                HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
                source.AddHook(new HwndSourceHook(WndProc));
                SetGlassFrame(true);
            }

            cbTitles.ItemsSource = Titles;
        }

        #endregion

        #region To Enable/Disable for the Aero glass effect

        public Rect GetBoundsForGlassFrame()
        {
            return VisualTreeHelper.GetContentBounds(gridlayout);
        }
        public bool SetGlassFrame(bool ExtendGlass)
        {
            bool success = false;
            if (ExtendGlass)
            {
                // Extend glass
                Rect bounds = GetBoundsForGlassFrame();
                success = GlassHelper.ExtendGlassFrame(this, new Thickness(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom));
            }
            else
            {
                // turn off glass...
                GlassHelper.DisableGlassFrame(this);
                success = true;
            }
            return success;
        }
        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            int WM_DWMCOMPOSITIONCHANGED = 0x031E; // for glass (when DWM / glass setting is changed)
            // handle the message for DWM when the aero glass is turned on or off
            if (msg == WM_DWMCOMPOSITIONCHANGED)
            {
                SetGlassFrame(GlassHelper.IsGlassEnabled);
                handled = true;
            }

            return IntPtr.Zero;
        }

        #endregion

        private void bntOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        
    }
}
