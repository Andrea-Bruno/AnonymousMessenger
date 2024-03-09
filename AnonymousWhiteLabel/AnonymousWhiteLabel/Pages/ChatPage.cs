using System.IO;
using System;
using Xamarin.Forms;
using System.Linq;
using EncryptedMessaging;
using MessageCompose;
using XamarinShared;
using XamarinShared.ViewCreator;
using Xamarin.Essentials;
using System.Collections.Generic;
using AnonymousWhiteLabel.Views;
using EncryptedMessaging.Resources;

namespace AnonymousWhiteLabel.Pages
{
    internal class ChatPage : ContentPage
    {
        private IAudioPlayer Player;
        //public static bool IsVideoUploading;
        private Composer _composer;
        private ActivityIndicator activityIndicator;


        public ChatPage()
        {
            activityIndicator = new ActivityIndicator { IsRunning = true, IsVisible = false };
            Disappearing += (m, n) =>
            {
                if ((!Navigation.NavigationStack.Take(Navigation.NavigationStack.Count - 2).Contains(this))
                && (Device.RuntimePlatform == Device.UWP)) // Change the current contact only if you go back on the navigation stack
                    App.Context.Messaging.CurrentChatRoom = null;
                //Navigation.PopAsync();
            };
            InitToolbar();
            Appearing += (m, n) =>
            {
                //MessageContainer.ScrollToAsync(0, MessageContainer., true);
                if (App.Context.Messaging.CurrentChatRoom == null)
                    Navigation.PopAsync(); // Automatic go back when delete the contact in ContactPage
            };
            var editMessage = new Views.EditMessage(MessageContainer);
            editMessage.VerticalOptions = LayoutOptions.FillAndExpand;
            var stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children = { activityIndicator, editMessage }
            };
            _composer = editMessage.Composer;
            Content = stackLayout;
            Player = DependencyService.Get<IAudioPlayer>();
            InitializeComposer();
        }

        private void InitToolbar()
        {
            var edit = new ToolbarItem
            {
                Text = Dictionary.Edit
            };
            edit.Command = new Command(() => Navigation.PushAsync(new ContactPage(App.Context.Messaging.CurrentChatRoom)));
            
            ToolbarItems.Add(edit);
        }

        private void InitializeComposer()
        {
            _composer.Init(onSend: OnSend, onMediaFileSelected: OnMediaFileSelected, videoFileSelectEvent: OnVideoFileSelected);//, (progress =>
            //{
            //    if (progress < 100.0 && !activityIndicator.IsVisible)
            //    {
            //        activityIndicator.IsVisible = true;

            //        IsVideoUploading = true;
            //    }

            //    if (progress == 100.0)
            //    {
            //        IsVideoUploading = false;
            //        activityIndicator.IsVisible = false;
            //    }
            //}));
            _composer.AudioPlayerEvent += AudioPlayer_Clicked;
            _composer.OnSendClick += RemoveUnreadedMessagesView;
            _composer.AudioOutputDeleteEvent += DeleteRecording;

            //ComposerPlaceHolder.TextMessage.Unfocused += TextMessage_Unfocused;
            //ComposerPlaceHolder.TextMessage.Focused += TextMessage_Focused;
            //ComposerPlaceHolder.OnSendClick += RemoveUnreadedMessagesView;
            //ComposerPlaceHolder.SizeChanged += ComposerPlaceHolder_SizeChanged;
            //ComposerPlaceHolder.OnForwardClick += () => MFForward_Clicked(null, null);
            //ComposerPlaceHolder.OnReplyClick += () => MFReply_Clicked(null, null);
            //ComposerPlaceHolder.SetLastUnsentMessage(_contactViewItems.LastUnsentMessage);
           // _composer.IsVideoUploading = ChatPage.IsVideoUploading;
        }

        private void AudioPlayer_Clicked()
        {
            if (Player == null) return;
            if (Player.IsPlaying)
            {
                StopAudio();
                return;
            }

            PlayAudio(DependencyService.Get<MessageCompose.Services.IAudioRecorder>().GetOutput());
            Player.PlaybackEnded += (s, args) => StopAudio();
        }

        private void PlayAudio(byte[] data)
        {
            var ms = new MemoryStream(data);
            Player?.Load(ms);
            Player?.Play();
            _composer?.PlayAudio();
        }

        private void StopAudio()
        {
            _composer?.StopAudio();
            Player?.Stop();
        }

