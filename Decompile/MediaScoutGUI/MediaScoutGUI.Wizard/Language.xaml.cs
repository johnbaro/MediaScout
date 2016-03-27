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
    public partial class Language : Page, IComponentConnector
	{
		public Language()
		{
			this.InitializeComponent();
			this.lstLanguages.SelectedIndex = Settings.Default.language;
		}

		private void btnNext_Click(object sender, RoutedEventArgs e)
		{
			Settings.Default.language = this.lstLanguages.SelectedIndex;
			Settings.Default.Save();
			(base.Parent as Window).DialogResult = new bool?(true);
		}
	}
}
