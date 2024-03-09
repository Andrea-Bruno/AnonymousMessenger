using NotificationService.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Services
{
    public class NotificationManager
    {
        private static readonly string FCM_API = "https://fcm.googleapis.com";
        private static readonly string serverKey = "key=AAAAYXDykOk:APA91bG_GSApHmIdZc5IKy6zNgQdRXNnCQtslUkmDgPbH2JIO7mF3pBgxIuIzArXrt44VaC_jP9lnRqjODPeIXqv4rKDboWl7LqGs_DxVc_O8yZX0GRCz-ljFScjB9AThgQNBKGbLuyp";

        private string _userId;
        private readonly string _priority = "high";

        public NotificationManager(string userId)
        {
            _userId = userId;
        }

        public async Task SendNotification(string contactName, string chatId, bool isGroup, bool isAndroid, List<string> registrationIds, int badgeCount, NotificationType notificationType, string language)
        {
            var titleBody = GetNotificationTitleBody(isGroup, contactName, notificationType, language);
            string json = GenerateNotificationContent(isAndroid, chatId, contactName, notificationType, registrationIds, titleBody, badgeCount);
            await PostNotification(json);
        }

        [Obsolete("SendNotification is deprecated, please use the new SendNotification method.")]
        public async Task SendNotification(string chatId, bool isGroup, List<string> registrationIds, string language)
        {
            await SendNotification(null, chatId, isGroup, true, registrationIds, 0, NotificationType.TEXT, language);
        }

        private string GenerateNotificationContent(bool isRemoteAndroid, string chatId, string contactName, NotificationType notificationType, List<string> registrationIds, Tuple<string, string> titleBody, int badgeCount)
        {
            if (isRemoteAndroid)
                return Newtonsoft.Json.JsonConvert.SerializeObject(new AndroidNotificationMessage(
                    registrationIds,
                    new Data(titleBody.Item1, titleBody.Item2, _userId, chatId, contactName, _priority, notificationType),
                    _priority));
            else
                return Newtonsoft.Json.JsonConvert.SerializeObject(new IOSNotificationMessage(
                    registrationIds,
                    new Data(titleBody.Item1, titleBody.Item2, _userId, chatId, contactName, _priority, notificationType),
                    new Notification(titleBody.Item1, titleBody.Item2, _priority, "default", badgeCount),
                    _priority));
        }

        private async Task PostNotification(string json)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", serverKey);
                client.BaseAddress = new Uri(FCM_API);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("/fcm/send", content);

                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine("-- >> Notification Response " + result);
            }
            catch (Exception e)
            {
                Console.WriteLine("-- >> Notification Error Response " + e.Message);

            }
        }


        private Tuple<string, string> GetNotificationTitleBody(bool isGroup, string contactRemoteName, NotificationType notificationType, string language)
        {
            contactRemoteName = " " + contactRemoteName;
            string title = null, body;
            CultureInfo cultureInfo = GetCultureInfo(language);
            switch (notificationType)
            {
                case NotificationType.TEXT:
                    body = GetString("YouHaveReceivedNewMessageFrom", cultureInfo) + contactRemoteName;
                    break;
                case NotificationType.IMAGE:
                    body = GetString("YouHaveReceivedNewMessageFrom", cultureInfo) + contactRemoteName;
                    break;
                case NotificationType.AUDIO:
                    body = GetString("YouHaveReceivedNewAudioFrom", cultureInfo) + contactRemoteName;
                    break;
                case NotificationType.LOCATION:
                    body = GetString("YouHaveReceivedNewLocationFrom", cultureInfo) + contactRemoteName; // add localization
                    break;
                case NotificationType.DOCUMENT:
                    body = GetString("YouHaveReceivedNewDocumentFrom", cultureInfo) + contactRemoteName; // add localization
                    break;
                case NotificationType.CONTACT:
                    if (!isGroup)
                        body = contactRemoteName + GetString("SomeoneAddedYourContact", cultureInfo);
                    else
                        body = GetString("YouHaveBeenAddedToTheGroup", cultureInfo) + " " + contactRemoteName;
                    break;
                case NotificationType.PHONE_CONTACT:
                    body = GetString("YouHaveReceivedNewContactFrom", cultureInfo) + contactRemoteName; // add localization
                    break;
                case NotificationType.P2P_START_AUDIO_CALL:
                case NotificationType.P2P_DECLINE_CALL:
                    body = GetString("NewAudioCall", cultureInfo) + contactRemoteName; // in call title and body are ignored
                    break;
                case NotificationType.P2P_START_VIDEO_CALL:
                    body = GetString("NewVideoCall", cultureInfo) + contactRemoteName; // in call title and body are ignored
                    break;
                case NotificationType.P2P_MISSED_AUDIO_CALL:
                    title = GetString("NewAudioCall", cultureInfo);
                    body = GetString("MissedAudioCallFrom", cultureInfo) + contactRemoteName;
                    break;
                case NotificationType.P2P_MISSED_VIDEO_CALL:
                    title = GetString("VideoCall", cultureInfo);
                    body = GetString("MissedVideoCallFrom", cultureInfo) + contactRemoteName;
                    break;
                case NotificationType.GROUP_START_AUDIO_CALL:
                    title = GetString("NewAudioCall", cultureInfo);
                    body = GetString("YouReceiveCallFromGroup", cultureInfo) + contactRemoteName;
                    break;
                case NotificationType.GROUP_START_VIDEO_CALL:
                    title = GetString("NewVideoCall", cultureInfo);
                    body = GetString("YouReceiveCallFromGroup", cultureInfo) + contactRemoteName;
                    break;
                case NotificationType.GROUP_END_CALL:
                    title = GetString("CallEnded", cultureInfo) + " @ " + contactRemoteName;
                    body = GetString("CallEnded", cultureInfo);
                    break;
                default:
                    body = GetString("NewNotification", cultureInfo);
                    break;
            }
            if (title == null)
                title = GetString("NewNotification", cultureInfo);
            return new Tuple<string, string>(title, body);
        }

        private CultureInfo GetCultureInfo(string twoLetterISOLanguageName)
        {
            if (twoLetterISOLanguageName == null) return new CultureInfo("en-US");
            if (twoLetterISOLanguageName.ToLower() == "it")
                return new CultureInfo("it-IT");
            if (twoLetterISOLanguageName.ToLower() == "de")
                return new CultureInfo("de-DE");
            if (twoLetterISOLanguageName.ToLower() == "fr")
                return new CultureInfo("fr-FR");
            if (twoLetterISOLanguageName.ToLower() == "es")
                return new CultureInfo("es-ES");

            return new CultureInfo("en-US");
        }

        private string GetString(string key, CultureInfo cultureInfo)
        {
            return Localization.Resources.Dictionary.ResourceManager.GetString(key, cultureInfo);
        }
    }
}
