using System;
using Foundation;
using Cryptogram.iOS.Services;
using Cryptogram.Services;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: Dependency(typeof(StatusBarColorRenderer))]
namespace Cryptogram.iOS.Services
{
    public class StatusBarColorRenderer : IStatusBarColor
    {
        public StatusBarColorRenderer()
        {
        }

        public void SetStatusbarColor(Color color)
        {
            try
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                {
                    if (UIApplication.SharedApplication?.KeyWindow?.WindowScene != null)
                    {
                        var statusBar = new UIView(UIApplication.SharedApplication.KeyWindow.WindowScene.StatusBarManager.StatusBarFrame)
                        {
                            BackgroundColor = color.ToUIColor()
                        };
                        UIApplication.SharedApplication.KeyWindow.AddSubview(statusBar);
                    }
                }
                else
                {
                    var statusBar = UIApplication.SharedApplication.ValueForKey(new NSString("statusBar")) as UIView;
                    if (statusBar.RespondsToSelector(new ObjCRuntime.Selector("setBackgroundColor:")))
                    {
                        statusBar.BackgroundColor = color.ToUIColor();
                        UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.BlackOpaque;
                    }
                }
            }
            catch(Exception e)
            {
                // some of the values can be null
            }
        }
    }
}
