using MediaScoutGUI.Properties;
using MediaScoutGUI.Wizard;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Shell;

namespace MediaScoutGUI
{
	public class MediaScoutApp : System.Windows.Application
	{
		public JumpList jumplist;

		private int SelectedTabIndex = 2;

		public MainWindow MyWindow
		{
			get;
			private set;
		}

		private void SetUpJumpList()
		{
			JumpTask jumpTask = new JumpTask();
			jumpTask.ApplicationPath = Assembly.GetExecutingAssembly().Location;
			jumpTask.Arguments = "/Tab:Movies";
			jumpTask.IconResourcePath = Assembly.GetExecutingAssembly().Location;
			jumpTask.IconResourceIndex = 1;
			jumpTask.Title = "Movies";
			jumpTask.Description = "Open Movies Tab";
			JumpTask jumpTask2 = new JumpTask();
			jumpTask2.ApplicationPath = Assembly.GetExecutingAssembly().Location;
			jumpTask2.Arguments = "/Tab:TVSeries";
			jumpTask2.IconResourcePath = Assembly.GetExecutingAssembly().Location;
			jumpTask2.IconResourceIndex = 2;
			jumpTask2.Title = "TV Series";
			jumpTask2.Description = "Open TV Series Tab";
			JumpTask jumpTask3 = new JumpTask();
			jumpTask3.ApplicationPath = Assembly.GetExecutingAssembly().Location;
			jumpTask3.Arguments = "/Tab:Options";
			jumpTask3.IconResourcePath = Assembly.GetExecutingAssembly().Location;
			jumpTask3.IconResourceIndex = 3;
			jumpTask3.Title = "Options";
			jumpTask3.Description = "Open Options Tab";
			this.jumplist = new JumpList();
			this.jumplist.JumpItems.Add(jumpTask);
			this.jumplist.JumpItems.Add(jumpTask2);
			this.jumplist.JumpItems.Add(jumpTask3);
			this.jumplist.ShowFrequentCategory = false;
			this.jumplist.ShowRecentCategory = false;
			JumpList.SetJumpList(System.Windows.Application.Current, this.jumplist);
		}

		public MediaScoutApp()
		{
			base.MainWindow = this.MyWindow;
		}

		public void App_WakeUp()
		{
			this.MyWindow.Show();
			this.MyWindow.Activate();
			if (this.MyWindow.WindowState == WindowState.Minimized)
			{
				this.MyWindow.WindowState = WindowState.Normal;
			}
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			try
			{
				About about = new About(true);
				about.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				about.Topmost = true;
				about.Show();
				base.OnStartup(e);
				this.SetUpJumpList();
				this.ProcessArgs(e.Args, true);
				this.MyWindow = new MainWindow(this.SelectedTabIndex);
				base.MainWindow = this.MyWindow;
				if (Settings.Default.FirstRun)
				{
					StringCollection stringCollection = new StringCollection();
					StringCollection stringCollection2 = new StringCollection();
					if (Settings.Default.TVFolders != null && Settings.Default.TVFolders.Count > 0)
					{
						stringCollection = Settings.Default.TVFolders;
					}
					else
					{
						string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MediaScout\\TVfolders.set";
						if (File.Exists(path))
						{
							StreamReader streamReader = new StreamReader(path);
							while (!streamReader.EndOfStream)
							{
								stringCollection.Add(streamReader.ReadLine());
							}
							streamReader.Close();
						}
					}
					if (Settings.Default.MovieFolders != null && Settings.Default.MovieFolders.Count > 0)
					{
						stringCollection2 = Settings.Default.MovieFolders;
					}
					else
					{
						string path2 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MediaScout\\Moviefolders.set";
						if (File.Exists(path2))
						{
							StreamReader streamReader2 = new StreamReader(path2);
							while (!streamReader2.EndOfStream)
							{
								stringCollection2.Add(streamReader2.ReadLine());
							}
							streamReader2.Close();
						}
					}
					Settings.Default.Reset();
					Settings.Default.TVFolders = stringCollection;
					Settings.Default.MovieFolders = stringCollection2;
					about.Close();
					Wizard wizard = new Wizard();
					wizard.ShowDialog();
					Settings.Default.FirstRun = false;
				}
				this.MyWindow.Show();
				if (about.IsLoaded)
				{
					about.Close();
				}
			}
			catch (Exception ex)
			{
				this.SaveAppLog();
				this.SaveSettingLog();
				this.SaveCrashLog(ex);
			}
		}

