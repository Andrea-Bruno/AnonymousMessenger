using CustomViewElements;
using EncryptedMessaging;
using Rg.Plugins.Popup.Services;
using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Cryptogram.Services.GoogleTranslationService;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

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

                                PopupNavigation.Instance.PopAsync(false);
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