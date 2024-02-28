using Plugin.Toast;
using Rg.Plugins.Popup.Services;
using System;
using Telegraph.Services;
using System.Collections.Generic;
using EncryptedMessaging;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Text;
using XamarinShared;
using Telegraph.Views;
using System.Threading.Tasks;
using Telegraph;
using CustomViewElements.Services;
using CustomViewElements;
using System.Collections.ObjectModel;
using Telegraph.Services.GoogleTranslationService;
using System.Globalization;
using System.Threading;
using XamarinShared.ViewCreator;
using Telegraph.DesignHandler;
using System.IO;
using MessageCompose.Services;
using Telegraph.PopupViews;

[assembly: Dependency(typeof(ChatRoom))]
namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatRoom : BasePage, IKeyboardHeightChange
    {
        private int count = 0;
        private Contact _contact;
        public static bool IsAudioRecordCancelled = true;
        public static bool IsAudioSendCancelled = true;
        private bool _isGroup;
        private readonly List<string> _listOfMembers = new List<string>();
        public static ContentView KeyboardView;
        public static readonly ScrollView MessageContainer = new ScrollView();
        private bool isUnreadedMessagesViewRemoved = true;
        public static ChatRoom Instance;
        private static readonly int DISABLE_SCROLLING_DIFF = 50;
        public delegate void AttachPictureHandler(byte[] image);
        public event AttachPictureHandler AttachPicture;
        private ObservableCollection<Tuple<Message, Label>> selectedMessages;
        private IAudioPlayer Player;
        public ChatRoom()
        {
        }
        public ChatRoom(Contact contact, byte[] file = null)
        {
            if (contact != null)
            {
                BindingContext = ChatPageSupport.GetContactViewItems(contact);
                _contact = contact;
                selectedMessages = ChatPageSupport.GetContactViewItems(_contact).SelectedMessagesList;
                InitializeChatRoom();
                InitializeComposer();
                if (file != null)
                {
                    NavigationTappedPage.Context.Messaging.SendPicture(file, _contact);
                }
                selectedMessages.CollectionChanged += SelectedMessages_CollectionChanged;
                Player = DependencyService.Get<IAudioPlayer>();
                User_Photo.Contact = contact;

            }
        }
        //TODO: Clear selections,transfer to binding
        private void SelectedMessages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var count = selectedMessages.Count;
            MFCount.Text = Convert.ToString(count);

            if (count == 1)
            {
                foreach (var data in selectedMessages)
                {
                    MFTranslate.IsVisible = data.Item2 != null;
                }
            }
            else
            {
                MFTranslate.IsVisible = false;
            }

            if (count == 0 && ChatPageSupport.GetContactViewItems(_contact).IsMessageSelection) ClearMFToolbarState();
        }

        private void InitializeChatRoom()
        {
            InitializeComponent();
            Instance = this;
            MessagesLyt.Children.Add(MessageContainer);
            _isGroup = _contact.IsGroup;
            if (_isGroup)
                SetGroupInfo();
            else
                LastSeen.IsVisible = false;
            if (_contact.UnreadMessages > 0)
                isUnreadedMessagesViewRemoved = false;
            MessageContainer.Scrolled += OnContentScrollViewScrolled;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CheckContactIsBlocked();
            if (_contact != null && !_contact.IsBlocked)
            {
                ChatPageSupport.SetCurrentContact(_contact);
                ScrollPageToFirstUnreadedMessage();
            }
            ChatPageSupport.GetContactViewItems(_contact).IsMessageSelection = false;
            ChatPageSupport.GetContactViewItems(_contact).SelectedMessagesList.Clear();
            isUnreadedMessagesViewRemoved = false;
            App.CurrentChatId = _contact.ChatId;
            Username.Text = _contact.Name;

            SetAudioVideoButtonVisibility();
            if (Device.RuntimePlatform == Device.iOS)
            {
                var dependency = DependencyService.Get<IKeyboardRegistrationService>();
                dependency.RegisterForKeyboardNotifications();
            }
            DependencyService.Get<IStatusBarColor>().SetStatusbarColor(DesignResourceManager.GetColorFromStyle("Color2"));
            if(_contact.MessageContainerUI !=null)
                (_contact.MessageContainerUI as StackLayout).ChildAdded += MessagesLyt_ChildAdded;

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            RemoveUnreadedMessagesView();
            App.CurrentChatId = 0;
            ClearMFToolbarState();
            ChatPageSupport.SetCurrentContact(null);
            IsAudioSendCancelled = true;
            ResetAudioRecorderView();
            if (Device.RuntimePlatform == Device.iOS)
                DependencyService.Get<IKeyboardRegistrationService>().UnregisterForKeyboardNotifications();
            DependencyService.Get<IStatusBarColor>().SetStatusbarColor(DesignResourceManager.GetColorFromStyle("Color1"));
            if (_contact.MessageContainerUI != null)
                (_contact.MessageContainerUI as StackLayout).ChildAdded -= MessagesLyt_ChildAdded;
        }

        private void InitializeComposer()
        {
            ComposerPlaceHolder.Init(MessageContainer, OnSend, OnMediaFileSelected);
            ComposerPlaceHolder.AudioPlayerEvent += AudioPlayer_Clicked;
            ComposerPlaceHolder.TextMessage.Unfocused += TextMessage_Unfocused;
            ComposerPlaceHolder.TextMessage.Focused += TextMessage_Focused;
            ComposerPlaceHolder.OnSendClick += RemoveUnreadedMessagesView;
            ComposerPlaceHolder.SizeChanged += ComposerPlaceHolder_SizeChanged;
        }

        private void ComposerPlaceHolder_SizeChanged(object sender, EventArgs e)
        {
            Constraint yconstraint = Constraint.RelativeToParent((parent) => { return parent.Height - ComposerPlaceHolder.Height - 16-DownScroll.Height; });
            RelativeLayout.SetYConstraint(DownScroll, yconstraint);
        }
        
        private void OnSend(string text = null, byte[] image = null, byte[] audio = null, Tuple<double, double> coordinate = null, byte[] pdf = null, byte[] phoneContact = null)
        {
            var currentContact = _contact;
            if (!string.IsNullOrEmpty(text))
                NavigationTappedPage.Context.Messaging.SendText(text, currentContact);
            if (image != null)
                NavigationTappedPage.Context.Messaging.SendPicture(image, currentContact);
            if (audio != null)
                NavigationTappedPage.Context.Messaging.SendAudio(audio, currentContact);
            if (coordinate != null)
                NavigationTappedPage.Context.Messaging.SendLocation(coordinate.Item1, coordinate.Item2, currentContact);
            if (pdf != null)
                NavigationTappedPage.Context.Messaging.SendPdfDocument(pdf, currentContact);
            if (phoneContact != null)
                NavigationTappedPage.Context.Messaging.SendPhoneContact(phoneContact, currentContact);
        }

        private void OnMediaFileSelected(byte[] image)
        {
            var editImagePage = new EditImagePage(image);
            editImagePage.AttachPicture += (data) => OnSend(image: data);
            Application.Current.MainPage.Navigation.PushAsync(editImagePage, false);
        }

        private void CheckContactIsBlocked()
        {
            if (_contact.IsBlocked)
            {
                ComposerPlaceHolder.IsVisible = false;
                BlockUserTxt.IsVisible = true;
            }
            else
            {
                ComposerPlaceHolder.IsVisible = true;
                BlockUserTxt.IsVisible = false;
            }
        }

        public void ResetAudioRecorderView() => ComposerPlaceHolder.ResetAudioRecorderView();

        public async void Photo_Clicked(object sender, EventArgs _)
        {
            if (User_Photo.Content is Image)
                await PopupNavigation.Instance.PushAsync(new ImageViewPopupPage((User_Photo.Content as Image).Source), true).ConfigureAwait(true);
        }

        private void SetGroupInfo()
        {
            foreach (var key in _contact.Participants)
            {
                if (key != null && Convert.ToBase64String(key) != NavigationTappedPage.Context.My.GetPublicKey())
                {
                    Contact participant = NavigationTappedPage.Context.Contacts.GetParticipant(key);
                    if (participant != null && !string.IsNullOrWhiteSpace(participant.Name))
                        _listOfMembers.Add(participant.Name);
                }
            }
            LastSeen.Text = string.Join(", ", _listOfMembers.ToArray());
        }

        private void Back_Clicked(object _, EventArgs e_) => OnBackButtonPressed();

        protected override bool OnBackButtonPressed()
        {
            if (PopupNavigation.Instance.PopupStack.Count > 0)
            {
                PopupNavigation.Instance.PopAsync(true);
            }
            else
            if (KeyboardView != null && KeyboardView.IsVisible) KeyboardView.IsVisible = false;

            else if (ChatPageSupport.GetContactViewItems(_contact).IsMessageSelection)
            {
                ClearMFToolbarState();
            }
            else
            {
                Navigation.PopToRootAsync(false);
            }
            return true;
        }

        private void JoinGroupCall_Clicked(object _, EventArgs e_) => ((App)Application.Current)?.CallManager.StartCall(_contact, false, true);

        private void AudioCall_Clicked(object _, EventArgs e_) => ((App)Application.Current)?.CallManager.StartCall(_contact, false, ChatPageSupport.GetContactViewItems(_contact).IsCallGoingOn);

        private void VideoCall_Clicked(object _, EventArgs e_) => ((App)Application.Current)?.CallManager.StartCall(_contact, true, ChatPageSupport.GetContactViewItems(_contact).IsCallGoingOn);

        private void SetAudioVideoButtonVisibility()
        {
            if (_contact.IsBlocked)
                AudioButton.IsVisible = VideoButton.IsVisible = false;
            else
                AudioButton.IsVisible = VideoButton.IsVisible = true;
        }

        private async void Username_Clicked(object _, EventArgs e) => await Navigation.PushAsync(new ChatUserProfilePage(_contact), false).ConfigureAwait(true);


        private void RemoveUnreadedMessagesView()
        {
            if (!isUnreadedMessagesViewRemoved)
                ChatPageSupport.RemoveUnreadedMessageView();
            isUnreadedMessagesViewRemoved = true;
          //  ComposerPlaceHolder.OnNewMessageReceived(true);
        }
        private void TextMessage_Unfocused(object _, FocusEventArgs f)
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                Root.Margin = 0;
            }
        }

        public void OnKeyboardHeightChange(double height)
        {
            if (Device.RuntimePlatform == Device.iOS) //&& !App.Setting.KeyBoardVis)
                Instance.Root.Margin = new Thickness(0, 0, 0, height);
        }

        private void DownScroll_Clicked(object sender, EventArgs e) => ScrollToEnd(true);

        private void OnContentScrollViewScrolled(object sender, ScrolledEventArgs e)
        {
          
            if (MessagesLyt.Height !=0 && e.ScrollY + MessagesLyt.Height + DISABLE_SCROLLING_DIFF < MessageContainer.ContentSize.Height && DownScroll.IsVisible == false)
            {
                if (DownScroll.Source == null)
                    DownScroll.Source = DesignResourceManager.GetImageSource("ic_scroll_down.png");
                DownScroll.IsVisible = true;
            }
       
            else if (e.ScrollY + MessagesLyt.Height + DISABLE_SCROLLING_DIFF >= MessageContainer.ContentSize.Height && DownScroll.IsVisible == true)
            {
                DownScroll.IsVisible = false;
                NewMessageScrollInfo.IsVisible = false;
                count=0;
            }
        }

        private void TextMessage_Focused(object _, FocusEventArgs f)
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                ScrollToEnd(false);
            }
            else
                new Timer((obj) => { ScrollToEnd(false); }, null, 100, Timeout.Infinite);  // wait for to add the new message.
        }


        private void MessagesLyt_ChildAdded(object sender, ElementEventArgs e)
        {
        
            Device.BeginInvokeOnMainThread(() =>
            {
                if (MessageContainer.Content != null && MessageContainer.ScrollY > MessageContainer.ContentSize.Height - Application.Current.MainPage.Height)
                    new Timer((obj) => { ScrollToEnd(false); }, null, 100, Timeout.Infinite);  // wait for to add the new message.                                     
                
                else
                {
                    if (DownScroll.IsVisible && !_contact.LastMessageIsMy)
                    {
                        NewMessageScrollInfo.IsVisible = true;
                        count++;
                        NewMessageScrollInfo.Text = count.ToString();
                    }
                }
            });
        }

        public void ScrollToEnd(bool animate = false)
        {
            if (MessageContainer == null || MessageContainer.Content == null) // chatroom hot reload
                return;
            try
            {
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (Device.RuntimePlatform == Device.iOS)
                        {
                            var lastElement = (MessageContainer.Content as StackLayout).Children.Count>0 ?(MessageContainer.Content as StackLayout).Children[(MessageContainer.Content as StackLayout).Children.Count - 1] : MessageContainer.Content;
                            MessageContainer.ScrollToAsync(lastElement, ScrollToPosition.MakeVisible, animate);

                        }
                        else

                            MessageContainer.ScrollToAsync(0, MessageContainer.ContentSize.Height, animate);
                    });
                }
            }
            catch (Exception e)
            {
                //prevent element does not belong to this ScrollView Parameter name: element
            }
        }

        private void ScrollPageToFirstUnreadedMessage()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (MessageContainer.Content is StackLayout layout)
                {
                    var child = ChatPageSupport.CheckContainterContainsUnreadedMessageView(layout.Children);
                    if (child != null)
                    {
                        if (Device.RuntimePlatform == Device.iOS)
                        {
                            await MessageContainer.ScrollToAsync(child, ScrollToPosition.End, false);
                            await MessageContainer.ScrollToAsync(0, MessageContainer.ScrollY + 50, false);
                        }
                        else
                            await MessageContainer.ScrollToAsync(child, ScrollToPosition.Start, false);
                    }
                    else ScrollToEnd();
                }
            });
        }

        public void ClearMFToolbarState()
        {
            if (ChatPageSupport.GetContactViewItems(_contact).SelectedMessagesList.Count != 0) // prevent loop for collection change SelectedMessages_CollectionChanged
                ChatPageSupport.GetContactViewItems(_contact).SelectedMessagesList.Clear();
             ChatPageSupport.GetContactViewItems(_contact).IsMessageSelection = false;
        }

        private void MFCancel_Clicked(object sender, EventArgs e) => ClearMFToolbarState();

        private void MFForward_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new GroupUserSelectPage(GetSelectedMessages()), false);
            ClearMFToolbarState();
        }

        private List<Message> GetSelectedMessages()
        {
            List<Message> list = new List<Message>();

            foreach (var data in selectedMessages)
            {
                list.Add(data.Item1);
            }

            return list;
        }

        //in case of encrypted will not work
        private void MFCopy_Clicked(object sender, EventArgs e)
        {
            var myId = NavigationTappedPage.Context.My.GetId();
            StringBuilder sb = new StringBuilder();
            foreach (Message m in GetSelectedMessages())
            {
                if (m != null && m.Type == MessageFormat.MessageType.Text && m.GetData() != null)
                {
                    var username = myId == m.AuthorId ? NavigationTappedPage.Context.My.Name : m.AuthorName();

                    sb.Append(username);
                    sb.Append("[" + m.Creation.ToLocalTime().ToString() + "]: ");
                    sb.Append(Encoding.Unicode.GetString(m.GetData()));
                    sb.AppendLine("");
                }
            }
            Xamarin.Essentials.Clipboard.SetTextAsync(sb.ToString());
            CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.CopiedToClipboard);
            ClearMFToolbarState();
        }

        private void MFDelete_Clicked(object sender, EventArgs e)
        {
            List<Message> messages = GetSelectedMessages();
            var isContainsOppenentMessage = messages.Find(m => NavigationTappedPage.Context.My.GetId() != m.AuthorId) != null;
            PopupNavigation.Instance.PushAsync(new DeleteMessagePopupPage(messages, !isContainsOppenentMessage), true);
            ClearMFToolbarState();
        }

        private void MFTranslate_Clicked(object sender, EventArgs e)
        {
            Message message = null;
            Contact contact = null;
            Label textLabel = null;

            if (selectedMessages.Count != 1)
                return;

            foreach (var data in selectedMessages)
            {
                message = data.Item1;
                textLabel = data.Item2;
                contact = message.Contact;
                break;
            }

            if (message.Type != MessageFormat.MessageType.Text || textLabel == null)
                return;

            if (!contact.TranslationOfMessages && MessageViewCreator.MessageTranslationCounter >= 2)
            {
                contact.TranslationOfMessages = true;
                contact.Save();
                MessageViewCreator.MessageTranslationCounter = 0;
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.AutoMessageTranslationEnabled);
            }

            if (message.Translation == null)
                TranslateText(message, textLabel);
            else
            {
                ChatPageSupport.TransalateMessageDisplay(message, textLabel);
                ClearMFToolbarState();
            }
        }

        private void TranslateText(Message message, Label textLabel)
        {
            Task task = Task.Run(() =>
            {
                if (message != null && message.GetData() != null)
                {
                    string TranslatedText = GoogleTranslateService.Translate(Encoding.Unicode.GetString(message.GetData()), CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
                    UpdateTranslationView(TranslatedText, message, textLabel);
                }
            });
        }

        private void UpdateTranslationView(string TranslatedText, Message message, Label textLabel)
        {
            if (TranslatedText != null)
            {
                if (!message.Contact.TranslationOfMessages)
                {
                    MessageViewCreator.MessageTranslationCounter++;
                }
                Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                {
                    message.Translation = TranslatedText;
                    ChatPageSupport.ForceTransalateMessageDisplay(TranslatedText, textLabel);
                    ClearMFToolbarState();
                });
            }
            else
            {
                Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                {
                    CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.MessageCouldNotTranslate);
                    ClearMFToolbarState();
                });
            }
        }

        private void AudioPlayer_Clicked()
        {
            if (Player.IsPlaying)
            {
                StopAudio();
                return;
            }
            PlayAudio(DependencyService.Get<IAudioRecorder>().GetOutput());
            Player.PlaybackEnded += (s, args) =>
            {
                StopAudio();
            };
        }

        private void PlayAudio(byte[] data)
        {
            var ms = new MemoryStream(data);
            Player?.Load(ms);
            Player?.Play();
            ComposerPlaceHolder?.PlayAudio();

        }

        private void StopAudio()
        {
            ComposerPlaceHolder?.StopAudio();
            Player?.Stop();
        }

    }
}
