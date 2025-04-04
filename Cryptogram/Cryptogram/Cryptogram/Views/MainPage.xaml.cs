using ChatComposer;
using CustomViewElements;
using System;
using EncryptedMessaging;
using XamarinShared;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Cryptogram.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : BasePage
    {
        private string query;
        public MainPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                InitializeComponent();
            }
            ChatList.SetPlaceHolderVisibility(PlaceHolderVisibility);
            ChatList.Init(ChatItemClicked, NavigationTappedPage.Context.Contacts.GetContacts());
            Toolbar.AddNewChatButton(AddNewChat_Click);
            Toolbar.AddSearchButton(null);
            Toolbar.SearchEntry.TextChanged += Search_TextChanged;
        }

        public void PlaceHolderVisibility(bool isVisible)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                NoItemPage.IsVisible = isVisible && string.IsNullOrWhiteSpace(query);
                NoResultPage.IsVisible = isVisible && !string.IsNullOrWhiteSpace(query);
                ChatList.IsVisible = !isVisible;
            });
        }

        protected override void OnDisappearing()
        {
            Toolbar.ClearViewState();
            PopupView.IsVisible = false;
        }

        private void AddNewChat_Click(object sender, EventArgs e)
        {
            PopupView.IsVisible = !PopupView.IsVisible;
        }

        private void AddNewGroup_Click(object sender, EventArgs e)
        {
            Application.Current.MainPage.Navigation.PushAsync(new GroupUserSelectPage(), false).ConfigureAwait(true);
        }

        private void AddNewContact_Click(object sender, EventArgs e)
        {
            Application.Current.MainPage.Navigation.PushAsync(new EditItemPage(), false).ConfigureAwait(true);
        }

        private void ChatItemClicked(Contact contact, ChatItemClickType chatItemClick)
        {
            switch (chatItemClick)
            {
                case ChatItemClickType.CLEAR:
                    OnChatItemClearClicked(contact);
                    break;
                case ChatItemClickType.EDIT:
                    OnChatItemEditClicked(contact);
                    break;
                case ChatItemClickType.DELETE:
                    OnChatItemDeleteClicked(contact);
                    break;
                default:
                    OnChatItemTapped(contact);
                    break;
            }
        }

        private async void OnChatItemTapped(Contact contact)
        {
            if (App.IsVideoUploading && App.ContactVideoUploading == contact)
                await Application.Current.MainPage.Navigation.PushAsync(App.ChatRoomVideoUploading, false).ConfigureAwait(true);
            else
                await Application.Current.MainPage.Navigation.PushAsync(new ChatRoom(contact), false).ConfigureAwait(true);
        }

        private async void OnChatItemEditClicked(Contact contact)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new ChatUserProfilePage(contact), false).ConfigureAwait(true);
            ChatList.ResetSwipe();
        }

        private async void OnChatItemClearClicked(Contact contact)
        {
            var clearAnswer = await Application.Current.MainPage.DisplayAlert(Localization.Resources.Dictionary.Alert, Localization.Resources.Dictionary.ClearAlertQuestion, Localization.Resources.Dictionary.Yes, Localization.Resources.Dictionary.No);
            if (clearAnswer)
            {
                Calls.GetInstance().ClearCleanedContactCalls(contact.PublicKeys);
                NavigationTappedPage.Context.Contacts.ClearContact(contact.PublicKeys);
                ChatList.ClearState();
                ChatList.ResetSwipe();
            }
        }

        private async void OnChatItemDeleteClicked(Contact contact)
        {
            var deleteAnswer = await Application.Current.MainPage.DisplayAlert(Localization.Resources.Dictionary.Alert, Localization.Resources.Dictionary.DeleteAlertQuestion, Localization.Resources.Dictionary.Yes, Localization.Resources.Dictionary.No);
            if (deleteAnswer)
            {
                contact.IsBlocked = true;
                Calls.GetInstance().ClearCleanedContactCalls(contact.PublicKeys);
                NavigationTappedPage.Context.Contacts.RemoveContact(contact);
                ChatList.ClearState();
                ChatList.ResetSwipe();
            }
        }

        private void Search_TextChanged(object sender, EventArgs e)
        {
            query = ((CustomEntry)sender).Text;
            ChatList.FilterContacts(query);
        }
    }
}