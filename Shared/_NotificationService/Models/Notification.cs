using System;
namespace NotificationService.Models
{
    public class Notification
    {
        public Notification(string title, string body, string priority)
        {
            Title = title;
            Body = body;
            Priority = priority;
        }

        private string Title { get; }
        private string Body { get; }
        private string Priority { get; }
    }
}
