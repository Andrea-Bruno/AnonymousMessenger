using System;
namespace NotificationService.Models
{
    public class Data
    {
        public Data(string title, string body,  string chatId, string priority)
        {
            Title = title;
            Body = body;
            ChatId = chatId;
            Priority = priority;
        }

        private string Title { get; }
        private string Body { get; }
        private string ChatId { get; }
        private string Priority { get; }
    }
}
