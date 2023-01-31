using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Toast;
using Xamarin.Forms;
using System;
using Telegraph.Services;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using CustomViewElements;
using System.Collections.Generic;

namespace Telegraph.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : BasePage
	{
		private MediaFile _mediaFile;
		public LoginPage()
		{
			InitializeComponent();		
			Username.Keyboard = Keyboard.Chat;
		}

		private async void SelectImage_Clicked(object sender, EventArgs e)
		{
			var status = await PermissionManager.CheckStoragePermission();
			if (status)
			{
				if (!CrossMedia.Current.IsPickPhotoSupported)
				{
					await DisplayAlert(Localization.Resources.Dictionary.NoUpload, Localization.Resources.Dictionary.PickingaphotoIsNotSupported, Localization.Resources.Dictionary.Ok);
					return;
				}
				_mediaFile = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
				{
					CompressionQuality = 25
				});
				if (_mediaFile == null) return;

				UserProfilePhotoBg.BorderColor = Color.Yellow;
				UserProfilePhoto.Source = ImageSource.FromStream(() => _mediaFile.GetStream());
			}
		}

		private void Save_Clicked(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(Username.Text))
			{
				CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.PleaseAddValidName);
				return;
			}
			if (Validation())
			{
				ShowProgressDialog();
				var timer = new System.Threading.Timer((object obj) => { Device.BeginInvokeOnMainThread(() => Save()); }, null, 100, System.Threading.Timeout.Infinite);
			}
        }

		private void Save()
		{
			Application.Current.MainPage.Navigation.PushAsync(new NavigationTappedPage(Username.Text, _mediaFile)); // normal runn
			var a = new List<Page>(Navigation.NavigationStack );
			foreach (Page page in a)
				Navigation.RemovePage(page);
		}

		private void Terms_Clicked(object sender, EventArgs e)
		{
			Browser.OpenAsync("https://uupsocial.tech/tos.html", BrowserLaunchMode.SystemPreferred);
			//Navigation.PushAsync(new TermsAndConditionsPage("https://uupsocial.tech/tos.html"), false);
			checkBox.IsChecked = true;
		}

		private bool Validation()
		{
			if (checkBox.IsChecked)
				return true;
            else
				CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.PleaseReadAndAcceptTermsAndConditionToContinue);
			return false;
		}

		private void Back_Clicked(object sender, EventArgs args) => OnBackButtonPressed();
	}
}