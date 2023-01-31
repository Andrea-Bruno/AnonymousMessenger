using CoreGraphics;
using System;
using System.Drawing;
using AnonymousWhiteLabel.UWP;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(ImageResizeService))]
namespace AnonymousWhiteLabel.UWP
{
	public class ImageResizeService : IImageResizeService
	{
		public byte[] ResizeImage(byte[] imageData, float width, float height)
		{
			using (UIImage originalImage = ImageFromByteArray(imageData))
			{
				UIImageOrientation orientation = originalImage.Orientation;
				//create a 24bit RGB image
				using (var deviceRGB = CGColorSpace.CreateDeviceRGB())
				{
					using (var context = new CGBitmapContext(IntPtr.Zero, (int)width, (int)height, 8, 4 * (int)width, deviceRGB, CGImageAlphaInfo.PremultipliedFirst))
					{
						var imageRect = new RectangleF(0, 0, width, height);
						// draw the image
						context.DrawImage(imageRect, originalImage.CGImage);
						var resizedImage = UIImage.FromImage(context.ToImage(), 0, orientation);
						// save the image as a PNG
						return resizedImage.AsPNG().ToArray();
					}
				}
			}
		}

		public static UIImage ImageFromByteArray(byte[] data)
		{
			if (data == null)
			{
				return null;
			}
			UIImage image;
			image = new UIImage(Foundation.NSData.FromArray(data));
			return image;
		}

	}
}
