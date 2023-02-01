using AnonymousWhiteLabel.Droid.Services;
using Xamarin.Forms;
using Android.Content;
using Android.Locations;
using XamarinShared.ViewCreator;

[assembly: Dependency(typeof(GpsManager))]
namespace AnonymousWhiteLabel.Droid.Services
{
    public class GpsManager : IGpsService
    {
        public bool IsGpsEnable()
        {
            var locationManager = (LocationManager)Android.App.Application.Context.GetSystemService(Context.LocationService);
            return locationManager.IsProviderEnabled(LocationManager.GpsProvider);
        }

        public void OpenSettings()
        {

            var intent = new Intent(Android.Provider.Settings.ActionLocat‌​ionSourceSettings);
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);

            try
            {
                Android.App.Application.Context.StartActivity(intent);

            }
            catch (ActivityNotFoundException activityNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine(activityNotFoundException.Message);
                Android.Widget.Toast.MakeText(Android.App.Application.Context, "Error: Gps Activity", Android.Widget.ToastLength.Short).Show();
            }

        }

    }
}