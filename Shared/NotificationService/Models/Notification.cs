using System;
namespace NotificationService.Models
{
    public class Notification
    {
        public Notification(string _title, string _body, string _priority, string _sound, int _badge)
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
