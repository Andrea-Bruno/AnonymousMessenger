using System;
using System.Collections.Generic;
using System.Linq;
using Telegraph.ViewModels;
using EncryptedMessaging;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static EncryptedMessaging.Contacts;

namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShareFileFromExternalAppPage : BasePage
    {
        public byte[] SharedImage;
        private readonly Observable<Contact> _contacts;

        public ShareFileFromExternalAppPage(byte[] sharedImage)
        {
            InitializeComponent();
            SharedImage = sharedImage;
            _contacts = NavigationTappedPage.Context.Contacts.GetContacts();
        }

        private Contact _lastItemSelected;

        private async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            _lastItemSelected = args.SelectedItem as Contact;
            if (_lastItemSelected != null)
            {
                await Navigation.PushAsync(new ChatRoom(new ItemDetailViewModel(_lastItemSelected), SharedImage), false);
                Navigation.RemovePage(this);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _lastItemSelected = null;
            PopulateList(_contacts.ToArray(), true);

        }

        private static readonly Dictionary<Contact, Contact> _list = new Dictionary<Contact, Contact>();
        public void PopulateList(Contact[] contacts, bool isItemChanged)
        {
            ItemsListView.SelectedItem = null;
            if (contacts != null)
            {

                ItemsListView.ItemsSource = null;
                var toRemove = new List<Contact>();
                if (!isItemChanged)
                {
                    foreach (KeyValuePair<Contact, Contact> contact in _list)
                    {
                        if (!contacts.Contains(contact.Value))
                        {
                            toRemove.Add(contact.Key);
                        }
                    }
                    toRemove.ForEach(x => { _list.Remove(x); });

                    foreach (Contact x in contacts)
                    {
                        if (!_list.Values.Contains(x))
                            _list.Add((Contact)x.Clone(), x);
                    }
                }
                else
                {
                    foreach (KeyValuePair<Contact, Contact> contact in _list)
                    {
                        toRemove.Add(contact.Key);
                    }
                    toRemove.ForEach(x => { _list.Remove(x); });

                    foreach (Contact x in contacts)
                    {
                        _list.Add((Contact)x.Clone(), x);
                    }
                }
                var sorted = _list.Keys.OrderBy(o => o.Name).ToList();
                ItemsListView.ItemsSource = sorted;
            }
        }

        private void Search_TextChanged(object sender, EventArgs e)
        {
            var searchBar = (SearchBar)sender;
            var txt = searchBar.Text.ToLower();
            PopulateList(_contacts.ToList().FindAll(x => x.Name.ToLower().Contains(txt)).ToArray(), false);
        }

        protected override bool OnBackButtonPressed()
        {
            Application.Current.MainPage = new NavigationPage(new NavigationTappedPage());
            return true;
        }

        private void Back_Clicked(object sender, EventArgs args) => OnBackButtonPressed();

    }
}