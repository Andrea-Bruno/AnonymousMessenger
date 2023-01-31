using System;
using WhiteLabelDesignComponents.Styles;
using Xamarin.Forms;

namespace WhiteLabelDesignComponents
{
    public class DesignElementsHandler
    {
        private static string SourceBasePath = typeof(DesignElementsHandler).Namespace + ".Image.";

        public static ResourceDictionary ChangeTheme()
        {
            return new Theme();
        }

        public static Color GetColorFromStyle(string color)
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

        public static ImageSource GetImageSource(string Source)
        {
            if (Source == null) return null;
            return ImageSource.FromResource(SourceBasePath + Source);
        }

    }
}
