using System;
using System.Globalization;
using Xamarin.Forms;

namespace CommunityClient.Converters
{
    public class BoolToFontAttribute : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value == true ? FontAttributes.Bold : FontAttributes.None;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (FontAttributes)value == FontAttributes.Bold;
        }
    }
}
