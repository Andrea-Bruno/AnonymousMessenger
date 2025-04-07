using CustomViewElements;
using EncryptedMessaging;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using static EncryptedMessaging.Contacts;

namespace ChatComposer
{
    public partial class ContactList : BaseContentView
    {
        private Contact _lastItemSelected;
        private string _searchQuery;

        public delegate void ItemClickEvent(Contact contact);
        private ItemClickEvent _onItemClicked;

        public ContactList()
        {
            InitializeComponent();
            ItemsListView.SelectionChanged += OnItemSelected;
        }

        public void Init(ObservableCollection<Contact> contacts)
        {
            ItemsListView.ItemsSource = contacts;
            _lastItemSelected = null;
        }

        private void OnItemSelected(object sender, SelectionChangedEventArgs args)
        {
            _lastItemSelected = args.CurrentSelection.FirstOrDefault() as Contact;
            if (_lastItemSelected != null)
                _onItemClicked?.Invoke(_lastItemSelected);
            _lastItemSelected = null; // remove highlight on back click
        }

        public void FilterContacts(string query)
        {
            _searchQuery = query;
            var collectionView = ItemsListView.ItemsSource as ObservableCollection<Contact>;
            if (collectionView != null)
            {
                var filteredContacts = collectionView.Where(c => c.Name.ToLower().Contains(_searchQuery.ToLower())).ToList();
                ItemsListView.ItemsSource = new ObservableCollection<Contact>(filteredContacts);
            }
        }

        public override void OnAppearing()
        {
        }

        public override void OnDisappearing()
        {
        }
    }
}
