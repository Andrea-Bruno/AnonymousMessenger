using System;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace Cryptogram.Services
{
    // Ported implementations of the legacy Xamarin.Android DependencyService
    // contracts. The original platform implementations lived in the removed
    // Cryptogram.Android head; these use the .NET MAUI / Android equivalents.
    internal sealed class StatusBarColorStub : IStatusBarColor
    {
        public void SetStatusbarColor(Color color)
        {
#if ANDROID
            var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
            if (window != null)
            {
                int a = (int)Math.Round(color.Alpha * 255);
                int r = (int)Math.Round(color.Red * 255);
                int g = (int)Math.Round(color.Green * 255);
                int b = (int)Math.Round(color.Blue * 255);
                window.SetStatusBarColor(new Android.Graphics.Color((a << 24) | (r << 16) | (g << 8) | b));
            }
#endif
        }
    }

    internal sealed class EndCallStub : IEndCall
    {
        public void FinishCall(string chatId, string remoteName = "", bool isVideoCall = false) { }
    }

    internal sealed class SharedPreferenceStub : ISharedPreference
    {
        public void AddContact(string chatId, string name, bool isOsAndroid)
        {
            Preferences.Set($"contact_name_{chatId}", name);
            Preferences.Set($"contact_os_{chatId}", isOsAndroid);
        }
        public string GetContactName(string chatId) => Preferences.Get($"contact_name_{chatId}", string.Empty);
        public bool GetContactOS(string chatId) => Preferences.Get($"contact_os_{chatId}", false);
        public bool GetValueByKey(string key) => Preferences.Get(key, false);
        public void SetValueByKey(string key, bool value) => Preferences.Set(key, value);
    }

    internal sealed class AudioPlayerSpeakerStub : IAudioPlayerSpeakerService
    {
        public void ChangeSpeaker(bool isEarpiece)
        {
#if ANDROID
            var ctx = Microsoft.Maui.ApplicationModel.Platform.AppContext;
            if (ctx?.GetSystemService(Android.Content.Context.AudioService) is Android.Media.AudioManager am)
            {
                am.SpeakerphoneOn = !isEarpiece;
                am.Mode = isEarpiece ? Android.Media.Mode.InCall : Android.Media.Mode.Normal;
            }
#endif
        }
    }

    internal sealed class ProgressStub : IProgressInterface
    {
        public void Show(string title = "Loading") { }
        public void Hide() { }
    }

    internal sealed class SecurityFlagStub : ISecurityFlag
    {
        public void EnableSecureFlag()
        {
#if ANDROID
            var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
            window?.AddFlags(Android.Views.WindowManagerFlags.Secure);
#endif
        }
        public void DisableSecureFlag()
        {
#if ANDROID
            var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
            window?.ClearFlags(Android.Views.WindowManagerFlags.Secure);
#endif
        }
    }

    // CustomViewElements resolves its OWN same-named contract (a different CLR
    // type from the Cryptogram.Services one). BasePage / CustomTabbedPage call it
    // without a null-conditional, so an unregistered lookup NREs on the shared
    // code path (e.g. LoginPage SAVE -> BasePage.ShowProgressDialog).
    internal sealed class CustomViewProgressStub : CustomViewElements.Services.IProgressInterface
    {
        public void Show(string title = "Loading") { }
        public void Hide() { }
    }
}
