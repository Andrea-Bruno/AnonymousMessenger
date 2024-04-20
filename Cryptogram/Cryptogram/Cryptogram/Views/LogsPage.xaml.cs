using System;
using CommunicationChannel;
using CustomViewElements;

namespace Anonymous.Views
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
            CloudPubKey.Text = "CloudPubKey: ";  // + EncryptedMessaging.Contacts.CloudPubKey;
         //   CloudUserId.Text = "CloudUserId: " + TelegrahLibrary.Contacts.CloudUserId;
            ID.Text = "My ID: " + NavigationTappedPage.Context.My.GetId();
            Internet.Text = "Internet: " + Channel.InternetAccess;
            ClientExists.Text = "ClientExists: " + NavigationTappedPage.Context.Channel.ClientExists;
            ClientConnected.Text = "ClientConnected: " + NavigationTappedPage.Context.Channel.ClientConnected;
            Logged.Text = "Logged: " + NavigationTappedPage.Context.Channel.Logged;
            QueeCount.Text = "QueeCount: " + NavigationTappedPage.Context.Channel.QueeCount;
            LastMessageParts.Text = "LastMessageParts: " + NavigationTappedPage.Context.Channel.LastPostParts;
            PostCounter.Text = "PostCounter: " + NavigationTappedPage.Context.Channel.PostCounter;
            Error.Text = "Error: " + NavigationTappedPage.Context.Channel.ErrorLog;
            ServerUrl.Text = "ServerUrl: " + NavigationTappedPage.Context.Channel.ServerUri.Host;
            Port.Text = "Port: " + NavigationTappedPage.Context.Channel.ServerUri.Port;
        }

        private void Back_Clicked(object sender, EventArgs e) => OnBackButtonPressed();
    }
}