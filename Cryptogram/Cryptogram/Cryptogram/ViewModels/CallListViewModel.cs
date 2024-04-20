using System;
using EncryptedMessaging;
using Xamarin.Forms;

namespace Anonymous.ViewModels
{
    public class CallListViewModel : BaseViewModel
    {
        public Contact Contact { get; set; }
        public string Name { get; set; }
        public ImageSource SmallCallIcon { get; set; }
        public ImageSource BigCallIcon { get; set; }
        public string CallStatus { get; set; }
        public string Time { get; set; }
        public bool IsVideoCall { get; set; }

        public CallListViewModel(Contact contact, string name, ImageSource smallImage, ImageSource bigImage, string callStatus, string time, bool isVideoCall)
        {
            Contact = contact;
            Name = name;
            SmallCallIcon = smallImage;
            BigCallIcon = bigImage;
            CallStatus = callStatus;
            Time = time;
            IsVideoCall = isVideoCall;
        }

    }
}
