using System;
using System.Threading.Tasks;
using Plugin.Toast;
using Xamarin.Essentials;
using static Xamarin.Essentials.Permissions;

namespace Telegraph.Services
{
    public class PermissionManager
    {

        public static async Task<bool> CheckStoragePermission()
        {
            var status = await CheckAndRequestPermissionAsync(new StorageRead());
            if (status != PermissionStatus.Granted)
            {
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.StoragePermissionIsNeeded);
                return false;
            }
            return true;
        }

        public static async Task<bool> CheckMicrophonePermission()
        {
            var status = await CheckAndRequestPermissionAsync(new Microphone());
            if (status != PermissionStatus.Granted)
            {
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.MicrophonePermissionIsNeeded);
                return false;
            }
            return true;
        }

        public static async Task<bool> CheckCameraPermission()
        {
            var status = await CheckAndRequestPermissionAsync(new Camera());
            if (status != PermissionStatus.Granted)
            {
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.CameraPermissionIsNeeded);
                return false;
            }
            return true;
        }

        public static async Task<bool> CheckLocationPermission()
        {
            var status = await CheckAndRequestPermissionAsync(new LocationWhenInUse());
            if (status != PermissionStatus.Granted)
            {
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.LocationPermissionDidNotGranted);
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
