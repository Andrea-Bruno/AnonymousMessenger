using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AnonymousWhiteLabel.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RestoreAccount : ContentPage
    {
        public RestoreAccount(System.Action<string> onClose)
        {
            InitializeComponent();
            CreateAccount.Command = new Command(() => {
                Xamarin.Essentials.Preferences.Set("firstStartup", false);
                onClose.Invoke(null); });
            Restore.Command = new Command(() =>
           {
               try
               {
                   var passphrase = Passphrase.Text.ToLower();
                   _ = new NBitcoin.Mnemonic(passphrase, NBitcoin.Wordlist.AutoDetect(passphrase));
                   Xamarin.Essentials.Preferences.Set("firstStartup", false);
                   onClose.Invoke(passphrase);
               }
               catch (System.Exception)
               {
                   DisplayAlert(EncryptedMessaging.Resources.Dictionary.Alert, EncryptedMessaging.Resources.Dictionary.InvalidPassphrase, EncryptedMessaging.Resources.Dictionary.Ok);
               }
           }
            );
        }
    }
}