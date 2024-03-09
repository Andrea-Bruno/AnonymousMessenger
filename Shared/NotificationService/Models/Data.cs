using System;
namespace NotificationService.Models
{
    public class Data
    {
        public Data(string _title, string _body, string _userId, string _chatId, string _remoteName, string _priority, NotificationType _notificationType)
        {
            title = _title;
            body = _body;
            userId = _userId;
            chatId = _chatId;
            remoteName = _remoteName;
            priority = _priority;
            notificationType = _notificationType;
        }

        public string title { get; set; }
        public string body { get; set; }
        public string userId { get; set; }
        public string chatId { get; set; }
        public string remoteName { get; set; }
        public string priority { get; set; }
        public NotificationType notificationType { get; set; }
    }
}
