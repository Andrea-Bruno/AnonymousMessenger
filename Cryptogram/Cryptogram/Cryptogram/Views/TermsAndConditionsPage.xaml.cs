using System;
using CustomViewElements;
using Utils;
using Xamarin.Forms.Xaml;

namespace Anonymous.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TermsAndConditionsPage : BasePage
    {
        public TermsAndConditionsPage(string url)
        {
            InitializeComponent();
            //var path = DependencyService.Get<IPathService>().AssetsPathUrl;
            //var url = Path.Combine(path, "PrivacyPolicy.html");
            Toolbar.TitleLabel.FontSize = 23;
            privacyPolicy.Source = url;
        }

        protected override void OnAppearing() => base.OnAppearing();

        private void Back_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            OnBackButtonPressed();
        }
    }
}