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
	public class Metadata : Page, IComponentConnector
	{
		internal CheckBox chkSaveXBMCMeta;

		internal CheckBox chkSaveMMMeta;

		internal Button btnNext;

		internal Label label1;

		internal TextBlock label2;

		private bool _contentLoaded;

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

		[DebuggerNonUserCode]
		public void InitializeComponent()
		{
			if (this._contentLoaded)
			{
				return;
			}
			this._contentLoaded = true;
			Uri resourceLocator = new Uri("/MediaScoutGUI;component/wizard/metadata.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}

		[EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				this.chkSaveXBMCMeta = (CheckBox)target;
				return;
			case 2:
				this.chkSaveMMMeta = (CheckBox)target;
				return;
			case 3:
				this.btnNext = (Button)target;
				this.btnNext.Click += new RoutedEventHandler(this.btnNext_Click);
				return;
			case 4:
				this.label1 = (Label)target;
				return;
			case 5:
				this.label2 = (TextBlock)target;
				return;
			default:
				this._contentLoaded = true;
				return;
			}
		}
	}
}
