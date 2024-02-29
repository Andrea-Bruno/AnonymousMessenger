using System;
using WhiteLabelDesignComponents;
using Xamarin.Forms;

namespace AnonymousWhiteLabel.DesignHandler
{
    public class DesignResourceManager
    {
        public DesignResourceManager()
        {
        }

        public static ResourceDictionary ChangeTheme()
        {
            return DesignElementsHandler.ChangeTheme();
        }

        public static Color GetColorFromStyle(string color)
        {
            return DesignElementsHandler.GetColorFromStyle(color);
        }

        public static ImageSource GetImageSource(string Source)
        {
            if (!Source.EndsWith(".png"))
                Source += ".png";
            return DesignElementsHandler.GetImageSource(Source);
        }

    }
}
