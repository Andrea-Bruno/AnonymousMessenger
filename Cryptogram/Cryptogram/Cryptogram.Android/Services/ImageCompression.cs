using Android.Graphics;
using System.IO;
using Android.Media;
using Java.IO;
using Cryptogram.Droid.Services;
using Cryptogram.Services;
using Xamarin.Forms;
using Stream = System.IO.Stream;

[assembly: Dependency(typeof(ImageCompression))]

namespace Cryptogram.Droid.Services
{
    public class ImageCompression : IImageCompressionService
    {
        public ImageCompression()
        {
        }


        public byte[] CompressImage(byte[] imageData, int compressionPercentage, string destinationPath = null)
        {
            return imageData;
            var resizedImage = GetResizedImage(imageData, compressionPercentage);

            if (destinationPath == null) return resizedImage;
            var stream = new FileStream(destinationPath, FileMode.Create);
            stream.Write(resizedImage, 0, resizedImage.Length);
            stream.Flush();
            stream.Close();
            return resizedImage;
        }

        private static byte[] GetResizedImage(byte[] imageData, int compressionPercentage)
        {
            var originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);


            using var ms = new MemoryStream();
            var result = originalImage.Compress(Bitmap.CompressFormat.Jpeg, compressionPercentage, ms);
            ms.Seek(0, SeekOrigin.Begin);
            CopyExif(imageData, ms);
            //Bitmap decoded = BitmapFactory.DecodeStream(ms);

            //decoded = ExifRotateBitmap(imageData, decoded);
            return ms.ToArray();
        }


        private static void CopyExif(byte[] original, Stream copy)
        {
            var oldExif = new ExifInterface(new MemoryStream(original));
            var attributes = new[]
            {
                ExifInterface.TagOrientation,
         
            };

            var newExif = new ExifInterface(copy);
            foreach (var t in attributes)
            {
                var value = oldExif.GetAttribute(t);
                if (value != null)
                    newExif.SetAttribute(t, value);
            }

        }

        private static Bitmap ExifRotateBitmap(byte[] originalImage, Bitmap bitmap)
        {
            if (bitmap == null)
                return null;

            var exif = new ExifInterface(new MemoryStream(originalImage));
            var rotation = exif.GetAttributeInt(ExifInterface.TagOrientation, (int)Orientation.Normal);
            var rotationInDegrees = ExifToDegrees(rotation);
            if (rotationInDegrees == 0)
                return bitmap;

            using (var matrix = new Matrix())
            {
                matrix.PreRotate(rotationInDegrees);
                Bitmap oriented =  Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
                oriented.Recycle();
                return oriented;
            }
        }

        private static int ExifToDegrees(int exifOrientation)
        {
            switch (exifOrientation)
            {
                case (int)Orientation.Rotate90:
                    return 90;
                case (int)Orientation.Rotate180:
                    return 180;
                case (int)Orientation.Rotate270:
                    return 270;
                default:
                    return 0;
            }
        }
    }
}