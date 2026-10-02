using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Cryptogram
{
    public class ContactAvatarIsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
	        return value is null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
