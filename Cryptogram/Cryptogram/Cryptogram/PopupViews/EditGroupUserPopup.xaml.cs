using CustomViewElements;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using Cryptogram.ViewModels;
using EncryptedMessaging;
using Xamarin.Forms.Xaml;
using Utils;

namespace Cryptogram.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditGroupUserPopup : BasePopupPage
    {
        private readonly Contact _contact;
        public EditGroupUserPopup(Contact contact)
        {
            InitializeComponent();

            _contact = contact;
            ViewText.Text += " " + contact.Name;
            MessageText.Text += " " + contact.Name;
        }

        private async void Message_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            await Navigation.PushAsync(new ChatRoom(_contact), false);
        }

        private async void View_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            await Navigation.PushAsync(new ChatUserProfilePage(_contact), false);
        }

        private void Background_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            OnBackButtonPressed();
        }
    }
}