using System;
using System.Collections.Generic;
using Cryptogram.Models;

namespace Cryptogram.Services
{
    // Remaining legacy DependencyService contracts whose original platform impls
    // lived in the removed Cryptogram.Android head. Registered as singletons in
    // MauiProgram so the shared code paths (ChatRoom, ProfilePage, MenuPage,
    // Composer, etc.) get a non-null instance instead of dereferencing null.
    // GPS and app-close are real implementations; image compression is a faithful
    // no-op (the original also returned the input unchanged); Drive backup is
    // disabled (SupportBackup() == false).

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
}
