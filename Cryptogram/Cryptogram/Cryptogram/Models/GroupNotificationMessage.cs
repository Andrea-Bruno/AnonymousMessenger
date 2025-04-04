using System.Collections.Generic;

namespace Cryptogram.Models
{
    public class GroupNotificationMessage
    {
        public GroupNotificationMessage(List<string> _registration_ids, NotificationMessage.MessageData _messageData, NotificationMessage.NotificationData _notification, string _priority)
        {
            registration_ids = _registration_ids;
            data = _messageData;
            notification = _notification;
            priority = _priority;
        }

        public List<string> registration_ids { get; set; }
        public NotificationMessage.MessageData data { get; set; }
        public NotificationMessage.NotificationData notification { get; set; }
        public string priority { get; set; }
    }
}
