using System;
using System.Linq;
using Microsoft.VisualBasic.ApplicationServices;

namespace MediaScoutGUI
{
    public sealed class SingleInstanceManager : WindowsFormsApplicationBase
    {        
        [STAThread]
        public static void Main(string[] args)
        { (new SingleInstanceManager()).Run(args); }

        public SingleInstanceManager()
        { IsSingleInstance = true; }

        public MediaScoutApp App { get; private set; }

        protected override bool OnStartup(StartupEventArgs e)
        {
            App = new MediaScoutApp();
            App.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
            App.Run();
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            base.OnStartupNextInstance(eventArgs);            
            App.App_WakeUp();
            App.ProcessArgs(eventArgs.CommandLine.ToArray(), false);
        }
    }
}
