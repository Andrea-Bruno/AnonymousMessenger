using MessageCompose.Services;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Toast;
using Rg.Plugins.Popup.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using EncryptedMessaging;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AttachmentPopupPage : BasePopupPage
    {

        public delegate void AttachPictureHandler(byte[] image);
        public delegate void AttachAudioHandler(byte[] audio);
        public delegate void AttachLocationHandler(double lat, double lng);
        public delegate void AttachPdfDocumentHandler(byte[] audio);

        public event AttachPictureHandler AttachPicture;
        public event AttachAudioHandler AttachAudio;
        public event AttachLocationHandler AttachLocation;
        public event AttachPdfDocumentHandler AttachPdfDocument;
        private Contact contact;

        public AttachmentPopupPage(Contact _contact)
        {
            InitializeComponent();
            contact = _contact;
        }

        private async void Image_Clicked(object _, EventArgs e)
        {
            if (await PermissionManager.CheckStoragePermission())
            {
                ShowProgressDialog();
                await OpenEditImagePageAsync(await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions { CompressionQuality = 30 }));
            }
            else
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.RequestedPermissionIsNeeded);
        }
        private async void Camera_Clicked(object _, EventArgs e)
        {
            if (await PermissionManager.CheckCameraPermission() && await PermissionManager.CheckStoragePermission())
            {
                ShowProgressDialog();
                await OpenEditImagePageAsync(await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions { AllowCropping = false, RotateImage= true, DefaultCamera= CameraDevice.Rear, CompressionQuality = 50 }).ConfigureAwait(true));
            }
            else
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.RequestedPermissionIsNeeded);
        }

        private async void Location_Clicked(object _, EventArgs e)
        {
            try
            {
                if (!DependencyService.Get<IGpsService>().IsGpsEnable())
                {
                    CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.LocationPermissionDidNotGranted);
                    DependencyService.Get<IGpsService>().OpenSettings();
                }
                else
                {

                    var location = await Xamarin.Essentials.Geolocation.GetLocationAsync(new Xamarin.Essentials.GeolocationRequest(Xamarin.Essentials.GeolocationAccuracy.Low,timeout: TimeSpan.FromSeconds(10))).ConfigureAwait(false);
                    ShowProgressDialog();
                    Task.Delay(200);
                    if (location != null)
                    {
                        AttachLocation(location.Latitude, location.Longitude);
                        if (PopupNavigation.Instance.PopupStack.Count > 0)
                            await PopupNavigation.Instance.PopAsync(true).ConfigureAwait(true);
                        HideProgressDialog();

                    }
                    else
                    {
                        CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.PleaseClickAgain);
                        HideProgressDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                HideProgressDialog();

            }
        }

        private async Task<Xamarin.Essentials.PermissionStatus> CheckLocationPermissionAsync()
        {
            var status = await Xamarin.Essentials.Permissions.CheckStatusAsync<Xamarin.Essentials.Permissions.LocationWhenInUse>();
            if (status == Xamarin.Essentials.PermissionStatus.Unknown)
                status = await Xamarin.Essentials.Permissions.RequestAsync<Xamarin.Essentials.Permissions.LocationWhenInUse>();
            return status;
        }

        async Task OpenEditImagePageAsync(MediaFile file)
        {
            if (file != null)
            {
                if (AttachPicture != null)
                {
                    var image = ConvertFileToByte(file);
                    var editImagePage = new EditImagePage(image);
                    editImagePage.AttachPicture += AttachPicture;
                    Application.Current.MainPage.Navigation.PushAsync(editImagePage, false);
                }
                else HideProgressDialog();
            }
            else HideProgressDialog();
            if (PopupNavigation.Instance.PopupStack.Count > 0)
                await PopupNavigation.Instance.PopAsync(true).ConfigureAwait(true);
        }

        private byte[] ConvertFileToByte(MediaFile file)
        {
            var memoryStream = new MemoryStream();
            file.GetStreamWithImageRotatedForExternalStorage().CopyTo(memoryStream);
            file.Dispose();
            return memoryStream.ToArray();
        }

        private async void Audio_Clicked(object _, EventArgs e)
        {
            try
            {
                FileData file = null;
                if (Device.RuntimePlatform == Device.Android)
                    file = await CrossFilePicker.Current.PickFile(new string[] { "audio/*" }).ConfigureAwait(true);
                else if (Device.RuntimePlatform == Device.iOS)
                    file = await CrossFilePicker.Current.PickFile(new string[] { "public.audio" }).ConfigureAwait(true);
                else if (Device.RuntimePlatform == Device.UWP)
                    file = await CrossFilePicker.Current.PickFile(new string[] { ".mp3", ".mp4", ".wav", ".m4a" }).ConfigureAwait(true);
                if (file != null)
                {
                    if (AttachAudio != null)
                    {
                        var audio = file.DataArray;
                        AttachAudio(audio);
                    }
                }
                if (PopupNavigation.Instance.PopupStack.Count > 0)
                    await PopupNavigation.Instance.PopAsync(true).ConfigureAwait(true);
            }
            catch (Exception)
            {

            }

        }

        private async void Background_Clicked(object _, EventArgs e) => await PopupNavigation.Instance.PopAsync(true).ConfigureAwait(true);

        protected override bool OnBackButtonPressed()
        {
            PopupNavigation.Instance.PopAsync(false);
            return true;
        }
        private async void PdfDocument_Clicked(object sender, EventArgs e)
        {
            FileData file = null;
            if (Device.RuntimePlatform == Device.Android)
                file = await CrossFilePicker.Current.PickFile(new string[] { "application/pdf" }).ConfigureAwait(true);
            else if (Device.RuntimePlatform == Device.iOS)
                file = await CrossFilePicker.Current.PickFile(new string[] { "com.adobe.pdf" }).ConfigureAwait(true);
            else if (Device.RuntimePlatform == Device.UWP)
                file = await CrossFilePicker.Current.PickFile(new string[] { ".pdf" }).ConfigureAwait(true);
            if (file != null)
            {
                if (AttachPdfDocument != null)
                {
                    var data = file.DataArray;
                    AttachPdfDocument(data);
                }
            }
            if (PopupNavigation.Instance.PopupStack.Count > 0)
                await PopupNavigation.Instance.PopAsync(true).ConfigureAwait(true);

        }
    }
}