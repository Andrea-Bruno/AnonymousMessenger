
using Foundation;
using UIKit;
using Xamarin.Forms;

namespace AnonymousWhiteLabel.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register("AppDelegate")]
	public partial class AppDelegate : Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			Forms.Init();
			LoadApplication(new App());
			

			//var statusBar = UIApplication.SharedApplication.ValueForKey(new NSString("statusBar")) as UIView;
			//if (statusBar != null && statusBar.RespondsToSelector(new ObjCRuntime.Selector("setBackgroundColor:")))
			//{
			//	statusBar.BackgroundColor = UIColor.Black; 
			//}
			app.SetStatusBarStyle(UIStatusBarStyle.Default, true);

			return base.FinishedLaunching(app, options);
		}
	}
}
