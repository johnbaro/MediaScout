using MediaScoutGUI.Properties;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;

namespace MediaScoutGUI.Wizard
{
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public class Folders : Page, IComponentConnector
	{
		internal Button btnNext;

		internal Label label1;

		internal Button btnSetTVFolders;

		internal Button btnSetMovieFolders;

		private bool _contentLoaded;

		public Folders()
		{
			this.InitializeComponent();
		}

		private void btnSetTVFolders_Click(object sender, RoutedEventArgs e)
		{
			FoldersDialog foldersDialog = new FoldersDialog(false);
			foldersDialog.Owner = (base.Parent as NavigationWindow);
			if (foldersDialog.ShowDialog() == true)
			{
				if (Settings.Default.TVFolders == null)
				{
					Settings.Default.TVFolders = new StringCollection();
				}
				else
				{
					Settings.Default.TVFolders.Clear();
				}
				foreach (string value in ((IEnumerable)foldersDialog.lstFolders.Items))
				{
					Settings.Default.TVFolders.Add(value);
				}
				Settings.Default.Save();
			}
		}

		private void btnSetMovieFolders_Click(object sender, RoutedEventArgs e)
		{
			FoldersDialog foldersDialog = new FoldersDialog(true);
			foldersDialog.Owner = (base.Parent as NavigationWindow);
			if (foldersDialog.ShowDialog() == true)
			{
				if (Settings.Default.MovieFolders == null)
				{
					Settings.Default.MovieFolders = new StringCollection();
				}
				else
				{
					Settings.Default.MovieFolders.Clear();
				}
				foreach (string value in ((IEnumerable)foldersDialog.lstFolders.Items))
				{
					Settings.Default.MovieFolders.Add(value);
				}
				Settings.Default.Save();
			}
		}

		private void btnNext_Click(object sender, RoutedEventArgs e)
		{
			Metadata root = new Metadata();
			base.NavigationService.Navigate(root);
		}

		[DebuggerNonUserCode]
		public void InitializeComponent()
		{
			if (this._contentLoaded)
			{
				return;
			}
			this._contentLoaded = true;
			Uri resourceLocator = new Uri("/MediaScoutGUI;component/wizard/folders.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}

		[EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				this.btnNext = (Button)target;
				this.btnNext.Click += new RoutedEventHandler(this.btnNext_Click);
				return;
			case 2:
				this.label1 = (Label)target;
				return;
			case 3:
				this.btnSetTVFolders = (Button)target;
				this.btnSetTVFolders.Click += new RoutedEventHandler(this.btnSetTVFolders_Click);
				return;
			case 4:
				this.btnSetMovieFolders = (Button)target;
				this.btnSetMovieFolders.Click += new RoutedEventHandler(this.btnSetMovieFolders_Click);
				return;
			default:
				this._contentLoaded = true;
				return;
			}
		}
	}
}
