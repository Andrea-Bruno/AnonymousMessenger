using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Toast;
using System;
using System.IO;
using Telegraph.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Services;
using System.Threading.Tasks;
using CustomViewElements;
using Telegraph.DesignHandler;

namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : BasePage
    {
        private string _usrnm;
        private MediaFile _file;
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
            _publickey = NavigationTappedPage.Context.My.GetPublicKey();
            PublicKey.Text = _publickey;
            image = NavigationTappedPage.Context.My.GetAvatar();
            _ = ImageDisplayAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Back();
        }

        public async Task ImageDisplayAsync()
        {
            try
            {
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

        private async void ExportPrivateKey_ClickedAsync(object sender, EventArgs e)
        {
            var status = await PermissionManager.CheckStoragePermission().ConfigureAwait(true);
            if (!status)
            {
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.TheFileCannotBeExportedWithoutGrantingPermission);

                return;
            }
            var fn = "private_key.txt";
            var privateKey = NavigationTappedPage.Context.My.GetPrivateKey();

            if (Device.RuntimePlatform == Device.iOS)
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                documentsPath = Path.Combine(documentsPath, Localization.Resources.Dictionary.Downloads);
                Directory.CreateDirectory(documentsPath);
                var filePath = Path.Combine(documentsPath, fn);
                File.WriteAllText(filePath, privateKey);
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.SuccessfullyExportedToDownloadsFile);
            }

            else if (Device.RuntimePlatform == Device.Android)
            {
                var directory = DependencyService.Get<IPathService>().PublicExternalFolder;
                var file = Path.Combine(directory, fn);
                File.WriteAllText(file, privateKey);
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.SuccessfullyExportedToDownloadsFile);
            }
        }

        private void Edit_Clicked(object sender, EventArgs e) => ChangeViewState(true);

        private async void Image_Clicked(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await Application.Current.MainPage.DisplayAlert(Localization.Resources.Dictionary.NoUpload, Localization.Resources.Dictionary.PickingaphotoIsNotSupported, Localization.Resources.Dictionary.Ok).ConfigureAwait(true);
                return;
            }
            var status = await PermissionManager.CheckStoragePermission().ConfigureAwait(true);
            if (!status)
            {
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.StoragePermissionIsNeeded);
                return;
            }
            _file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
            {
                CompressionQuality = 30
            });

            if (_file == null)
                return;
            Profile_Photo.Source = ImageSource.FromFile(_file.Path);
            ChangeViewState(true);
        }

            private void Save_Clicked(object sender, EventArgs e)
             {            
                if (!string.IsNullOrWhiteSpace(Name.Text))
                {
                    NavigationTappedPage.Context.My.Name = Name.Text.Trim();
                    Username.Text = Name.Text.Trim();
                    Name.Unfocus();
                }
                else
                {
                    CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.PleaseAddValidName);
                    return;                  
                }
                if (_file != null)
                {
                    image = GetByteFromStream(_file);
                    NavigationTappedPage.Context.My.SetAvatar(image);
                    _file = null;
                }
                XamarinShared.Setup.Settings.Save();
                if (image != null) ;
                ChangeViewState(false);
        }

        private async void DeletePicture_Clicked(object sender, EventArgs e)
        {
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

        private byte[] GetByteFromStream(MediaFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                file.GetStream().CopyTo(memoryStream);
                file.Dispose();
                return memoryStream.ToArray();
            }
        }

        private void CustomEntry_Focused(object sender, FocusEventArgs e)
        {
            (sender as CustomEntry).PlaceholderColor = DesignResourceManager.GetColorFromStyle("BackgroundSecondary");
        }

        private void CustomEntry_Unfocused(object sender, FocusEventArgs e)
        {
            (sender as CustomEntry).PlaceholderColor = DesignResourceManager.GetColorFromStyle("WhiteColor");
        }

        private void Back() 
        {          
            Name.Unfocus();
            ChangeViewState(false);
            _ = ImageDisplayAsync();
            _file = null;
        }

        private void ChangeViewState(bool isEditEnabled) 
        {
            CancelSave_layout.IsVisible = isEditEnabled;
            NameEntry_lyt.IsVisible = isEditEnabled;
            Username.IsVisible = !isEditEnabled;
            Edit.IsVisible = !isEditEnabled;
        }

        private void Cancel_Clicked(object sender, EventArgs args) => Back();
    }
}