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
    public partial class Folders : Page, IComponentConnector
	{
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
	}
}
