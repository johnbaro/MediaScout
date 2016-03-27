using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace MediaScoutGUI.Controls
{
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public partial class ImageControl : UserControl, IComponentConnector
	{
		private bool setLoading;

		public ImageSource source;

		public string filename;

		public Stretch stretch = Stretch.Fill;

		internal Image myImage;

		internal ProgressBar LoadingPB;

		private bool _contentLoaded;

		public bool SetLoading
		{
			get
			{
				return this.setLoading;
			}
			set
			{
				this.setLoading = value;
				if (this.setLoading)
				{
					this.LoadingPB.Visibility = Visibility.Visible;
					return;
				}
				this.LoadingPB.Visibility = Visibility.Collapsed;
			}
		}

		public ImageSource Source
		{
			get
			{
				return this.source;
			}
			set
			{
				Image arg_10_0 = this.myImage;
				this.source = value;
				arg_10_0.Source = value;
				this.SetLoading = false;
			}
		}

		public string Filename
		{
			get
			{
				return this.filename;
			}
			set
			{
				this.filename = value;
			}
		}

		public Stretch Stretch
		{
			get
			{
				return this.stretch;
			}
			set
			{
				Image arg_10_0 = this.myImage;
				this.stretch = value;
				arg_10_0.Stretch = value;
			}
		}

		public ImageControl()
		{
			this.InitializeComponent();
		}

		[DebuggerNonUserCode]
		public void InitializeComponent()
		{
			if (this._contentLoaded)
			{
				return;
			}
			this._contentLoaded = true;
			Uri resourceLocator = new Uri("/MediaScoutGUI.Controls/ImageControl.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}

		[EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				this.myImage = (Image)target;
				return;
			case 2:
				this.LoadingPB = (ProgressBar)target;
				return;
			default:
				this._contentLoaded = true;
				return;
			}
		}
	}
}
