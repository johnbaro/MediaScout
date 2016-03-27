using MediaScoutGUI.Properties;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MediaScoutGUI.Wizard
{
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public partial class Metadata : Page, IComponentConnector
	{
		public Metadata()
		{
			this.InitializeComponent();
		}

		private void btnNext_Click(object sender, RoutedEventArgs e)
		{
			Language root = new Language();
			base.NavigationService.Navigate(root);
			Settings.Default.SaveXBMCMeta = this.chkSaveXBMCMeta.IsChecked.Value;
			Settings.Default.SaveMyMoviesMeta = this.chkSaveMMMeta.IsChecked.Value;
			Settings.Default.Save();
		}
	}
}
