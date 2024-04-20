using Xamarin.Forms;
using System;
using Anonymous.Services;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using CustomViewElements;
using System.Collections.Generic;
using Xamarin.CommunityToolkit.Extensions;
using Utils;

namespace Anonymous.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : BasePage
	{
		private FileResult _mediaFile;
		public LoginPage()
		{
			InitializeComponent();		
			Username.Keyboard = Keyboard.Chat;
		}

		private async void SelectImage_Clicked(object sender, EventArgs e)
		{
			sender.HandleButtonSingleClick();
			var status = await PermissionManager.CheckStoragePermission();
			if (status)
			{
				if (!MediaPicker.IsCaptureSupported)
				{
					await DisplayAlert(Localization.Resources.Dictionary.NoUpload, Localization.Resources.Dictionary.PickingaphotoIsNotSupported, Localization.Resources.Dictionary.Ok).ConfigureAwait(true);
					return;
				}

				_mediaFile = await MediaPicker.PickPhotoAsync();
				if (_mediaFile == null)
					return;

				else if (_mediaFile.ContentType == "image/jpeg" | _mediaFile.ContentType == "image/png")
				{
					var stream = await _mediaFile.OpenReadAsync();

					UserProfilePhoto.Source = ImageSource.FromStream(() => stream);
					UserProfilePhotoBg.BorderColor = Color.Yellow;
				}
				else
				{
					await DisplayAlert(Localization.Resources.Dictionary.Alert, Localization.Resources.Dictionary.SelectedImageTypeIsNotSupported, Localization.Resources.Dictionary.Ok).ConfigureAwait(true);
					return;
				}
			}
		}

		private void Save_Clicked(object sender, EventArgs e)
		{
			sender.HandleButtonSingleClick();
			if (string.IsNullOrWhiteSpace(Username.Text))
			{
				this.DisplayToastAsync(Localization.Resources.Dictionary.PleaseAddValidName);
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
			sender.HandleButtonSingleClick();
			Browser.OpenAsync("https://Anonymoussocial.tech/tos.html", BrowserLaunchMode.SystemPreferred);
			//Navigation.PushAsync(new TermsAndConditionsPage("https://Anonymoussocial.tech/tos.html"), false);
			checkBox.IsChecked = true;
		}

		private bool Validation()
		{
			if (checkBox.IsChecked)
				return true;
            else
				this.DisplayToastAsync(Localization.Resources.Dictionary.PleaseReadAndAcceptTermsAndConditionToContinue);
			return false;
		}

		private void Back_Clicked(object sender, EventArgs args)
		{
			sender.HandleButtonSingleClick();
			OnBackButtonPressed();
		}
	}
}