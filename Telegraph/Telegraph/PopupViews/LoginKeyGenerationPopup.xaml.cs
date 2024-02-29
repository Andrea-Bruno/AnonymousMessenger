using Plugin.Toast;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;


namespace Telegraph.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginKeyGenerationPopup : Rg.Plugins.Popup.Pages.PopupPage
	{
		private static string _key = "";
		public LoginKeyGenerationPopup()
		{
			InitializeComponent();
		
			_key = NavigationTappedPage.Context.My.GetPassphrase();
			var parts = _key.Split(' ');
			if (parts.Length == 12)
			{
				_key = parts[0] + " " + parts[1] + " " + parts[2] + " " + parts[3] + "\n" + parts[4] + " " + parts[5] + " " + parts[6] + " " + parts[7] + "\n" + parts[8] + " " + parts[9] + " " + parts[10] + " " + parts[11];
				keyLabel.Text = _key;
			}
			else
			{
				keyLabel.Text = _key;
			}
		}

		private async void ExportPrivateKey_ClickedAsync(object sender, EventArgs e)
		{
			if (PopupNavigation.Instance.PopupStack.Count > 0)
				await PopupNavigation.Instance.PopAsync(false);
		}

		private string encryptPrivateKey(string privateKey, string key) => Models.Crypto.Encrypt(privateKey, key);

		private async void CopyButton_ClickAsync(object sender, EventArgs e)
		{
			await Clipboard.SetTextAsync(keyLabel.Text);
			CrossToastPopUp.Current.ShowToastMessage("Copied to clipboard successfully");
		}

	}
}