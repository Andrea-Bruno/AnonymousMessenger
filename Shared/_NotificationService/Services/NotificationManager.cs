using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NotificationService.Models;

namespace NotificationService.Services
{
    public class NotificationManager
    {
        private static readonly string FCM_API = "https://fcm.googleapis.com";
        private static readonly string ServerKey = "key=AAAAv7g0CTw:APA91bH87OLdNgKrarTn4rc5f_dFgNyy4uDvznONKHB20pQx9LRZX1R_yAxpInYmootdAj24AQZxDGYQsueg_xQLlptnsVs0IwmWyldt0q5Kf-3JNBf6UIxbpu26BmbiN6iffhEXEGd5";

        private const string Priority = "high";
        public async Task SendNotification(string chatId, bool isAndroid, List<string> registrationIds, string language)
        {
            var cultureInfo = GetCultureInfo(language);
            var message = GetString("NewNotification", cultureInfo);
            string json = GenerateNotificationContent(isAndroid, chatId, registrationIds, message);
            await PostNotification(json);
        }

        private string GenerateNotificationContent(bool isRemoteAndroid, string chatId, List<string> registrationIds, string message)
        {
            if (isRemoteAndroid)
                return Newtonsoft.Json.JsonConvert.SerializeObject(new AndroidNotificationMessage(
                    registrationIds,
                    new Data(message, message, chatId, Priority),
                    Priority));
            return Newtonsoft.Json.JsonConvert.SerializeObject(new IOSNotificationMessage(
                registrationIds,
                new Data(message, message, chatId, Priority),
                new Notification(message, message, Priority),
                Priority));
        } 

        private async Task PostNotification(string json)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", ServerKey);
                client.BaseAddress = new Uri(FCM_API);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync("/fcm/send", content);
            }
            catch (Exception e)
            {
                // ignored
            }
        }
        private CultureInfo GetCultureInfo(string twoLetterIsoLanguageName)
        {
            if(twoLetterIsoLanguageName == null) return new CultureInfo("en-US");
            if (twoLetterIsoLanguageName.ToLower() == "it")
                return new CultureInfo("it-IT");
            if (twoLetterIsoLanguageName.ToLower() == "de")
                return new CultureInfo("de-DE");
            if (twoLetterIsoLanguageName.ToLower() == "fr")
                return new CultureInfo("fr-FR");
            if (twoLetterIsoLanguageName.ToLower() == "es")
                return new CultureInfo("es-ES");

            return new CultureInfo("en-US");
        }

        private string GetString(string key, CultureInfo cultureInfo)
        {
            return Localization.Resources.Dictionary.ResourceManager.GetString(key, cultureInfo);
        }
    }
}
