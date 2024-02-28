using System;
using System.Collections.Generic;
using System.Linq;
using Telegraph.ViewModels;
using Xamarin.Forms.Xaml;
using Plugin.Toast;
using EncryptedMessaging;
using System.Collections.ObjectModel;
using static EncryptedMessaging.Contacts;
using static EncryptedMessaging.MessageFormat;
using System.Text;
using CustomViewElements;
using Utils;
using Telegraph.DesignHandler;

namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GroupUserSelectPage : BasePage
    {
        private SelectUserModel _lastItemSelected;
        private readonly List<Contact> _selectedUsers = new List<Contact>();
        private readonly ObservableCollection<SelectUserModel> _selectedUsersModel = new ObservableCollection<SelectUserModel>();
        private bool IsMessageForwarding;
        private bool IsImageForwarding;
        private Message Message;
        private readonly List<Message> Messages;
        private readonly byte[] _sharedImage;

        private bool IsPlaceholderVisible
        {
            set
            {
                NoResultPage.IsVisible = value;
                ItemsListView.IsVisible = !value;
            }
        }

        public GroupUserSelectPage(List<Message> _messages = null)
        {
            InitializeComponent();
            SelectedUsers.Text = "0 " + Localization.Resources.Dictionary.Selected;
            IsMessageForwarding = _messages != null;
            Messages = _messages;
        }

        public GroupUserSelectPage(List<Contact> participants, Contact contact)
        {
            InitializeComponent();
            SelectedUsers.Text = "0 " + Localization.Resources.Dictionary.Selected;
            _selectedUsers = participants;
            IsMessageForwarding = false;
        }

        public GroupUserSelectPage(byte[] image)
        {
            InitializeComponent();
            _sharedImage = image;
            IsImageForwarding = true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _lastItemSelected = null;
            GetSelectedUserList();
        }

        private void OnItemSelected(object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs args)
        {
            _lastItemSelected = args.ItemData as SelectUserModel;
            if (_lastItemSelected != null)
            {
                foreach (SelectUserModel selectUserModel in _selectedUsersModel)
                {
                    if (_lastItemSelected.contact.ChatId == selectUserModel.contact.ChatId)
                        selectUserModel.isVisible = !selectUserModel.isVisible;
                }
                if (_selectedUsers.Contains(_lastItemSelected.contact))
                    _selectedUsers.Remove(_lastItemSelected.contact);
                else
                    _selectedUsers.Add(_lastItemSelected.contact);
            }
            ItemsListView.SelectedItem = null;
            SelectedUsers.Text = string.Format("{0} " + Localization.Resources.Dictionary.Selected, _selectedUsers.Count); ;
            ObservableCollection<SelectUserModel> _selectedUsersModelList = new ObservableCollection<SelectUserModel>();
            foreach (var item in _selectedUsers)
            {
                _selectedUsersModelList.Add(new SelectUserModel(item, _selectedUsers.Contains(item)));
            }
            SelectedItemsListView.ItemsSource = _selectedUsersModelList;
            if (_selectedUsersModelList.Count > 0)
            {
               if (!Next.GetTag().Equals("ic_next_new.png"))
                {
                    SelectedItemPane.IsVisible = true;
                    Next.Source = DesignResourceManager.GetImageSource("ic_next_new.png");
                    Next.SetTag("ic_next_new.png");
                }                    
            }
            else if(_selectedUsersModelList.Count == 0) 
            {
                    if (!Next.GetTag().Equals("ic_next_disabled.png")) {
                        SelectedItemPane.IsVisible = false;
                        Next.Source = DesignResourceManager.GetImageSource("ic_next_disabled.png");
                        Next.SetTag("ic_next_disabled.png");
                    }
            }
            else
            {
                if (!Next.GetTag().Equals("ic_next_disabled.png"))
                {
                    SelectedItemPane.IsVisible = false;
                    Next.Source = DesignResourceManager.GetImageSource("ic_next_disabled.png");
                    Next.SetTag("ic_next_disabled.png");
                }
            }
        }

        private async void Next_Clicked(object sender, EventArgs args)
        {
            if (_selectedUsers.Count() > 1 && !IsMessageForwarding)
                await Navigation.PushAsync(new GroupCreatePage(_selectedUsers), false);
            else if (_selectedUsers.Count() > 0 && IsMessageForwarding)
                ForwardMessages();
            else if (_selectedUsers.Count() > 0 && IsImageForwarding)
                ForwardImage();
            else if (_selectedUsers.Count() == 0 && IsMessageForwarding) { }
            else  
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.YouNeedToSelectAtLeastTwoUser);
        }

        private void Search_TextChanged(object sender, EventArgs e)
        {
            var find = ((CustomEntry)sender).Text;
            if (ItemsListView.DataSource != null)
            {
                ItemsListView.DataSource.Filter = FilterContacts;
                ItemsListView.DataSource.RefreshFilter();
                var filteredDataCount = ItemsListView.DataSource.Items.Count;
                IsPlaceholderVisible = filteredDataCount == 0;
            }
        }

        private bool FilterContacts(object obj)
        {
            if (SearchBar == null || SearchBar.Text == null)
                return true;

            var selectUser = obj as SelectUserModel;
            if (selectUser.contact.Name.ToLower().Contains(SearchBar.Text.ToLower()))
                return true;
            else
                return false;
        }

        private void SearchClearButton_Clicked(object sender, EventArgs e)
        {
            ToolbarSelectUserLayout.IsVisible = true;
            ToolbarSearchLayout.IsVisible = false;
            SearchBar.Text = "";
        }

        private void SearchButton_Clicked(object sender, EventArgs e)
        {
            ToolbarSelectUserLayout.IsVisible = false;
            ToolbarSearchLayout.IsVisible = true;
            SearchBar.Focus();
        }

        private void GetSelectedUserList()
        {
            _selectedUsersModel.Clear();
            Observable<Contact> contactList = NavigationTappedPage.Context.Contacts.GetContacts();
            foreach (Contact co in contactList)
            {
                if (IsMessageForwarding || !co.IsGroup || IsMessageForwarding)
                {
                    _selectedUsersModel.Add(new SelectUserModel(co, _selectedUsers.Contains(co)));
                }
            }
            ObservableCollection<SelectUserModel> sorted = new ObservableCollection<SelectUserModel>(_selectedUsersModel.OrderBy(o => o.contact.Name));
            ItemsListView.ItemsSource = sorted;
        }

        private async void ForwardImage()
        {
            foreach (Contact contact in _selectedUsers)
            {
                SendMessage(contact, _sharedImage, MessageType.Image);
            }

            if (_selectedUsers.Count == 1)
            {
                await Navigation.PushAsync(new ChatRoom(_selectedUsers[0]), false);
                Navigation.RemovePage(this);
            }
            else if (_selectedUsers.Count >= 1)
            {
                OnBackButtonPressed();
            }
        }

        private async void ForwardMessages()
        {
            foreach (Message message in Messages)
            {
                if(!CheckMessageDeleted())
                    foreach (Contact contact in _selectedUsers)
                    {
                        SendMessage(contact, message.GetData(), message.Type);
                    }
            }

            if (_selectedUsers.Count == 1)
            {
                await Navigation.PushAsync(new ChatRoom(_selectedUsers[0]), false);
                Navigation.RemovePage(this);
            }
            else if (_selectedUsers.Count >= 1)
            {
                OnBackButtonPressed();
            }
        }

        private void SendMessage(Contact contact, byte[] messageData, MessageType messageType)
        {
            try
            {
                switch (messageType)
                {
                    case MessageType.Text:
                        NavigationTappedPage.Context.Messaging.SendText(Encoding.Unicode.GetString(messageData), contact);
                        break;

                    case MessageType.Audio:
                        NavigationTappedPage.Context.Messaging.SendAudio(messageData, contact);
                        break;

                    case MessageType.Image:
                        NavigationTappedPage.Context.Messaging.SendPicture(messageData, contact);
                        break;

                    case MessageType.Location:
                        NavigationTappedPage.Context.Messaging.SendLocation(BitConverter.ToDouble(messageData, 0), BitConverter.ToDouble(messageData, 8), contact);
                        break;
                    case MessageType.PdfDocument:
                        NavigationTappedPage.Context.Messaging.SendPdfDocument(messageData, contact);
                        break;
                }
            }
            catch (Exception)
            {
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.SomethingWentWrong);
            }

        }

        private bool CheckMessageDeleted()
        {
            if (Message == null)
            {
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.MessageAlreadDeleted);
                return false;
            }
            return true;
        }

        private void Back_Clicked(object sender, EventArgs args) => OnBackButtonPressed();
    }
}
