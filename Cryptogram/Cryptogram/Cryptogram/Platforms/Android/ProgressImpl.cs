#if ANDROID
using System;
using Microsoft.Maui.ApplicationModel;
using Mopups.Services;

namespace Cryptogram.Services
{
    // Ported from Cryptogram.Droid.Services.ProgressLoader (Xamarin.Android + AndHUD).
    // AndHUD is unavailable in MAUI, so the HUD is a Mopups popup (LoadingPopupPage).
    // Implements the CustomViewElements contract actually consumed by BasePage /
    // CustomTabbedPage / BaseContentView / BasePopupPage.
    // The interface is synchronous, so show/hide are fired-and-forgotten on the UI thread.
    // Double-show / double-dismiss is guarded by tracking the currently shown popup.
    public sealed class ProgressImpl : CustomViewElements.Services.IProgressInterface
    {
        private LoadingPopupPage _current;

        public void Show(string title = "Loading")
        {
            if (_current != null)
                return;

            string text = ResolveTitle(title);
            var popup = new LoadingPopupPage(text);
            _current = popup;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await MopupService.Instance.PushAsync(popup, false);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ProgressImpl.Show: " + e.Message);
                }
            });
        }

        public void Hide()
        {
            var popup = _current;
            if (popup == null)
                return;

            _current = null;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await MopupService.Instance.RemovePageAsync(popup, false);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ProgressImpl.Hide: " + e.Message);
                }
            });
        }

        // The original overrode the passed title with the localized "Loading" string.
        // Use it when reachable; otherwise fall back to the caller-supplied title.
        private static string ResolveTitle(string fallback)
        {
            try
            {
                return Localization.Resources.Dictionary.Loading ?? fallback;
            }
            catch
            {
                return fallback;
            }
        }
    }
}
#endif
