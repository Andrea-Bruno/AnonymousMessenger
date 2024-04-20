using Plugin.Toast;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Banking.Views
{
    public partial class PaymentTermsAndConditions : BasePage
    {
        public PaymentTermsAndConditions() => InitializeComponent();
        private void Back_Clicked(object sender, EventArgs e) => OnBackButtonPressed();
        protected override bool OnBackButtonPressed()
        {
            Application.Current.MainPage.Navigation.PopAsync();
            return true;
        }
        private async void Accept_Clicked(object sender, EventArgs e)
        {
            if (checkBox.IsChecked)
            {
                XamarinShared.Setup.Setting.PaymentTick = true;
                XamarinShared.Setup.Settings.Save();
                // CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.TheFunctionalityHasNotBeenImplemented);
                await Application.Current.MainPage.Navigation.PushAsync(new AddCardPage(), false).ConfigureAwait(true);
                Navigation.RemovePage(this);
            }
            else
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.PleaseReadAndAcceptTermsAndConditionToContinue);
        }
#pragma warning disable CS1998 // In questo metodo asincrono non sono presenti operatori 'await', pertanto verrà eseguito in modo sincrono. Provare a usare l'operatore 'await' per attendere chiamate ad API non di blocco oppure 'await Task.Run(...)' per effettuare elaborazioni basate sulla CPU in un thread in background.
        private async void Terms_Clicked(object sender, EventArgs e)
#pragma warning restore CS1998 // In questo metodo asincrono non sono presenti operatori 'await', pertanto verrà eseguito in modo sincrono. Provare a usare l'operatore 'await' per attendere chiamate ad API non di blocco oppure 'await Task.Run(...)' per effettuare elaborazioni basate sulla CPU in un thread in background.
        {
            //await PopupNavigation.Instance.PushAsync(new TermsAndConditionPopup(), true);
            checkBox.IsChecked = true;
        }

    }
}