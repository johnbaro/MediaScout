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
using System.Windows.Interop;
using System.IO;

namespace MediaScoutGUI
{
    /// <summary>
    /// Interaction logic for FoldersDialog.xaml
    /// </summary>
    public partial class FoldersDialog : Window
    {
        public FoldersDialog(bool IsMovieFoldersDialog)
        {
            InitializeComponent();

            System.Collections.Specialized.StringCollection Dirs;
            if (IsMovieFoldersDialog)
                Dirs = Properties.Settings.Default.MovieFolders;
            else
                Dirs = Properties.Settings.Default.TVFolders;

            if(Dirs!= null)
                foreach (String dir in Dirs)
                    lstFolders.Items.Add(dir);
        }

        #region Sets the Folders
       
        private void btnBrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog myBrowser = new System.Windows.Forms.FolderBrowserDialog();
            myBrowser.Description = "Select folder";

            if (myBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                lstFolders.Items.Add(myBrowser.SelectedPath);
        }

        #endregion

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
        }

        #endregion

        #region To Enable/Disable for the Aero glass effect

        public Rect GetBoundsForGlassFrame()
        {
            return VisualTreeHelper.GetContentBounds(GridLayout);
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

        #region OK/Cancel Button Routines
        
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        
        #endregion

        #region Show/Hide Delete Button

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            lstFolders.Items.Remove(lstFolders.SelectedItem);
        }
        private void lstFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstFolders.SelectedIndex != -1)
            {
                if (btnDelete.Visibility == Visibility.Collapsed)
                    btnDelete.Visibility = Visibility.Visible;
            }
            else
                btnDelete.Visibility = Visibility.Collapsed;
        }
        
        #endregion
    }
}
