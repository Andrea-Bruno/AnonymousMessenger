using Plugin.Toast;
using System;
using System.Security.Cryptography;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinShared;
using Telegraph.Services;
using static Utils.Utils;
using Utils;
using CustomViewElements;
using Telegraph.DesignHandler;

namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreatePinPage : BasePage
    {
        private string _confirmPin;
        private string _firstPin;
        private bool _isPinConfirmEnable;
        private string _lockPin;
        public CreatePinPage()
        {
            InitializeComponent();
            _lockPin = Setup.GetSecureValue("LockPin");
            if (_lockPin != null)
            {
                TopLabel.IsVisible = true;
                TopLabel.Text = Localization.Resources.Dictionary.PleaseEnterYourPin;
            }
            else
            {
                Toolbar.OnBackBtnClicked += Back_Clicked;
            }
        }

        private void CheckPinMatching()
        {
            if (_confirmPin == _firstPin)
            {
                Setup.SetSecureValue("LockPin", GetHashString(_firstPin));
                SaveValues(0);
                Application.Current.MainPage.Navigation.PopAsync(false);
            }
            else
            {
                TopLabel.Text = Localization.Resources.Dictionary.ThePinDoesNotMatch;
                ClearAllPins();
                _confirmPin = "";
            }
        }

        private bool FillPin()
        {
            Frame frame = null;
            foreach (Frame frame1 in PinGrid.Children)
            {
                if (!frame1.IsEnabled)
                {
                    frame = frame1;
                    break;
                }
            }
            if (frame == null)
                return false;

            frame.IsEnabled = true;
            frame.BackgroundColor = DesignResourceManager.GetColorFromStyle("Theme");
            return true;
        }

        private void ClearAllPins()
        {
            foreach (Frame frame in PinGrid.Children)
            {
                ClearPin(frame);
            }
            if (!_isPinConfirmEnable)
            {
                _firstPin = "";
            }
            else 
            {
                _confirmPin = "";
            }
        }
        private void ClearPin(Frame frame)
        {
            frame.IsEnabled = false;
            frame.BackgroundColor = DesignResourceManager.GetColorFromStyle("Color1");
        }

        private void CheckLockPin()
        {
            var numberOfAttempts = Convert.ToInt32(Setup.GetSecureValue("NumberOfAttempts"));
            var lastAttemptTime = Convert.ToInt64(Setup.GetSecureValue("LastAttemptTime"));
            if (Math.Abs(ConvertToUnixTimestamp() - lastAttemptTime) > 60)
            {
                Setup.SetSecureValue("NumberOfAttempts", 0 + "");
                numberOfAttempts = 0;
            }
            if (numberOfAttempts != 0 && numberOfAttempts % Defaults.MaxNumberOfLoginAttempts == 0 && Math.Abs(ConvertToUnixTimestamp() - lastAttemptTime) < 600)
            {
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.YouHaveEnteredSixtimeswrongPinPleaseWait + Math.Ceiling((Math.Abs(lastAttemptTime - ConvertToUnixTimestamp() + 600)) / 60.0) + " " + Localization.Resources.Dictionary.Minutes);
            }
            else
            {
                if (GetHashString(_firstPin) == _lockPin)
                {
                    ShowProgressDialog();
                    new System.Threading.Timer((object obj) => { Device.BeginInvokeOnMainThread(() => OpenNavigationPage()); }, null, 100, System.Threading.Timeout.Infinite);

                }
                else
                {
                    SaveValues(numberOfAttempts + 1);
                    CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.WrongPinYouHave + (Defaults.MaxNumberOfLoginAttempts - numberOfAttempts % 6) + " " + Localization.Resources.Dictionary.attempts);
                    ClearAllPins();
                }
            }
        }

        private void OpenNavigationPage()
        {
            Setup.SetSecureValue("LockPin", GetHashString(_firstPin));
            SaveValues(0);
            Application.Current.MainPage = new NavigationPage(new NavigationTappedPage());

            if (Device.RuntimePlatform == Device.iOS)
                DependencyService.Get<IStatusBarColor>().SetStatusbarColor(DesignResourceManager.GetColorFromStyle("Color1"));
        }

        private void SaveValues(int numberOfAttemps)
        {
            Setup.SetSecureValue("LastAttemptTime", ConvertToUnixTimestamp()+ "");
            Setup.SetSecureValue("NumberOfAttempts", numberOfAttemps + "");
        }
        private void Number_Button_Clicked(object sender, EventArgs _)
        {
            var s = (sender as Button).Text;
            if (FillPin())
            {
                if (!_isPinConfirmEnable)
                    _firstPin += s;
                else
                    _confirmPin += s;
            }
        }

        private void Clean_Clicked(object _, EventArgs e)
        {
            Frame frame;
            for (int i = PinGrid.Children.Count - 1; i >= 0; i--)
            {
                frame = PinGrid.Children[i] as Frame;
                if (frame.IsEnabled)
                {
                    ClearPin(frame);
                    break;
                }
            }

            if (!string.IsNullOrEmpty(_firstPin) && !_isPinConfirmEnable)
            {
                _firstPin = _firstPin.Substring(0, _firstPin.Length - 1);
            }
            else if (!string.IsNullOrEmpty(_confirmPin) && _isPinConfirmEnable)
            {
                _confirmPin = _confirmPin.Substring(0, _confirmPin.Length - 1);
            }
        }

        private void Confirm_Clicked(object _, EventArgs e)
        {
            if (!PinGrid.Children[PinGrid.Children.Count - 1].IsEnabled)
            {
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.ValidPin);
                return;
            }
            if (_lockPin != null)
            {
                CheckLockPin();
                return;
            }
            if (!_isPinConfirmEnable)
            {
                // Open ConfirmPin page
                _isPinConfirmEnable = true;
                TopLabel.Text = Localization.Resources.Dictionary.PleaseConfirmYourPinCode;
                ClearAllPins();
            }
            else
                CheckPinMatching();
        }       

        protected override bool OnBackButtonPressed()
        {
            if (_isPinConfirmEnable)
            {
                TopLabel.Text = Localization.Resources.Dictionary.PleaseEnterYourPin;
                _firstPin = "";
                _confirmPin = "";
                ClearAllPins();
            }
            else
                Navigation.PopAsync(false);
            return false;
        }

        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            var sb = new StringBuilder();
            foreach (var b in GetHash(inputString))
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        private void Back_Clicked(object _, EventArgs e) => OnBackButtonPressed();
    }
}