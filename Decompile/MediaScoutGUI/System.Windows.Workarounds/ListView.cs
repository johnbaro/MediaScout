using System;
using System.Windows.Controls;

namespace System.Windows.Workarounds
{
	public static class ListView
	{
		public static readonly DependencyProperty AutoScrollProperty = DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(System.Windows.Controls.ListView), new PropertyMetadata(false));

		public static readonly DependencyProperty AutoScrollHandlerProperty = DependencyProperty.RegisterAttached("AutoScrollHandler", typeof(AutoScrollHandler), typeof(System.Windows.Controls.ListView));

		public static bool GetAutoScroll(System.Windows.Controls.ListView instance)
		{
			return (bool)instance.GetValue(ListView.AutoScrollProperty);
		}

		public static void SetAutoScroll(System.Windows.Controls.ListView instance, bool value)
		{
			AutoScrollHandler autoScrollHandler = (AutoScrollHandler)instance.GetValue(ListView.AutoScrollHandlerProperty);
			if (autoScrollHandler != null)
			{
				autoScrollHandler.Dispose();
				instance.SetValue(ListView.AutoScrollHandlerProperty, null);
			}
			instance.SetValue(ListView.AutoScrollProperty, value);
			if (value)
			{
				instance.SetValue(ListView.AutoScrollHandlerProperty, new AutoScrollHandler(instance));
			}
		}
	}
}
