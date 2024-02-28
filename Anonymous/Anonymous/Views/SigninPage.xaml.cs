using System;
using System.Collections.Generic;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Rg.Plugins.Popup.Services;
using Plugin.Toast;
using Xamarin.Forms;
using Telegraph.Services;
using Xamarin.Forms.Xaml;
using Telegraph.PopupViews;
using System.IO;

namespace Telegraph.Views
{
	public partial class SigninPage : BasePage
	{
		//private MediaFile _file;
		public SigninPage()
		{
			InitializeComponent();
		}

		private async void Restore_Clicked(object sender, EventArgs e)
		{
			if (Restore_Validation() && CheckPassPhrase())
			{
				//ShowProgressDialog();
				Save();
			}
		}

		private void Save()
		{
			Xamarin.Essentials.Preferences.Set("isPassphrase", true);
			Application.Current.MainPage.Navigation.PushAsync(new NavigationTappedPage(), false);
			Navigation.RemovePage(this);
			//HideProgressDialog();
		}

		private bool CheckPassPhrase()
		{
			var passphrase = (p1.Text + " " + p2.Text + " " + p3.Text + " " + p4.Text + " " + p5.Text + " " + p6.Text + " " + p7.Text + " " + p8.Text + " " + p9.Text + " " + p10.Text + " " + p11.Text + " " + p12.Text).Trim();
			var validate = EncryptedMessaging.Functions.PassphraseValidation(passphrase);
			if (!validate)
				CrossToastPopUp.Current.ShowToastMessage(EncryptedMessaging.Resources.Dictionary.InvalidPassphrase);
			else
				App.Passphrase = passphrase;
			return validate;
		}

		private bool Restore_Validation()
		{
			if (checkBox_restore.IsChecked)
				return true;
			else
				CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.PleaseMakeSureAndAcceptThatThisKeyWillBeOnlyUseForThisDevice);
			return false;
		}

		//private byte[] GetByteFromStream(MediaFile file)
		//{
		//	using (var memoryStream = new MemoryStream())
		//	{
		//		file.GetStream().CopyTo(memoryStream);
		//		file.Dispose();
		//		return memoryStream.ToArray();
		//	}
		//}

		protected override bool OnBackButtonPressed()
		{
			Application.Current.MainPage.Navigation.PushAsync(new UupLoginSignupPage(), false);
			Navigation.RemovePage(this);
			return true;
		}

		private void Back_Clicked(object sender, EventArgs e) => OnBackButtonPressed();

		void Entry_FocusChanged(object sender, FocusEventArgs e)
		{
			(sender as CustomEntry).PlaceholderColor = Color.FromHex("#9B9B9B");
			EntryColorChange();
		}
		void Entry_UnfocusChanged(object sender, FocusEventArgs e)
		{
			(sender as CustomEntry).PlaceholderColor = Color.FromHex("#ffffff");
			EntryColorChange();
		}

		private void EntryColorChange()
		{
			p1_frame.BackgroundColor = string.IsNullOrEmpty(p1.Text) ? Color.White : Color.FromHex("#FFD62C");
			p2_frame.BackgroundColor = string.IsNullOrEmpty(p2.Text) ? Color.White : Color.FromHex("#FFD62C");
			p3_frame.BackgroundColor = string.IsNullOrEmpty(p3.Text) ? Color.White : Color.FromHex("#FFD62C");
			p4_frame.BackgroundColor = string.IsNullOrEmpty(p4.Text) ? Color.White : Color.FromHex("#FFD62C");
			p5_frame.BackgroundColor = string.IsNullOrEmpty(p5.Text) ? Color.White : Color.FromHex("#FFD62C");
			p6_frame.BackgroundColor = string.IsNullOrEmpty(p6.Text) ? Color.White : Color.FromHex("#FFD62C");
			p7_frame.BackgroundColor = string.IsNullOrEmpty(p7.Text) ? Color.White : Color.FromHex("#FFD62C");
			p8_frame.BackgroundColor = string.IsNullOrEmpty(p8.Text) ? Color.White : Color.FromHex("#FFD62C");
			p9_frame.BackgroundColor = string.IsNullOrEmpty(p9.Text) ? Color.White : Color.FromHex("#FFD62C");
			p10_frame.BackgroundColor = string.IsNullOrEmpty(p10.Text) ? Color.White : Color.FromHex("#FFD62C");
			p11_frame.BackgroundColor = string.IsNullOrEmpty(p11.Text) ? Color.White : Color.FromHex("#FFD62C");
			p12_frame.BackgroundColor = string.IsNullOrEmpty(p12.Text) ? Color.White : Color.FromHex("#FFD62C");

		}
	}
}