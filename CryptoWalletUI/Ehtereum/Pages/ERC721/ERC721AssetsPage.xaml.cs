using CryptoWalletLibrary.Ehtereum.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CryptoWalletUI.Ehtereum.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ERC721AssetsPage : BasePage
    {
        public ERC721AssetsPage()
        {
            BindingContext = ERC721ViewModelLocator.ERC721ViewModel;
            InitializeComponent();

            if (Device.RuntimePlatform == Device.UWP)
            {
                var refreshButton = new Button
                {
                    Text = "Refresh"
                };

                refreshButton.Clicked += async (object _, EventArgs e) =>
                {
                    refreshButton.IsEnabled = false;
                    await EthTxViewModelLocator.EthTxViewModel.Refresh();
                    refreshButton.IsEnabled = true;
                };

                RefreshLayout.Children.Add(refreshButton);
            }
        }
    }
}