
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Tasks;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Com.Xamarin.Formsviewgroup;
using Firebase;
using Firebase.Iid;
using System;
using System.IO;
using System.Threading.Tasks;



namespace AnonymousWhiteLabel.Droid
{
    [Activity(Label = "AnonymousWhiteLabel", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IOnSuccessListener
    {
        internal static MainActivity Instance { get; private set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Xamarin.Forms.Platform.Android.Resource.Layout.Tabbar;
            ToolbarResource = Xamarin.Forms.Platform.Android.Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Rg.Plugins.Popup.Popup.Init(this);
            LoadApplication(new App());
            Instance = this;

            App.IgnoreBatteryOptimizations = () =>
            {
                //============= Ignore Battery Optimizations =============
                //https://stackoverflow.com/questions/32627342/how-to-whitelist-app-in-doze-mode-android-6-0
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    using var intent = new Intent();
                    var packageName = Firebase.BuildConfig.ApplicationId;
                    var pm = (PowerManager)GetSystemService(PowerService);
                    if (!pm.IsIgnoringBatteryOptimizations(packageName))
                    {
                        intent.SetAction(Settings.ActionRequestIgnoreBatteryOptimizations);
                        intent.SetData(Android.Net.Uri.Parse("package:" + packageName));
                        StartActivity(intent);
                    }
                }
                //========================================================
            };

            //var fapp = FirebaseApp.InitializeApp(this);
            //if (FirebaseInstanceId.Instance != null)
            //    App.FirebaseToken = FirebaseInstanceId.Instance.Token;
            //FirebaseInstanceId.Instance.GetInstanceId().AddOnSuccessListener(this, this);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        // Field, property, and method for Picture Picker
        public static readonly int PickImageId = 1000;

        public TaskCompletionSource<Stream> PickImageTaskCompletionSource { set; get; }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            if (requestCode == PickImageId)
            {
                if ((resultCode == Result.Ok) && (intent != null))
                {
                    Android.Net.Uri uri = intent.Data;
                    Stream stream = ContentResolver.OpenInputStream(uri);

                    // Set the Stream as the completion of the Task
                    PickImageTaskCompletionSource.SetResult(stream);
                }
                else
                {
                    PickImageTaskCompletionSource.SetResult(null);
                }
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            CreateNotificationFromIntent(intent);
        }

        void CreateNotificationFromIntent(Intent intent)
        {
            if (intent?.Extras != null)
            {
                var title = intent.Extras.GetString(AndroidNotificationManager.TitleKey);
                var message = intent.Extras.GetString(AndroidNotificationManager.MessageKey);
                Xamarin.Forms.DependencyService.Get<INotificationManager>().ReceiveNotification(title, message);
            }
        }


        public void OnSuccess(Java.Lang.Object result)
        {
            if (FirebaseInstanceId.Instance != null)
            {
                var refreshedToken = FirebaseInstanceId.Instance.Token;
                Console.WriteLine("------->>> TOKEN " + refreshedToken);
                App.FirebaseToken = refreshedToken;
                App.UpdateFirebaseToken(refreshedToken);
            }
        }

    }
}