namespace Cryptogram.Models
{
    public class IOSNotificationCall
    {
        public IOSNotificationCall(string _to, NotificationCall.MessageData _messageData, string _priority)
        {
            to = _to;
            data = _messageData;
            notification = _messageData;
            priority = _priority;
        }

        public string to { get; set; }
        public NotificationCall.MessageData data { get; set; }
        public NotificationCall.MessageData notification { get; set; }

        public string priority { get; set; }
    }


}
