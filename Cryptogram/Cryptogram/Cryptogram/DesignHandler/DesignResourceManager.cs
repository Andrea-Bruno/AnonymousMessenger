using System;
using WhiteLabelDesignComponents.Styles;
using Xamarin.Forms;

namespace Anonymous.DesignHandler
{
    public class DesignResourceManager
    {
        public DesignResourceManager()
        {
        }

        public static ResourceDictionary ChangeTheme(bool _isDarkTheme)
        {
            return WhiteLabelDesignComponents.DesignElementsHandler.ChangeTheme(_isDarkTheme);
        }

        public static Color GetColorFromStyle(string color)
        {
            return WhiteLabelDesignComponents.DesignElementsHandler.GetColorFromStyle(color);
        }

        public static ImageSource GetImageSource(string Source)
        {
             if (!Source.EndsWith(".png"))
                Source += ".png";
            return WhiteLabelDesignComponents.DesignElementsHandler.GetImageSource(Source);
        }

    }
}
