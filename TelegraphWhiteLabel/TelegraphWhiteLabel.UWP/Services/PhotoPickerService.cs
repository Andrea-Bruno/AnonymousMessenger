using System;
using System.IO;
using System.Threading.Tasks;
using AnonymousWhiteLabel.UWP;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Xamarin.Forms;

[assembly: Dependency(typeof(PhotoPickerService))]
namespace AnonymousWhiteLabel.UWP
{
	public class PhotoPickerService : IPhotoPickerService
	{
		public async Task<Stream> GetImageStreamAsync()
		{
			// Create and initialize the FileOpenPicker
			var openPicker = new FileOpenPicker
			{
				ViewMode = PickerViewMode.Thumbnail,
				SuggestedStartLocation = PickerLocationId.PicturesLibrary,
			};

			openPicker.FileTypeFilter.Add(".jpg");
			openPicker.FileTypeFilter.Add(".jpeg");
			openPicker.FileTypeFilter.Add(".png");

			// Get a file and return a Stream 
			var storageFile = await openPicker.PickSingleFileAsync();

			if (storageFile == null)
			{
				return null;
			}
			var raStream = await storageFile.OpenReadAsync();
			return raStream.AsStreamForRead();
		}
	}
}
