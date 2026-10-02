using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CustomViewElements;
using EncryptedMessaging;
using Microsoft.Maui.Controls;

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

        private string _searchQuery;
        private SwipeView _currentSwipeView;

        public Command<Contact> ClearCommand { get; private set; }
        public Command<Contact> DeleteCommand { get; private set; }
        public Command<Contact> EditCommand { get; private set; }

        public ChatList()
        {
            try
            {
                InitializeComponent();
                BindingContext = this;
                ClearCommand = new Command<Contact>(contact => _onChatItemClicked?.Invoke(contact, ChatItemClickType.CLEAR));
                DeleteCommand = new Command<Contact>(contact => _onChatItemClicked?.Invoke(contact, ChatItemClickType.DELETE));
                EditCommand = new Command<Contact>(contact => _onChatItemClicked?.Invoke(contact, ChatItemClickType.EDIT));
            }
            catch (Exception)
            {
                Debugger.Break();
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

        public void SetPlaceHolderVisibility(PlaceHolderVisibility placeHolderVisibility)
        {
            _placeHolderVisibility = placeHolderVisibility;
        }

        private void OnItemSelected(object sender, SelectionChangedEventArgs args)
        {
            if (args.CurrentSelection.FirstOrDefault() is Contact conrectSelected)
                _onChatItemClicked?.Invoke(conrectSelected, ChatItemClickType.TAP);
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
