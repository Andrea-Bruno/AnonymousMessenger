using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace MessageCompose.Services
{
    public class PermissionManager
    {
        public static async Task<bool> CheckStoragePermission()
        {
            var status = await CheckAndRequestPermissionAsync(new StorageRead());
            if (status != PermissionStatus.Granted)
            {
                await Toast.Make(Localization.Resources.Dictionary.StoragePermissionIsNeeded).Show();
                return false;
            }
            return true;
        }

        public static async Task<bool> CheckMicrophonePermission()
        {
            var status = await CheckAndRequestPermissionAsync(new Microphone());
            if (status != PermissionStatus.Granted)
            {
                await Toast.Make(Localization.Resources.Dictionary.MicrophonePermissionIsNeeded).Show();
                return false;
            }
            return true;
        }

        public static async Task<bool> CheckCameraPermission()
        {
            var status = await CheckAndRequestPermissionAsync(new Camera());
            if (status != PermissionStatus.Granted)
            {
                await Toast.Make(Localization.Resources.Dictionary.CameraPermissionIsNeeded).Show();
                return false;
            }
            return true;
        }

        public static async Task<bool> CheckLocationPermission()
        {
            var status = await CheckAndRequestPermissionAsync(new LocationWhenInUse());
            if (status != PermissionStatus.Granted)
            {
                //C  Application.Current?.MainPage?.DisplayToastAsync(Localization.Resources.Dictionary.LocationPermissionDidNotGranted);
                return false;
            }
            return true;

        }

        public static async Task<bool> CheckContactPermission()
        {
            var status = await CheckAndRequestPermissionAsync(new ContactsRead());
            if (status != PermissionStatus.Granted)
            {
                await Toast.Make(Localization.Resources.Dictionary.ContactPermissionIsNeeded).Show();
                return false;
            }
            return true;
        }


        public static async Task<PermissionStatus> CheckAndRequestPermissionAsync<T>(T permission) where T : BasePermission
        {
            var status = await permission.CheckStatusAsync();
            if (status != PermissionStatus.Granted)
            {
                status = await permission.RequestAsync();
            }

            return status;
        }
    }
}
