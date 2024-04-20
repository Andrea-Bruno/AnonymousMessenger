using System;
using System.Globalization;
using Xamarin.Forms;

namespace Anonymous
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
