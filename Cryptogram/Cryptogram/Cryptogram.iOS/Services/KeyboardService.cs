using System;
using Cryptogram.iOS.Services;
using Cryptogram.Services;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(KeyboardService))]
namespace Cryptogram.iOS.Services
{
    public class KeyboardService : IKeyboardService
    {
        public event EventHandler KeyboardIsShown;
        public event EventHandler KeyboardIsHidden;

        public KeyboardService()
        {
            SubscribeEvents();
        }
        private void SubscribeEvents()
        {
            UIKeyboard.Notifications.ObserveDidShow(OnKeyboardDidShow);
            UIKeyboard.Notifications.ObserveDidHide(OnKeyboardDidHide);
        }

        private void OnKeyboardDidShow(object sender, EventArgs e)
        {
            KeyboardIsShown?.Invoke(this, EventArgs.Empty);
        }

        private void OnKeyboardDidHide(object sender, EventArgs e)
        {
            KeyboardIsHidden?.Invoke(this, EventArgs.Empty);
        }

        public void HideKeyboard()
        {
            UIApplication.SharedApplication.KeyWindow.EndEditing(true);
        }
    }
}