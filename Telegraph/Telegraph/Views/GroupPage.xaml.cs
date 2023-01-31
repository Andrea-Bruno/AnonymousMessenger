using System;
using System.Collections.Generic;
using Telegraph.ViewModels;
using EncryptedMessaging;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GroupPage : BaseContentView
    {
        public static GroupPage Instance;
        private Contact _lastItemSelected;

        public GroupPage()
        {
            InitializeComponent();
        }

        public override void OnAppearing()
        {
            Instance = this;

            _lastItemSelected = null;
            ItemsListView.SelectedItem = null;
            SearchBar.Text = "";
            PopulateList();
        }


        public override void OnDisappearing()
        {
            Instance = null;
        }

        private async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            _lastItemSelected = args.SelectedItem as Contact;
            if (_lastItemSelected != null)

                await Navigation.PushAsync(new ChatRoom(new ItemDetailViewModel(_lastItemSelected)), false).ConfigureAwait(true);
        }

        public void PopulateList(Contacts.Observable<Contact> contacts = null)
        {
            ItemsListView.SelectedItem = null;
            ItemsListView.ItemsSource = null;
            if (contacts != null)
            {
                var _contacts = new Contacts.Observable<Contact>();
                foreach (var item in contacts)
                {
                    if (item.IsGroup)
                    {
                        _contacts.Add(item);
                    }
                }
                ItemsListView.ItemsSource = _contacts;
            }
            else
            {
                //ItemsListView.ItemsSource = NavigationTappedPage.Context.Contacts.GetContacts()
                var _contacts = new Contacts.Observable<Contact>();
                foreach (var item in NavigationTappedPage.Context.Contacts.GetContacts())
                {
                    if (item.IsGroup)
                    {
                        _contacts.Add(item);
                    }
                }
                ItemsListView.ItemsSource = _contacts;
            }
        }

        private void Search_TextChanged(object sender, EventArgs e)
        {
            var find = ((CustomEntry)sender).Text;
            if (!string.IsNullOrEmpty(find))
            {
                ic_search_clear.IsVisible = true;
                var filtered = new Contacts.Observable<Contact>();
                NavigationTappedPage.Context.Contacts.ForEachContact(contact =>
                {
                    if (contact.Name.ToLower().Contains(find.ToLower()) && !string.IsNullOrEmpty(contact.LastMessageTimeText) && !contact.IsBlocked)
                        filtered.Add(contact);
                });
                PopulateList(filtered);
            }
            else
            {
                ic_search_clear.IsVisible = false;
                PopulateList();
            }
        }

        private void SearchClearButton_Clicked(object sender, EventArgs e)
        {
            SearchBar.Text = "";
        }

        private async void AddGroup_Tapped(Object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GroupUserSelectPage(), false);
        }



        private async void ItemsListView_ItemTapped(Object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs e)
        {
            _lastItemSelected = e.ItemData as Contact;
            if (_lastItemSelected != null)

                await Navigation.PushAsync(new ChatRoom(new ItemDetailViewModel(_lastItemSelected)), false).ConfigureAwait(true);
        }
    }
}
