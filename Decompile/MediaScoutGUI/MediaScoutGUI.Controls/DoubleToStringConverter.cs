using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaScoutGUI.Controls
{
	public class DoubleToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((int)((double)value * 100.0)).ToString() + "%";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string text = value as string;
			if (string.IsNullOrEmpty(text))
			{
				text = "100%";
			}
			string s = text.Replace("%", "");
			double num = double.Parse(s) / 100.0;
			if (num < 0.5)
			{
				num = 0.5;
			}
			else if (num > 2.0)
			{
				num = 2.0;
			}
			return num;
		}
	}
}