		public void SaveAppLog()
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MediaScout\\app.log";
			TextRange textRange = new TextRange(this.MyWindow.rtbLog.Document.ContentStart, this.MyWindow.rtbLog.Document.ContentEnd);
			File.WriteAllText(path, textRange.Text.Replace("\r", Environment.NewLine), Encoding.UTF8);
		}

		public void SaveSettingLog()
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MediaScout\\setting.log";
			StreamWriter streamWriter = new StreamWriter(path, false);
			foreach (SettingsPropertyValue settingsPropertyValue in Settings.Default.PropertyValues)
			{
				streamWriter.WriteLine(settingsPropertyValue.Name + ":" + settingsPropertyValue.PropertyValue.ToString());
			}
			streamWriter.Close();
		}

		public void SaveCrashLog(Exception ex)
		{
			string text = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MediaScout\\crash.log";
			StackTrace stackTrace = new StackTrace(ex);
			StreamWriter streamWriter = new StreamWriter(text, false);
			streamWriter.WriteLine("Exception : " + ex.Message);
			streamWriter.WriteLine("Exception Data : " + ex.Data);
			streamWriter.WriteLine("Inner Exception : " + ex.InnerException);
			streamWriter.WriteLine("Source : " + ex.Source);
			streamWriter.WriteLine("StackTrace : " + ex.StackTrace);
			streamWriter.WriteLine("TargetSite : " + ex.TargetSite.Name);
			streamWriter.WriteLine("Attributes :" + ex.TargetSite.Attributes);
			streamWriter.WriteLine("Details");
			for (int i = 0; i < stackTrace.FrameCount; i++)
			{
				StackFrame frame = stackTrace.GetFrame(i);
				streamWriter.WriteLine(string.Concat(new object[]
				{
					"Method : ",
					frame.GetMethod().Name,
					", Line : ",
					frame.GetFileLineNumber()
				}));
			}
			streamWriter.Close();
			System.Windows.Forms.MessageBox.Show("System Exception Occured. Crash Log saved at " + text, "MediaScout");
		}

		public bool ProcessArgs(string[] args, bool IsfirstInstance)
		{
			bool flag = true;
			int i = 0;
			while (i < args.Length)
			{
				string text = args[i];
				string text2 = text.Substring(0, text.LastIndexOf(':'));
				string a;
				if ((a = text2) == null)
				{
					goto IL_129;
				}
				if (a == "/Tab")
				{
					string text3 = text.Substring(text.LastIndexOf(':') + 1);
					string a2;
					if ((a2 = text3) == null)
					{
						goto IL_D0;
					}
					if (!(a2 == "Movies"))
					{
						if (!(a2 == "TVSeries"))
						{
							if (!(a2 == "Options"))
							{
								goto IL_D0;
							}
							this.SelectedTabIndex = 0;
						}
						else
						{
							this.SelectedTabIndex = 3;
						}
					}
					else
					{
						this.SelectedTabIndex = 2;
					}
					IL_D2:
					if (!IsfirstInstance)
					{
						this.MyWindow.tcTabs.SelectedIndex = this.SelectedTabIndex;
						goto IL_12B;
					}
					goto IL_12B;
					IL_D0:
					flag = false;
					goto IL_D2;
				}
				if (!(a == "/Cancel"))
				{
					if (!(a == "/CancelAll"))
					{
						if (!(a == "/Reset"))
						{
							goto IL_129;
						}
						if (IsfirstInstance)
						{
							Settings.Default.Reset();
						}
						else
						{
							flag = false;
						}
					}
					else if (!IsfirstInstance)
					{
						this.MyWindow.AbortAllThreads();
					}
					else
					{
						flag = false;
					}
				}
				else if (!IsfirstInstance)
				{
					this.MyWindow.CancelOperation(null);
				}
				else
				{
					flag = false;
				}
				IL_12B:
				i++;
				continue;
				IL_129:
				flag = false;
				goto IL_12B;
			}
			if (!flag)
			{
				System.Windows.MessageBox.Show("Invalid Arguments");
			}
			return flag;
		}
	}
}
