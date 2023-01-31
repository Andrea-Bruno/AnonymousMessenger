using System;
using EncryptedMessaging;

namespace WebSupport
{
    internal class MessageJson
    {
        public MessageJson(Message message, bool isMy)
        {
            //Message = message;
            IsMy = isMy;
            ChatId = message.ChatId;
            Type = message.Type.ToString();
            ReplyToPostId = message.ReplyToPostId;
            Creation = message.Creation.Ticks;
            Reception = message.ReceptionTime.Ticks;
            PostId = message.PostId;
            Author = message.AuthorName();
            switch (message.Type)
            {
                case MessageFormat.MessageType.Text:
                    Text = System.Text.Encoding.Unicode.GetString(message.GetData());
                    break;
                default:
                    Data = message.GetData();
                    break;
            }
        }
        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public MessageJson() { }
        //private readonly Message Message;
        public string Type;
        public byte[] Data;
        public string Text;
        public bool IsMy;
        public ulong? ChatId;
        public ulong? ReplyToPostId;
        public long Creation;
        public long Reception;
        public ulong PostId;
        public string Author;
    }
}
