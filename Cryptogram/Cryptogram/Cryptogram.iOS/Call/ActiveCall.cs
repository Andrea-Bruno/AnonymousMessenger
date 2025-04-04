using System;
using CoreFoundation;
using Foundation;
using Cryptogram.Services;
using Xamarin.Forms;

namespace Cryptogram.iOS.Call
{
    public class ActiveCall
    {
        #region Private Variables
        private bool isConnecting;
        private bool isConnected;
        private bool isOnhold;
        #endregion

        #region Computed Properties
        public NSUuid UUID { get; set; }
        public bool isOutgoing { get; set; }
        public bool isVideoCall { get; set; }
        public string Handle { get; set; }
        public string ChatId { get; set; }
        public string Username { get; set; }
        public DateTime StartedConnectingOn { get; set; }
        public DateTime ConnectedOn { get; set; }
        public DateTime EndedOn { get; set; }

        public bool IsConnecting
        {
            get { return isConnecting; }
            set
            {
                isConnecting = value;
                if (isConnecting) StartedConnectingOn = DateTime.Now;
                RaiseStartingConnectionChanged();
            }
        }

        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                isConnected = value;
                if (isConnected)
                {
                    ConnectedOn = DateTime.Now;
                }
                else
                {
                    EndedOn = DateTime.Now;
                }
                RaiseConnectedChanged();
            }
        }

        public bool IsOnHold
        {
            get { return isOnhold; }
            set
            {
                isOnhold = value;
            }
        }
        #endregion

        #region Constructors
        public ActiveCall()
        {
        }

        public ActiveCall(NSUuid uuid)
        {
            this.UUID = uuid;
        }

        public ActiveCall(NSUuid uuid, string handle, bool outgoing, string chatId, string username, bool videoCall)
        {
            // Initialize
            this.UUID = uuid;
            this.Handle = handle;
            this.isOutgoing = outgoing;
            this.ChatId = chatId;
            this.Username = username;
            this.isVideoCall = videoCall;
        }
        #endregion

        #region Public Methods
        public void StartCall(ActiveCallbackDelegate completionHandler)
        {
            // Simulate the call starting successfully
            completionHandler(true);

            // Simulate making a starting and completing a connection
            DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, 3000), () => {
                // Note that the call is starting
                IsConnecting = true;

                // Simulate pause before connecting
                DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, 1500), () => {
                    // Note that the call has connected
                    IsConnecting = false;
                    IsConnected = true;
                });
            });
        }

        public void AnswerCall(ActiveCallbackDelegate completionHandler)
        {
            // Simulate the call being answered
            IsConnected = true;
            completionHandler(true);
        }

        public void EndCall(ActiveCallbackDelegate completionHandler)
        {
            // Simulate the call ending
            IsConnected = false;
            completionHandler(true);
            AppDelegate.Instance.CallManager.Calls.Remove(AppDelegate.Instance.CallManager.FindCall(UUID));
        }
        #endregion

        #region Events
        public delegate void ActiveCallbackDelegate(bool successful);
        public delegate void ActiveCallStateChangedDelegate(ActiveCall call);

        public event ActiveCallStateChangedDelegate StartingConnectionChanged;
        internal void RaiseStartingConnectionChanged()
        {
            if (this.StartingConnectionChanged != null) this.StartingConnectionChanged(this);
        }

        public event ActiveCallStateChangedDelegate ConnectedChanged;
        internal void RaiseConnectedChanged()
        {
            if (this.ConnectedChanged != null) this.ConnectedChanged(this);
        }
        #endregion
    }
}