using Plugin.Toast;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Telegraph.Utils;
using static Telegraph.Utils.Utils;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Telegraph.Services;

namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LockScreenPage : BasePage
    {
        private static string _pin = "";
        public LockScreenPage()
        {
            InitializeComponent();
            _pin = "";
        }

        private async void Number_Button_Clicked(object sender, EventArgs e)
        {
            var s = (sender as Button).Text;

            if (Pin.Text.Length < Defaults.PinLength)
            {
                _pin += s;
                Pin.Text += s;
                await Task.Delay(300);
                Pin.Text = string.Concat(Enumerable.Repeat("*", _pin.Length));
            }
        }
        private async void Confirm_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Pin.Text))
            {
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.PleaseEnterYourPin);
                return;
            }

            var name = NavigationTappedPage.Context.My.Name;
            var numberOfAttempts = App.Setting.NumberOfAttempts;
            var lastAttemptTime = App.Setting.LastAttemptTime;
            if (Math.Abs(ConvertToUnixTimestamp() - lastAttemptTime) > 60)
            {
                App.Setting.NumberOfAttempts = 0;
                numberOfAttempts = 0;
            }
            if (numberOfAttempts != 0 && numberOfAttempts % Defaults.MaxNumberOfLoginAttempts == 0 && Math.Abs(ConvertToUnixTimestamp() - lastAttemptTime) < 600)
            {
                Status.Text = Localization.Resources.Dictionary.YouHaveEnteredSixtimeswrongPinPleaseWait + Math.Ceiling((Math.Abs(lastAttemptTime - ConvertToUnixTimestamp() + 600)) / 60.0) + Localization.Resources.Dictionary.Minutes;
            }
            else
            {
                if (GetHashString(_pin) == App.Setting.Pin)
                {
                    ShowProgressDialog();

                    App.Setting.NumberOfAttempts = 0;
                    App.Setting.NumberOfWrongAttemps = 0;
                    App.Setting.LastAttemptTime = ConvertToUnixTimestamp();
                    Application.Current.MainPage = new NavigationPage(new NavigationTappedPage());
                    if (Device.RuntimePlatform == Device.iOS)
                        DependencyService.Get<IStatusBarColor>().SetStatusbarColor(Color.FromHex("#DEAF03"));
                }
                else
                {
                    App.Setting.NumberOfAttempts = numberOfAttempts + 1;
                    App.Setting.LastAttemptTime = ConvertToUnixTimestamp();
                    Status.Text = Localization.Resources.Dictionary.WrongPinYouHave + (Defaults.MaxNumberOfLoginAttempts - numberOfAttempts % 6) + Localization.Resources.Dictionary.attempts;
                    ClearPin();
                }
            }
            TelegraphXamarinShared.Setup.Settings.Save();
        }

        private void Clean_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_pin))
            {
                _pin = _pin.Substring(0, _pin.Length - 1);
                Pin.Text = Pin.Text.Substring(0, Pin.Text.Length - 1);
            }
        }

        private void ClearPin()
        {
            Pin.Text = string.Empty;
            _pin = "";
        }

        public static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            var sb = new StringBuilder();
            foreach (var b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}