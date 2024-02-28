using System;
using System.Collections.Generic;
using Telegraph.ViewModels;
using EncryptedMessaging;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static EncryptedMessaging.Contacts;

namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RequestMoneyUsersPage : BasePage
    {
#pragma warning disable CS0169 // Il campo 'RequestMoneyUsersPage._lastItemSelected' non viene mai usato
        private SelectUserModel _lastItemSelected;
#pragma warning restore CS0169 // Il campo 'RequestMoneyUsersPage._lastItemSelected' non viene mai usato
#pragma warning disable CS0169 // Il campo 'RequestMoneyUsersPage._contacts' non viene mai usato
        private Observable<Contact> _contacts;
#pragma warning restore CS0169 // Il campo 'RequestMoneyUsersPage._contacts' non viene mai usato
        private readonly List<Contact> _selectedUsers = new List<Contact>();
        private readonly List<SelectUserModel> _selectedUsersModel = new List<SelectUserModel>();

        private Contact _contact;
        private readonly List<Contact> _contactsList;

        public Contact Contact
        {
            get => _contact;
            set
            {
                _contact = value;
            }
        }
        public RequestMoneyUsersPage(List<Contact> contacts)
        {
            InitializeComponent();
            ItemsListView.ItemsSource = contacts;
            _contactsList = contacts;
            string publicKeys = null;
            foreach (Contact contact in contacts)
                publicKeys += contact.PublicKeys;
            ItemsListView.ItemTapped += (object sender, ItemTappedEventArgs e) => {
                if (e.Item == null) return;

                if (sender is ListView lv) lv.SelectedItem = null;
            };
        }

        void btnRequest_Clicked(object sender, EventArgs e)
        {
        }

        protected override bool OnBackButtonPressed()
        {
            Application.Current.MainPage.Navigation.PopAsync(false);
            return true;
        }

        private void Back_Clicked(object sender, EventArgs args) => OnBackButtonPressed();
    }
}
