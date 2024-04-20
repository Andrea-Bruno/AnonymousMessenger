using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Banking;
using Banking.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Banking.Views
{
    public partial class PaymentHistory : BasePage
    {
        // public readonly string cardNumber;
        public PaymentHistory()
        {
            //       BindingContext = cardNumber = _cardNumber;
            InitializeComponent();
            XamarinShared.Setup.Settings.Save();
        }
        /* private void initializeCardNumber(string _cardNumber)
         {
             InitializeComponent();
             CardNumber.Text = cardNumber;
         }
        */
        private void CheckBoxCase()
        {
            checkBoxMaster.IsChecked = XamarinShared.Setup.Setting.CheckBoxTick_Master != false;
            checkBoxVisa.IsChecked = XamarinShared.Setup.Setting.CheckBoxTick_Visa != false;
        }
        private void Back_Clicked(object sender, EventArgs e)
        {
            OnBackButtonPressed();
        }
        protected override bool OnBackButtonPressed()
        {
            Application.Current.MainPage.Navigation.PopAsync(false);
            return true;
        }

        private void AddNewAccount_Clicked(object sender, EventArgs e) => Application.Current.MainPage.Navigation.PushAsync(new AddCardPage(), false);

        private void Help_Clicked(object sender, EventArgs e) {
            //Application.Current.MainPage.Navigation.PushAsync(new FAQPage(), false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CheckBoxCase();
        }

        private void OnItemSelected(object sender, ItemTappedEventArgs args)
        {
        }

#pragma warning disable CS1998 // In questo metodo asincrono non sono presenti operatori 'await', pertanto verrà eseguito in modo sincrono. Provare a usare l'operatore 'await' per attendere chiamate ad API non di blocco oppure 'await Task.Run(...)' per effettuare elaborazioni basate sulla CPU in un thread in background.
        public async void Save_Clicked(object sender, EventArgs e)
#pragma warning restore CS1998 // In questo metodo asincrono non sono presenti operatori 'await', pertanto verrà eseguito in modo sincrono. Provare a usare l'operatore 'await' per attendere chiamate ad API non di blocco oppure 'await Task.Run(...)' per effettuare elaborazioni basate sulla CPU in un thread in background.
        {
            //Application.Current.MainPage = new NavigationPage(new NavigationTappedPage());
            XamarinShared.Setup.Settings.Save();
            //if (Device.RuntimePlatform == Device.iOS)
             //   DependencyService.Get<IStatusBarColor>().SetStatusbarColor(Color.FromHex("#DEAF03"));
        }

        void checkBoxMaster_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (checkBoxMaster.IsChecked)
            {
                XamarinShared.Setup.Setting.CheckBoxTick_Visa = false;
                checkBoxVisa.IsChecked = false;
            }
            else
            {
                XamarinShared.Setup.Setting.CheckBoxTick_Visa = true;
                checkBoxVisa.IsChecked = true;
            }
            XamarinShared.Setup.Settings.Save();
        }

        void checkBoxVisa_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (checkBoxVisa.IsChecked)
            {
                XamarinShared.Setup.Setting.CheckBoxTick_Master = false;
                checkBoxMaster.IsChecked = false;
            }
            else
            {
                XamarinShared.Setup.Setting.CheckBoxTick_Master = true;
                checkBoxMaster.IsChecked = true;
            }
            XamarinShared.Setup.Settings.Save();
        }
    }
}