using System;
using System.Globalization;
using Xamarin.Forms;

namespace XamarinShared.ViewCreator
{
    public class FontSizeConverter : IValueConverter
    {
        public static readonly float[] FontRatios = new float[] { 0.7f, 1.0f, 1.3f, 1.6f };
        public static float DefaultSelectedFontRatio = 1f;


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
