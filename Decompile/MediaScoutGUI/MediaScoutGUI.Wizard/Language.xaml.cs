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
		internal Button btnNext;

		internal Label label1;

		internal ComboBox lstLanguages;

		private bool _contentLoaded;

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

		[DebuggerNonUserCode]
		public void InitializeComponent()
		{
			if (this._contentLoaded)
			{
				return;
			}
			this._contentLoaded = true;
			Uri resourceLocator = new Uri("/MediaScoutGUI.Wizard/Language.xaml", UriKind.Relative);
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
				this.lstLanguages = (ComboBox)target;
				return;
			default:
				this._contentLoaded = true;
				return;
			}
		}
	}
}
