using Microsoft.Maui.Graphics;

namespace Cryptogram.Services
{
    // Neutralized implementations of the legacy Xamarin.Android DependencyService
    // contracts. The original platform implementations lived in the removed
    // Cryptogram.Android head; these no-op stubs keep the shared startup path
    // (App ctor -> SetStyle / OpenNavigationPage) from dereferencing a null
    // DependencyService.Get result. Call/push/analytics remain non-functional.
    internal sealed class StatusBarColorStub : IStatusBarColor
    {
        public void SetStatusbarColor(Color color) { }
    }

    internal sealed class EndCallStub : IEndCall
    {
        public void FinishCall(string chatId, string remoteName = "", bool isVideoCall = false) { }
    }

    internal sealed class SharedPreferenceStub : ISharedPreference
    {
        public void AddContact(string chatId, string name, bool isOsAndroid) { }
        public string GetContactName(string chatId) => string.Empty;
        public bool GetContactOS(string chatId) => false;
        public bool GetValueByKey(string key) => false;
        public void SetValueByKey(string key, bool value) { }
    }

    internal sealed class AudioPlayerSpeakerStub : IAudioPlayerSpeakerService
    {
        public void ChangeSpeaker(bool isEarpiece) { }
    }

    internal sealed class ProgressStub : IProgressInterface
    {
        public void Show(string title = "Loading") { }
        public void Hide() { }
    }

    internal sealed class SecurityFlagStub : ISecurityFlag
    {
        public void EnableSecureFlag() { }
        public void DisableSecureFlag() { }
    }
}
