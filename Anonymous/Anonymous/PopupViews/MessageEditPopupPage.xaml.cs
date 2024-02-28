using Rg.Plugins.Popup.Services;
using System;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using EncryptedMessaging;
using XamarinShared;
using System.Collections.Generic;
using Plugin.Toast;
using System.Globalization;
using System.Threading.Tasks;
using CustomViewElements;
using XamarinShared.ViewCreator;
using static XamarinShared.ViewCreator.MessageViewCreator;
using Telegraph.Services.GoogleTranslationService;
using Telegraph.Views;

namespace Telegraph.PopupViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessageEditPopupPage : BasePopupPage
    {
        private readonly Message _message;
        readonly bool _isMyMessage;
        List<Contact> readContacts = new List<Contact>();
        private SwipeView _view;
        private MessageInfoClickedEvent _messageInfoClicked;


        public MessageEditPopupPage(Message message, MessageReadStatus.MessageStatus messageStatus, bool isMyMessage, SwipeView view,
           MessageInfoClickedEvent messageInfoClicked)
        {
            InitializeComponent();
            ReplyImage.Source = Utils.Icons.IconProvider?.Invoke("ic_reply");
            ForwardImage.Source = Utils.Icons.IconProvider?.Invoke("ic_forward"); 
            CopyImage.Source = Utils.Icons.IconProvider?.Invoke("ic_copy_message");
            DeleteImage.Source = Utils.Icons.IconProvider?.Invoke("ic_popup_message_delete"); 
            MessageInfoImage.Source = Utils.Icons.IconProvider?.Invoke("ic_message_info");
            TranslateImage.Source = Utils.Icons.IconProvider?.Invoke("ic_translate"); 
            _view = view;
            _message = message;
            _isMyMessage = isMyMessage;
            //_messageForwardClicked = messageForwardClicked;
            _messageInfoClicked = messageInfoClicked;
            if (messageStatus != null && message.Contact.IsGroup)
                foreach (var userID in messageStatus.ParticipantsReaded)
                    readContacts.Add(NavigationTappedPage.Context.Contacts.GetContactByUserID(userID));

            if (message.Contact.IsGroup && isMyMessage)
                MessageInfoLayout.IsVisible = false; // change it after redesign
            else
                MessageInfoLayout.IsVisible = false;

            if (message.Type == MessageFormat.MessageType.Text)
            {
                if (!isMyMessage)
                {
                    TranslateLayout.IsVisible = true;
                }
                CopyLayout.IsVisible = true;
            }
            else
            {
                CopyLayout.IsVisible = false;
            }

            if (message.Type == MessageFormat.MessageType.Text
                || message.Type == MessageFormat.MessageType.Image
                || message.Type == MessageFormat.MessageType.PdfDocument
                || message.Type == MessageFormat.MessageType.Audio
                || message.Type == MessageFormat.MessageType.Location
                || message.Type == MessageFormat.MessageType.PhoneContact)
                ForwardLayout.IsVisible = true;
            else
                ForwardLayout.IsVisible = false;
        }
        private async void Forward_Clicked(object sender, EventArgs e)
        {
            CheckMessageDeleted();
            //_messageForwardClicked(_message);           
            if (PopupNavigation.Instance.PopupStack.Count > 0)
                await PopupNavigation.Instance.PopAsync(false);
        }
        private async void Reply_Clicked(object sender, EventArgs e) => await PopupNavigation.Instance.PopAsync(false);

        private async void MessageInfo_Clicked(object sender, EventArgs e)
        {
            CheckMessageDeleted();
            _messageInfoClicked(readContacts);
            if (PopupNavigation.Instance.PopupStack.Count > 0)
                await PopupNavigation.Instance.PopAsync(false);
        }

        private async void Delete_Clicked(object sender, EventArgs e)
        {
            CheckMessageDeleted();
            await PopupNavigation.Instance.PopAsync(false);
            //await PopupNavigation.Instance.PushAsync(new DeleteMessagePopupPage(_message, _isMyMessage), true);
        }

        /*	private async void DeleteFromEveryOne_Clicked(object sender, EventArgs e)
			{
				if (_message != null && App.SelectedMessageHashCode != _message.GetHashCode())
					ChatPageSupport.RemoveMessage(_message, true);
				else
					CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.SelectedMessageCannotBeDeleted);
				await PopupNavigation.Instance.PopAsync(false);
			}*/


        private async void Translate_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync(false);
            //await PopupNavigation.Instance.PushAsync(new MessageTranslationPopup(_message), true);

            if (!_message.Contact.TranslationOfMessages && MessageTranslationCounter >= 2)
            {
                _message.Contact.TranslationOfMessages = true;
                _message.Contact.Save();
                MessageTranslationCounter = 0;
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.AutoMessageTranslationEnabled);
            }

            string TranslatedText;

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
                            if (!_message.Contact.TranslationOfMessages)
                            {
                                MessageTranslationCounter++;
                            }
                            Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                            {
                                _message.Translation = TranslatedText;
                                //Need to implement Translation switching with true and false
                                //ChatPageSupport.TransalateMessageDisplay(_message, _view);
                            });
                        }
                        else
                        {
                            Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                            {
                                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.MessageCouldNotTranslate);
                            });
                        }
                    }
                });
            }
            else
            {
                //ChatPageSupport.TransalateMessageDisplay(_message, _view);
            }
        }

        private void Copy_Clicked(object sender, EventArgs e)
        {
            CheckMessageDeleted();
            if (_message != null && _message.GetData() != null)
                Xamarin.Essentials.Clipboard.SetTextAsync(Encoding.Unicode.GetString(_message.GetData()));
            CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.CopiedToClipboard);
            PopupNavigation.Instance.PopAsync(false);
        }

        private void Background_Clicked(object sender, EventArgs e) => OnBackButtonPressed();

        private void CheckMessageDeleted()
        {
            if (_message == null)
            {
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.MessageAlreadDeleted);
                PopupNavigation.Instance.PopAsync(false);
                return;
            }

        }
    }
}