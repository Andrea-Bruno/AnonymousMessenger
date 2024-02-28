using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using AnonymousWhiteLabel.UWP;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Xamarin.Forms;


[assembly: Dependency(typeof(ImageResizeService))]
namespace AnonymousWhiteLabel.UWP
{
	public class ImageResizeService : IImageResizeService
	{
		public byte[] ResizeImage(byte[] imageData, float width, float height)
		{
			return ResizeImageAsync(imageData, (int)width, (int)height).Result;
		}
		private async Task<byte[]> ResizeImageAsync(byte[] imageData, float width, float height)
		{
			//note: this function dont work inside the MainThread!
			byte[] resizedData;
			using (var streamIn = new MemoryStream(imageData))
			{
				using (var imageStream = streamIn.AsRandomAccessStream())
				{
					var decoder = await BitmapDecoder.CreateAsync(imageStream);
					var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
					using (var resizedStream = new InMemoryRandomAccessStream())
					{
						var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, resizedStream);
						encoder.SetSoftwareBitmap(softwareBitmap);
						encoder.BitmapTransform.ScaledWidth = (uint)height;
						encoder.BitmapTransform.ScaledHeight = (uint)width;
						encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
						await encoder.FlushAsync();
						resizedStream.Seek(0);
						resizedData = new byte[resizedStream.Size];
						await resizedStream.ReadAsync(resizedData.AsBuffer(), (uint)resizedStream.Size, InputStreamOptions.None);
					}
				}
			}
			return resizedData;
		}

		//private async Task<byte[]> Resize(byte[] imageData, int width, int height)
		//{
		//	using (var memStream = new MemoryStream(imageData))
		//	{
		//		IRandomAccessStream imageStream = memStream.AsRandomAccessStream();
		//		BitmapDecoder decoder = await BitmapDecoder.CreateAsync(imageStream);
		//		SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
		//		using (imageStream)
		//		{
		//			using (var dest = new InMemoryRandomAccessStream())
		//			{
		//				BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(dest, decoder);
		//				encoder.SetSoftwareBitmap(softwareBitmap);
		//				encoder.BitmapTransform.ScaledWidth = (uint)height;
		//				encoder.BitmapTransform.ScaledHeight = (uint)width;
		//				encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
		//				await encoder.FlushAsync();
		//				dest.Seek(0);
		//				var outBuffer = new byte[dest.Size];
		//				await dest.ReadAsync(outBuffer.AsBuffer(), (uint)dest.Size, InputStreamOptions.None);
		//				return outBuffer;
		//			}
		//		}
		//	}
		//}

	}
}
