using System;
using System.Threading.Tasks;
using CommunityToolkit.Maui;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;
using Microsoft.Maui.Networking;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace Cryptogram.Services
{
    public class PermissionManager
    {

        public static async Task<bool> CheckStoragePermission()
        {
            var status = await CheckAndRequestPermissionAsync(new StorageRead());
            if (status != PermissionStatus.Granted)
            {
                Application.Current?.MainPage?.DisplayToastAsync(Localization.Resources.Dictionary.StoragePermissionIsNeeded);
                return false;
            }
            return true;
        }

        public static async Task<bool> CheckMicrophonePermission()
        {
            var status = await CheckAndRequestPermissionAsync(new Microphone());
            if (status != PermissionStatus.Granted)
            {
                Application.Current?.MainPage?.DisplayToastAsync(Localization.Resources.Dictionary.MicrophonePermissionIsNeeded);
                return false;
            }
            return true;
        }

        public static async Task<bool> CheckCameraPermission()
        {
            var status = await CheckAndRequestPermissionAsync(new Camera());
            if (status != PermissionStatus.Granted)
            {
                Application.Current?.MainPage?.DisplayToastAsync(Localization.Resources.Dictionary.CameraPermissionIsNeeded);
                return false;
            }
            return true;
        }

        public static async Task<bool> CheckLocationPermission()
        {
            var status = await CheckAndRequestPermissionAsync(new LocationWhenInUse());
            if (status != PermissionStatus.Granted)
            {
                Application.Current?.MainPage?.DisplayToastAsync(Localization.Resources.Dictionary.LocationPermissionDidNotGranted);
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
