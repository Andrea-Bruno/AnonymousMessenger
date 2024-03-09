using System;
using CustomViewElements;
using EncryptedMessaging;
using Utils;
using static EncryptedMessaging.Contacts;

namespace ChatComposer
{
    public partial class ChatList : BaseContentView
    {
        public delegate void ItemClickEvent(Contact contact, ChatItemClickType chatItemClick);
        public delegate void PlaceHolderVisibility(bool isVisible);
        private ItemClickEvent _onChatItemClicked;
        private PlaceHolderVisibility _placeHolderVisibility;
        private Observable<Contact> _contacts;
        private bool isPlaceholderVisible
        {
            set
            {
                if (_placeHolderVisibility != null)
                    _placeHolderVisibility(value);

            }
        }


        private Contact _lastItemSelected;
        private string _searchQuery;

        public ChatList()
        {
            try
            {
                InitializeComponent();
            }
            catch(Exception e)
            {
                InitializeComponent(); // Some bugs on xamarin forms load view
            }
        }

        public void Init(ItemClickEvent onChatItemClicked, Observable<Contact> contacts)
        {
            lock (contacts)
            {
                _contacts = contacts;
                _onChatItemClicked = onChatItemClicked;
                ItemsListView.ItemsSource = contacts;
                isPlaceholderVisible = contacts.Count == 0;
                _lastItemSelected = null;
                ItemsListView.QueryItemSize += ItemsListView_QueryItemSize;
                contacts.CollectionChanged += Contacts_CollectionChanged;
            }
        }
        private void Contacts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (ItemsListView?.DataSource != null && !string.IsNullOrWhiteSpace(_searchQuery))
                isPlaceholderVisible = ItemsListView.DataSource.Items.Count == 0;
            else
                isPlaceholderVisible = _contacts.Count == 0;
        }

        private void Clear_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            if (_lastItemSelected != null)
                _onChatItemClicked(_lastItemSelected, ChatItemClickType.CLEAR);
        }

        public void SetPlaceHolderVisibility(PlaceHolderVisibility placeHolderVisibility)
        {
            _placeHolderVisibility = placeHolderVisibility;
        }

        private void Delete_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            if (_lastItemSelected != null)
                _onChatItemClicked(_lastItemSelected, ChatItemClickType.DELETE);
        }

        private void Edit_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            if (_lastItemSelected != null)
                _onChatItemClicked(_lastItemSelected, ChatItemClickType.EDIT);
        }

        private void OnItemSelected(object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs args)
        {
            _lastItemSelected = args.ItemData as Contact;
            if (_lastItemSelected != null)
                _onChatItemClicked(_lastItemSelected, ChatItemClickType.TAP);
            _lastItemSelected = null; // remove highlight on back click

        }

        private void ItemsListView_SwipeStarted(object sender, Syncfusion.ListView.XForms.SwipeStartedEventArgs e)
        {
            _lastItemSelected = null;
        }

        private void ItemsListView_SwipeEnded(object sender, Syncfusion.ListView.XForms.SwipeEndedEventArgs e)
        {
            _lastItemSelected = e.ItemData as Contact;
        }

        public void FilterContacts(string query)
        {
            _searchQuery = query;
            if (ItemsListView.DataSource != null)
            {
                ItemsListView.DataSource.Filter = FilterContacts;
                ItemsListView.DataSource.RefreshFilter();
                isPlaceholderVisible = ItemsListView.DataSource.Items.Count == 0;
            }      
        }

        private void ItemsListView_QueryItemSize(object sender, Syncfusion.ListView.XForms.QueryItemSizeEventArgs e)
        {
            var size = e.ItemSize;
        }

        private bool FilterContacts(object obj)
        {
            var contacts = obj as Contact;
            if (contacts.Name.ToLower().Contains(_searchQuery.ToLower()))
                return true;
            else
                return false;
        }
        public void ClearState()
        {
            _lastItemSelected = null;
            ItemsListView.SelectedItem = null;
        }

        public void ResetSwipe()
        {
            ItemsListView.ResetSwipe();
        }

        public override void OnAppearing()
        {
        }

        public override void OnDisappearing()
        {
        }
    }

}
