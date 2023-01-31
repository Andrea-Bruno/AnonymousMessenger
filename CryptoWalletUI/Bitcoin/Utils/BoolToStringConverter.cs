
using System;
using Xamarin.Forms;

namespace CryptoWalletUI.Bitcoin.Utils
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? "Confirmed" : "Unconfirmed";
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (string)value == "Confirmed";
        }
    }
}
