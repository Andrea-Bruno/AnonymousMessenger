using System;
using Android.App;
using Android.Content;
using Android.Views.InputMethods;
using CustomViewElements.Services;
using Anonymous.Droid.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(KeyboardService))]
namespace Anonymous.Droid.Services
{
    public class KeyboardService : IKeyboardService
    {
        public event EventHandler KeyboardIsShown;
        public event EventHandler KeyboardIsHidden;

        private InputMethodManager inputMethodManager;
        private int countFirstTimes = 0;
        private bool wasShown = false;

        public KeyboardService()
        {
            GetInputMethodManager();
            SubscribeEvents();
            countFirstTimes = 0;
        }

        public void OnGlobalLayout(object sender, EventArgs args)
        {
            GetInputMethodManager();
            IsCurrentlyShown();
            KeyboardIsShown?.Invoke(this, EventArgs.Empty);
        }

        private bool IsCurrentlyShown()
        {
            return inputMethodManager.IsAcceptingText;
        }

        private void GetInputMethodManager()
        {
            if (inputMethodManager == null || inputMethodManager.Handle == IntPtr.Zero)
            {
                inputMethodManager = (InputMethodManager)((Activity)Xamarin.Forms.Forms.Context).GetSystemService(Context.InputMethodService);
            }
        }

        private void SubscribeEvents()
        {
            ((Activity)Xamarin.Forms.Forms.Context).Window.DecorView.ViewTreeObserver.GlobalLayout += this.OnGlobalLayout;
        }

        public void HideKeyboard()
        {
            var context = Forms.Context;
            var inputMethodManager = context.GetSystemService(Context.InputMethodService) as InputMethodManager;
            if (inputMethodManager != null && context is Activity)
            {
                var activity = context as Activity;
                var token = activity.CurrentFocus?.WindowToken;
                inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.None);
                activity.Window.DecorView.ClearFocus();
            }
        }
    }
}