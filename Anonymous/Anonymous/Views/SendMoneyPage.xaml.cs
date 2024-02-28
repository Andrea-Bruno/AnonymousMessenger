using System;
using EncryptedMessaging;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SendMoneyPage : BasePage
    {
        private readonly Contact _contact;


        public SendMoneyPage(Contact contacts)
        {
            InitializeComponent();
            _contact = contacts;
            Username.Text = _contact.Name;
        }


        protected override bool OnBackButtonPressed()
        {
            Application.Current.MainPage.Navigation.PopAsync(false);
            return true;
        }

        private void Back_Clicked(object sender, EventArgs args) => OnBackButtonPressed();

        void btrSend_Clicked(object sender, EventArgs e)
        {
        }
    }
}