using System.Collections.Generic;

namespace Anonymous.Models
{
    public class GroupNotificationCall
    {
        public GroupNotificationCall(List<string> _registration_ids, NotificationCall.MessageData _messageData, string _priority)
        {
            registration_ids = _registration_ids;
            data = _messageData;
            priority = _priority;
        }

        public List<string> registration_ids { get; set; }
        public NotificationCall.MessageData data { get; set; }
        public string priority { get; set; }
    }
}
