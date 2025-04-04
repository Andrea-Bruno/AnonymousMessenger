using CustomViewElements;
using Rg.Plugins.Popup.Services;
using System.Collections.Generic;
using EncryptedMessaging;
using XamarinShared;
using Xamarin.Forms;
using Xamarin.CommunityToolkit.Extensions;
using Utils;

namespace Cryptogram.PopupViews
{
    public partial class DeleteMessagePopupPage : BasePopupPage
    {
        private readonly List<Message> _message;

        public DeleteMessagePopupPage(List<Message> message, bool isMyMessage)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            _message = message;
            if (!isMyMessage)
            {
                DeleteForEveryone.IsVisible = false;
            }
        }
    
        private async void DeleteForMe_Clicked(object sender, System.EventArgs e)
        {
            sender.HandleButtonSingleClick();
            CheckMessageDeleted();

            if (_message != null && XamarinShared.ViewCreator.MessageViewCreator.SelectedMessageHashCode != _message.GetHashCode())
                ChatPageSupport.RemoveMessages(_message, false);
            else
                await this.DisplayToastAsync(Localization.Resources.Dictionary.SelectedMessageCannotBeDeleted);
            await PopupNavigation.Instance.PopAsync(false);

        }

        private async void DeleteForEveryone_Clicked(object sender, System.EventArgs e)
        {
            sender.HandleButtonSingleClick();
            CheckMessageDeleted();

            if (_message != null && XamarinShared.ViewCreator.MessageViewCreator.SelectedMessageHashCode != _message.GetHashCode())
                ChatPageSupport.RemoveMessages(_message, true);
            else
                await this.DisplayToastAsync(Localization.Resources.Dictionary.SelectedMessageCannotBeDeleted);
            await PopupNavigation.Instance.PopAsync(false);
        }

        private void CheckMessageDeleted()
        {
            if (_message == null)
            {
                this.DisplayToastAsync(Localization.Resources.Dictionary.MessageAlreadDeleted);
                PopupNavigation.Instance.PopAsync(false);
                return;
            }
        }

        private void Cancel_Clicked(object sender, System.EventArgs e)
        {
            sender.HandleButtonSingleClick(500);
            PopupNavigation.Instance.PopAsync(false);
        }
    }
}
