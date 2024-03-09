using System;
using WhiteLabelDesignComponents.Styles;
using Xamarin.Forms;

namespace WhiteLabelDesignComponents
{
    public class DesignElementsHandler
    {
        private static bool IsDarkTheme;
        private static string SourceBasePath;
        private static string CommonSourceBasePath;
        public DesignElementsHandler()
        {
        }

        public static ResourceDictionary ChangeTheme(bool? _isDarkTheme = null)
        {
            Type currentClassType = typeof(DesignElementsHandler);
            CommonSourceBasePath = currentClassType.Namespace + ".Image.Common.";
            if (_isDarkTheme == null)
                _isDarkTheme = !IsDarkTheme;
            IsDarkTheme = (bool)_isDarkTheme;
            if (!IsDarkTheme)
            {
                SourceBasePath = currentClassType.Namespace + ".Image.LightImage.";
                return new LightTheme();
            }

            else
            {
                SourceBasePath = currentClassType.Namespace + ".Image.DarkImage.";
                return new DarkTheme();
            }
        }

        public static Color GetColorFromStyle(string color)
        {

            {
                try
                {
                    return (Color)Application.Current.Resources[color];
                }
                catch (Exception e)
                {
                    return Color.FromHex("#14131A");
                }
            }
        }

        public static ImageSource GetImageSource(string Source)
        {
            if (Source == null) return null;
            return ImageSource.FromResource(SourceBasePath + Source) ?? ImageSource.FromResource(CommonSourceBasePath + Source); // if image not found in theme packages, take from common
        }

    }
}