        private void DeleteRecording()
        {
            Player?.Stop();
            DependencyService.Get<MessageCompose.Services.IAudioRecorder>()?.DeleteOutput();
        }


        private void RemoveUnreadedMessagesView()
        {
            if (!isUnreadedMessagesViewRemoved)
                ChatPageSupport.RemoveUnreadedMessageView();
            isUnreadedMessagesViewRemoved = true;
        }

        private async void OnSend(ulong? replyToPostId = null, string text = null, byte[] image = null, byte[] audio = null,
            Tuple<double, double> coordinate = null, byte[] pdf = null, byte[] phoneContact = null, byte[] video = null,
            string videoType = null)
        {
            _composer.RemoveReplyState();
            var currentContact = App.Context.Messaging.CurrentChatRoom;
            if (!string.IsNullOrEmpty(text))
                App.Context.Messaging.SendText(text, currentContact, replyToPostId);
            if (image != null)
                App.Context.Messaging.SendPicture(image, currentContact, replyToPostId);
            if (audio != null)
                App.Context.Messaging.SendAudio(audio, currentContact, replyToPostId);
            if (coordinate != null)
                App.Context.Messaging.SendLocation(coordinate.Item1, coordinate.Item2, currentContact, replyToPostId);
            if (pdf != null)
                App.Context.Messaging.SendPdfDocument(pdf, currentContact, replyToPostId);
            if (phoneContact != null)
                App.Context.Messaging.SendPhoneContact(phoneContact, currentContact, replyToPostId);
            //if (video != null && videoType != null)
            //    App.Context.Messaging.ShareEncryptedContent(currentContact, videoType, video,
            //        string.Empty, replyToPostId: replyToPostId);
        }

        private void OnMediaFileSelected(List< byte[]> images, ulong? replyToPostId)
        {
            foreach ( var image in images)
            {
                OnSend(replyToPostId, image: image);
            }
            // Navigation.NavigationStack.ElementAt(1);
            //var editImagePage = new EditImagePage(image);
            //editImagePage.AttachPicture += (data) => OnSend(image: data);
            //Application.Current.MainPage.Navigation.PushAsync(editImagePage, false);
        }

        private async void OnVideoFileSelected(FileResult data, bool showVideoPreview, ulong? replyToPostId = null)
        {
            //if (showVideoPreview)
            //{
            //    var videoPreviewPage = new VideoPreviewPage(data);
            //    videoPreviewPage.AttachVideo += async (selectedVideo) => { await UploadSelectedVideo(selectedVideo); };
            //    await Application.Current.MainPage.Navigation.PushAsync(videoPreviewPage, false);
            //}
            //else
            //{
            //    await UploadSelectedVideo(data).ConfigureAwait(false);
            //}

            //// Initiate video uploading process
            //async Task UploadSelectedVideo(FileResult selectedVideo)
            //{
            //    _videoUploadTokenSource = new CancellationTokenSource();
            //    CancellationToken cancellationToken = _videoUploadTokenSource.Token;

            //    OnUpdateVideoProgress(0.0);
            //    byte[] thumbnail = DependencyService.Get<IThumbnailService>().GenerateThumbImage(selectedVideo.FullPath, 1);
            //    var privateKey = await VideoManagementService.UploadVideo(selectedVideo, OnUpdateVideoProgress, thumbnail, cancellationToken).ConfigureAwait(false);
            //    if (privateKey != null)
            //    {
            //        OnSend(replyToPostId, video: privateKey, videoType: selectedVideo.FullPath.Split('.').LastOrDefault());
            //    }
            //    OnUpdateVideoProgress(100.0);
            //}

            //// Update video uploading progress
            //void OnUpdateVideoProgress(double progress)
            //{
            //    MainThread.BeginInvokeOnMainThread(() =>
            //    {
            //        if (progress < 100.0 && !VideoProgress_Lyt.IsVisible)
            //        {
            //            App.IsVideoUploading = true;
            //            ComposerPlaceHolder.IsVideoUploading = true;
            //            VideoProgress_Lyt.IsVisible = true;
            //        }

            //        // update the label 
            //        VideoProgress.Text = progress + "% Uploaded";
            //        if (progress == 100.0)
            //        {
            //            App.IsVideoUploading = false;
            //            ComposerPlaceHolder.IsVideoUploading = false;
            //            VideoProgress_Lyt.IsVisible = false;
            //        }
            //    });
            //}
        }


