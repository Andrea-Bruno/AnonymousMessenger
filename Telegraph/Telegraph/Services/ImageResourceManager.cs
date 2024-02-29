using System;
using Xamarin.Forms;

namespace Telegraph.Services
{
    public class ImageResourceManager
    {
        public ImageResourceManager()
        {
        }

        public static ImageSource GetImageSource(string Source)
        {
            return UupDesignComponents.DesignElementsHandler.GetImageSource(Source);
        }
    }
}
