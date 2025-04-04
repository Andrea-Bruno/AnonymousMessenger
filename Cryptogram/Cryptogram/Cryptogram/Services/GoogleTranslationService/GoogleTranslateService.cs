using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace Cryptogram.Services.GoogleTranslationService
{
    static class GoogleTranslateService
    {
        public static string Translate(string Text)
        {
            var current = Connectivity.NetworkAccess;

            if (current == NetworkAccess.Internet)
            {
                var credential = GoogleCredential.FromJson(JsonConvert.SerializeObject(new GoogleTranslateApiKey()));
                var client = TranslationClient.Create(credential, TranslationModel.NeuralMachineTranslation);
                var sourceLanguage = client.DetectLanguage(Text).Language;

                if (sourceLanguage == LanguageCodes.English)
                {
                    return null;
                }

                var Result = client.TranslateText(Text, LanguageCodes.English, sourceLanguage, TranslationModel.NeuralMachineTranslation);
                return Result.TranslatedText;
            }
            else
            {
                return null;
            }
        }

        public static string Translate(string Text, string target = "en")
        {
            var current = Connectivity.NetworkAccess;

            if (current == NetworkAccess.Internet)
            {
                var credential = GoogleCredential.FromJson(JsonConvert.SerializeObject(new GoogleTranslateApiKey()));
                var client = TranslationClient.Create(credential, TranslationModel.NeuralMachineTranslation);
                var sourceLanguage = client.DetectLanguage(Text).Language;

                if (sourceLanguage == target)
                {
                    return null;
                }

                var Result = client.TranslateText(Text, target, sourceLanguage, TranslationModel.NeuralMachineTranslation);
                return Result.TranslatedText;
            }
            else
            {
                return null;
            }
        }

        public static string Translate(string Text, string source, string target = "en")
        {
            if (source == target)
            {
                return null;
            }

            var current = Connectivity.NetworkAccess;

            if (current == NetworkAccess.Internet)
            {
                var credential = GoogleCredential.FromJson(JsonConvert.SerializeObject(new GoogleTranslateApiKey()));
                var client = TranslationClient.Create(credential, TranslationModel.NeuralMachineTranslation);
                var Result = client.TranslateText(Text, target, source, TranslationModel.NeuralMachineTranslation);
                return Result.TranslatedText;
            }
            else
            {
                return null;
            }
        }

    }
}