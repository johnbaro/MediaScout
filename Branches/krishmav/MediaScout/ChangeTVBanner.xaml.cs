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
using System.Threading;
using System.IO;
using System.Net;
using System.Drawing.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Windows.Interop;
using MediaScoutGUI.GUITypes;
using MediaScout.Providers;

namespace MediaScoutGUI
{
    /// <summary>
    /// Interaction logic for ChangeTVBanner.xaml
    /// </summary>
    public partial class ChangeTVBanner : Window
    {
        private delegate void PostersLoadingHandler();
        private MediaScout.Posters[] posters;
        private ObservableCollection<MediaScout.Posters> Posters = new ObservableCollection<MediaScout.Posters>();
        private DispatchingCollection<ObservableCollection<MediaScout.Posters>, MediaScout.Posters> dispatchPosters;
        
        private MediaScout.Providers.TheTVDBProvider tvdb;
        private String TVShowID = null;

        public bool DownloadAll = false;
        public MediaScout.Posters selected = null;

        public ChangeTVBanner(String tvshowid, TheTVDBProvider tvdb)
		{
			InitializeComponent();
            TVShowID = tvshowid;
            this.tvdb = tvdb;
            this.dispatchPosters = new DispatchingCollection<ObservableCollection<MediaScout.Posters>, MediaScout.Posters>(Posters, Dispatcher);
        }

        private void lbPosters_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPosters();
            lbPosters.ItemsSource = dispatchPosters;
        }

        public void LoadPosters()
        {
            Thread th = new Thread(delegate()
            {
                posters = tvdb.GetPosters(TVShowID, TVShowPosterType.Banner, null);
                AddPosters();
            });

            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }

        private void AddPosters()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new PostersLoadingHandler(AddPosters));
                return;
            }
            if (posters.Length == 0)
                lblNoPosters.Content = "No Banners found";
            else
                lblNoPosters.Visibility = Visibility.Collapsed;
            foreach (MediaScout.Posters poster in posters)
                Posters.Add(poster);
        }

        private void btnSelectBanner_Click(object sender, RoutedEventArgs e)
        {
            selected = (MediaScout.Posters)lbPosters.SelectedItem;
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void btnDownloadAll_Click(object sender, RoutedEventArgs e)
        {
            DownloadAll = true;
            this.DialogResult = false;
        }

        #region Images Hyperlink Click
        private void HandleLinkClick(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            string navigateUri = hl.NavigateUri.ToString();
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(navigateUri));
            e.Handled = true;
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
            return VisualTreeHelper.GetContentBounds(lbPosters);
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
