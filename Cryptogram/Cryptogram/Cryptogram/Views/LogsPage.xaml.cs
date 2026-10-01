using System;
using CustomViewElements;

namespace Cryptogram.Views
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
            var ctx = NavigationTappedPage.Context;
            DeviceToken.Text = "Device token: " + App.DeviceToken;
            CloudPubKey.Text = "CloudPubKey: ";  // + EncryptedMessaging.Contacts.CloudPubKey;
            ID.Text = "My ID: " + ctx.My.Id;
            Internet.Text = "Internet: " + ctx.HasConnectivity;
            ClientConnected.Text = "ClientConnected: " + ctx.IsConnected;
            Logged.Text = "Logged: " + ctx.IsConnected;
            ServerUrl.Text = "ServerUrl: " + (ctx.EntryPoint != null ? ctx.EntryPoint.Host : "n/a");
            Port.Text = "Port: " + (ctx.EntryPoint != null ? ctx.EntryPoint.Port.ToString() : "n/a");
            // ClientExists, QueueCount, LastPostParts, PostCounter and ErrorLog are
            // internal to EncryptedMessaging (Context.Channel is not public), so they
            // are shown as unavailable until a public accessor is added upstream.
            ClientExists.Text = "ClientExists: n/a";
            QueeCount.Text = "QueeCount: n/a";
            LastMessageParts.Text = "LastMessageParts: n/a";
            PostCounter.Text = "PostCounter: n/a";
            Error.Text = "Error: n/a";
        }

        private void Back_Clicked(object sender, EventArgs e) => OnBackButtonPressed();
    }
}