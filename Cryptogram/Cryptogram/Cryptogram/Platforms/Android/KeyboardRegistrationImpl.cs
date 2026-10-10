#if ANDROID
using System;
using Android.Graphics;
using Android.Views;
using CustomViewElements.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Cryptogram.Services
{
    // Android counterpart of the iOS KeyboardViewRenderer (CustomViewElements.Services.
    // IKeyboardRegistrationService). Instead of UIKit keyboard notifications, this watches the
    // activity decor view's global layout and derives the keyboard height from the difference
    // between the root view height and the visible display frame, converting to DIPs.
    //
    // The height is delivered to the current MAUI page that implements
    // IKeyboardHeightChange.OnKeyboardHeightChange(double). Resolution is done at delivery
    // time so it survives navigation; if no listener is found the call is a no-op.
    public sealed class KeyboardRegistrationImpl : IKeyboardRegistrationService
    {
        // A keyboard smaller than this fraction of the screen is treated as the navigation bar
        // (or noise), not the IME, and reported as 0.
        private const double KeyboardShownThreshold = 0.15;

        private Android.Views.View _decor;
        private KeyboardLayoutListener _listener;

        public void RegisterForKeyboardNotifications()
        {
            // Avoid stacking duplicate listeners if Register is called twice.
            UnregisterForKeyboardNotifications();

            var activity = Platform.CurrentActivity;
            _decor = activity?.Window?.DecorView;
            if (_decor == null)
                return;

            _listener = new KeyboardLayoutListener(OnLayoutChanged);
            _decor.ViewTreeObserver?.AddOnGlobalLayoutListener(_listener);
        }

        public void UnregisterForKeyboardNotifications()
        {
            if (_decor != null && _listener != null)
            {
                try
                {
                    _decor.ViewTreeObserver?.RemoveOnGlobalLayoutListener(_listener);
                }
                catch (Exception e)
                {
                    Console.WriteLine("KeyboardRegistrationImpl.Unregister: " + e.Message);
                }
            }

            _listener = null;
            _decor = null;
        }

        private void OnLayoutChanged()
        {
            var decor = _decor;
            if (decor == null)
                return;

            var visible = new Rect();
            decor.GetWindowVisibleDisplayFrame(visible);

            int rootHeight = decor.RootView?.Height ?? decor.Height;
            if (rootHeight <= 0)
                return;

            int keyboardPx = rootHeight - visible.Bottom;
            if (keyboardPx < 0)
                keyboardPx = 0;

            float density = decor.Resources?.DisplayMetrics?.Density ?? 1f;
            if (density <= 0f)
                density = 1f;

            bool shown = keyboardPx > rootHeight * KeyboardShownThreshold;
            double heightDip = shown ? keyboardPx / (double)density : 0d;

            ResolveTarget()?.OnKeyboardHeightChange(heightDip);
        }

        // Resolve the topmost page that listens for keyboard height changes.
        private static IKeyboardHeightChange ResolveTarget()
        {
            Page page = Application.Current?.MainPage;
            if (page == null)
                return null;

            if (page is NavigationPage nav)
                page = nav.CurrentPage ?? page;

            if (page is IKeyboardHeightChange direct)
                return direct;

            var modal = page?.Navigation?.ModalStack;
            if (modal != null && modal.Count > 0 && modal[modal.Count - 1] is IKeyboardHeightChange topModal)
                return topModal;

            return null;
        }

        private sealed class KeyboardLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
        {
            private readonly Action _onLayout;

            public KeyboardLayoutListener(Action onLayout) => _onLayout = onLayout;

            public void OnGlobalLayout() => _onLayout?.Invoke();
        }
    }
}
#endif
