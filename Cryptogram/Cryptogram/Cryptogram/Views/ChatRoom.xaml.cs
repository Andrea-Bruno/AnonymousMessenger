using Rg.Plugins.Popup.Services;
using System;
using Anonymous.Services;
using System.Collections.Generic;
using EncryptedMessaging;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Text;
using XamarinShared;
using Anonymous.Views;
using System.Threading.Tasks;
using CustomViewElements.Services;
using CustomViewElements;
using System.Collections.ObjectModel;
using System.Threading;
using Anonymous.DesignHandler;
using System.IO;
using MessageCompose.Services;
using Anonymous.PopupViews;
using Xamarin.CommunityToolkit.Extensions;
using System.Linq;
using Utils;
using Xamarin.Essentials;
using Contact = EncryptedMessaging.Contact;
using VideoFileCryptographyLibrary;
using XamarinShared.ViewCreator.Views;

[assembly: Dependency(typeof(ChatRoom))]

namespace Anonymous.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatRoom : BasePage, IKeyboardHeightChange
    {
        private int _downIconUnreadedMessagecount = 0;
        private Contact _contact;
        public static bool IsAudioRecordCancelled = true;
        public static bool IsAudioSendCancelled = true;
        private bool _isGroup;
        private readonly List<string> _listOfMembers = new List<string>();
        public static ContentView KeyboardView;
        private bool isUnreadedMessagesViewRemoved = true;
        public static ChatRoom Instance;
        private double lastScrollY = -10;
        private static readonly int DISABLE_SCROLLING_DIFF = 50;

        public delegate void AttachPictureHandler(List<byte[]> image);

        private ObservableCollection<Tuple<Message, Label>> selectedMessages;
        private XamarinShared.ViewCreator.IAudioPlayer _audioPlayer;
        private bool isMessageSent;
        private double previousScrollYState;
        private bool canReadPosts = true;
        private ContactViewItems _contactViewItems;
        private Frame dateFrame;
        private CancellationTokenSource _stickyHeaderToken;
        private CancellationTokenSource _videoUploadTokenSource;

        public ChatRoom()
        {
        }

        public ChatRoom(Contact contact, byte[] file = null)
        {
            if (contact != null)
            {
                _contact = contact;
                _contactViewItems = ChatPageSupport.GetContactViewItems(_contact);
                BindingContext = _contactViewItems;
                selectedMessages = _contactViewItems.SelectedMessagesList;
                InitializeChatRoom();
                InitializeComposer();
                if (file != null)
                {
                    NavigationTappedPage.Context.Messaging.SendPicture(file, _contact);
                }

                var messageContainerUI = (EncryptedMessaging.Contacts.Observable<object>)contact.MessageContainerUI;
                var a = messageContainerUI.Count;
                messageContainerUI.CollectionChanged += ((sender, args) => { });
                selectedMessages.CollectionChanged += SelectedMessages_CollectionChanged;
                _audioPlayer = DependencyService.Get<XamarinShared.ViewCreator.IAudioPlayer>();
                User_Photo.Contact = contact;
                NavigationTappedPage.Context.Setting.MessagePagination = 5;
                // Setting.MessagePageCount = 5;                
                AddStickyDateFrame();
            }
        }

        private void AddStickyDateFrame()
        {
            var header = ChatPageSupport.GetDateFrame("Date Header");
            header.CornerRadius = 12;
            header.Padding = 0;
            header.IsVisible = false;
            header.BackgroundColor = Root.BackgroundColor;
            header.SizeChanged += (sender, e) =>
            {
                var xConstraint = Constraint.RelativeToParent(parent => parent.Width / 2 - header.Width / 2);
                RelativeLayout.SetXConstraint(header, xConstraint);
            };
            Root.Children.Add(header,
                Constraint.RelativeToParent(parent => parent.Width / 2 - header.Width / 2),
                Constraint.RelativeToParent(parent => 70));
            dateFrame = header;
        }


        private void HideStickyDateHeader()
        {
            _stickyHeaderToken?.Cancel();
            _stickyHeaderToken = new CancellationTokenSource();
            HideStickyDateHeader(_stickyHeaderToken);
        }

        private async Task HideStickyDateHeader(CancellationTokenSource tokenSource)
        {
            await Task.Delay(1000, tokenSource.Token);
            tokenSource.Token.ThrowIfCancellationRequested();
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await dateFrame.FadeTo(0);
                dateFrame.IsVisible = false;
            });
        }


        //TODO: Clear selections,transfer to binding
        private void SelectedMessages_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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


            MFCopy.IsVisible = selectedMessages.Count(tuple => tuple.Item1?.Type != MessageFormat.MessageType.Text) ==
                               0;

            ComposerPlaceHolder.UpdateSelectionState(selectedMessages, count);

            if (count == 0 && _contactViewItems.IsMessageSelection) ClearMFToolbarState();
        }

        private void InitializeChatRoom()
        {
            InitializeComponent();
            Instance = this;
            // MessagesLyt.Children.Add(MessageContainer);
            _isGroup = _contact.IsGroup;
            if (_isGroup)
                SetGroupInfo();
            else
                LastSeen.IsVisible = false;
            if (_contact.UnreadMessages > 0)
                isUnreadedMessagesViewRemoved = false;
            // MessageContainer.Scrolled += OnContentScrollViewScrolled;
        }

        protected override void OnAppearing()
        {
            /// <summary>
            /// Code for Video Sending
            /// </summary>
            if (_contact == App.ContactVideoUploading)
            {
                // if (MessagesLyt.Children.Count == 1)
                // {
                //     MessagesLyt.Children.Clear();
                // }
                //
                // MessagesLyt.Children.Add(MessageContainer);
            }

            base.OnAppearing();
            CheckContactIsBlocked();
            if (_contact != null)
            {
                ChatPageSupport.SetCurrentContact(_contact);
                ScrollPageToFirstUnreadedMessage();
            }

            _contactViewItems.IsMessageSelection = false;
            _contactViewItems.SelectedMessagesList.Clear();
            ComposerPlaceHolder.OnAppearing();
            isUnreadedMessagesViewRemoved = false;
            App.CurrentChatId = _contact.ChatId;
            Username.Text = _contact.Name;

            if (Device.RuntimePlatform == Device.iOS)
            {
                var dependency = DependencyService.Get<IKeyboardRegistrationService>();
                dependency.RegisterForKeyboardNotifications();
            }

            DependencyService.Get<IStatusBarColor>()
                .SetStatusbarColor(DesignResourceManager.GetColorFromStyle("Color2"));

            MessagesLyt.ItemsSource = (EncryptedMessaging.Contacts.Observable<object>)_contact.MessageContainerUI;
            // MessagesLyt.ItemTemplate = new RowTemplateSelector(Setup.MessageReadStatus, App.SendNotification);

            // MessagesLyt.ItemTemplate = new DataTemplate(() =>
            // {
            //     var label = new Label
            //     {
            //         TextColor = Color.White,
            //         FontSize = 50,
            //         Text = "ALOO"
            //     };
            //
            //     return label;
            // });


            // if (_contact.MessageContainerUI != null)
            //     (_contact.MessageContainerUI as StackLayout).ChildAdded += MessagesLyt_ChildAdded;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            var unsentMessage = ComposerPlaceHolder.GetUnsentMessage();
            _contactViewItems.LastUnsentMessage = unsentMessage;
            ComposerPlaceHolder.OnDisappearing();
            RemoveUnreadedMessagesView();
            App.CurrentChatId = 0;
            ClearMFToolbarState();
            ChatPageSupport.SetCurrentContact(null);
            IsAudioSendCancelled = true;
            ResetAudioRecorderView();
            if (Device.RuntimePlatform == Device.iOS)
                DependencyService.Get<IKeyboardRegistrationService>().UnregisterForKeyboardNotifications();
            DependencyService.Get<IStatusBarColor>()
                .SetStatusbarColor(DesignResourceManager.GetColorFromStyle("Color1"));
            // if (_contact.MessageContainerUI != null)
            //     ((StackLayout)_contact.MessageContainerUI).ChildAdded -= MessagesLyt_ChildAdded;
            _stickyHeaderToken?.Cancel();

            /// <summary>
            /// Code for Video Sending
            /// </summary>
            if (App.IsVideoUploading)
            {
                if (App.ContactVideoUploading == null || App.ChatRoomVideoUploading == null)
                {
                    App.ChatRoomVideoUploading = this;
                    App.ContactVideoUploading = _contact;
                }
            }
            else
            {
                App.ChatRoomVideoUploading = null;
                App.ContactVideoUploading = null;
            }
        }

        private void InitializeComposer()
        {
            ComposerPlaceHolder.Init(OnSend, OnMediaFileSelected, OnVideoFileSelected);
            ComposerPlaceHolder.AudioPlayerEvent += AudioPlayer_Clicked;
            ComposerPlaceHolder.AudioOutputDeleteEvent += DeleteRecordingOutput;
            ComposerPlaceHolder.TextMessage.Unfocused += TextMessage_Unfocused;
            ComposerPlaceHolder.TextMessage.Focused += TextMessage_Focused;
            ComposerPlaceHolder.OnSendClick += RemoveUnreadedMessagesView;
            ComposerPlaceHolder.SizeChanged += ComposerPlaceHolder_SizeChanged;
            ComposerPlaceHolder.OnForwardClick += () => MFForward_Clicked(null, null);
            ComposerPlaceHolder.OnReplyClick += () => MFReply_Clicked(null, null);
            ComposerPlaceHolder.SetLastUnsentMessage(_contactViewItems.LastUnsentMessage);
        }

        private void ComposerPlaceHolder_SizeChanged(object sender, EventArgs e)
        {
            Constraint yconstraint = Constraint.RelativeToParent((parent) =>
            {
                return parent.Height - ComposerPlaceHolder.Height - 16 - DownScroll.Height;
            });
            RelativeLayout.SetYConstraint(DownScroll, yconstraint);
        }

        private void OnSend(ulong? replyToPostId = null, string text = null, byte[] image = null, byte[] audio = null,
            Tuple<double, double> coordinate = null, byte[] pdf = null, byte[] phoneContact = null, byte[] video = null,
            string videoType = null)
        {
            ComposerPlaceHolder.RemoveReplyState();
            var currentContact = _contact;
            if (!string.IsNullOrEmpty(text))
                NavigationTappedPage.Context.Messaging.SendText(text, currentContact, replyToPostId);
            if (image != null)
                NavigationTappedPage.Context.Messaging.SendPicture(image, currentContact, replyToPostId);
            if (audio != null)
                NavigationTappedPage.Context.Messaging.SendAudio(audio, currentContact, replyToPostId);
            if (coordinate != null)
                NavigationTappedPage.Context.Messaging.SendLocation(coordinate.Item1, coordinate.Item2, currentContact,
                    replyToPostId);
            if (pdf != null)
                NavigationTappedPage.Context.Messaging.SendPdfDocument(pdf, currentContact, replyToPostId);
            if (phoneContact != null)
                NavigationTappedPage.Context.Messaging.SendPhoneContact(phoneContact, currentContact, replyToPostId);
            if (video != null && videoType != null)
                NavigationTappedPage.Context.Messaging.ShareEncryptedContent(currentContact, videoType, video, string.Empty);
            isMessageSent = true;
        }


        private void OnMediaFileSelected(List<byte[]> images, ulong? replyToPostId = null)
        {
            var editImagePage = new EditImagePage(images);
            editImagePage.AttachPicture += async (data) =>
            {
                await Task.Run(() =>
                {
                    Parallel.ForEach(data,
                        (image, index) => { OnSend(replyToPostId: replyToPostId, image: image); });
                });
            };
            Application.Current.MainPage.Navigation.PushAsync(editImagePage, false);
        }

        private async void OnVideoFileSelected(FileResult data, bool showVideoPreview, ulong? replyToPostId = null)
        {
            if (showVideoPreview)
            {
                var videoPreviewPage = new VideoPreviewPage(data);
                videoPreviewPage.AttachVideo += async (selectedVideo) => { await UploadSelectedVideo(selectedVideo); };
                await Application.Current.MainPage.Navigation.PushAsync(videoPreviewPage, false);
            }
            else
            {
                await UploadSelectedVideo(data).ConfigureAwait(false);
            }

            // Initiate video uploading process
            async Task UploadSelectedVideo(FileResult selectedVideo)
            {
                _videoUploadTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = _videoUploadTokenSource.Token;

                OnUpdateVideoProgress(0.0);
                byte[] thumbnail = DependencyService.Get<IThumbnailService>()
                    .GenerateThumbImage(selectedVideo.FullPath, 1);
                var privateKey = await VideoManagementService
                    .UploadVideo(selectedVideo, OnUpdateVideoProgress, thumbnail, cancellationToken)
                    .ConfigureAwait(false);
                if (privateKey != null)
                {
                    OnSend(replyToPostId, video: privateKey,
                        videoType: selectedVideo.FullPath.Split('.').LastOrDefault());
                }

                OnUpdateVideoProgress(100.0);
            }

            // Update video uploading progress
            void OnUpdateVideoProgress(double progress)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (progress < 100.0 && !VideoProgress_Lyt.IsVisible)
                    {
                        App.IsVideoUploading = true;
                        ComposerPlaceHolder.IsVideoUploading = true;
                        VideoProgress_Lyt.IsVisible = true;
                    }

                    // update the label 
                    VideoProgress.Text = progress + "% Uploaded";
                    if (progress == 100.0)
                    {
                        App.IsVideoUploading = false;
                        ComposerPlaceHolder.IsVideoUploading = false;
                        VideoProgress_Lyt.IsVisible = false;
                    }
                });
            }
        }

        private void CheckContactIsBlocked()
        {
            AudioButton.IsVisible = VideoButton.IsVisible = ComposerPlaceHolder.IsVisible = !_contact.IsBlocked;
            BlockUserTxt.IsVisible = _contact.IsBlocked;
        }

        public void ResetAudioRecorderView() => ComposerPlaceHolder.ResetAudioRecorderView();

        public async void Photo_Clicked(object sender, EventArgs _)
        {
            sender.HandleButtonSingleClick();
            if (User_Photo.Content is Image)
                await PopupNavigation.Instance
                    .PushAsync(new ImageViewPopupPage((User_Photo.Content as Image).Source), true).ConfigureAwait(true);
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

        private void Back_Clicked(object sender, EventArgs e_)
        {
            sender.HandleButtonSingleClick();
            OnBackButtonPressed();
        }

        protected override bool OnBackButtonPressed()
        {
            if (PopupNavigation.Instance.PopupStack.Count > 0)
            {
                PopupNavigation.Instance.PopAsync(true);
            }
            else if (KeyboardView != null && KeyboardView.IsVisible) KeyboardView.IsVisible = false;

            else if (_contactViewItems.IsMessageSelection)
            {
                ClearMFToolbarState();
            }
            else
            {
                Navigation.PopToRootAsync(true);
            }

            return true;
        }

        private void JoinGroupCall_Clicked(object sender, EventArgs e_)
        {
            sender.HandleButtonSingleClick();
            ((App) Application.Current)?.CallManager.StartCall(_contact, _contactViewItems.IsVideoCall, true);
        }

        private void AudioCall_Clicked(object sender, EventArgs e_)
        {
            sender.HandleButtonSingleClick();
            ((App) Application.Current)?.CallManager.StartCall(_contact, false, _contactViewItems.IsCallGoingOn);
        }

        private void VideoCall_Clicked(object sender, EventArgs e_)
        {
            sender.HandleButtonSingleClick();
            ((App) Application.Current)?.CallManager.StartCall(_contact, true, _contactViewItems.IsCallGoingOn);
        }

        private async void Username_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            await Navigation.PushAsync(new ChatUserProfilePage(_contact), false).ConfigureAwait(true);
        }

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

        private void DownScroll_Clicked(object sender, EventArgs e) => ScrollToEnd();

        private double scrollContentSize;
        private View lastLoadedElement;

        // private void OnContentScrollViewScrolled(object sender, ScrolledEventArgs e)
        // {
        //     if (e.ScrollY < 60 && previousScrollYState > e.ScrollY && !_contactViewItems.IsAllMessagesLoaded &&
        //         canReadPosts)
        //     {
        //         // var view = _contact.MessageContainerUI as StackLayout;
        //         // if (view?.Children?.Count > 1) lastLoadedElement = view.Children[1];
        //         scrollContentSize = MessageContainer.ContentSize.Height;
        //         _contactViewItems.IsAllMessagesLoaded = true;
        //         canReadPosts = false;
        //         _contact.ReadPosts();
        //         new Timer((obj) => { canReadPosts = true; }, null, 500,
        //             Timeout.Infinite); // wait for to add the new message.                                     
        //     }
        //
        //     if (MessagesLyt.Height != 0 &&
        //         e.ScrollY + MessagesLyt.Height + DISABLE_SCROLLING_DIFF < MessageContainer.ContentSize.Height &&
        //         !DownScroll.IsVisible)
        //     {
        //         if (DownScroll.Source == null)
        //             DownScroll.Source = DesignResourceManager.GetImageSource("ic_scroll_down.png");
        //         DownScroll.IsVisible = true;
        //     }
        //
        //     else if (e.ScrollY + MessagesLyt.Height + DISABLE_SCROLLING_DIFF >= MessageContainer.ContentSize.Height &&
        //              DownScroll.IsVisible)
        //     {
        //         DownScroll.IsVisible = false;
        //         NewMessageScrollInfo.IsVisible = false;
        //         _downIconUnreadedMessagecount = 0;
        //     }
        //
        //
        //     if (e.ScrollY > 30 && e.ScrollY + MessagesLyt.Height < MessageContainer.ContentSize.Height)
        //     {
        //         dateFrame.IsVisible = true;
        //         dateFrame.Opacity = 1;
        //         HideStickyDateHeader();
        //     }
        //     else
        //     {
        //         dateFrame.IsVisible = false;
        //     }
        //
        //
        //     if (e.ScrollY + MessagesLyt.Height < MessageContainer.ContentSize.Height &&
        //         Math.Abs(lastScrollY - e.ScrollY) > 10)
        //     {
        //         lastScrollY = e.ScrollY;
        //         double y = 0;
        //
        //         // if (!(_contact?.MessageContainerUI is StackLayout messageLayout)) return;
        //         // double desiredY;
        //         //
        //         // if (e.ScrollY < messageLayout.Height / 2)
        //         // {
        //         //     desiredY = e.ScrollY;
        //         //
        //         //     string lastLoadedDateHeader = "Not Found";
        //         //     foreach (var itemView in messageLayout.Children)
        //         //     {
        //         //         y += itemView.Height + 8;
        //         //         var tag = XamarinShared.ViewCreator.Utils.GetTag(itemView);
        //         //
        //         //         if (tag.StartsWith("MessageDateFrameContainer"))
        //         //         {
        //         //             lastLoadedDateHeader = tag.Split('@')[1];
        //         //         }
        //         //
        //         //         if (y >= desiredY) break;
        //         //     }
        //         //
        //         //     ((Label)dateFrame.Content).Text = lastLoadedDateHeader;
        //         // }
        //         // else
        //         // {
        //         //     desiredY = messageLayout.Height - e.ScrollY;
        //         //
        //         //     for (var i = messageLayout.Children.Count - 1; i >= 0; i--)
        //         //     {
        //         //         var itemView = messageLayout.Children[i];
        //         //         y += itemView.Height + 8;
        //         //
        //         //         var tag = XamarinShared.ViewCreator.Utils.GetTag(itemView);
        //         //
        //         //         if (!(y >= desiredY) || !tag.StartsWith("MessageDateFrameContainer")) continue;
        //         //         ((Label)dateFrame.Content).Text = tag.Split('@')[1];
        //         //         break;
        //         //     }
        //         // }
        //     }
        //
        //     previousScrollYState = e.ScrollY;
        // }

        private void TextMessage_Focused(object _, FocusEventArgs f)
        {
            // if (MessageContainer.Content != null &&
            //     (MessageContainer.ScrollY + Application.Current.MainPage.Height + 100 >
            //      MessageContainer.ContentSize.Height))
            // {
            //     if (Device.RuntimePlatform == Device.iOS)
            //         ScrollToEnd();
            //     else
            //     {
            //         var c = new CancellationToken();
            //         var timer = new Timer((obj) => { ScrollToEnd(false); }, null, 200,
            //             Timeout.Infinite);
            //     }
            // }
        }

        private void MessagesLyt_ChildAdded(object sender, ElementEventArgs e)
        {
            // var index = (_contact.MessageContainerUI as StackLayout)?.Children?.IndexOf(e.Element as View);
            // if (index > 1)
            // {
            // lastLoadedElement = null;
            // scrollContentSize = 0;
            // }

            if (scrollContentSize != 0 && lastLoadedElement != null)
            {
                // MessageContainer.ScrollToAsync(lastLoadedElement, ScrollToPosition.Start, false);

                //  Device.BeginInvokeOnMainThread(() =>
                // MessageContainer.ScrollToAsync(0, MessageContainer.ContentSize.Height - scrollContentSize, false));
                //new Timer((obj) =>
                //{
                //Device.BeginInvokeOnMainThread(() =>
                //{ MessageContainer.ScrollToAsync(0, MessageContainer.ContentSize.Height - scrollContentSize, false); });

                //null, 100, Timeout.Infinite);  // wait for to add the new message.
            }
            // else if (MessageContainer.Content != null && ((isMessageSent && MessageContainer.ScrollY == 0) ||
            //                                               (MessageContainer.ScrollY +
            //                                                Application.Current.MainPage.Height + 350 >
            //                                                MessageContainer.ContentSize.Height) && canReadPosts))
            // {
            //     new Timer((obj) => { ScrollToEnd(false); }, null, 100,
            //         Timeout.Infinite); // wait for to add the new message.
            // }

            else
            {
                // if (DownScroll.IsVisible && index > 1)
                // {
                //     NewMessageScrollInfo.IsVisible = true;
                //     _downIconUnreadedMessagecount++;
                //     NewMessageScrollInfo.Text = _downIconUnreadedMessagecount.ToString();
                // }
            }

            isMessageSent = false;
        }

        public void ScrollToEnd(bool animate = false)
        {
            // if (MessageContainer == null || MessageContainer.Content == null) // chatroom hot reload
            //     return;
            // try
            // {
            //     {
            //         Device.BeginInvokeOnMainThread(() =>
            //         {
            //             if (Device.RuntimePlatform == Device.iOS)
            //             {
            //                 var lastElement = (MessageContainer.Content as StackLayout).Children.Count > 0
            //                     ? (MessageContainer.Content as StackLayout).Children[
            //                         (MessageContainer.Content as StackLayout).Children.Count - 1]
            //                     : MessageContainer.Content;
            //                 MessageContainer.ScrollToAsync(lastElement, ScrollToPosition.End, animate);
            //             }
            //             else
            //
            //                 MessageContainer.ScrollToAsync(0, MessageContainer.ContentSize.Height, animate);
            //         });
            //     }
            // }
            // catch (Exception e)
            // {
            //     //prevent element does not belong to this ScrollView Parameter name: element
            // }
        }

        private void ScrollPageToFirstUnreadedMessage()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                // if (MessageContainer.Content is StackLayout layout)
                // {
                //     var child = ChatPageSupport.CheckContainterContainsUnreadedMessageView(layout.Children);
                //     if (child != null)
                //     {
                //         if (Device.RuntimePlatform == Device.iOS)
                //         {
                //             MessageContainer.ScrollToAsync(child, ScrollToPosition.End, false);
                //             MessageContainer.ScrollToAsync(0, MessageContainer.ScrollY + 50, false);
                //         }
                //         else
                //             MessageContainer.ScrollToAsync(child, ScrollToPosition.Start, false);
                //     }
                //     else ScrollToEnd();
                // }
            });
        }

        public void ClearMFToolbarState()
        {
            if (_contactViewItems.SelectedMessagesList.Count !=
                0) // prevent loop for collection change SelectedMessages_CollectionChanged
                _contactViewItems.SelectedMessagesList.Clear();
            _contactViewItems.IsMessageSelection = false;
        }

        private void MFCancel_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick(500);
            ClearMFToolbarState();
        }

        private void MFForward_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            Navigation.PushAsync(new GroupUserSelectPage(GetSelectedMessages()), false);
            ClearMFToolbarState();
        }

        private void MFReply_Clicked(object sender, EventArgs e)
        {
            List<Message> messages = GetSelectedMessages();
            if (messages.Count != 1) return;
            ClearMFToolbarState();
            ComposerPlaceHolder.LoadReplyMessage(messages[0]);
        }

        private List<Message> GetSelectedMessages()
        {
            return selectedMessages.Select(data => data.Item1).ToList();
        }

        //in case of encrypted will not work
        private void MFCopy_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick(500);
            var myId = NavigationTappedPage.Context.My.GetId();
            StringBuilder sb = new StringBuilder();
            List<Message> selectedMessages = GetSelectedMessages();

            if (selectedMessages.Count == 1)
            {
                if (selectedMessages[0].GetData() != null && selectedMessages[0].Type == MessageFormat.MessageType.Text)
                    sb.Append(Encoding.Unicode.GetString(selectedMessages[0].GetData()));
            }
            else
            {
                foreach (var m in selectedMessages)
                {
                    if (m?.GetData() == null) continue;
                    var username = myId == m.AuthorId ? NavigationTappedPage.Context.My.Name : m.AuthorName();

                    sb.Append(username);
                    sb.Append("[" + m.Creation.ToLocalTime().ToString() + "]: ");
                    sb.Append(Encoding.Unicode.GetString(m.GetData()));
                    sb.AppendLine("");
                }
            }

            Clipboard.SetTextAsync(sb.ToString());
            this.DisplayToastAsync(Localization.Resources.Dictionary.CopiedToClipboard);
            ClearMFToolbarState();
        }

        private void MFDelete_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            List<Message> messages = GetSelectedMessages();
            var isContainsOppenentMessage =
                messages.Find(m => NavigationTappedPage.Context.My.GetId() != m.AuthorId) != null;
            PopupNavigation.Instance.PushAsync(new DeleteMessagePopupPage(messages, !isContainsOppenentMessage), true);
            ClearMFToolbarState();
        }

        private void MFTranslate_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();

            if (selectedMessages.Count != 1)
                return;
            var (message, textLabel) = selectedMessages[0];

            if (message.Type != MessageFormat.MessageType.Text || textLabel == null)
                return;

            var tag = XamarinShared.ViewCreator.Utils.GetTag(textLabel);
            if (!tag.Equals("translated"))
                message.Translate(
                    () => { XamarinShared.ViewCreator.MessageViewCreator.UpdateDisplayedText(message, textLabel); },
                    true);
            else
            {
                //todo need to add localization
                Application.Current?.MainPage?.DisplayToastAsync(@"Message already translated");
            }

            ClearMFToolbarState();
        }

        private void AudioPlayer_Clicked()
        {
            if (_audioPlayer.IsPlaying)
            {
                StopAudio();
                return;
            }

            PlayAudio(DependencyService.Get<IAudioRecorder>().GetOutput());
            _audioPlayer.PlaybackEnded += (s, args) => { StopAudio(); };
        }

        private void DeleteRecordingOutput()
        {
            if (_audioPlayer != null && _audioPlayer.IsPlaying)
                _audioPlayer.Stop();
            DependencyService.Get<IAudioRecorder>().DeleteOutput();
        }

        private void PlayAudio(byte[] data)
        {
            var ms = new MemoryStream(data);
            _audioPlayer?.Load(ms);
            _audioPlayer?.Play();
            ComposerPlaceHolder?.PlayAudio();
        }

        private void StopAudio()
        {
            ComposerPlaceHolder?.StopAudio();
            _audioPlayer?.Stop();
        }

        private void OnVideoCancel_Btn_Clicked(object _, EventArgs e_)
        {
            _videoUploadTokenSource.Cancel();
            ComposerPlaceHolder.IsVideoUploading = false;
            VideoProgress_Lyt.IsVisible = false;
            App.IsVideoUploading = false;
        }
    }
}