using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;

namespace MediaScoutGUI.Controls
{
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (((int)(((double)value) * 100)).ToString() + "%");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String val = value as String;
            if (String.IsNullOrEmpty(val))
                val = "100%";

            String z = val.Replace("%", "");
            double zoom = Double.Parse(z) / 100;

            if (zoom < 0.5)
                zoom = 0.5;
            else if (zoom > 2.0)
                zoom = 2.0;
            return zoom;
        }
    }
}
