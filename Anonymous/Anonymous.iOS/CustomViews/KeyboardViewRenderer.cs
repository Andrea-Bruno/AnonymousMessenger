using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Foundation;
using CoreGraphics;
using CustomViewElements;
using CustomViewElements.Services;
using Telegraph.iOS.CustomViews;

[assembly: Dependency(typeof(KeyboardViewRenderer))]
[assembly: ExportRenderer(typeof(KeyboardView), typeof(KeyboardViewRenderer))]
namespace Telegraph.iOS.CustomViews
{
    public class KeyboardViewRenderer : ViewRenderer, IKeyboardRegistrationService
    {
        private NSObject _keyboardShowObserver;
        private static bool _isChatPageOpen;
        public KeyboardViewRenderer()
        {
            RegisterForKeyboardNotifications();
        }
       
        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

        }

        public void UnregisterForKeyboardNotifications()
        {
            _isChatPageOpen = false;
            if (_keyboardShowObserver != null) NSNotificationCenter.DefaultCenter.RemoveObserver(_keyboardShowObserver);
        }

        public void RegisterForKeyboardNotifications()
        {

            _isChatPageOpen = true;
            _keyboardShowObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, (notification) => {
                if (_isChatPageOpen)
                {
                    var result = (NSValue)notification.UserInfo.ObjectForKey(new NSString(UIKeyboard.FrameEndUserInfoKey));
                    int bottomMargin = (int)UIApplication.SharedApplication.KeyWindow.SafeAreaInsets.Bottom;
                    var difference = result.RectangleFValue.Size.Height - bottomMargin;

                    DependencyService.Get<IKeyboardHeightChange>().OnKeyboardHeightChange(difference);
                }
            });
            

        }

    }
}