using System;
using System.Threading.Tasks;
using Anonymous.iOS.Services;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Anonymous.iOS.Services
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
