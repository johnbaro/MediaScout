using System;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using System.Windows.Shell;

namespace MediaScoutGUI
{
    public class MediaScoutApp : Application
    {
        public MainWindow MyWindow { get; private set; }
        //public System.Windows.Forms.NotifyIcon notifyIcon;
        public JumpList jumplist;
        int SelectedTabIndex = 1;        

        #region Jump list Routines
        void SetUpJumpList()
        {
            JumpTask MovieTab = new JumpTask();
            MovieTab.ApplicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            MovieTab.Arguments = "/Tab:Movies";
            MovieTab.IconResourcePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            MovieTab.IconResourceIndex = 1;
            MovieTab.Title = "Movies";
            MovieTab.Description = "Open Movies Tab";

            JumpTask TVTab = new JumpTask();
            TVTab.ApplicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            TVTab.Arguments = "/Tab:TVSeries";
            TVTab.IconResourcePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            TVTab.IconResourceIndex = 2;
            TVTab.Title = "TV Series";
            TVTab.Description = "Open TV Series Tab";

            JumpTask OptionsTab = new JumpTask();
            OptionsTab.ApplicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            OptionsTab.Arguments = "/Tab:Options";
            OptionsTab.IconResourcePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            OptionsTab.IconResourceIndex = 3;
            OptionsTab.Title = "Options";
            OptionsTab.Description = "Open Options Tab";

            jumplist = new JumpList();
            jumplist.JumpItems.Add(MovieTab);
            jumplist.JumpItems.Add(TVTab);
            jumplist.JumpItems.Add(OptionsTab);
            jumplist.ShowFrequentCategory = false;
            jumplist.ShowRecentCategory = false;
            JumpList.SetJumpList(Application.Current, jumplist);
        }
        #endregion
        
        #region Notify Icon Routines

        //public bool showballoontip = true;
        //void notifyIcon_Click(object sender, EventArgs e)
        //{
        //    App_WakeUp();
        //}
        //void mnuExit_Click(object sender, EventArgs e)
        //{
        //    this.Shutdown();
        //}
        //private void SetBalloonTipStatus(object sender, EventArgs e)
        //{
        //    showballoontip = false;
        //}
        //void SetUpNotifyIcon()
        //{
        //    notifyIcon = new System.Windows.Forms.NotifyIcon();
        //    notifyIcon.Icon = new System.Drawing.Icon(ResourceAssembly.GetManifestResourceStream("MediaScoutGUI.Resources.tvscouticon.ico"));
        //    notifyIcon.DoubleClick += new EventHandler(notifyIcon_Click);
        //    notifyIcon.Visible = true;
        //    notifyIcon.BalloonTipClicked += new EventHandler(SetBalloonTipStatus);

        //    //Add the Context menu to the Notify Icon Object
        //    System.Windows.Forms.ToolStripMenuItem mnuExit = new System.Windows.Forms.ToolStripMenuItem();
        //    mnuExit.Text = "Exit";
        //    mnuExit.Click += new EventHandler(mnuExit_Click);
        //    notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
        //    notifyIcon.ContextMenuStrip.Items.Add(mnuExit);

        //}

        #endregion
        
        public MediaScoutApp()
            : base()
        {
            this.MainWindow = MyWindow;            
        }

        #region App Routines

        public void App_WakeUp()
        {
            MyWindow.Show();
            MyWindow.Activate();
            if (MyWindow.WindowState == WindowState.Minimized)
                MyWindow.WindowState = WindowState.Normal;
        }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            //SplashScreen splashScreen = new SplashScreen("Resources/tvscoutsplash.png");            
            //splashScreen.Show(false, false);
            About ab = new About(true);
            ab.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ab.Topmost = true;
            ab.Show();
                        
            base.OnStartup(e);            
            //SetUpNotifyIcon();
            SetUpJumpList();
            ProcessArgs(e.Args, true);
            MyWindow = new MainWindow(SelectedTabIndex);
            this.MainWindow = MyWindow;
            
            if (MediaScoutGUI.Properties.Settings.Default.FirstRun)
            {
                MediaScoutGUI.Properties.Settings.Default.Reset();
                ab.Close();
                Wizard.Wizard wizard = new Wizard.Wizard();
                wizard.ShowDialog();               
                MediaScoutGUI.Properties.Settings.Default.FirstRun = false;
            }
            MyWindow.Show();
            
            if(ab.IsLoaded)
                ab.Close();
            //splashScreen.Close(TimeSpan.FromMilliseconds(500));
        }

        //protected override void OnExit(ExitEventArgs e)
        //{            
        //    base.OnExit(e);            
        //    notifyIcon.Visible = false;
        //    notifyIcon.Dispose();
        //    notifyIcon = null;
        //}

        #endregion
        
        #region Process Arguments

        public bool ProcessArgs(string[] args, bool IsfirstInstance)
        {
            bool success = true;
            //Process Command Line Arguments Here
            foreach (String arg in args)
            {
                String opt = arg.Substring(0, arg.LastIndexOf(':'));
                switch (opt)
                {
                    case "/Tab":
                        String opttab = arg.Substring(arg.LastIndexOf(':')+1);
                        switch (opttab)
                        {
                            case "Movies":
                                SelectedTabIndex = 2;
                                break;
                            case "TVSeries":
                                SelectedTabIndex = 3;
                                break;
                            case "Options":
                                SelectedTabIndex = 0;
                                break;
                            default: success = false;
                                break;
                        }
                        if (!IsfirstInstance)
                            MyWindow.tcTabs.SelectedIndex = SelectedTabIndex;
                        break;
                    case "/Cancel":
                        if (!IsfirstInstance)
                            MyWindow.CancelOperation(null);
                        else
                            success = false;
                        break;
                    case "/CancelAll":
                        if (!IsfirstInstance)
                            MyWindow.AbortAllThreads();
                        else
                            success = false;
                        break;
                    default: success = false;
                        break;
                }
            }
            if (!success)
                MessageBox.Show("Invalid Arguments");
            return success;
        }
        
        #endregion
    }
}
