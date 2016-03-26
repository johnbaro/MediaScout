using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Linq;
using System.Windows;

namespace MediaScoutGUI
{
	public sealed class SingleInstanceManager : WindowsFormsApplicationBase
	{
		public MediaScoutApp App
		{
			get;
			private set;
		}

		[STAThread]
		public static void Main(string[] args)
		{
			new SingleInstanceManager().Run(args);
		}

		public SingleInstanceManager()
		{
			base.IsSingleInstance = true;
		}

		protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs e)
		{
			try
			{
				this.App = new MediaScoutApp();
				this.App.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
				this.App.Run();
			}
			catch (Exception ex)
			{
				this.App.SaveAppLog();
				this.App.SaveSettingLog();
				this.App.SaveCrashLog(ex);
			}
			return false;
		}

		protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
		{
			base.OnStartupNextInstance(eventArgs);
			this.App.App_WakeUp();
			this.App.ProcessArgs(eventArgs.CommandLine.ToArray<string>(), false);
		}
	}
}
