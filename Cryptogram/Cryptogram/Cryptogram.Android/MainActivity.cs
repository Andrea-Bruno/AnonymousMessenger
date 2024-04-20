using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.OS;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Android.Content;
using Xamarin.Forms;
using Anonymous.Services;
using Rg.Plugins.Popup.Services;
using System.IO;
using Firebase.Iid;
using System;
using Android.Gms.Tasks;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Anonymous.Droid.Services;
using Xamarin.Forms.Platform.Android;
using Android.Content.Res;
using Anonymous.Droid;
using NotificationService;
using Anonymous.DesignHandler;
using Android.Widget;
using Plugin.Fingerprint;
using Plugin.CurrentActivity;
using System.Threading;
using Android.Provider;

[assembly: Dependency(typeof(MainActivity))]
namespace Anonymous.Droid
{
	[Activity(Theme = "@style/Theme.Splash", Icon = "@drawable/Company_logo",
		Label = "Anonymous", MainLauncher = true, LaunchMode = LaunchMode.SingleTask,
		NoHistory = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
		ScreenOrientation = ScreenOrientation.Portrait)] [IntentFilter(new[] { Intent.ActionSend },
		Categories = new[] { Intent.CategoryDefault, Intent.CategoryLeanbackLauncher },
		DataMimeTypes = new[] { "image/*","application/pdf", "audio/*" })]

	public class MainActivity : FormsAppCompatActivity, IOnSuccessListener, IStatusBarColor
	{
		private static Window _currentWindow;
		public static MainActivity Context;

