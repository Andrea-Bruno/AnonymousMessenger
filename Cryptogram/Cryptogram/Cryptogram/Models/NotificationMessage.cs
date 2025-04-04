namespace Cryptogram.Models
{
    public class NotificationMessage
    {
        public NotificationMessage(string _to, MessageData _messageData, NotificationData _notification, string _priority)
        {
            to = _to;
            data = _messageData;
            notification = _notification;
            priority = _priority;
        }

        public string to { get; set; }
        public MessageData data { get; set; }
        public NotificationData notification { get; set; }
        public string priority { get; set; }
        public class MessageData
        {
            public MessageData(string _title, string _message, string _token, string _chatId, string _priority)
            {
                title = _title;
                message = _message;
                token = _token;
                chatId = _chatId;
                priority = _priority;
            }
            public string title { get; set; }
            public string message { get; set; }
            public string token { get; set; }
            public string chatId { get; set; }
            public string priority { get; set; }

        }
        public class NotificationData
        {
            public NotificationData(string _title, string _body, string _priority, string _sound, int _badge)
            {
                title = _title;
                body = _body;
                priority = _priority;
                sound = _sound;
                badge = _badge;
            }
            public string title { get; set; }
            public string body { get; set; }
            public string priority { get; set; }
            public string sound { get; set; }
            public int badge { get; set; }


        }
    }


}
