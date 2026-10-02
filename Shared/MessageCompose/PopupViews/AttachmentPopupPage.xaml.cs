using Mopups.Services;
using System;
using System.Threading.Tasks;
using MessageCompose.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using CustomViewElements;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;
using System.Collections.Generic;
using MessageCompose.Model;
using CommunityToolkit.Maui.Alerts;
using Utils;
using System.Threading;
using System.Linq;

namespace MessageCompose
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AttachmentPopupPage : BasePopupPage
    {
        public delegate void AttachPictureHandler(List<byte[]> images);
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
            if (MopupService.Instance.PopupStack.Count > 0)
                await MopupService.Instance.PopAsync(true).ConfigureAwait(true);

            if (await PermissionManager.CheckStoragePermission())
            {
                try
                {
                    ShowProgressDialog();
                    var request = new MediaPickerOptions { Title = "Select" };
                    var images = await MediaPicker.PickPhotosAsync(request);
                    if (images != null && images.Count > 0)
                    {
                        var listImages = new List<byte[]>();
                        foreach (var image in images)
                        {
                            using (var stream = await image.OpenReadAsync())
                            {
                                listImages.Add(Utils.Utils.StreamToByteArray(stream));
                            }
                        }
                        if (listImages.Count > 0)
                            AttachPicture(listImages);
                        else
                            await Toast.Make(Localization.Resources.Dictionary.SelectedImageTypeIsNotSupported).Show();
                    }
                }
                catch (Exception){}
                finally
                {
                    HideProgressDialog();
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
                        if (MopupService.Instance.PopupStack.Count > 0)
                            await MopupService.Instance.PopAsync(true).ConfigureAwait(true);
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
                    return;
                }
                if (!DependencyService.Get<IGpsService>().IsGpsEnable())
                {
                    await Toast.Make(Localization.Resources.Dictionary.LocationPermissionDidNotGranted).Show();
                    DependencyService.Get<IGpsService>().OpenSettings();
                }
                else
                {
                    ShowProgressDialog();
                    new System.Threading.Timer(async (object obj) => { await Task.Run(() => SendLocation()); }, null, 100, System.Threading.Timeout.Infinite);
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
                if (MopupService.Instance.PopupStack.Count > 0)
                    await MopupService.Instance.PopAsync(true).ConfigureAwait(true);
                HideProgressDialog();
            }
            else
            {
                await Toast.Make(Localization.Resources.Dictionary.PleaseClickAgain).Show();
                HideProgressDialog();
            }
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
                    if (MopupService.Instance.PopupStack.Count > 0)
                        await MopupService.Instance.PopAsync(true).ConfigureAwait(true);
                }

                return result;
            }
            catch (Exception)
            {
                // The user canceled or something went wrong
            }

            return null;
        }

        private async void Background_Clicked(object _, EventArgs e) => await MopupService.Instance.PopAsync(true).ConfigureAwait(true);

        private async void PdfDocument_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            if (!await PermissionManager.CheckStoragePermission())
                return;

            var customFileType =
                    new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "com.adobe.pdf" } }, // or general UTType values
                        { DevicePlatform.Android, new[] { "application/pdf" } },
                        { DevicePlatform.UWP, new[] { ".pdf"} },
                    });
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
                            SerializableFileData serializableFileData = new SerializableFileData(data, result.FileName);
                            AttachPdfDocument(Utils.Utils.ObjectToByteArray(serializableFileData));
                        }
                    }
                    if (MopupService.Instance.PopupStack.Count > 0)
                        await MopupService.Instance.PopAsync(true).ConfigureAwait(true);
                }

                return result;
            }
            catch (Exception)
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
            catch (Exception)
            {

            }
            if (MopupService.Instance.PopupStack.Count > 0)
                await MopupService.Instance.PopAsync(true).ConfigureAwait(true);
        }

        private ContactDetails ConvertToContactDetails(Contact contact)
        {
            string phoneNumber = contact.Phones != null && contact.Phones.Count > 0 ? contact.Phones[0].PhoneNumber : null;
            return new ContactDetails(contact.DisplayName, phoneNumber);
        }
    }
}