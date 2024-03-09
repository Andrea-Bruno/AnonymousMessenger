using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using MessageCompose.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CustomViewElements;
using Xamarin.Essentials;
using System.Collections.Generic;
using MessageCompose.Model;
using Xamarin.CommunityToolkit.Extensions;
using Utils;
// using XamarinShared.ViewCreator;

namespace MessageCompose
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AttachmentPopupPage : BasePopupPage
    {
        public delegate void AttachPictureHandler(byte[] image);
        public delegate void AttachAudioHandler(byte[] audio);
        public delegate void AttachLocationHandler(double lat, double lng);
        public delegate void AttachPdfDocumentHandler(byte[] audio);
        public delegate void AttachPhoneContactHandler(byte[] phoneContact);
        public delegate void AttachVideoHandler(FileResult video);
        public event AttachPictureHandler AttachPicture;
        public event AttachAudioHandler AttachAudio;
        public event AttachLocationHandler AttachLocation;
        public event AttachPdfDocumentHandler AttachPdfDocument;
        public event AttachPhoneContactHandler AttachPhoneContact;
        public event AttachVideoHandler AttachVideo;
        public bool IsVideoUploading = false;

        public AttachmentPopupPage()
        {
            InitializeComponent();
            PdfDocument.Source = Icons.IconProvider?.Invoke("ic_pdf_menu");
            Video.Source = Icons.IconProvider?.Invoke("ic_pick_video");
            Image.Source = Icons.IconProvider?.Invoke("ic_gallery");
            AudioImage.Source = Icons.IconProvider?.Invoke("ic_audio");
            PhoneContactImage.Source = Icons.IconProvider?.Invoke("ic_contact");
            Location.Source = Icons.IconProvider?.Invoke("ic_location");
        }

        private async void Image_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            if (await PermissionManager.CheckStoragePermission())
            {
                //ShowProgressDialog();
                //MediaPickerOptions options = new MediaPickerOptions() ;

                var photo = await MediaPicker.PickPhotoAsync();
                if (photo == null) return;
                if (photo.ContentType == "image/jpeg" | photo.ContentType == "image/png")
                {
                    ShowProgressDialog();
                    await OpenEditImagePageAsync(photo);
                }
                else
                {
                    await DisplayAlert(Localization.Resources.Dictionary.Alert, Localization.Resources.Dictionary.SelectedImageTypeIsNotSupported, Localization.Resources.Dictionary.Ok).ConfigureAwait(true);
                    if (PopupNavigation.Instance.PopupStack.Count > 0)
                        await PopupNavigation.Instance.PopAsync(true).ConfigureAwait(true);
                }

            }
        }
        private async void Video_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            if (!IsVideoUploading)
            {
                try
                {
                    if (await PermissionManager.CheckStoragePermission())
                    {
                        ShowProgressDialog();
                        // Select file from gallary
                        var selectedFile = await MediaPicker.PickVideoAsync();
                        if (selectedFile != null)
                        {
                            AttachVideo(selectedFile);
                            IsVideoUploading = true;
                            HideProgressDialog();
                        }
                        if (PopupNavigation.Instance.PopupStack.Count > 0)
                            await PopupNavigation.Instance.PopAsync(true).ConfigureAwait(true);
                        HideProgressDialog();
                    }
                }
                catch (Exception)
                {
                    HideProgressDialog();
                }
            }
            else
            {
                await DisplayAlert(Localization.Resources.Dictionary.Alert, Localization.Resources.Dictionary.VideoAlreadyUploading, Localization.Resources.Dictionary.Ok).ConfigureAwait(true);
            }
        }

        private async void Location_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick(2000);
            try
            {
                if (!await PermissionManager.CheckLocationPermission())
                {
                    await this.DisplayToastAsync(Localization.Resources.Dictionary.LocationPermissionDidNotGranted);
                    return;
                }
                if (!DependencyService.Get<XamarinShared.ViewCreator.IGpsService>().IsGpsEnable())
                {
                    await this.DisplayToastAsync(Localization.Resources.Dictionary.LocationPermissionDidNotGranted);
                    DependencyService.Get<XamarinShared.ViewCreator.IGpsService>().OpenSettings();
                }
                else
                {
                    ShowProgressDialog();
                    new System.Threading.Timer(async (object obj) => await Task.Run(() =>
                    {
                        if (Device.RuntimePlatform == Device.UWP)
                            MainThread.BeginInvokeOnMainThread(() => SendLocation());
                        else
                            SendLocation();

                    }), null, 100, System.Threading.Timeout.Infinite);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                HideProgressDialog();
            }
        }

        private async void SendLocation()
        {
            var location = await Geolocation
                           .GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Low,
                               timeout: TimeSpan.FromSeconds(10))).ConfigureAwait(false);
            _ = Task.Delay(200);
            if (location != null)
            {
                AttachLocation(location.Latitude, location.Longitude);
                if (PopupNavigation.Instance.PopupStack.Count > 0)
                    await PopupNavigation.Instance.PopAsync(true).ConfigureAwait(true);
                HideProgressDialog();
            }
            else
            {
                await this.DisplayToastAsync(Localization.Resources.Dictionary.PleaseClickAgain);
                HideProgressDialog();
            }
        }


        private async Task OpenEditImagePageAsync(FileResult file)
        {
            if (file != null)
            {
                var stream = await file.OpenReadAsync();
                AttachPicture(Utils.Utils.StreamToByteArray(stream));
            }

            else HideProgressDialog();

            if (PopupNavigation.Instance.PopupStack.Count > 0)
                await PopupNavigation.Instance.PopAsync(true).ConfigureAwait(true);
        }


        private async void Audio_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            if (!await PermissionManager.CheckStoragePermission())
                return;

            var customFileType =
                    new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "public.audio" } }, // or general UTType values
                        { DevicePlatform.Android, new[] {  "audio/*" } },
                        { DevicePlatform.UWP, new[] { ".mp3", ".mp4", ".wav", ".m4a" } },
                    });
            var options = new PickOptions
            {
                PickerTitle = Localization.Resources.Dictionary.PleaseSelectAudio,
                FileTypes = customFileType,
            };
            await HandleAudioPick(options);
        }

        private async Task<FileResult> HandleAudioPick(PickOptions options)
        {
            try
            {
                var result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    var stream = await result.OpenReadAsync();

                    if (stream != null && AttachAudio != null)
                    {
                        var audio = Utils.Utils.StreamToByteArray(stream);
                        AttachAudio(audio);
                    }
                    if (PopupNavigation.Instance.PopupStack.Count > 0)
                        await PopupNavigation.Instance.PopAsync(true).ConfigureAwait(true);
                }

                return result;
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
            }

            return null;
        }

        private async void Background_Clicked(object _, EventArgs e) => await PopupNavigation.Instance.PopAsync(true).ConfigureAwait(true);

        private async void PdfDocument_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            if (!await PermissionManager.CheckStoragePermission())
                return;

            var customFileType = FilePickerFileType.Pdf;
            //new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            //{
            //    { DevicePlatform.iOS, new[] { "com.adobe.pdf" } }, // or general UTType values
            //    { DevicePlatform.Android, new[] { "application/pdf" } },
            //    { DevicePlatform.UWP, new[] { ".pdf"} },
            //});
            var options = new PickOptions
            {
                PickerTitle = Localization.Resources.Dictionary.PleaseSelectAudio,
                FileTypes = customFileType,
            };
            await HandleDocumentPick(options);
        }

        private async Task<FileResult> HandleDocumentPick(PickOptions options)
        {
            try
            {
                var result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    var stream = await result.OpenReadAsync();
                    var data = Utils.Utils.StreamToByteArray(stream);
                    if (data != null)
                    {
                        if (AttachPdfDocument != null)
                        {
                            var serializableFileData = new SerializableFileData(data, result.FileName);
                            AttachPdfDocument(Utils.Utils.ObjectToByteArray(serializableFileData));
                        }
                    }
                    if (PopupNavigation.Instance.PopupStack.Count > 0)
                        await PopupNavigation.Instance.PopAsync(true).ConfigureAwait(true);
                }

                return result;
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
            }

            return null;
        }

        private async void PhoneContact_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            if (!await PermissionManager.CheckContactPermission())
                return;
            try
            {
                var contact = await Contacts.PickContactAsync();

                if (contact == null)
                    return;
                AttachPhoneContact(Utils.Utils.ObjectToByteArray(ConvertToContactDetails(contact)));
            }
            catch (Exception ex)
            {

            }
            if (PopupNavigation.Instance.PopupStack.Count > 0)
                await PopupNavigation.Instance.PopAsync(true).ConfigureAwait(true);
        }

        private ContactDetails ConvertToContactDetails(Contact contact)
        {
            var phoneNumber = contact.Phones != null && contact.Phones.Count > 0 ? contact.Phones[0].PhoneNumber : null;
            return new ContactDetails(contact.DisplayName, phoneNumber);
        }
    }
}