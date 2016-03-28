using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MediaScout.GUI.Controls
{
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public partial class zoomSlider : UserControl, INotifyPropertyChanged, IComponentConnector
	{

		public event PropertyChangedEventHandler PropertyChanged;

		public double Value
		{
			get
			{
				return this.zoomslider.Value;
			}
			set
			{
				this.zoomslider.Value = value;
				this.NotifyPropertyChanged("Value");
			}
		}

		private void NotifyPropertyChanged(string info)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}

		public void ResetZoom()
		{
			this.Value = 1.0;
		}

		public zoomSlider()
		{
			this.InitializeComponent();
			this.lblZoom.DataContext = this;
		}

		private void zoomslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			this.Value = e.NewValue;
		}
	}
}
