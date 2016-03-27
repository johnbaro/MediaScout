using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MediaScoutGUI.Controls
{
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public class zoomSlider : UserControl, INotifyPropertyChanged, IComponentConnector
	{
		internal ComboBox lblZoom;

		internal Slider zoomslider;

		private bool _contentLoaded;

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

		[DebuggerNonUserCode]
		public void InitializeComponent()
		{
			if (this._contentLoaded)
			{
				return;
			}
			this._contentLoaded = true;
			Uri resourceLocator = new Uri("/MediaScoutGUI.Controls/zoomSlider.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}

		[EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				this.lblZoom = (ComboBox)target;
				return;
			case 2:
				this.zoomslider = (Slider)target;
				this.zoomslider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.zoomslider_ValueChanged);
				return;
			default:
				this._contentLoaded = true;
				return;
			}
		}
	}
}
