using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TokenSharePage : BasePage
    {
        public TokenSharePage()
        {
            InitializeComponent();
            tokenLabel.Text += tokenGeneration();
        }
        protected override bool OnBackButtonPressed()
        {
            Application.Current.MainPage.Navigation.PopAsync(false);
            return true;
        }

        private void Back_Clicked(object sender, EventArgs args) => OnBackButtonPressed();


        private async void ShareInviteLink_ClickedAsync(object sender, EventArgs args)
        {
            await Share.RequestAsync(new ShareTextRequest
            {
                Title = Localization.Resources.Dictionary.InviteLink,
                Text = (tokenLabel as Label).Text
            });
        }


        private string tokenGeneration()
        {
            var publicKey = NavigationTappedPage.Context.My.GetPublicKey().Substring(0, 10);
            return publicKey;
        }
    }
}