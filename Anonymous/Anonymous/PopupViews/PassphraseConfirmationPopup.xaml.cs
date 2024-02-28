using System;
using System.Collections.Generic;
using System.IO;
using Plugin.Media.Abstractions;
using Plugin.Toast;
using Rg.Plugins.Popup.Services;
using Telegraph.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Telegraph.PopupViews
{
	public partial class PassphraseConfirmationPopup : Rg.Plugins.Popup.Pages.PopupPage
	{
		//private static string _key = "";
#pragma warning disable CS0169 // The field 'PassphraseConfirmationPopup._file' is never used
		private MediaFile _file;
#pragma warning restore CS0169 // The field 'PassphraseConfirmationPopup._file' is never used

		public PassphraseConfirmationPopup()
		{
			InitializeComponent();
			CloseWhenBackgroundIsClicked = false;
			//_key = NavigationTappedPage.Context.My.GetPassphrase();
			//var parts = _key.Split(' ');
			//if (parts.Length == 12)
			//{
			//	for(int i =0; i < 12; i++)
			//  {
			//	Random random = new Random();
			//_key += parts[0] + ", ";
			//_key += random.Next(0, _key.Length);
			//       }
			// }
			//_key = parts[0] + " " + parts[1] + " " + parts[2] + " " + parts[3] + "\n" + parts[4] + " " + parts[5] + " " + parts[6] + " " + parts[7] + "\n" + parts[8] + " " + parts[9] + " " + parts[10] + " " + parts[11];
			//
			//
			//
			//ConfimKeyLabel.Text = _key;
		}
		private async void Confirm_Clicked(object sender, EventArgs e)
		{
			if (CheckPassPhrase())
			{
				// Application.Current.MainPage.Navigation.PushAsync(new NavigationTappedPage(), false);
				Preferences.Set("isPassphrase", true);
				Preferences.Set("isSkip", false);
				await PopupNavigation.Instance.PopAsync(false);
			}
		}
		private bool CheckPassPhrase()
		{
			var passphrase = (p1.Text + " " + p2.Text + " " + p3.Text + " " + p4.Text + " " + p5.Text + " " + p6.Text + " " + p7.Text + " " + p8.Text + " " + p9.Text + " " + p10.Text + " " + p11.Text + " " + p12.Text).Trim();
			var validate = EncryptedMessaging.Functions.PassphraseValidation(passphrase);
			if (!validate)
			{
				CrossToastPopUp.Current.ShowToastMessage(EncryptedMessaging.Resources.Dictionary.InvalidPassphrase);
			}
			else
			{
				App.Passphrase = passphrase;
				CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.Done);
			}
			return validate;
		}


		private async void Back_Clicked(object sender, EventArgs e)
		{
			await PopupNavigation.Instance.PopAsync(false);
			await PopupNavigation.Instance.PushAsync(new PassphrasePopup(), false);
		}
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

		protected override bool OnBackButtonPressed()
		{
			return true;
		}
	}
}
