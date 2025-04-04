using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using EncryptedMessaging;
using Cryptogram.Services.GoogleTranslationService;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using XamarinShared;

namespace Cryptogram
{
    public static class CryptogramUtils
    {
        public static void Translate(this Message message, Action onTranslationSuccess, bool withToast = false)
        {
            var contact = message?.Contact;
            ChatPageSupport.GetContactViewItems(contact, out var contactViewItems);
            if (message?.Type != MessageFormat.MessageType.Text) return;

            // Enable auto translation if user translate the message more than 3 times
            if (!contact.TranslationOfMessages && contactViewItems.MessageTranslationCounter >= 2)
            {
                contact.TranslationOfMessages = true;
                contact.Save();
                contactViewItems.MessageTranslationCounter = 0;
                Application.Current?.MainPage?.DisplayToastAsync(Localization.Resources.Dictionary
                    .AutoMessageTranslationEnabled);
            }
            else
            {
                contactViewItems.MessageTranslationCounter++;
            }

            // Perform translation if translation does not exists
            if (string.IsNullOrWhiteSpace(message.Translation))
            {
                Task.Run(() =>
                {
                    var originalText = Encoding.Unicode.GetString(message.GetData());
                    var translatedText = GoogleTranslateService.Translate(originalText, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

                    if (string.IsNullOrWhiteSpace(translatedText) || originalText.ToLower().Equals(translatedText.ToLower()))
                    {
                        message.Translation = null;
                        if (withToast)
                        {
                            Application.Current?.MainPage?.DisplayToastAsync(Localization.Resources.Dictionary
                                .MessageCouldNotTranslate);
                        }
                    }
                    else
                    {
                        message.Translation = Localization.Resources.Dictionary.Translated + ": " + translatedText;
                        MainThread.BeginInvokeOnMainThread(onTranslationSuccess);
                    }
                });
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(onTranslationSuccess);
            }
        }
    }
}