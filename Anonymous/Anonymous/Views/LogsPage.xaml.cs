using System;
using CommunicationChannel;
using CustomViewElements;

namespace Telegraph.Views
{
    public partial class LogsPage : BasePage
    {
        public LogsPage()
        {
            InitializeComponent();
            UpdateValues();
        }

        void Button_Clicked(object sender, EventArgs e)
        {
            UpdateValues();
        }

        private void UpdateValues()
        {
            DeviceToken.Text = "Device token: " + App.DeviceToken;
            CloudPubKey.Text = "CloudPubKey: " + EncryptedMessaging.Contacts.CloudPubKey;
         //   CloudUserId.Text = "CloudUserId: " + EncryptedMessaging.Contacts.CloudUserId;
            ID.Text = "My ID: " + NavigationTappedPage.Context.My.GetId();
            Internet.Text = "Internet: " + Channell.InternetAccess;
            ClientExists.Text = "ClientExists: " + NavigationTappedPage.Context.Channell.ClientExists;
            ClientConnected.Text = "ClientConnected: " + NavigationTappedPage.Context.Channell.ClientConnected;
            Logged.Text = "Logged: " + NavigationTappedPage.Context.Channell.Logged;
            QueeCount.Text = "QueeCount: " + NavigationTappedPage.Context.Channell.QueeCount;
            LastMessageParts.Text = "LastMessageParts: " + NavigationTappedPage.Context.Channell.LastPostParts;
            PostCounter.Text = "PostCounter: " + NavigationTappedPage.Context.Channell.PostCounter;
            Error.Text = "Error: " + NavigationTappedPage.Context.Channell.ErrorLog;
            ServerUrl.Text = "ServerUrl: " + NavigationTappedPage.Context.Channell.ServerUri.Host;
            Port.Text = "Port: " + NavigationTappedPage.Context.Channell.ServerUri.Port;
        }

        private void Back_Clicked(object sender, EventArgs e) => OnBackButtonPressed();
    }
}