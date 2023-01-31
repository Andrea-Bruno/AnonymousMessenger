using System;

namespace WebSupport
{
    internal class ContactJson
    {
        public ContactJson(EncryptedMessaging.Contact contact)
        {
            Contact = contact;
        }        
        private readonly EncryptedMessaging.Contact Contact;
        public string Name => Contact.GetRealName();
        public string Pseudonym => Contact.Pseudonym();
        public string ChatId => Contact.ChatId.ToString();
        public string DarkColor => Contact.DarkColorAsHex;
        public string LightColor => Contact.LightColorAsHex;
        public DateTime LastMessageTime =>  new DateTime( Contact.LastMessageTime.Ticks / 10000000 * 10000000); // round to second: Do not change!!, if the precision exceeds one second the JavaScript in the browser converts the value to UTC format, and the data will be wrong!!   
        public bool HasAvatars => Contact.Avatar != null;
    }
}