        public static readonly ScrollView MessageContainer = new ScrollView();
        private bool isUnreadedMessagesViewRemoved;

        public void SetCurrentContact(EncryptedMessaging.Contact contact)
        {
            Title = contact?.Name;
            //App.Context.Messaging.CurrentChatRoom = contact;
            XamarinShared.ChatPageSupport.SetCurrentContact(contact);
        }
        //        public static void OnViewMessage(Message message, bool isMyMessage, out View content)
        //        {
        //            Contact contact = message.Contact;
        //            var paddingLeft = 5; var paddingRight = 5;
        //            Color background;
        //            if (isMyMessage)
        //            {
        //                paddingLeft = 20;
        //                background = Template.BackgroundMyMessage;
        //            }
        //            else
        //            {
        //                background = Template.BackgroundMessage;
        //                paddingRight = 20;
        //            }
        //            var frame = new Frame { CornerRadius = 10, BackgroundColor = background, Padding = new Thickness(paddingLeft, 5, paddingRight, 5), Margin = 5 };
        //            frame.HasShadow = true;
        //            var box = new StackLayout();
        //            frame.Content = box;
        //            TextAlignment align = isMyMessage ? TextAlignment.End : TextAlignment.Start;
        //            if (!isMyMessage && contact?.Participants.Count > 2)
        //            {
        //                var authorLabel = new Label() { Text = message.AuthorName() + ":" };
        //                authorLabel.HorizontalTextAlignment = align;
        //                box.Children.Add(authorLabel);
        //            }
        //            switch (message.Type)
        //            {
        //                case MessageFormat.MessageType.Text:
        //                    var textMessage = new Label { Text = System.Text.Encoding.Unicode.GetString(message.GetData()) };
        //                    textMessage.HorizontalTextAlignment = align;
        //                    box.Children.Add(textMessage);
        //                    break;
        //                case MessageFormat.MessageType.Image:
        //                    var image = new Image { Source = ImageSource.FromStream(() => new MemoryStream(message.GetData())), Aspect = Aspect.AspectFill };
        //                    box.Children.Add(image);
        //                    break;
        //                case MessageFormat.MessageType.Audio:
        //                    break;
        //                default:
        //                    break;
        //            }
        //            var timeLabel = new Label();
        //            DateTime messageLocalTime = message.Creation.ToLocalTime();
        //            TimeSpan difference = DateTime.Now - messageLocalTime;
        //            timeLabel.Text = difference.TotalDays < 1 ? messageLocalTime.ToLongTimeString() : messageLocalTime.ToLongDateString() + " - " + messageLocalTime.ToLongTimeString();
        //            timeLabel.HorizontalTextAlignment = align;
        //#if !WINDOWS_UWP
        //            //In windows this throws an error because it is outside the GUI thread
        //            try
        //            { //For an alleged bug of the current version of VS the compiler does not exclude the project UWP
        //                if (Device.RuntimePlatform != Device.UWP) //Here the WINDOWS_UWP symbol cannot work because we are in the common library
        //                {
        //                    timeLabel.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
        //                }
        //            }
        //            catch (Exception) { }
        //#endif

        //            var flagAndTime = new StackLayout() { Orientation = StackOrientation.Horizontal };
        //            var statusLabel = new Label() { Text = Status.pending.ToString() };
        //            flagAndTime.Children.Add(timeLabel);

        //            lock (_statusContacts)
        //            {
        //                if (!_statusContacts.TryGetValue(contact, out List<Tuple<DateTime, Label>> flags))
        //                {
        //                    flags = new List<Tuple<DateTime, Label>>();
        //                    _statusContacts.Add(contact, flags);
        //                }
        //                flags.Add(Tuple.Create(message.Creation, statusLabel));
        //            }
        //            box.Children.Add(flagAndTime);
        //            content = frame;
        //        }
        //        private enum Status
        //        {
        //            pending = '☐',
        //            delivered = '☑',
        //            readed = '✅',
        //        }
        //        private static Dictionary<Contact, List<Tuple<DateTime, Label>>> _statusContacts = new Dictionary<Contact, List<Tuple<DateTime, Label>>>();

        internal struct Template
        {
            public static Color BackgroundMessage = Color.FromRgb(0xb7, 0xcb, 0xf2);
            public static Color BackgroundMyMessage = Color.FromRgb(0xe2, 0xe8, 0xf3);
        }
    }
}