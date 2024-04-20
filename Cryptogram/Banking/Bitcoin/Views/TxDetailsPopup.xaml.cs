using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms.Xaml;


namespace Banking.Bitcoin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TxDetailsPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        public TxDetailsPopup()
        {
            InitializeComponent();
        }

        public async void Confirm_Clicked(object _, EventArgs e) => await PopupNavigation.Instance.PopAsync(false);

        private async void Back_Clicked(object sender, EventArgs e) => await PopupNavigation.Instance.PopAsync(false);
    }
}