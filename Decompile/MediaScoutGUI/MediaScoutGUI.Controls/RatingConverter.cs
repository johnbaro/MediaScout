using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaScoutGUI.Controls
{
	public class RatingConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (decimal)value / 10m;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			decimal num = (decimal)((double)value * 10.0);
			return num;
		}
	}
}
