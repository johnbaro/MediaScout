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
using System.IO;
using System.Threading;
using System.Net;
using System.Drawing.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using MediaScout.Providers;
using MediaScoutGUI.GUITypes;
using System.Text.RegularExpressions;
using System.Windows.Interop;

namespace MediaScoutGUI
{
    /// <summary>
    /// Interaction logic for ChangeMovieBackdrop.xaml
    /// </summary>
    public partial class ChangeActorImageDialog : Window
    {
        private delegate void PostersLoadingHandler();
        private MediaScout.Posters[] posters;
        private ObservableCollection<LocalPosters> localPosters = new ObservableCollection<LocalPosters>();
        private ObservableCollection<MediaScout.Posters> Posters = new ObservableCollection<MediaScout.Posters>();
        private DispatchingCollection<ObservableCollection<MediaScout.Posters>, MediaScout.Posters> dispatchPosters;

        private TheMovieDBProvider tmdb;
        private TheTVDBProvider tvdb;

        private String ID = null;
        private String ActorsID = null;
        private String ActorsName = null;        
        private bool IsMovieActor = false;

        public MediaScout.Posters selected = null;
        public LocalPosters selectedLocalPoster = null;
        public MediaScout.Person SelectedActor = null;
        public ImageWindowDecisionbType Decision = ImageWindowDecisionbType.Cancel;

        public ChangeActorImageDialog(String ID, String ActorsName, String ActorsID, bool IsMovieActor)
        {
            InitializeComponent();
            this.ID = ID;
            this.ActorsID = ActorsID;
            this.ActorsName = ActorsName;
            this.IsMovieActor = IsMovieActor;
            if (IsMovieActor)
                tmdb = new TheMovieDBProvider(null);
            else
                tvdb = new TheTVDBProvider(null);
            this.Title = "Select " + ActorsName + " Image";
            this.dispatchPosters = new DispatchingCollection<ObservableCollection<MediaScout.Posters>, MediaScout.Posters>(Posters, Dispatcher);
        }

        #region Load Posters

        public void LoadPosters()
        {
            Thread th = new Thread(delegate()
            {
                if (ActorsID == null)
                {
                    List<MediaScout.Person> Persons = (IsMovieActor) ? tmdb.SearchPerson(ActorsName) : tvdb.GetActors(ID);
                    //List<MediaScout.Person> Persons = (IsMovieActor) ? tmdb.GetActors(ID) : tvdb.GetActors(ID);

                    foreach (MediaScout.Person p in Persons)
                    {
                        if (ActorsName == p.Name)
                            SelectedActor = p;                            
                    }
                }
                ActorsID = SelectedActor.ID;
                posters = IsMovieActor ? tmdb.GetPersonImage(ActorsID).ToArray() : null;
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
            if (posters == null || posters.Length == 0)
                lblNoPosters.Content = "No Images found";
            else
            {
                if (!btnDownloadAll.IsEnabled)
                    btnDownloadAll.IsEnabled = true;
                if (!btnSelectPoster.IsEnabled)
                    btnSelectPoster.IsEnabled = true;
                lblNoPosters.Visibility = Visibility.Collapsed;
                foreach (MediaScout.Posters poster in posters)
                    Posters.Add(poster);
            }
        }
        private void AddLocalPosters()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new PostersLoadingHandler(AddLocalPosters));
                return;
            }
            if (localPosters.Count == 0)
            {
                lbNoLocals.Visibility = Visibility.Visible;
                lbNoLocals.Content = "Add Local Images";
            }
            else
            {
                if (!btnSelectPoster.IsEnabled)
                    btnSelectPoster.IsEnabled = true;
                lbNoLocals.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Button Routines

        private void btnSelectPoster_Click(object sender, RoutedEventArgs e)
        {
            if (lbPosters.SelectedIndex != -1)
            {
                this.Decision = ImageWindowDecisionbType.PosterSelected;
                selected = (MediaScout.Posters)lbPosters.SelectedItem;
            }
            else
            {
                this.Decision = ImageWindowDecisionbType.LocalPosterSelected;
                selectedLocalPoster = (LocalPosters)lblocalPosters.SelectedItem;

            }
            this.DialogResult = true;
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Decision = ImageWindowDecisionbType.Cancel;
            this.DialogResult = false;
        }
        private void btnDownloadAll_Click(object sender, RoutedEventArgs e)
        {
            this.Decision = ImageWindowDecisionbType.DownloadAll;
            this.DialogResult = true;
        }

        #endregion

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

            lblocalPosters.ItemsSource = localPosters;
            LoadPosters();
            lbPosters.ItemsSource = dispatchPosters;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            localPosters.Clear();
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

        #region Poster Selection

        bool jump = true;
        private void lbPosters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lblocalPosters.SelectedIndex != -1 && jump)
            {
                jump = false;
                lblocalPosters.UnselectAll();
            }
            else if (!jump)
                jump = true;
        }
        private void lblocalPosters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((lbPosters.SelectedIndex != -1) && jump)
            {
                jump = false;
                lbPosters.UnselectAll();
            }
            else if (!jump)
                jump = true;
        }

        #endregion

        #region Local Poster Routines

        private void btnDeleteAllPoster_Click(object sender, RoutedEventArgs e)
        {
            foreach (LocalPosters lp in localPosters)
                DeleteSelectedPoster(lp, false);

            localPosters.Clear();
            AddLocalPosters();
        }
        private void DeleteSelectedPoster(LocalPosters lp, bool remove)
        {
            String file;
            file = lp.Poster;
            if (File.Exists(file))
                File.Delete(file);

            if (remove)
            {
                int index = localPosters.IndexOf(lp);
                localPosters.RemoveAt(index);
                if (localPosters.Count == 0)
                    AddLocalPosters();
            }
        }
        private void btnDeletePoster_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedPoster(lblocalPosters.SelectedItem as LocalPosters, true);
        }
        private void btnAddPoster_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Filter = "Image Files|*.jpg;*.tbn";
            if (ofd.ShowDialog(this) == true)
            {
                foreach (String Filename in ofd.FileNames)
                {
                    try
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                          new Action(
                            delegate()
                            {
                                localPosters.Add(new LocalPosters(Filename));
                                AddLocalPosters();
                            }
                        ));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
        
        #endregion
    }
}
