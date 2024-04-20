namespace Anonymous.Models
{
    public class NotificationCall
    {
        public NotificationCall(string _to, MessageData _messageData, string _priority)
        {
            to = _to;
            data = _messageData;
            priority = _priority;
        }

        public string to { get; set; }
        public MessageData data { get; set; }
        public string priority { get; set; }
        public class MessageData
        {
            public MessageData(string _title, string _message, string _token, string _chatId, string _priority, int _callType, bool _isGroupCall)
            {
                title = _title;
                message = _message;
                token = _token;
                chatId = _chatId;
                priority = _priority;
                callType = _callType;
                isGroupCall = _isGroupCall;
            }
            public string title { get; set; }
            public string message { get; set; }
            public string token { get; set; }
            public string chatId { get; set; }
            public string priority { get; set; }
            public int callType { get; set; }
            public bool isGroupCall { get; set; }

        }
    }


}
