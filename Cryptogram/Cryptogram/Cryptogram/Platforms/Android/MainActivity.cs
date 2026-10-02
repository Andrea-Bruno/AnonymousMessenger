using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Cryptogram
{
    [Activity(Theme = "@style/Theme.Splash",
              MainLauncher = true,
              LaunchMode = LaunchMode.SingleTask,
              ScreenOrientation = ScreenOrientation.Portrait,
              Icon = "@drawable/Company_logo",
              Exported = true)]
    [IntentFilter(new[] { Intent.ActionSend },
        Categories = new[] { "android.intent.category.DEFAULT" },
        DataMimeType = "image/*")]
    [IntentFilter(new[] { Intent.ActionSend },
        Categories = new[] { "android.intent.category.DEFAULT" },
        DataMimeType = "application/pdf")]
    [IntentFilter(new[] { Intent.ActionSend },
        Categories = new[] { "android.intent.category.DEFAULT" },
        DataMimeType = "audio/*")]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Legacy plugin inits (Rg.Plugins, NativeMedia, Firebase, AppCenter, Agora,
            // Xamarin.Auth, Glide) are intentionally not initialized in this stub build.
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
        }
    }
}
