using System.Collections.Generic;

namespace NotificationService.Models
{
    public class IOSNotificationMessage
    {
        public IOSNotificationMessage(List<string> _registration_ids, Data _data, Notification _notification, string _priority)
        {
            registration_ids = _registration_ids;
            data = _data;
            notification = _notification;
            priority = _priority;
        }

        public List<string> registration_ids { get; set; }
        public Data data { get; set; }
        public Notification notification { get; set; }
        public string priority { get; set; }
    }
}
