using System;
using System.Collections.Generic;
using CustomViewElements;
using EncryptedMessaging;
using Xamarin.Forms.Xaml;

namespace Cryptogram.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessageInfoPage : BasePage
    {
        private List<Contact> ReadContacts = new List<Contact>();
        public MessageInfoPage(List<Contact> contacts)
        {
            InitializeComponent();
            ReadContacts = contacts;
            ItemsListView.ItemsSource = ReadContacts;
        }

        protected override void OnAppearing() => base.OnAppearing();

        protected override void OnDisappearing() => base.OnDisappearing();

        private void Back_Clicked(object sender, EventArgs args) => OnBackButtonPressed();
    }
}
