using Xamarin.Forms;

namespace CryptoWalletUI
{
    public class BasePage : ContentPage
    {
        public BasePage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            BackgroundColor = Color.FromHex("#F6F8FB");
        }
    }
}