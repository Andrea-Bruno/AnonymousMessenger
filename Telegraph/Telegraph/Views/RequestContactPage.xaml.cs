using System;
using System.Collections.Generic;
using System.Linq;
using Plugin.Toast;
using Telegraph.ViewModels;
using EncryptedMessaging;
using Xamarin.Forms;

namespace Telegraph.Views
{
    public partial class RequestContactPage : BasePage
    {
        private SelectUserModel _lastItemSelected;

        private readonly List<Contact> _selectedUsers = new List<Contact>();
        private readonly List<SelectUserModel> _selectedUsersModel = new List<SelectUserModel>();
#pragma warning disable CS0414 // Il campo 'RequestContactPage._isEditMode' è assegnato, ma il suo valore non viene mai usato
        private readonly bool _isEditMode = false;
#pragma warning restore CS0414 // Il campo 'RequestContactPage._isEditMode' è assegnato, ma il suo valore non viene mai usato
        private readonly Contact _contact;

        public RequestContactPage() => InitializeComponent();

        public RequestContactPage(List<Contact> participants, Contact contact)
        {
            InitializeComponent();

            _selectedUsers = participants;
            _isEditMode = true;
            _contact = contact;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _lastItemSelected = null;
            getSelectedUserList();
            PopulateList(_selectedUsersModel);
        }

        private void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            _lastItemSelected = args.SelectedItem as SelectUserModel;
            if (_lastItemSelected != null)
            {
                if (_selectedUsers.Find(v => v.Name == _lastItemSelected.contact.Name) != null)
                    _selectedUsers.Remove(_selectedUsers.Find(e => e.Name == _lastItemSelected.contact.Name));
                else
                    _selectedUsers.Add(_lastItemSelected.contact);
                _lastItemSelected.isVisible = !_lastItemSelected.isVisible;
            }
            PopulateList(_selectedUsersModel);
        }


        private void PopulateList(List<SelectUserModel> contacts = null)
        {
            ItemsListView.SelectedItem = null;
            List<SelectUserModel> sorted = _contact != null
                                ? contacts.Where(o => o.contact.IsGroup == false && o.contact.PublicKeys != _contact.PublicKeys).OrderBy(o => o.contact.Name).ToList()
                                : contacts.Where(o => o.contact.IsGroup == false).OrderBy(o => o.contact.Name).ToList();
            ItemsListView.ItemsSource = sorted;
        }

        private async void Request_Clicked(object sender, EventArgs args)
        {
            if (_selectedUsers.Count() >= 1)
                await Navigation.PushAsync(new RequestMoneyUsersPage(_selectedUsers), false);
            else
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.YouNeedToSelectAtLeastTwoUser);

        }


        private void Search_TextChanged(object sender, EventArgs e)
        {
            var find = ((SearchBar)sender).Text;
            if (!string.IsNullOrEmpty(find))
            {
                var filtered = new List<SelectUserModel>();
                foreach (SelectUserModel selectUserModel in _selectedUsersModel)
                {
                    if (selectUserModel.contact.Name.ToLower().Contains(find.ToLower()))
                        filtered.Add(selectUserModel);
                }
                PopulateList(filtered);
            }
            else
                PopulateList(_selectedUsersModel);
        }

        private void getSelectedUserList()
        {
            _selectedUsersModel.Clear();
            NavigationTappedPage.Context.Contacts.ForEachContact(contact => _selectedUsersModel.Add(new SelectUserModel(contact, _selectedUsers.Contains(contact))));
        }
        protected override bool OnBackButtonPressed()
        {
            Application.Current.MainPage.Navigation.PopAsync(false);
            return true;
        }

        private void Back_Clicked(object sender, EventArgs args) => _ = OnBackButtonPressed();
    }
}