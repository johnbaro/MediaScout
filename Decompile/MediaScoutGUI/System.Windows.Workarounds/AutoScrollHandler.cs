using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Data;

namespace System.Windows.Workarounds
{
	public class AutoScrollHandler : DependencyObject, IDisposable
	{
		public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(AutoScrollHandler), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(AutoScrollHandler.ItemsSourcePropertyChanged)));

		private System.Windows.Controls.ListView Target;

		public IEnumerable ItemsSource
		{
			get
			{
				return (IEnumerable)base.GetValue(AutoScrollHandler.ItemsSourceProperty);
			}
			set
			{
				base.SetValue(AutoScrollHandler.ItemsSourceProperty, value);
			}
		}

		public AutoScrollHandler(System.Windows.Controls.ListView target)
		{
			this.Target = target;
			Binding binding = new Binding("ItemsSource");
			binding.Source = this.Target;
			BindingOperations.SetBinding(this, AutoScrollHandler.ItemsSourceProperty, binding);
		}

		public void Dispose()
		{
			BindingOperations.ClearBinding(this, AutoScrollHandler.ItemsSourceProperty);
		}

		private static void ItemsSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			((AutoScrollHandler)o).ItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
		}

		private void ItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
		{
			INotifyCollectionChanged notifyCollectionChanged = oldValue as INotifyCollectionChanged;
			if (notifyCollectionChanged != null)
			{
				notifyCollectionChanged.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.Collection_CollectionChanged);
			}
			notifyCollectionChanged = (newValue as INotifyCollectionChanged);
			if (notifyCollectionChanged != null)
			{
				notifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(this.Collection_CollectionChanged);
			}
		}

		private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems == null || e.NewItems.Count < 1)
			{
				return;
			}
			this.Target.ScrollIntoView(e.NewItems[e.NewItems.Count - 1]);
		}
	}
}
