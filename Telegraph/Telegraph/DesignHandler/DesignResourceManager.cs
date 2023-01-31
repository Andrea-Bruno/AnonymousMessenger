using System;
using UupDesignComponents.Styles;
using Xamarin.Forms;

namespace Telegraph.DesignHandler
{
    public class DesignResourceManager
    {
        public DesignResourceManager()
        {
        }

        public static ResourceDictionary ChangeTheme(bool _isDarkTheme)
        {
            return UupDesignComponents.DesignElementsHandler.ChangeTheme(_isDarkTheme);
        }

        public static Color GetColorFromStyle(string color)
        {
            return UupDesignComponents.DesignElementsHandler.GetColorFromStyle(color);
        }

        public static ImageSource GetImageSource(string Source)
        {
            if (!Source.EndsWith(".png"))
                Source += ".png";
            return UupDesignComponents.DesignElementsHandler.GetImageSource(Source);
        }

    }
}
