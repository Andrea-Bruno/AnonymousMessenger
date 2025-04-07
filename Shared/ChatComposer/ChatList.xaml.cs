using System;
using System.Collections.ObjectModel;
using System.Linq;
using CustomViewElements;
using EncryptedMessaging;
using Utils;
using Xamarin.Forms;
using static EncryptedMessaging.Contacts;

namespace ChatComposer
{
    public partial class ChatList : BaseContentView
    {
        public delegate void ItemClickEvent(Contact contact, ChatItemClickType chatItemClick);
        public delegate void PlaceHolderVisibility(bool isVisible);
        private ItemClickEvent _onChatItemClicked;
        private PlaceHolderVisibility _placeHolderVisibility;
        private ObservableCollection<Contact> _contacts;
        private bool isPlaceholderVisible
        {
            set
            {
                _placeHolderVisibility?.Invoke(value);
            }
        }

        private Contact _lastItemSelected;
        private string _searchQuery;
        private SwipeView _currentSwipeView;

        public Command<Contact> ClearCommand { get; }
        public Command<Contact> DeleteCommand { get; }
        public Command<Contact> EditCommand { get; }

        public ChatList()
        {
            try
            {
                InitializeComponent();
                ClearCommand = new Command<Contact>(Clear_Clicked);
                DeleteCommand = new Command<Contact>(Delete_Clicked);
                EditCommand = new Command<Contact>(Edit_Clicked);
            }
            catch (Exception e)
            {
                InitializeComponent(); // Some bugs on xamarin forms load view
            }
        }

        public void Init(ItemClickEvent onChatItemClicked, ObservableCollection<Contact> contacts)
        {
            lock (contacts)
            {
                _contacts = contacts;
                _onChatItemClicked = onChatItemClicked;
                ItemsListView.ItemsSource = contacts;
                isPlaceholderVisible = contacts.Count == 0;
                _lastItemSelected = null;
                contacts.CollectionChanged += Contacts_CollectionChanged;
            }
        }

        private void Contacts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_searchQuery))
                isPlaceholderVisible = _contacts.Count == 0;
            else
                isPlaceholderVisible = _contacts.Count == 0;
        }

        private void Clear_Clicked(Contact contact)
        {
            _onChatItemClicked?.Invoke(contact, ChatItemClickType.CLEAR);
        }

        public void SetPlaceHolderVisibility(PlaceHolderVisibility placeHolderVisibility)
        {
            _placeHolderVisibility = placeHolderVisibility;
        }

        private void Delete_Clicked(Contact contact)
        {
            _onChatItemClicked?.Invoke(contact, ChatItemClickType.DELETE);
        }

        private void Edit_Clicked(Contact contact)
        {
            _onChatItemClicked?.Invoke(contact, ChatItemClickType.EDIT);
        }

        private void OnItemSelected(object sender, SelectionChangedEventArgs args)
        {
            _lastItemSelected = args.CurrentSelection.FirstOrDefault() as Contact;
            if (_lastItemSelected != null)
                _onChatItemClicked?.Invoke(_lastItemSelected, ChatItemClickType.TAP);
            _lastItemSelected = null; // remove highlight on back click
        }

        public void FilterContacts(string query)
        {
            _searchQuery = query;
            var filteredContacts = _contacts.Where(c => c.Name.ToLower().Contains(_searchQuery.ToLower())).ToList();
            ItemsListView.ItemsSource = new ObservableCollection<Contact>(filteredContacts);
            isPlaceholderVisible = filteredContacts.Count == 0;
        }

        public void ClearState()
        {
            _lastItemSelected = null;
            ItemsListView.SelectedItem = null;
        }

        public void ResetSwipe()
        {
            _currentSwipeView?.Close();
        }

        public override void OnAppearing()
        {
        }

        public override void OnDisappearing()
        {
        }
    }

}
