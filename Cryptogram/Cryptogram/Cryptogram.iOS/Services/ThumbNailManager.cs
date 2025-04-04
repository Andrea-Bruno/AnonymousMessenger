using System;
using AVFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;
using MessageCompose.Services;
using Cryptogram.iOS.Services;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(ThumbNailManager))]
namespace Cryptogram.iOS.Services
{
    public class ThumbNailManager : IThumbnailService
    {
        public ThumbNailManager()
        {
        }
        public byte[] GenerateThumbImage(string url, long usecond)
        {
            var asset = AVAsset.FromUrl(NSUrl.FromFilename(url));
            AVAssetImageGenerator imageGenerator = AVAssetImageGenerator.FromAsset(asset);
            imageGenerator.AppliesPreferredTrackTransform = true;

            CMTime cmTime = new CMTime(1, 60);
            var imageRef = imageGenerator.CopyCGImageAtTime(cmTime, out _, out _);

            if (imageRef == null)
                return null;

            var image = UIImage.FromImage(imageRef);

            return image.AsPNG().ToArray();
        }
    }
}
