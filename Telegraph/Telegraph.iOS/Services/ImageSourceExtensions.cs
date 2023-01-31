using System;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Telegraph.iOS.Services
{
    public static class ImageSourceExtensions
    {
        static ImageLoaderSourceHandler s_imageLoaderSourceHandler;

        static ImageSourceExtensions()
        {
            s_imageLoaderSourceHandler = new ImageLoaderSourceHandler();
        }
        public static Task<UIImage> ToUIImage(this ImageSource imageSource)
        {
            return s_imageLoaderSourceHandler.LoadImageAsync(imageSource);
        }
    }
}
