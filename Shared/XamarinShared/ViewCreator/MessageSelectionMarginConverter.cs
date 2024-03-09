using System;
using System.Globalization;
using Xamarin.Forms;

namespace XamarinShared.ViewCreator
{
    public class MessageSelectionMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
                return new Thickness(System.Convert.ToBoolean(value)? 45 : 0, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
