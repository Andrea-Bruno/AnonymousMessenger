
using System;
using System.IO;
using Anonymous.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Services;
using System.Threading.Tasks;
using CustomViewElements;
using Anonymous.DesignHandler;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Utils;

namespace Anonymous.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : BasePage
    {
        private string _usrnm;
        private FileResult _profilePicFileResult;
        private byte[] image;
        private string _publickey;
        public ProfilePage()
        {
            InitializeComponent();
            _usrnm = NavigationTappedPage.Context.My.Name;
            Name.Text = _usrnm;
            Username.Text = _usrnm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Username.Text = NavigationTappedPage.Context.My.Name;
            _publickey = NavigationTappedPage.Context.My.GetPublicKey();
            PublicKey.Text = _publickey;
            image = NavigationTappedPage.Context.My.GetAvatar();
            if (Profile_Photo.Source == null)
                _ = ImageDisplayAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Back(false);
        }

        public async Task ImageDisplayAsync()
        {
            try
            {
                if (_profilePicFileResult != null) return;
                if (image != null)
                {
                    Task<ImageSource> result = Task<ImageSource>.Factory.StartNew(() => ImageSource.FromStream(() => new MemoryStream(image)));
                    Profile_Photo.Source = await result;
                }
                else
                {
                    Profile_Photo.Source = DesignResourceManager.GetImageSource("ic_add_contact_profile.png");
                }
            }
            catch (Exception)
            {
            }
        }

        public async void Photo_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            try
            {
                if (((FileImageSource)Edit.Source).File == "ic_save_profile")
                    await PopupNavigation.Instance.PushAsync(new ImageViewPopupPage(Profile_Photo.Source), false).ConfigureAwait(true);
                else
                {
                    await PopupNavigation.Instance.PushAsync(new ImageViewPopupPage(image), false).ConfigureAwait(true);
                }
            }
            catch (Exception)
            {
            }
        }

        private void Edit_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            ChangeViewState(true);
            Name.Focus();
            Name.Text = Username.Text;
        }

        private async void Image_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            if (!MediaPicker.IsCaptureSupported)
            {
                await Application.Current.MainPage.DisplayAlert(Localization.Resources.Dictionary.NoUpload, Localization.Resources.Dictionary.PickingaphotoIsNotSupported, Localization.Resources.Dictionary.Ok).ConfigureAwait(true);
                return;
            }
            var status = await PermissionManager.CheckStoragePermission();
            if (!status) return;
            _profilePicFileResult = await MediaPicker.PickPhotoAsync();
            if (_profilePicFileResult == null)
                return;
            else if (_profilePicFileResult.ContentType == "image/jpeg" | _profilePicFileResult.ContentType == "image/png")
            {
                var stream = await _profilePicFileResult.OpenReadAsync();
                Profile_Photo.Source = ImageSource.FromStream(() => stream);
                ChangeViewState(true);
            }
            else
            {
                await DisplayAlert(Localization.Resources.Dictionary.Alert, Localization.Resources.Dictionary.SelectedImageTypeIsNotSupported, Localization.Resources.Dictionary.Ok).ConfigureAwait(true);
                return;
            }
        }

        private async void Save_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            if (!string.IsNullOrWhiteSpace(Name.Text))
            {
                NavigationTappedPage.Context.My.Name = Name.Text.Trim();
                Username.Text = Name.Text.Trim();
                Name.Unfocus();
            }
            else
            {
                await this.DisplayToastAsync(Localization.Resources.Dictionary.PleaseAddValidName);
                return;
            }
            if (_profilePicFileResult != null)
            {
                var stream = await _profilePicFileResult.OpenReadAsync();
                image = DependencyService.Get<IImageCompressionService>().CompressImage(Utils.Utils.StreamToByteArray(stream), 25); ;

                NavigationTappedPage.Context.My.SetAvatar(image);
                _profilePicFileResult = null;
            }
            XamarinShared.Setup.Settings.Save();
            ChangeViewState(false);
        }

        private async void DeletePicture_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            var DeletePictures = await Application.Current.MainPage.DisplayAlert(Localization.Resources.Dictionary.Alert, Localization.Resources.Dictionary.DeletePictureQuestion, Localization.Resources.Dictionary.Yes, Localization.Resources.Dictionary.No);
            if (DeletePictures)
            {
                if (image != null)
                {
                    image = null;
                    NavigationTappedPage.Context.My.SetAvatar(image);
                }
            }
        }

        private void Back(bool isBackClicked)
        {
            _profilePicFileResult = null;
            Name.Unfocus();
            ChangeViewState(false);
            if(isBackClicked)
                _ = ImageDisplayAsync();
        }

        private void ChangeViewState(bool isEditEnabled)
        {
            CancelSave_layout.IsVisible = isEditEnabled;
            NameEntry_lyt.IsVisible = isEditEnabled;
            Username.IsVisible = !isEditEnabled;
            Edit.IsVisible = !isEditEnabled;
        }

        private void Cancel_Clicked(object sender, EventArgs args)
        {
            sender.HandleButtonSingleClick(500);
            Back(true);
        }
    }
}