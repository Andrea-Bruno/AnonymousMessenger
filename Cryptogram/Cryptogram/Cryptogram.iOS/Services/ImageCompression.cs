using Foundation;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Anonymous.iOS.Services;
using Anonymous.Services;
using UIKit;
using Xamarin.Forms;


[assembly: Dependency(typeof(ImageCompression))]
namespace Anonymous.iOS.Services
{
    public class ImageCompression : IImageCompressionService
    {
        public ImageCompression()
        { }

        public byte[] CompressImage(byte[] imageData,  int compressionPercentage, string destinationPath=null)
        {
            UIImage originalImage = ImageFromByteArray(imageData);

            if (originalImage != null )
            {
                nfloat compressionQuality = (nfloat)(compressionPercentage / 100.0);
                var resizedImage = originalImage.AsJPEG(compressionQuality).ToArray();
                if ( destinationPath == null) return resizedImage;
                var stream = new FileStream(destinationPath, FileMode.Create);
                stream.Write(resizedImage, 0, resizedImage.Length);
                stream.Flush();
                stream.Close();
                return resizedImage;
            }
            return imageData;
        }

        private static UIImage ImageFromByteArray(byte[] data)
        {

            if (data == null) return null;

            UIImage image;
            try
            {
                image = new UIImage(NSData.FromArray(data));
            }
            catch (Exception e)
            {

                // new Logger().LogError(new Exception("PhotoProcessingService:", e));
                return null;
            }
            return image;
        }

    }
}