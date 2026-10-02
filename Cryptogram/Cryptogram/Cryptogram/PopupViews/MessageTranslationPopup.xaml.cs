using CustomViewElements;
using EncryptedMessaging;
using Mopups.Services;
using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Cryptogram.Services.GoogleTranslationService;
using CommunityToolkit.Maui;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;
using Microsoft.Maui.Networking;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls.Xaml;

namespace Cryptogram.PopupViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessageTranslationPopup : BasePopupPage
    {
        private Message _message;
        private bool textTranslated;
        private string TranslatedText;

        public MessageTranslationPopup()
        {
            InitializeComponent();
        }

        public MessageTranslationPopup(Message message)
        {
            InitializeComponent();
            BackButton.Source = Utils.Icons.IconProvider?.Invoke("ic_back_icon");
            CopyImage.Source = Utils.Icons.IconProvider?.Invoke("ic_copy_message"); 
            _message = message;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_message.Translation == null)
            {
                Task task = Task.Run(() =>
                {
                    if (_message != null && _message.GetData() != null)
                    {
                        TranslatedText = GoogleTranslateService.Translate(
                            Encoding.Unicode.GetString(_message.GetData()),
                        CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
                        if (TranslatedText != null)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                TranslatedTextMessageLabel.LinksText = TranslatedText;
                                _message.Translation = TranslatedText;
                                textTranslated = true;
                            });
                        }
                        else
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {

                                MopupService.Instance.PopAsync(false);
                                this.DisplayToastAsync(Localization.Resources.Dictionary.MessageCouldNotTranslate);
                            });
                        }
                    }
                });

                TranslatedTextMessageLabel.LinksText = Localization.Resources.Dictionary.Translating + " . . .";
            }
            else
            {
                TranslatedTextMessageLabel.LinksText = _message.Translation;
                textTranslated = true;
            }
        }
  
        private void Copy_Tapped(object sender, EventArgs e)
        {
            if (textTranslated)
            {
                Clipboard.SetTextAsync(TranslatedText);
                this.DisplayToastAsync(Localization.Resources.Dictionary.CopiedToClipboard);
            }
        }

        private void Button_Clicked(object sender, EventArgs e) => OnBackButtonPressed();
    }
}