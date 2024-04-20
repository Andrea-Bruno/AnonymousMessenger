using System;
using CustomViewElements;
using Utils;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Anonymous.Views
{
    public partial class ShowPublicKeyPage : BasePage
    {
        private DateTime _logTime;
        private bool _isPassphrase;
        public ShowPublicKeyPage(DateTime logTime)
        {
            InitializeComponent();
            _logTime = logTime;
            _isPassphrase = Preferences.Get("isPassphrase", false);

            if ((DateTime.Now - logTime).TotalDays <= 7 && !_isPassphrase)
				DoItLaterButton.IsVisible = true;

            string _key = NavigationTappedPage.Context.My.GetPassphrase();
            KeyLabel.Text = _key;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _isPassphrase = Preferences.Get("isPassphrase", false);
            if ((DateTime.Now - _logTime).TotalDays <= 7 || _isPassphrase)
                Toolbar.OnBackBtnClicked += Back_Clicked;
            NextButtonLyt.IsVisible = !_isPassphrase;
        }

        private void Copy_Clicked(object sender, EventArgs e_)
        {
            sender.HandleButtonSingleClick(400);
            Clipboard.SetTextAsync(NavigationTappedPage.Context.My.GetPassphrase());
            this.DisplayToastAsync(Localization.Resources.Dictionary.CopiedToClipboard);
        }

        private void Next_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            var page = new VerifyPassphrase();
            page.CloseEventHandler += ClosePage;
            Application.Current.MainPage.Navigation.PushAsync(page,false);
        }

        private void ClosePage()
        {
            Application.Current.MainPage.Navigation.PopAsync(false);
        }

        protected override bool OnBackButtonPressed()
        {
            if ((DateTime.Now - _logTime).TotalDays > 7 && !Preferences.Get("isPassphrase", false))
            {
                return true;
            }
            Preferences.Set("isSkip", true);
            base.OnBackButtonPressed();
            return true;
        }

        private void SkipClicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            OnBackButtonPressed();
        }

        private void Back_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            OnBackButtonPressed();
        }
    }
}
