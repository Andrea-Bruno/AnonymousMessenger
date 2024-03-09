using System;
using EncryptedMessaging;
using Xamarin.Forms;
using static EncryptedMessaging.MessageFormat;

namespace XamarinShared.ViewModels
{
    public class CallViewModel
    {
        public Contact Contact { get; set; }
        public MessageType MessageType { get; set; }
        public string CallStatus { get; set; }
        public string Time { get; set; }
        public ulong MessageId { get; set; }
        public DateTime Creation { get; set; }
        public CallType CallType { get; set; }

        public CallViewModel(Contact contact, MessageType messageType, string callStatus, string time, ulong messageId, DateTime creation, CallType callType)
        {
            Contact = contact;
            MessageType = messageType;
            CallStatus = callStatus;
            Time = time;
            MessageId = messageId;
            Creation = creation;
            CallType = callType;
        }

    }
}
