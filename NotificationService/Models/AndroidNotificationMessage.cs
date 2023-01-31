using System.Collections.Generic;

namespace NotificationService.Models
{
    public class AndroidNotificationMessage
    {
        public AndroidNotificationMessage(List<string> _registration_ids, Data _data, string _priority)
        {
            registration_ids = _registration_ids;
            data = _data;
            priority = _priority;
        }

        public List<string> registration_ids { get; set; }
        public Data data { get; set; }
        public string priority { get; set; }
    }
}
