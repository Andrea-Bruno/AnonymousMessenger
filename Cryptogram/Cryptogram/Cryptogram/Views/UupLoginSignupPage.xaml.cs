using System;
using CustomViewElements;
using Utils;
using Microsoft.Maui.Controls;

namespace Cryptogram.Views
{
    public partial class CryptogramLoginSignupPage : BasePage
    {
        public CryptogramLoginSignupPage()
        {
            InitializeComponent();
        }

        public void SignUp_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            Application.Current.MainPage.Navigation.PushAsync(new LoginPage(), false);
        }

        public void Login_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            Application.Current.MainPage.Navigation.PushAsync(new RecoverPage(), false);
        }
    }
}
