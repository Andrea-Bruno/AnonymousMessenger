using System;
using System.Collections.Generic;
using System.IO;
using Cryptogram.Models;

namespace Cryptogram.Services
{
    // Neutralized implementations of the remaining legacy DependencyService
    // contracts whose original platform impls lived in the removed
    // Cryptogram.Android head. Registered as singletons in MauiProgram so the
    // shared code paths (ChatRoom, ProfilePage, MenuPage, Composer, etc.) get a
    // non-null instance instead of dereferencing null. The features themselves
    // (audio record/play, thumbnails, GPS, Drive backup, keyboard metrics,
    // app-close) remain non-functional no-ops until ported.

    internal sealed class ImageCompressionStub : IImageCompressionService
    {
        public byte[] CompressImage(byte[] imageData, int compressionPercentage, string destinationPath = null)
            => imageData;
    }

    internal sealed class CloseApplicationStub : ICloseApplication
    {
        public void CloseApplication()
        {
#if ANDROID
            Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.FinishAndRemoveTask();
#else
            Application.Current?.Quit();
#endif
        }
    }

    internal sealed class KeyboardRegistrationStub : CustomViewElements.Services.IKeyboardRegistrationService
    {
        public void RegisterForKeyboardNotifications() { }
        public void UnregisterForKeyboardNotifications() { }
    }

    internal sealed class ThumbnailStub : MessageCompose.Services.IThumbnailService
    {
        public byte[] GenerateThumbImage(string url, long usecond) => null;
    }

    internal sealed class AudioRecorderStub : MessageCompose.Services.IAudioRecorder
    {
        public void StartRecording() { }
        public void StopRecording() { }
        public void PlayRecording() { }
        public byte[] GetOutput() => null;
        public void DeleteOutput() { }
    }

    // Real GPS availability check via the Android LocationManager; opens the
    // device location settings when the caller requests it.
    internal sealed class GpsStub : MessageCompose.Services.IGpsService
    {
        public bool IsGpsEnable()
        {
#if ANDROID
            var ctx = Microsoft.Maui.ApplicationModel.Platform.AppContext;
            if (ctx?.GetSystemService(Android.Content.Context.LocationService) is Android.Locations.LocationManager lm)
                return lm.IsProviderEnabled(Android.Locations.LocationManager.GpsProvider)
                    || lm.IsProviderEnabled(Android.Locations.LocationManager.NetworkProvider);
            return false;
#else
            return true;
#endif
        }

        public void OpenSettings()
        {
#if ANDROID
            var intent = new Android.Content.Intent(Android.Provider.Settings.ActionLocationSourceSettings);
            intent.SetFlags(Android.Content.ActivityFlags.NewTask);
            Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.StartActivity(intent);
#endif
        }
    }

    // SupportBackup returns false so the Drive backup flow is skipped cleanly.
    internal sealed class DriveServiceStub : Cryptogram.Backup.IDriveService
    {
        public void Init() { }
        public string CreateOrUpdateFile(string folderId, string fileName, byte[] content) => null;
        public void DeleteFile(string fileId, string folderId = null) { }
        public string CreateFolder(string folderName, string parentFolderId) => null;
        public FileData GetFolder(string folderName) => null;
        public FileData GetFileById(string folderId, string fileId) => null;
        public FileData GetFileByName(string folderId, string fileName) => null;
        public List<FileData> GetFilesByFolderId(string folderId) => new List<FileData>();
        public List<FileData> GetFolders(string parentFolderId) => new List<FileData>();
        public byte[] ReadFileContent(string fileId, string folderId = null) => null;
        public bool SupportBackup() => false;
    }

    internal sealed class AudioPlayerStub : XamarinShared.ViewCreator.IAudioPlayer
    {
#pragma warning disable CS0067 // required by IAudioPlayer; never raised by the no-op stub
        public event EventHandler PlaybackEnded;
#pragma warning restore CS0067
        public double Duration => 0;
        public double CurrentPosition => 0;
        public double Volume { get; set; }
        public double Balance { get; set; }
        public bool IsPlaying => false;
        public string Path => string.Empty;
        public bool Loop { get; set; }
        public bool CanSeek => false;
        public bool Load(Stream audioStream) => false;
        public bool Load(string fileName) => false;
        public void Play() { }
        public void Pause() { }
        public void Stop() { }
        public void Seek(double position) { }
        public void ChangeSpeaker(bool isEarpieceEnabled, int currentPosition) { }
        public void Dispose() { }
    }
}
