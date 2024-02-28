using CustomViewElements;
using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Telegraph.DesignHandler;
using Telegraph.Services;
using Telegraph.ViewModels;
using EncryptedMessaging;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GroupCreatePage : BasePage
    {
        private readonly List<Contact> _contactsList;     

        public GroupCreatePage(List<Contact> contacts)
        {
            InitializeComponent();
            Toolbar.Title = Localization.Resources.Dictionary.NewGroup;
            ObservableCollection<SelectUserModel> _selectedUsersModelList = new ObservableCollection<SelectUserModel>();
            foreach (var item in contacts)
            {
                _selectedUsersModelList.Add(new SelectUserModel(item, contacts.Contains(item)));
            }
            ItemsListView.ItemsSource = _selectedUsersModelList;
            _contactsList = contacts;
            string publicKeys = null;
            foreach (Contact contact in contacts)
                publicKeys += contact.PublicKeys;
        }

        private void Save_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Group_Name.Text) || _contactsList == null)
            {
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.PleaseFillAllTheBlanks);
                return;
            }

            else if (_contactsList != null && !string.IsNullOrWhiteSpace(Group_Name.Text))
            {
                ShowProgressDialog();
                var timer = new System.Threading.Timer((object obj) => { Device.BeginInvokeOnMainThread(() => Save()); }, null, 100, System.Threading.Timeout.Infinite);
            }         
        }

        private async void Save()
        {
            Contact contact = NavigationTappedPage.Context.Contacts.AddContact(_contactsList, Group_Name.Text.Trim(), false, App.Setting.SendContact ? Contacts.SendMyContact.Send : Contacts.SendMyContact.None);
            HideProgressDialog();
            if (contact != null)
            {
                try
                {
                    if (Device.RuntimePlatform == Device.Android)
                        DependencyService.Get<ISharedPreference>().AddContact(contact.ChatId + "", contact.Name, contact.Os == Contact.RuntimePlatform.Android);
                    App.SendNotification(contact, NotificationService.NotificationType.CONTACT);
                }
                catch (Exception)
                {
                }
                await Application.Current.MainPage.Navigation.PopToRootAsync();
               ((App)Application.Current).GetRootPage()?.ShowRequiredView(chatIdRequired: contact.ChatId);
            }
            else
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.PublicKeyIsNotValid);
        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Group_Name.Text))
            {
                Next.Source = DesignResourceManager.GetImageSource("ic_next_new.png");  
            }
           else if (string.IsNullOrEmpty(Group_Name.Text))
            {
                Next.Source = DesignResourceManager.GetImageSource("ic_next_disabled.png");           
            }
        }

        private void Back_Clicked(object sender, EventArgs args) => OnBackButtonPressed();
    }
}
