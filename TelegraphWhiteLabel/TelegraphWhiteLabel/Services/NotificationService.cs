using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnonymousWhiteLabel.Helper;
using EncryptedMessaging;
using NotificationService;
using NotificationService.Services;

namespace AnonymousWhiteLabel
{
    public class NotificationService
    {
        private static NotificationManager _notificationService;
        private static void InitNotificationService()
        {
            if(_notificationService == null)
                _notificationService = new NotificationManager();
        }
       public static async Task SendNotification(Contact contact, MessageFormat.MessageType type) 
       {
            if (contact== null || contact.ImBlocked) return;
            var messageType = MessageTypeConverter.GetNotificationType(type);
            if (messageType == NotificationType.NONE || (!contact.IsGroup &&
                                                         (type == MessageFormat.MessageType.EndCall ||
                                                          type == MessageFormat.MessageType.DeclinedCall))) return;
            SendNotification(contact, messageType);
        }
        
        private static async void SendNotification(Contact contact, NotificationType notificationType)
        {
            if (contact == null || contact.ImBlocked) return;
            InitNotificationService();

            var androidRegistrationIds = GetAndroidGroupParticipantsRegistrationIds(contact);
            if(androidRegistrationIds.Count>0)
                await _notificationService.SendNotification( contact.ChatId + "", 
                    true, GetAndroidGroupParticipantsRegistrationIds(contact),
                     contact.Language).ConfigureAwait(false);

            var iosRegistrationIds = GetIosGroupParticipantsRegistrationIds(contact);
            if(iosRegistrationIds.Count>0)
                await _notificationService.SendNotification(contact.ChatId + "", 
                    false, GetIosGroupParticipantsRegistrationIds(contact),
                     contact.Language).ConfigureAwait(false);
        }
        
        private static List<string> GetAndroidGroupParticipantsRegistrationIds(Contact contact)
        {

            List<string> registrationIds = new List<string>();
            foreach (var key in contact.Participants)
            {
                if (key != null && Convert.ToBase64String(key) != App.Context.My.GetPublicKey())
                {
                    Contact participant = App.Context.Contacts.GetParticipant(key);
                    if (participant != null && !string.IsNullOrWhiteSpace(participant.FirebaseToken) && participant.Os == Contact.RuntimePlatform.Android)
                        registrationIds.Add(participant.FirebaseToken);
                }
            }
            return registrationIds;
        }

        private static List<string> GetIosGroupParticipantsRegistrationIds(Contact contact)
        {

            List<string> registrationIds = new List<string>();
            foreach (var key in contact.Participants)
            {
                if (key != null && Convert.ToBase64String(key) != App.Context.My.GetPublicKey())
                {
                    Contact participant = App.Context.Contacts.GetParticipant(key);
                    if (participant != null && !string.IsNullOrWhiteSpace(participant.FirebaseToken) && participant.Os == Contact.RuntimePlatform.iOS)
                        registrationIds.Add(participant.FirebaseToken);
                }
            }
            return registrationIds;
        }
    }
}