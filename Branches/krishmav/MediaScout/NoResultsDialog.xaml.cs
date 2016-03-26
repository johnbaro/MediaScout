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
using System.Windows.Interop;

namespace MediaScoutGUI
{
    public partial class NoResultsDialog : Window
    {
        private DecisionType _decision = DecisionType.Cancel;        

        public DecisionType Decision
        {
            get { return _decision; }
            set { _decision = value; }
        }

        public String Term;
        public NoResultsDialog(String term, bool Forced, bool Skip, bool IsMovieSearch)
        {
            InitializeComponent();
            txtTerm.Text = term;
            String item = IsMovieSearch ? "Movie" : "TV Show";
            lblmsg.Text = (Forced) ? "" : "No " + item + "s were found...";
            lblSearchTerm.Content = "Enter " + item + " name :";
            this.Title = "Search " + item;
            if (Skip)
            {
                btnSkip.Visibility = Visibility.Visible;
                _decision = DecisionType.Skip;
            }           
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this._decision = DecisionType.Cancel;
            this.DialogResult = false;
        }

        private void btnSkip_Click(object sender, RoutedEventArgs e)
        {
            this._decision = DecisionType.Skip;
            this.DialogResult = false;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this._decision = DecisionType.Continue;
            Term = txtTerm.Text;
            this.DialogResult = true;            
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
        }
       
        #endregion

        #region To Enable/Disable for the Aero glass effect

        public Rect GetBoundsForGlassFrame()
        {
            return VisualTreeHelper.GetContentBounds(gridBox);
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
    }
}
