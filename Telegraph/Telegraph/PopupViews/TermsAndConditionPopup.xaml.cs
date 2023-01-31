using Rg.Plugins.Popup.Services;
using System;
using System.IO;
using Telegraph.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Telegraph.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TermsAndConditionPopup : Rg.Plugins.Popup.Pages.PopupPage
	{
		public TermsAndConditionPopup()
		{
			InitializeComponent();
			var path = DependencyService.Get<IPathService>().AssetsPathUrl;
			var url = Path.Combine(path, "PrivacyPolicy.html");
			privacyPolicy.Source = url;
		}

		private void Button_Clicked(object sender, EventArgs e) => OnBackButtonPressed();

		protected override bool OnBackButtonPressed()
		{
			PopupNavigation.Instance.PopAsync(false);
			return true;
		}
	}
}