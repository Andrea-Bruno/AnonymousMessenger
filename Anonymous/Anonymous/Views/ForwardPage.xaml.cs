using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegraph.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using EncryptedMessaging;
using static EncryptedMessaging.MessageFormat;
using static EncryptedMessaging.Contacts;
using Plugin.Toast;

namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ForwardPage : BasePage
    {
        private ForwardUserModel _lastItemSelected;
        private readonly List<Contact> _forwardedUsers = new List<Contact>();
        private List<ForwardUserModel> _forwardedUsersModel = new List<ForwardUserModel>();
        private readonly Message _message;
        public ForwardPage(Message message)
        {
            InitializeComponent();
            _message = message;
            ItemsListView.ItemsSource = NavigationTappedPage.Context.Contacts.GetContacts();

        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _lastItemSelected = null;
        }

        private void OnItemSelected(object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs args)
        {
            _lastItemSelected = args.ItemData as ForwardUserModel;
            if (_lastItemSelected != null)
            {
                if (_forwardedUsers.Find(v => v.Name == _lastItemSelected.contact.Name) != null)
                {
                    _forwardedUsers.Remove(_forwardedUsers.Find(e => e.Name == _lastItemSelected.contact.Name));

                }
                else
                {
                    _forwardedUsers.Add(_lastItemSelected.contact);
                }
            }
            ItemsListView.SelectedItem = null;

        }

        protected override bool OnBackButtonPressed()
        {
            Application.Current.MainPage.Navigation.PopAsync(false);
            return true;
        }

        private void Back_Clicked(object sender, EventArgs args) => OnBackButtonPressed();

        private void Search_TextChanged(object sender, EventArgs e)
        {
            var find = ((CustomEntry)sender).Text;
            
            if (ItemsListView.DataSource != null)
            {
                ItemsListView.DataSource.Filter = FilterContacts;
                ItemsListView.DataSource.RefreshFilter();
            }
        }

        private bool FilterContacts(object obj)
        {
            if (SearchBar == null || SearchBar.Text == null)
                return true;

            var contacts = obj as Contact;
            if (contacts.Name.ToLower().Contains(SearchBar.Text.ToLower()))
                return true;
            else
                return false;
        }

        private async void Forward_Clicked(object sender, EventArgs e)
        {
            CheckMessageDeleted();

            if (_forwardedUsers.Count == 0)
            {
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.PleaseSelectAtLeastOneUser);
            }

            else if (_forwardedUsers.Count == 1)
            {
                SendMessage(_forwardedUsers[0], _message);
                await Navigation.PushAsync(new ChatRoom(new ItemDetailViewModel(_forwardedUsers[0])), false);
                Navigation.RemovePage(this);
            }
            else if(_forwardedUsers.Count >= 1)
            {
                foreach (Contact contact in _forwardedUsers)
                {
                    SendMessage(contact, _message);
                }
                OnBackButtonPressed();
            }
            
        }

        private void SendMessage(Contact contact,Message message)
        {
            try
            {
                switch (message.Type)
                {
                    case MessageType.Text:
                        NavigationTappedPage.Context.Messaging.SendText(Encoding.Unicode.GetString(_message.GetData()), contact);
                        break;

                    case MessageType.Audio:
                        NavigationTappedPage.Context.Messaging.SendAudio(_message.GetData(), contact);
                        break;

                    case MessageType.Image:
                        NavigationTappedPage.Context.Messaging.SendPicture(_message.GetData(), contact);
                        break;

                    case MessageType.Location:
												var data = _message.GetData();
                        NavigationTappedPage.Context.Messaging.SendLocation(BitConverter.ToDouble(data, 0), BitConverter.ToDouble(data, 8), contact);
                        break;
                    case MessageType.PdfDocument:
                        NavigationTappedPage.Context.Messaging.SendPdfDocument(_message.GetData(), contact);
                        break;
                }
            }
            catch(Exception)
            {
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.SomethingWentWrong);
            }

        }

        private void CheckMessageDeleted()
        {
            if (_message == null)
            {
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.MessageAlreadDeleted);
                return;
            }

        }
    }
}