		protected override void OnCreate(Bundle bundle)
		{
			lock (this)
			{
				SetTheme(Resource.Style.MainTheme);
				base.OnCreate(bundle);
				if (Context == null)
				{

					Rg.Plugins.Popup.Popup.Init(this);
					Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzc3MTg3QDMxMzgyZTM0MmUzMFk0TkIzd1YwUUEvNkJjcDNzd2dMYk9pU1ZFWVk2NEx1aUhyUytSZWl6Q3M9");
					if(!Forms.IsInitialized)
						Forms.Init(this, bundle);
					Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);
					_currentWindow = Window;
					NativeMedia.Platform.Init(this, bundle);
					Xamarin.Essentials.Platform.Init(this, bundle);
					CrossFingerprint.SetCurrentActivityResolver(() => CrossCurrentActivity.Current.Activity);
					Android.Glide.Forms.Init(this);
					global::Xamarin.Auth.Presenters.XamarinAndroid.AuthenticationConfiguration.Init(this, bundle);
					global::Xamarin.Auth.CustomTabsConfiguration.CustomTabsClosingMessage = null;
					CreateNotificationFromIntent(Intent);
					if (FirebaseInstanceId.Instance != null)
						App.FirebaseToken = FirebaseInstanceId.Instance.Token;
					var builder = new StrictMode.VmPolicy.Builder();
					StrictMode.SetVmPolicy(builder.Build());
					FirebaseInstanceId.Instance.GetInstanceId().AddOnSuccessListener(this, this);
					AppCenter.Start("2dcf6064-60c7-43ab-854d-7b238b251cc8",
								 typeof(Analytics), typeof(Crashes));
					DisableStandby();
					Context = this;
					string root;
					if (Android.OS.Environment.IsExternalStorageEmulated)
					{

						root = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);

					}
					else
						root = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
					Console.WriteLine("Root path: " + root);
				}
				else
				{
					LoadApplication(new App());
				}
				SetStatusbarColor(DesignResourceManager.GetColorFromStyle("Color1"));
			}
		}

		protected override void OnNewIntent(Intent intent)
		{
			base.OnNewIntent(intent);
			CreateNotificationFromIntent(intent);
		}

		protected override void OnResume()
		{
			base.OnResume();
			AndroidNotificationManager.GetInstance().CancelNotification();
			if (Xamarin.Forms.Application.Current != null)
				Xamarin.Forms.Application.Current.On<Xamarin.Forms.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
		}

		public static void EnableSecureFlag(bool isEnable)
		{
			if (isEnable)
				_currentWindow.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);
			else
				_currentWindow.ClearFlags(WindowManagerFlags.Secure);
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
		{
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		private async void CreateNotificationFromIntent(Intent intent)
		{
			try
			{
				LoadApplication(((App)Xamarin.Forms.Application.Current) ?? new App());
				if (intent.Action == Intent.ActionSend && intent.Extras.ContainsKey(Intent.ExtraStream))
				{
					var data = ReadFileData((Android.Net.Uri)intent.Extras.GetParcelable(Intent.ExtraStream));
					if (!await PermissionManager.CheckStoragePermission().ConfigureAwait(true))
						Toast.MakeText(this, Localization.Resources.Dictionary.StoragePermissionIsNeeded, ToastLength.Short);
					else if (data != null && intent.Type == "image/*")
						App.ShareData(data, SharedMessageType.IMAGE);
					else if (data != null && intent.Type == "application/pdf")
					{
						string fileName = GetFileName((Android.Net.Uri)intent.Extras.GetParcelable(Intent.ExtraStream));
						App.ShareData(data, SharedMessageType.PDF, fileName);
					}
					else if (data != null && intent.Type == "audio/*")
						App.ShareData(data, SharedMessageType.AUDIO);

				}
				else
				{
					if (intent?.Extras != null && intent.HasExtra(AndroidNotificationManager.ChatId))
					{
						string notificationTypeAsString = intent.GetStringExtra(AndroidNotificationManager.NotificationType);
						NotificationType type = (NotificationType)Enum.Parse(typeof(NotificationType), notificationTypeAsString);
						App.GoToChat(Convert.ToUInt64(intent.GetStringExtra(AndroidNotificationManager.ChatId)), type);
					}
				}
			}catch(Exception e)
            {
				Console.WriteLine("Error: " + e.Message);
            }
		}

		public override void OnBackPressed()
		{
			App app = (App)Xamarin.Forms.Application.Current;
			int navigationStackCount = app != null ? app.NavigationStackCount : 0;
			if (PopupNavigation.Instance.PopupStack.Count > 0
				|| (app != null && app.NavigationStackCount > 1 && navigationStackCount == app.NavigationStackCount))
            {
                base.OnBackPressed();
            }
        }

		protected override void OnDestroy()
		{
			try
			{
				App.DisableProximitySensor();
				base.OnDestroy();
				System.Environment.Exit(0);
			}
			catch(Exception e)
            {

            }
		}

		private byte[] ReadFileData(Android.Net.Uri uri)
		{
			try
			{
				Stream stream = ContentResolver.OpenInputStream(uri);
				byte[] byteArray;

				using (var memoryStream = new MemoryStream())
				{
					stream.CopyTo(memoryStream);
					byteArray = memoryStream.ToArray();
				}
				return byteArray;
			}
			catch (UnauthorizedAccessException e)
			{
				Console.WriteLine("---->>> UnauthorizedAccessException error message  " + e.Message);
				Toast.MakeText(this, Localization.Resources.Dictionary.StoragePermissionIsNeeded, ToastLength.Short);
				return null;
			}
		}

		public override Resources Resources
		{
			get
			{
				Configuration config = new Configuration();
				config.SetToDefaults();
				Context context = CreateConfigurationContext(config);
				Resources resources = context.Resources;
				return resources;
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
	
		private void DisableStandby()
		{
			_currentWindow.AddFlags(WindowManagerFlags.KeepScreenOn);

		}

        public void SetStatusbarColor(Color color)
        {
			_currentWindow.SetStatusBarColor(color.ToAndroid());
		}

		private string GetFileName(Android.Net.Uri uri)
		{
			string result = null;
			if (uri.Scheme == "content")
			{
				var cursor = ContentResolver.Query(uri, null, null, null, null);
				try
				{
					if (cursor != null && cursor.MoveToFirst())
					{
						result = cursor.GetString(cursor.GetColumnIndex(OpenableColumns.DisplayName));
					}
				}
				finally
				{
					cursor.Close();
				}
			}
			if (result == null)
			{
				result = uri.Path;
				int cut = result.LastIndexOf('/');
				if (cut != -1)
				{
					result = result.Substring(cut + 1);
				}
			}
			return result;
		}

		// For Xamarin.MediaGallery
		protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
		{
			if (NativeMedia.Platform.CheckCanProcessResult(requestCode, resultCode, intent))
				NativeMedia.Platform.OnActivityResult(requestCode, resultCode, intent);

			base.OnActivityResult(requestCode, resultCode, intent);
		}
	}

}