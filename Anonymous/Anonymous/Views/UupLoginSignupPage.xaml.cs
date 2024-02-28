using System;
using CustomViewElements;
using Xamarin.Forms;

namespace Telegraph.Views
{
    public partial class UupLoginSignupPage : BasePage
    {
        public UupLoginSignupPage()
        {
            InitializeComponent();
        }

        public void SignUp_Clicked(object sender, EventArgs e) => Application.Current.MainPage.Navigation.PushAsync(new LoginPage(), false);
      
        public void Login_Clicked(object sender, EventArgs e) => Application.Current.MainPage.Navigation.PushAsync(new RecoverPage(), false);     
    }
}
