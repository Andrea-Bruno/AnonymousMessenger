using AnonymousWhiteLabel.UWP.Services;
using System;
using Windows.Devices.Geolocation;
using Windows.System;
using Xamarin.Forms;

[assembly: Dependency(typeof(GpsService))]
namespace AnonymousWhiteLabel.UWP.Services
{
    public class GpsService : IGpsService
    {
        public bool IsGpsEnable()
        {
            var accessStatus = Geolocator.RequestAccessAsync();
            
            //switch (accessStatus)
            //{
            //    case GeolocationAccessStatus.:

            //}


            return true;
        }
        public void OpenSettings()
        {
            _ = Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-location"));
        }
    }
}
