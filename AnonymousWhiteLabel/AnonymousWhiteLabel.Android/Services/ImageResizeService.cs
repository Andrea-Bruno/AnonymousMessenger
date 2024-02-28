using Android.Graphics;
using System.IO;
using AnonymousWhiteLabel.Droid;
using Xamarin.Forms;


[assembly: Dependency(typeof(ImageResizeService))]
namespace AnonymousWhiteLabel.Droid
{
	public class ImageResizeService : IImageResizeService
	{
		public byte[] ResizeImage(byte[] imageData, float width, float height)
		{
			// Load the bitmap
			var originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
			var resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)width, (int)height, false);
			var ms = new MemoryStream();
			resizedImage.Compress(Bitmap.CompressFormat.Png, 100, ms);
			return ms.ToArray();
		}
	}
}
