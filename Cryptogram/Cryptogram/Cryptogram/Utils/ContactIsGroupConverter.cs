using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Cryptogram
{
    public class ContactIsGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            return !System.Convert.ToBoolean(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
