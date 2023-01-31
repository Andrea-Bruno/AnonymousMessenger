using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Telegraph.Views;

namespace Telegraph.PopupViews
{
	public partial class PassphrasePopup : Rg.Plugins.Popup.Pages.PopupPage
	{
		private static string _key = "";
		public PassphrasePopup()
		{
			InitializeComponent();
			CloseWhenBackgroundIsClicked = false;
			_key = NavigationTappedPage.Context.My.GetPassphrase();
			var logtime = NavigationTappedPage.Context.SecureStorage.Values.Get("LoggedTime", Preferences.Get("LoggedTime", DateTime.Now));
#if !DEBUG
			if ((DateTime.Now - logtime).TotalDays > 7)
			{
				Skip_Button.IsVisible = false;
				Next_Button.HorizontalOptions = LayoutOptions.CenterAndExpand;
				NextButton.HorizontalOptions = LayoutOptions.FillAndExpand;
			}
#endif
			var parts = _key.Split(' ');
			if (parts.Length == 12)
				_key = parts[0] + " " + parts[1] + " " + parts[2] + " " + parts[3] + "\n" + parts[4] + " " + parts[5] + " " + parts[6] + " " + parts[7] + "\n" + parts[8] + " " + parts[9] + " " + parts[10] + " " + parts[11];
			KeyLabel.Text = _key;
		}

		private async void Next_ClickedAsync(object sender, EventArgs e)
		{
			await PopupNavigation.Instance.PopAsync(false);
			await PopupNavigation.Instance.PushAsync(new PassphraseConfirmationPopup(), true).ConfigureAwait(true);
		}
		private async void Skip_ClickedAsync(object sender, EventArgs e)
		{
			Preferences.Set("isPassphrase", false);
			Preferences.Set("isSkip", true);
			await PopupNavigation.Instance.PopAsync(false);
		}
		private async void Ok_Clicked(object sender, EventArgs e)
		{
			if (PopupNavigation.Instance.PopupStack.Count > 0)
				await PopupNavigation.Instance.PopAsync(false);
		}

		protected override bool OnBackButtonPressed()
		{
			return true;
		}
	}
}