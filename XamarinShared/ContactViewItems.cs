using EncryptedMessaging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;

namespace XamarinShared
{
    public class ContactViewItems : INotifyPropertyChanged
    {

        public DateTime LastMessageTime { get; set; } = DateTime.MinValue;

        public DateTime LastUIAddedMessageDate { get; set; } = DateTime.MinValue;

        public DateTime FirstUIAddedMessageDate { get; set; } = DateTime.MaxValue;

        public bool IsAllMessagesLoaded;

        private byte[] _avatar = Array.Empty<byte>();
        
        public  int MessageTranslationCounter = 0;

    
        public byte[] Avatar {
            get => _avatar;
            set {
                _avatar = value;
                OnPropertyChanged(nameof(Avatar));
            }

        }

        private bool _isMessageSelection;
        public bool IsMessageSelection
        {
            get => _isMessageSelection;
            set
            {
                _isMessageSelection = value;
                IsMessageSelectionObs?.Invoke(value);
                OnPropertyChanged(nameof(IsMessageSelection));
            }
        }

        private string _lastUnsentMessage="";

        public string LastUnsentMessage
        {
            get {
                var s = _lastUnsentMessage?.Clone() as string;
                _lastUnsentMessage = "";
                return s;
            }
            set
            {
                _lastUnsentMessage = value;
                OnPropertyChanged(nameof(LastUnsentMessage));
            }
        }

        public delegate void MessageSelection(bool isOpened);
        public MessageSelection IsMessageSelectionObs;

        public ObservableCollection<Tuple<Message, Label>> SelectedMessagesList = new ObservableCollection<Tuple<Message, Label>>();

        private bool _isCallGoingOn;
        public bool IsCallGoingOn
        {
            get => _isCallGoingOn;
            set
            {
                _isCallGoingOn = value;
                OnPropertyChanged(nameof(IsCallGoingOn));
            }
        }

        private bool _isVideoCall;
        public bool IsVideoCall
        {
            get => _isVideoCall;
            set
            {
                _isVideoCall = value;
                OnPropertyChanged(nameof(IsVideoCall));
            }
        }
        public DateTime CallTime { get; set; } // start or end time

        public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
