using CoreLocation;
using Foundation;
using TelegraphWhiteLabel.iOS.Services;
using UIKit;
using Xamarin.Forms;
using XamarinShared.ViewCreator;

[assembly: Dependency(typeof(GpsManager))]
namespace TelegraphWhiteLabel.iOS.Services
{
    public class GpsManager : IGpsService
    {
        public bool IsGpsEnable()
        {
            if (CLLocationManager.Status == CLAuthorizationStatus.Denied)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void OpenSettings()
        {
            var WiFiURL = new NSUrl("prefs:root=WIFI");

            if (UIApplication.SharedApplication.CanOpenUrl(WiFiURL))
            {   //> Pre iOS 10
                UIApplication.SharedApplication.OpenUrl(WiFiURL);
            }
            else
            {   //> iOS 10
                UIApplication.SharedApplication.OpenUrl(new NSUrl("App-Prefs:root=WIFI"));
            }
        }
    }
}