using Foundation;
using System;
using System.IO;
using System.Threading.Tasks;
using AnonymousWhiteLabel.iOS;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(PhotoPickerService))]
namespace AnonymousWhiteLabel.iOS
{
	public class PhotoPickerService : IPhotoPickerService
	{
		private TaskCompletionSource<Stream> taskCompletionSource;
		private UIImagePickerController imagePicker;

		public Task<Stream> GetImageStreamAsync()
		{
			// Create and define UIImagePickerController
			imagePicker = new UIImagePickerController
			{
				SourceType = UIImagePickerControllerSourceType.PhotoLibrary,
				MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary)
			};

			// Set event handlers
			imagePicker.FinishedPickingMedia += OnImagePickerFinishedPickingMedia;
			imagePicker.Canceled += OnImagePickerCancelled;

			// Present UIImagePickerController;
			UIWindow window = UIApplication.SharedApplication.KeyWindow;
			var viewController = window.RootViewController;
			viewController.PresentModalViewController(imagePicker, true);

			// Return Task object
			taskCompletionSource = new TaskCompletionSource<Stream>();
			return taskCompletionSource.Task;
		}

		private void OnImagePickerFinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs args)
		{
			UIImage image = args.EditedImage ?? args.OriginalImage;

			if (image != null)
			{
				// Convert UIImage to .NET Stream object
				NSData data = args.ReferenceUrl.PathExtension.Equals("PNG") || args.ReferenceUrl.PathExtension.Equals("png")
										? image.AsPNG()
										: image.AsJPEG(1);
				Stream stream = data.AsStream();

				UnregisterEventHandlers();

				// Set the Stream as the completion of the Task
				taskCompletionSource.SetResult(stream);
			}
			else
			{
				UnregisterEventHandlers();
				taskCompletionSource.SetResult(null);
			}
			imagePicker.DismissModalViewController(true);
		}

		private void OnImagePickerCancelled(object sender, EventArgs args)
		{
			UnregisterEventHandlers();
			taskCompletionSource.SetResult(null);
			imagePicker.DismissModalViewController(true);
		}

		private void UnregisterEventHandlers()
		{
			imagePicker.FinishedPickingMedia -= OnImagePickerFinishedPickingMedia;
			imagePicker.Canceled -= OnImagePickerCancelled;
		}
	}
}
