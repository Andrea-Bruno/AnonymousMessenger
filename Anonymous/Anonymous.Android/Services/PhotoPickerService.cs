using System;
using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Telegraph.Droid.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(PhotoPickerService))]
namespace Telegraph.Droid.Services
{
    public class PhotoPickerService : IPhotoPickerService
    {
        public Task<Stream> GetImageStreamAsync()
        {
            // Define the Intent for getting images
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);

            // Start the picture-picker activity (resumes in MainActivity.cs)
            MainActivity.Context.StartActivityForResult(
                Intent.CreateChooser(intent, "Select Picture"),
                MainActivity.PickImageId);

            // Save the TaskCompletionSource object as a MainActivity property
            MainActivity.Context.PickImageTaskCompletionSource = new TaskCompletionSource<Stream>();

            // Return Task object
            return MainActivity.Context.PickImageTaskCompletionSource.Task;
        }
    }
}