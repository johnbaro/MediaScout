using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace MediaScout.GUI.Controls
{
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public partial class ImageControl : UserControl, IComponentConnector
	{
		private bool setLoading;

		public ImageSource source;

		public string filename;

		public Stretch stretch = Stretch.Fill;

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
	}
}
