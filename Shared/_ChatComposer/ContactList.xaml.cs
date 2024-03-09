using CustomViewElements;
using EncryptedMessaging;
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
        }

        public void Init(Observable<Contact> contacts)
        {
            ItemsListView.ItemsSource = contacts;
            _lastItemSelected = null;
        }

        private void OnItemSelected(object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs args)
        {
            _lastItemSelected = args.ItemData as Contact;
            if (_lastItemSelected != null)
                _onItemClicked(_lastItemSelected);
              _lastItemSelected = null; // remove highlight on back click

        }


        public void FilterContacts(string query)
        {
            _searchQuery = query;
            if (ItemsListView.DataSource != null)
            {
                ItemsListView.DataSource.Filter = FilterContacts;
                ItemsListView.DataSource.RefreshFilter();
            }
        }

        private bool FilterContacts(object obj)
        {
            var contacts = obj as Contact;
            if (contacts.Name.ToLower().Contains(_searchQuery.ToLower()))
                return true;
            else
                return false;
        }


        public override void OnAppearing()
        {
        }

        public override void OnDisappearing()
        {
        }
    }
}
