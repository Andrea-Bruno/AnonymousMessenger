using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EncryptedMessaging;
using XamarinShared.ViewCreator.Views.Incoming;
using XamarinShared.ViewCreator.Views.Outgoing;
using Xamarin.Forms;
using static EncryptedMessaging.MessageFormat;
using static XamarinShared.ViewCreator.FontSizeConverter;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Security.Cryptography;
using VideoFileCryptographyLibrary;
using System.Linq;
using Xamarin.Forms.Controls;

namespace XamarinShared.ViewCreator
{
    public class MessageViewCreator : INotifyPropertyChanged // for font binding
    {
        public float TextSize
        {
            get { return DefaultSelectedFontRatio; }
            set { OnPropertyChanged(nameof(TextSize)); }
        }


        private readonly double _AUDIO_SOUND_UPDATE_TIMER_DELAY = 0.05d;
        public static bool IsAudioPlaying = false;
        public static int SelectedMessageHashCode = 0;
        private static bool _isAppInSleepMode;

        public delegate void ImageMessageClickedEvent(byte[] data);

        private ImageMessageClickedEvent _onImageMessageClicked;

        public delegate void VideoMessageClickedEvent(string videoPath);

        private VideoMessageClickedEvent _onVideoMessageClicked;

        public delegate void LocationMessageClickedEvent(double lat, double lng);

        private LocationMessageClickedEvent _onLocationMessageClicked;

        public delegate void PdfMessageClickedEvent(byte[] data);

        private PdfMessageClickedEvent _onPdfMessageClicked;

        public delegate Tuple<string, string> ContactDetailsProvider(byte[] data);

        private ContactDetailsProvider _contactDetailsProvider;

        public delegate Tuple<byte[], string> FileDetailsProvider(byte[] data);

        private FileDetailsProvider _fileDetailsProvider;

        public enum RequiredComposerState
        {
            AudioRecord,
            AudioSend
        }

        public delegate bool ComposerStateProvider(RequiredComposerState requiredState, int newValue);

        private ComposerStateProvider _composerStateProvider;

        public delegate void TranslateService(Message message, Action onTranslationSuccess);

        private TranslateService _translateService;

        public delegate void MessageSwiped(Message message);

        private MessageSwiped _messageSwiped;

        public delegate void ContactAddedEvent(Contact contact);

        private ContactAddedEvent _onContactAdded;

        public delegate void FinishCallEvent(Message message);

        private FinishCallEvent _finishCallEvent;

        public delegate void ShowToast(string message);

        private ShowToast _showToast;

        public delegate void MessageInfoClickedEvent(List<Contact> contacts);

        private MessageInfoClickedEvent _onMessageInfoClicked;

        public delegate void ListenProximitySensorEvent(IAudioPlayer CurrentAudioPlayer);

        private ListenProximitySensorEvent listenProximitySensorEvent;

        private Action DisableProximitySensorEvent;

        internal static MessageReadStatus ReadStatus;

        private Func<Contact, MessageType, Task> _sendNotification;

        private FontSizeConverter _fontSizeConverter;

        private static MessageViewCreator _Instance;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static MessageViewCreator Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new MessageViewCreator();
                return _Instance;
            }
        }

        private MessageViewCreator()
        {
            _fontSizeConverter = new FontSizeConverter();
        }

        public void Setup(
            Func<Contact, MessageType, Task> sendNotification = null,
            ImageMessageClickedEvent imageMessageClicked = null,
            VideoMessageClickedEvent videoMessageClicked = null,
            LocationMessageClickedEvent locationMessageClicked = null,
            PdfMessageClickedEvent pdfMessageClicked = null,
            ContactAddedEvent contactAdded = null,
            MessageInfoClickedEvent messageInfoClicked = null,
            ListenProximitySensorEvent listenProximitySensor = null,
            Action disableProximitySensor = null,
            ShowToast showToast = null,
            TranslateService translateService = null,
            ComposerStateProvider composerStateProvider = null,
            ContactDetailsProvider contactDetailsProvider = null,
            FileDetailsProvider fileDetailsProvider = null,
            FinishCallEvent finishCallEvent = null
        )
        {
            _fileDetailsProvider = fileDetailsProvider;
            _contactDetailsProvider = contactDetailsProvider;
            _sendNotification = sendNotification;
            _onImageMessageClicked = imageMessageClicked;
            _onVideoMessageClicked = videoMessageClicked;
            _onLocationMessageClicked = locationMessageClicked;
            _onPdfMessageClicked = pdfMessageClicked;
            _onContactAdded = contactAdded;
            _onMessageInfoClicked = messageInfoClicked;
            listenProximitySensorEvent = listenProximitySensor;
            DisableProximitySensorEvent = disableProximitySensor;
            _showToast = showToast;
            _translateService = translateService;
            _composerStateProvider = composerStateProvider;
            _finishCallEvent = finishCallEvent;
        }

        public void OnMessageSwiped(MessageSwiped messageSwiped)
        {
            _messageSwiped = messageSwiped;
        }

        public void SetReadStatus(MessageReadStatus messageReadStatus)
        {
            ReadStatus = messageReadStatus;
        }

        public void OnViewMessage(Message message, bool isMyMessage, out View content)
        {
            content = null;
            var contact = message.Contact;
            CustomLinkLabel textLabel = null;
            ChatPageSupport.GetContactViewItems(contact, out var contactViewItems);

            BaseView baseView;
            if (isMyMessage)
                baseView = new OutgoingBaseView(message, readStatus: ReadStatus, _sendNotification);
            else
                baseView = new IncomingBaseView(message, readStatus: ReadStatus, _sendNotification);


            baseView.SetTag(message.PostId.ToString());

            contactViewItems.IsMessageSelectionObs += (isOpen) =>
            {
                if (!isOpen)
                {
                    baseView.SelectionCheckBox.IsChecked = false;
                }

                baseView.IsEnabled = !isOpen && baseView.IsSwipeEnabled;
            };

            baseView.SelectionCheckBox.CheckedChanged += (sender, args) =>
            {
                if (baseView.SelectionCheckBox.IsChecked)
                {
                    contactViewItems.SelectedMessagesList.Add(new Tuple<Message, Label>(message, textLabel));
                }
                else
                {
                    foreach (var data in contactViewItems.SelectedMessagesList)
                    {
                        if (data.Item1.PostId == message.PostId)
                        {
                            contactViewItems.SelectedMessagesList.Remove(data);
                            break;
                        }
                    }
                }
            };

            if (message.ReplyToPostId != null)
            {
                baseView.MessageFrameContent.SetTag(message.ReplyToPostId + "");
            }


            switch (message.Type)
            {
                case MessageType.ReplyToMessage:
                case MessageType.Text:
                    SetTextLayout(isMyMessage, message, baseView.MessageFrameContent, out textLabel);
                    break;

                case MessageType.Image:
                    var imageSource = ImageSource.FromStream(() => new MemoryStream(message.GetData()));
                    var image = AddImageLayout(imageSource, baseView);
                    image.Click(((s, e) => _onImageMessageClicked?.Invoke(message.GetData())));
                    break;

                case MessageType.Location:
                    var data = message.GetData();
                    var latitude = BitConverter.ToDouble(data, 0);
                    var longitude = BitConverter.ToDouble(data, 8);
                    var locationImageSource = new UriImageSource
                    {
                        CacheValidity = TimeSpan.FromDays(7),
                        CachingEnabled = true,
                        Uri = new Uri(Utils.GetAddressPreviewImageUrl(latitude, longitude))
                    };
                    var locationImage = AddImageLayout(locationImageSource, baseView);
                    locationImage.Click((s, e) => _onLocationMessageClicked?.Invoke(latitude, longitude));
                    break;

                case MessageType.Audio:
                    var view = GetAudioLayout(baseView, message, isMyMessage);
                    if (view != null)
                        baseView.MessageFrameContent.Children.Add(view);
                    break;

                case MessageType.AudioCall:
                case MessageType.VideoCall:
                case MessageType.EndCall:
                case MessageType.DeclinedCall:
                case MessageType.StartAudioGroupCall:
                case MessageType.StartVideoGroupCall:
                    addCallView(
                        message.Type == MessageType.VideoCall || message.Type == MessageType.StartVideoGroupCall);
                    break;

                case MessageType.PdfDocument:
                    addPdfDocument();
                    break;

                case MessageType.Contact:

                    baseView.IsSwipeEnabled = false; // disable reply 
                    if (message.Contact.IsGroup) addNewGroup();
                    else addNewContact();
                    _onContactAdded?.Invoke(message.Contact);
                    break;

                case MessageType.PhoneContact:
                    addPhoneContactView();
                    break;

                case MessageType.ShareEncryptedContent:
                    baseView.MessageFrame.HorizontalOptions = isMyMessage ? LayoutOptions.End : LayoutOptions.Start;
                    baseView.MessageFrameContent.Padding = new Thickness(0);
                    // no need , baseview  has own core ui parts.
                    // var videoFrame = new StackLayout();  
                    // baseView.MessageFrameContent.Children
                    //     .Add(videoFrame); // it might take to create video layout, so add it first to view

                    Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var contentType = string.Empty;
                        byte[] privateKey = null;
                        var description = string.Empty;
                        var serverUrl = string.Empty;
                        message.GetShareEncryptedContentData(out contentType, out privateKey, out description,
                            out serverUrl);

                        var videoLayout = GetVideoLayout();
                        var isReplyLayoutLoaded = false;
                        if (message.ReplyToPostId != null)
                        {
                            isReplyLayoutLoaded =
                                baseView.MessageFrameContent.Children[0].GetTag().Equals("ReplyLayout");
                        }

                        baseView.MessageFrameContent.Children.Insert(isReplyLayoutLoaded ? 1 : 0, videoLayout.Item1);
                        var thumbnailImage = videoLayout.Item3;
                        Task.Run(() => SetThumbnail(privateKey, thumbnailImage));

                        if (isMyMessage)
                        {
                            // Local file processing
                            var folder = Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "uploads");
                            if (!Directory.Exists(folder))
                            {
                                Directory.CreateDirectory(folder);
                            }

                            var sha256 = SHA256.Create();
                            var fileName = string.Format("{0}.{1}", ComputePsuedoHash.ToHex(sha256.ComputeHash(privateKey)),
                                contentType);
                            var filePath = Path.Combine(folder, fileName);

                            // Message view rendering
                            videoLayout.Item2.IsVisible = false;
                            videoLayout.Item4.IsVisible = true;
                            videoLayout.Item4.Source = Icons.AudioPlayerState(true); // should be play icon
                            //dont add click to entire view
                            // baseView.MessageFrameContent.Tapped += (s, e) => _onVideoMessageClicked?.Invoke(filePath);
                            videoLayout.Item1.Click((s, e) =>
                                //videoLayout.Item1.HandleButtonSingleClick(2000);
                                _onVideoMessageClicked?.Invoke(filePath));

                        }
                        else
                        {
                            videoLayout.Item2.IsVisible = false;
                            videoLayout.Item4.IsVisible = true;
                            var filePath = XamarinShared.Setup.GetSecureValue("video_" + message.PostId);

                            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                            {
                                videoLayout.Item4.Source = Icons.VideoDownload; // should be download icon
                                //dont add click to entire view
                                // baseView.MessageFrameContent.Tapped += (s, e) 
                                videoLayout.Item1.Click((s, e) =>
                                {
                                    if (videoLayout.Item2.IsVisible) return;

                                    var fPath = XamarinShared.Setup.GetSecureValue("video_" + message.PostId);
                                    if (string.IsNullOrWhiteSpace(fPath) || !File.Exists(fPath))
                                    {
                                        new Thread(new ThreadStart(videoDownloadProcess)).Start();
                                    }
                                    else
                                    {
                                        _onVideoMessageClicked?.Invoke(fPath);
                                    }
                                });
                            }
                            else
                            {
                                videoLayout.Item4.Source = Icons.AudioPlayerState(true); // should be play icon

                                //dont add click to entire view
                                // baseView.MessageFrameContent.Tapped +=
                                //     (s, e) => _onVideoMessageClicked?.Invoke(filePath);
                                videoLayout.Item1.Click((s, e) => _onVideoMessageClicked?.Invoke(filePath));
                            }

                            void videoDownloadProcess()
                            {
                                ProgressChange(videoLayout.Item2, 0.0, videoLayout.Item4);
                                var newfilePath = DownloadVideo(privateKey, videoLayout.Item2, ProgressChange,
                                    videoLayout.Item4).Result;
                                ProgressChange(videoLayout.Item2, 100.0, videoLayout.Item4);
                                if (!string.IsNullOrWhiteSpace(newfilePath))
                                {
                                    XamarinShared.Setup.SetSecureValue("video_" + message.PostId, newfilePath);
                                }
                            }
                        }
                    });
                    break;
            }

            void addPhoneContactView()
            {
                if (_contactDetailsProvider == null) return;
                var contactDetails = _contactDetailsProvider(message.GetData());


                var profileImage = new Image
                {
                    Source = Icons.PhoneContact,
                    WidthRequest = 50,
                    HeightRequest = 50,
                };

                var newContactShared = new Label
                {
                    Text = Localization.Resources.Dictionary.ContactShared,
                    TextColor = Palette.MainTextColor(isOutgoing: isMyMessage),
                    FontFamily = "PoppinsLight",
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    VerticalOptions = LayoutOptions.StartAndExpand,
                };

                var newContactLabel = new Label
                {
                    Text = contactDetails.Item1,
                    TextColor = Palette.MainTextColor(isOutgoing: isMyMessage),
                    FontFamily = "PoppinsSemiBold",
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    VerticalOptions = LayoutOptions.StartAndExpand,
                };

                var bottomLabel = new Label
                {
                    Text = Localization.Resources.Dictionary.CallThisNumber,
                    TextColor = Palette.PhoneContactBottomTextColor,
                    FontFamily = "PoppinsSemiBold",
                    Padding = new Thickness(0, 5),
                    BackgroundColor = Palette.PhoneContactBottomBackgroundColor,
                    HorizontalTextAlignment = TextAlignment.Center,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.End,
                };

                newContactLabel.SetFontSizeBinding(16);
                //newContactLabel.BindingContext = this;
                //newContactLabel.SetBinding(Label.FontSizeProperty, new Binding(nameof(TextSize), converter: _fontSizeConverter, converterParameter: 16));
                newContactLabel.SetFontSizeBinding(12);
                //newContactShared.BindingContext = this;
                //newContactShared.SetBinding(Label.FontSizeProperty, new Binding(nameof(TextSize), converter: _fontSizeConverter, converterParameter: 12));

                var stackLayoutCircle = new StackLayout
                {
                    Spacing = 16,
                    Orientation = StackOrientation.Horizontal,
                    Margin = new Thickness(32, 24, 0, 0),
                };

                var stackLayout = new StackLayout
                {
                    Spacing = 0,
                    Orientation = StackOrientation.Vertical,
                };

                stackLayoutCircle.Children.Add(profileImage);
                stackLayoutCircle.Children.Add(stackLayout);
                stackLayout.Children.Add(newContactShared);
                stackLayout.Children.Add(newContactLabel);
                baseView.MessageFrameContent.Padding = 0;
                baseView.MessageFrameContent.Children.Add(stackLayoutCircle);
                baseView.InitTimeLabel();
                baseView.FlagAndTime.Margin = new Thickness(0, 0, 12, 0);
                baseView.MessageFrameContent.Children.Add(new BoxView
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Margin = new Thickness(0, 4, 0, 0),
                    HeightRequest = 1,
                    BackgroundColor = Palette.PhoneContactBottomDividerColor
                });
                baseView.MessageFrameContent.Children.Add(bottomLabel);
                baseView.MessageFrameContent.Tapped += ((s, e) => PlacePhoneCall(contactDetails.Item2));


                void PlacePhoneCall(string number)
                {
                    try
                    {
                        Xamarin.Essentials.PhoneDialer.Open(number);
                    }
                    catch (Xamarin.Essentials.FeatureNotSupportedException)
                    {
                        _showToast?.Invoke(Localization.Resources.Dictionary.FeatureNotSupported);
                    }
                    catch (Exception)
                    {
                        _showToast?.Invoke(Localization.Resources.Dictionary.InvalidPhoneNumber);
                    }
                }
            }

            void addNewGroup()
            {
                var profileImage = new Image
                {
                    Source = Icons.NewGroup,
                    WidthRequest = 52,
                    HeightRequest = 52,
                };

                var newGroupCreate = new Label
                {
                    Text = Localization.Resources.Dictionary.NewGroupCreatedBy,
                    TextColor = Palette.CommonTextColor,
                    FontFamily = "PoppinsLight",
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    VerticalOptions = LayoutOptions.StartAndExpand,
                };

                var newGroupCreateBy = new Label
                {
                    Text = isMyMessage ? Localization.Resources.Dictionary.Me : message.AuthorName(),
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    FontFamily = "PoppinsSemiBold",
                    VerticalOptions = LayoutOptions.StartAndExpand,
                    TextColor = Palette.CommonTextColor,
                };

                newGroupCreate.SetFontSizeBinding(12);
                //newGroupCreate.BindingContext = this;
                //newGroupCreate.SetBinding(Label.FontSizeProperty, new Binding(nameof(TextSize), converter: _fontSizeConverter, converterParameter: 12));
                newGroupCreate.SetFontSizeBinding(16);
                //newGroupCreateBy.BindingContext = this;
                //newGroupCreateBy.SetBinding(Label.FontSizeProperty, new Binding(nameof(TextSize), converter: _fontSizeConverter, converterParameter: 16));

                var stackLayoutProfile = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                };

                var stackLayout = new StackLayout
                {
                    Spacing = 0,
                    Orientation = StackOrientation.Vertical,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(10, 10, 0, 0),
                };

                stackLayoutProfile.Children.Add(profileImage);
                stackLayoutProfile.Children.Add(stackLayout);
                stackLayout.Children.Add(newGroupCreate);
                stackLayout.Children.Add(newGroupCreateBy);
                baseView.MessageFrameContent.Children.Add(stackLayoutProfile);
                baseView.MessageFrame.CornerRadius = new CornerRadius(15, 15, 15, 15);
                baseView.MessageFrameContent.Padding = new Thickness(0, 10, 0, 10);
                baseView.MessageFrame.HorizontalOptions = LayoutOptions.FillAndExpand;
                baseView.MessageFrame.Margin = isMyMessage ? new Thickness(0, 0, 35, 0) : new Thickness(35, 0, 0, 0);
                baseView.MessageFrame.BackgroundColor = Palette.CommonFrameBackground;
            }

            void addNewContact()
            {
                var newContactLabel = new Label
                {
                    Text = Localization.Resources.Dictionary.NewContact,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    FontFamily = "PoppinsSemiBold",
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    TextColor = Palette.CommonTextColor,
                };

                newContactLabel.SetFontSizeBinding(16);
                //newContactLabel.BindingContext = this;
                //newContactLabel.SetBinding(Label.FontSizeProperty, new Binding(nameof(TextSize), converter: _fontSizeConverter, converterParameter: 16));
                baseView.MessageFrameContent.Children.Add(newContactLabel);
                baseView.MessageFrame.CornerRadius = new CornerRadius(15, 15, 15, 15);
                baseView.MessageFrameContent.Padding = new Thickness(0, 10, 0, 10);
                baseView.MessageFrame.HorizontalOptions = LayoutOptions.FillAndExpand;
                baseView.MessageFrame.Margin = isMyMessage ? new Thickness(0, 0, 35, 0) : new Thickness(35, 0, 0, 0);
                baseView.MessageFrame.BackgroundColor = Palette.CommonFrameBackground;
            }

            void addPdfDocument()
            {
                if (_fileDetailsProvider == null) return;
                byte[] pdfData;
                var pdfName = "PDF: ";

                try
                {
                    var fileDetails = _fileDetailsProvider(message.GetData());
                    pdfData = fileDetails.Item1;
                    pdfName += fileDetails.Item2;
                }
                catch (Exception e)
                {
                    pdfData = message.GetData();
                }

                var label = new Label
                {
                    FontFamily = "PoppinsSemiBold",
                    TextColor = Palette.MainTextColor(isOutgoing: isMyMessage),
                    Text = pdfName,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    VerticalTextAlignment = TextAlignment.Center,
                    MaxLines = 2,
                    LineBreakMode = LineBreakMode.TailTruncation
                };

                baseView.MessageFrameContent.Children.Add(
                    new StackLayout
                    {
                        Spacing = 8,
                        Orientation = StackOrientation.Horizontal,
                        Children =
                        {
                            new Image()
                            {
                                HeightRequest = 35,
                                WidthRequest = 30,
                                Source = Icons.Pdf,
                                HorizontalOptions = LayoutOptions.Start,
                                VerticalOptions = LayoutOptions.CenterAndExpand,
                            },
                            label
                        }
                    }
                );


                label.SetFontSizeBinding(14);
                //label.BindingContext = this;
                //label.SetBinding(Label.FontSizeProperty, new Binding(nameof(TextSize), converter: _fontSizeConverter, converterParameter: 14));
                baseView.MessageFrameContent.Tapped += ((s, e) => _onPdfMessageClicked?.Invoke(pdfData));
            }

            void addCallView(bool isVideoCall)
            {
                baseView.IsSwipeEnabled = false;
                var duration = 0;
                try
                {
                    duration = Convert.ToInt32(Encoding.Unicode.GetString(message.GetData()));
                }
                catch (Exception)
                {
                }

                var header = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 14,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                };
                var body = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Spacing = 14,
                };

                var lay = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    Padding = new Thickness(0, 5, 0, 0),
                    Children =
                    {
                        header, body
                    }
                };
                var Title = " " + (isVideoCall
                    ? Localization.Resources.Dictionary.VideoCall
                    : Localization.Resources.Dictionary.AudioCall);

                var DurationText = Localization.Resources.Dictionary.NoAnswer;

                ImageSource arrowIcon = null;

                if (message.Type == MessageType.AudioCall || message.Type == MessageType.VideoCall ||
                    message.Type == MessageType.StartAudioGroupCall || message.Type == MessageType.StartVideoGroupCall)
                {
                    arrowIcon = Icons.CallArrow(isAnswered: true, isOutgoing: isMyMessage);
                    Title = (isMyMessage
                        ? Localization.Resources.Dictionary.Outgoing
                        : Localization.Resources.Dictionary.Incoming) + Title;
                    if (message.Type == MessageType.AudioCall || message.Type == MessageType.VideoCall)
                    {
                        if (duration > 0)
                            DurationText = Utils.FormatTime(duration);
                        else
                        {
                            Title = Localization.Resources.Dictionary.MissedCall;
                            arrowIcon = Icons.CallArrow(isAnswered: false, isOutgoing: isMyMessage);
                        }
                    }
                    else
                        DurationText = "";
                }
                else
                {
                    if (message.Type == MessageType.DeclinedCall)
                    {
                        Title = Localization.Resources.Dictionary.DeclinedCall;
                        arrowIcon = Icons.CallArrow(isAnswered: false, isOutgoing: !isMyMessage);
                    }
                    else if (message.Type == MessageType.EndCall)
                    {
                        if (duration > 0)
                        {
                            Title = Localization.Resources.Dictionary.EndedCall;
                            DurationText = Utils.FormatTime(duration);
                            arrowIcon = Icons.CallArrow(isAnswered: true, isOutgoing: isMyMessage);
                        }
                        else
                        {
                            Title = Localization.Resources.Dictionary.MissedCall;
                            arrowIcon = Icons.CallArrow(isAnswered: false, isOutgoing: isMyMessage);
                        }
                    }
                }

                header.Children.Add(
                    new Image
                    {
                        Source = Icons.CallIcon(isVideoCall, isOutgoing: isMyMessage),
                        WidthRequest = isVideoCall ? 29 : 24,
                        HeightRequest = 24
                    }
                );
                body.Children.Add(
                    new Image
                    {
                        Source = arrowIcon,
                        WidthRequest = 9,
                        HeightRequest = 9
                    }
                );

                var title = new Label
                {
                    FontFamily = "PoppinsSemiBold",
                    Text = Title,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    TextColor = Palette.MainTextColor(isOutgoing: isMyMessage),
                    HorizontalTextAlignment = TextAlignment.Start,
                };

                var durationLabel = new Label
                {
                    FontFamily = "PoppinsLight",
                    Text = DurationText,
                    TextColor = Palette.MainTextColor(isOutgoing: isMyMessage),
                    HorizontalTextAlignment = TextAlignment.Start,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                };
                title.SetFontSizeBinding(14);
                //title.BindingContext = this;
                //title.SetBinding(Label.FontSizeProperty, new Binding(nameof(TextSize), converter: _fontSizeConverter, converterParameter: 14));
                durationLabel.SetFontSizeBinding(10);
                //durationLabel.BindingContext = this;
                //durationLabel.SetBinding(Label.FontSizeProperty, new Binding(nameof(TextSize), converter: _fontSizeConverter, converterParameter: 10));

                header.Children.Add(title);
                body.Children.Add(durationLabel);
                baseView.MessageFrameContent.Children.Add(lay);

                if (message.Contact.IsGroup)
                {
                    if (message.Type == MessageType.StartAudioGroupCall ||
                        message.Type == MessageType.StartVideoGroupCall)
                    {
                        if (contactViewItems.CallTime == null ||
                            contactViewItems.CallTime.CompareTo(message.Creation) < 0)
                        {
                            contactViewItems.IsCallGoingOn = true;
                            contactViewItems.CallTime = message.Creation;
                            contactViewItems.IsVideoCall = message.Type == MessageType.StartVideoGroupCall;
                        }
                    }
                    else if (message.Type == MessageType.EndCall)
                    {
                        if (contactViewItems.CallTime == null ||
                            contactViewItems.CallTime.CompareTo(message.Creation) < 0)
                        {
                            contactViewItems.IsCallGoingOn = false;
                            contactViewItems.CallTime = message.Creation;
                        }
                    }
                }
                else if (!isMyMessage && duration == 0 &&
                         (DateTime.Now.ToLocalTime() - message.Creation.ToLocalTime()).TotalSeconds < 40)
                    _finishCallEvent?.Invoke(message);
            }

            content = baseView;
            baseView.MessageFrameContent.LongPressing += (s, e) =>
            {
                if (contact.IsBlocked) return;
                contactViewItems.IsMessageSelection = true;
                if (!baseView.SelectionCheckBox.IsChecked)
                    baseView.SelectionCheckBox.IsChecked = true;
            };
            baseView.MessageFrameContent.Tapped += (s1, e1) =>
            {
                if (contactViewItems.IsMessageSelection)
                {
                    baseView.SelectionCheckBox.IsChecked = !baseView.SelectionCheckBox.IsChecked;
                }
            };


            if (message.Type != MessageType.PhoneContact)
                baseView.InitTimeLabel();

            if(Device.RuntimePlatform == Device.UWP)
            {
                baseView.Click((s, e)=>{ });
                //baseView.IsSwipeEnabled = false;
                baseView.MessageFrameContent.DoubleTapped+=((sender,e) => 
                _messageSwiped?.Invoke(message));
            }
            else
            {
                baseView.OnReplySwiped += () => _messageSwiped?.Invoke(message);
            }
        }


        public static Frame GenerateReplyLayout(Message message = null)
        {
            var boxView = new BoxView
            {
                Color = Color.FromHex("#FFA500"),
                WidthRequest = 5,
            };

            var title = new Label
            {
                Text = message?.Contact.Name,
                TextColor = Color.FromHex("#FFA500"),
                FontFamily = "PoppinsSemiBold",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.StartAndExpand,
            };


            var description = new Label
            {
                TextColor = Color.FromHex("#201F24"),
                FontFamily = "PoppinsLight",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.StartAndExpand,
            };

            var stackLayout = new StackLayout
            {
                Children = { title, description },
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            var stackLayoutBox = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 8,
                Padding = new Thickness(10, 10, 10, 8)
            };
            stackLayoutBox.Children.Add(boxView);

            var frame = new Frame
            {
                BackgroundColor = Color.FromHex("#E5E5E5"),
                Padding = 0,
                CornerRadius = 8,
                Content = stackLayoutBox,
                HasShadow = false
            };
            description.FontAttributes = FontAttributes.Italic;
            description.MaxLines = 1;
            description.LineBreakMode = LineBreakMode.TailTruncation;

            switch (message?.Type)
            {
                case MessageType.Text:
                    description.Text = Encoding.Unicode.GetString(message.GetData());
                    description.FontAttributes = FontAttributes.None;
                    break;
                case MessageType.Image:
                case MessageType.Location:
                    {
                        var miniPhoto = new Image
                        {
                            Aspect = Aspect.AspectFill,
                            HeightRequest = 20,
                            WidthRequest = 40
                        };
                        if (message?.Type == MessageType.Image)
                        {
                            miniPhoto.SetSource(message.GetData());
                            description.Text = "Image";
                        }
                        else
                        {
                            var data = message.GetData();
                            var latitude = BitConverter.ToDouble(data, 0);
                            var longitude = BitConverter.ToDouble(data, 8);
                            miniPhoto.Source = new UriImageSource
                            {
                                CacheValidity = TimeSpan.FromDays(7),
                                CachingEnabled = true,
                                Uri = new Uri(Utils.GetAddressPreviewImageUrl(latitude, longitude))
                            };
                            description.Text = "Location";
                        }

                        stackLayoutBox.Children.Add(miniPhoto);
                        break;
                    }
                case MessageType.PhoneContact:
                    description.Text = "Contact";
                    break;
                case MessageType.ShareEncryptedContent:
                    description.Text = "Video";
                    break;
                case MessageType.PdfDocument:
                    description.Text = "Document";
                    break;
                case MessageType.Audio:
                    description.Text = "Audio message";
                    break;

                default:
                    description.Text = "";
                    break;
            }

            stackLayoutBox.Children.Add(stackLayout);
            frame.SetTag("ReplyLayout");
            return frame;
        }

        // ADD bool isMyMessage
        private View GetAudioLayout(BaseView baseview, Message message, bool isMyMessage)
        {
            var audio = message.GetData();
            var isAudioPlayingBeforeDragging = false;
            var Player = DependencyService.Get<IAudioPlayer>(DependencyFetchTarget.NewInstance);
         
            var timer = 0d;
            var isStarted = false;

            var audioMessageHolder = new StackLayout()
            {
                Margin = new Thickness(4, 4, 4, 0),
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            if (audio == null)
                return audioMessageHolder;

            var start_stop = new Image
            {
                HeightRequest = 35,
                WidthRequest = 35,
                Source = Icons.AudioPlayerState(isNormalState: true)
            };
            var rec_icon = new Image
            {
                Margin = new Thickness(0, 5, 5, 0),
                HeightRequest = 25,
                WidthRequest = 25,
                Source = Icons.AudioMic(isOutgoing: isMyMessage),
                HorizontalOptions = LayoutOptions.Center
            };

            if (Device.RuntimePlatform == Device.iOS)
            {
                rec_icon.Margin = new Thickness(0, 12, 0, 0);
            }

            var durationLabel = new Label()
            {
                Margin = new Thickness(0, 0, 8, 0),
                TextColor = Palette.MainTextColor(isOutgoing: isMyMessage),
                FontSize = 11,
                VerticalTextAlignment = TextAlignment.Start,
                VerticalOptions = LayoutOptions.Center
            };
            if (Device.RuntimePlatform == Device.iOS)
            {
                durationLabel.Margin = new Thickness(0, 6, 0, 0);
            }

            durationLabel.SetFontSizeBinding(11);
            //durationLabel.BindingContext = this;
            //durationLabel.SetBinding(Label.FontSizeProperty, new Binding(nameof(TextSize), converter: _fontSizeConverter, converterParameter: 11));

            var timerLabel = new Label()
            {
                Margin = new Thickness(15, 0, 8, 0),
                TextColor = Palette.AudioTimerColor,
                FontSize = 11,
                HorizontalTextAlignment = TextAlignment.Start,
                HorizontalOptions = LayoutOptions.StartAndExpand
            };

            timerLabel.SetFontSizeBinding(11);
            //timerLabel.BindingContext = this;
            //timerLabel.SetBinding(Label.FontSizeProperty, new Binding(nameof(TextSize), converter: _fontSizeConverter, converterParameter: 11));

            var sliderLayout = new StackLayout()
            {
                Padding = new Thickness(2, 0, 2, 0),
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };

            var slider = new Slider
            {
                Margin = Device.RuntimePlatform == Device.Android
                    ? new Thickness(0, 15, 0, 0)
                    : new Thickness(0, 5, 0, 0),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Value = 0,
                ThumbColor = Palette.SliderThumbColor(isOutgoing: isMyMessage),
                MinimumTrackColor = Palette.SliderMinimumColor(isOutgoing: isMyMessage),
                MaximumTrackColor = Palette.SliderMaximumColor(isOutgoing: isMyMessage),
                ThumbImageSource = Icons.SliderThumb(isOutgoing: isMyMessage),
            };
            if (Device.RuntimePlatform == Device.iOS)
            {
                sliderLayout.Margin = new Thickness(0, 7, 0, 0);

                slider.ThumbImageSource = Icons.SliderThumb(isOutgoing: isMyMessage);
            }

            if (Device.RuntimePlatform == Device.UWP){

                Player.NaturalDurationChanged += (_, events) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        var x = Player.Duration.ToString();
                        durationLabel.Text = Utils.FormatTime((int)Player.Duration);
                        if (timerLabel.Text == null) timerLabel.Text = Utils.FormatTime(0);
                        if (Player.Duration > 0)
                        {
                            slider.Maximum = Player.Duration;
                        }
                    });
              
                };
            }
         
            if (!initAudioStream())
                return null;

            start_stop.Click(playPauseButtonClick);

            slider.DragStarted += SliderDragStartedListener;
            slider.DragCompleted += SliderDragCompletedListener;
            Player.PlaybackEnded += audioPlayerPlaybackEndedListener;

            if (Player == null || _composerStateProvider == null) return audioMessageHolder;
            addViews();

            bool initAudioStream()
            {
                var ms = new MemoryStream(audio);
                Player.Load(ms);

                if (Device.RuntimePlatform == Device.UWP)
                {
                    durationLabel.Text = Utils.FormatTime(0);
                    slider.Maximum = 999;
                    return true;
                }
                durationLabel.Text = Utils.FormatTime(Convert.ToInt32(Player.Duration));
                timerLabel.Text = Utils.FormatTime(0);
                
                //if (Device.RuntimePlatform == Device.UWP)
                //{
                //    durationLabel.Text = Utils.FormatTime(999);
                //    slider.Maximum = 999;
                //    slider.IsEnabled = false;
                //    return true;
                //}
                //else
                if (Player.Duration > 0 )
                {
                
                    slider.Maximum = Player.Duration;
                    return true;
                }
                

                return false;
            }

            void SliderDragCompletedListener(object sender, EventArgs e)
            {
                baseview.IsEnabled = true && baseview.IsSwipeEnabled;
                if (slider.Value != 0)
                    timerLabel.Text = Utils.FormatTime(Convert.ToInt32(slider.Value));
                else
                    timerLabel.Text = Utils.FormatTime(0);
                timer = slider.Value;
                if (isAudioPlayingBeforeDragging)
                    startPlayer();
            }

            void SliderDragStartedListener(object sender, EventArgs e)
            {
                baseview.IsEnabled = false;

                isAudioPlayingBeforeDragging = Player.IsPlaying;
                Player.Stop();
                isStarted = false;
                start_stop.Source = Icons.AudioPlayerState(isNormalState: true);
            }

            void playPauseButtonClick(object s, EventArgs e)
            {
                var objFileImageSource = Icons.AudioPlayerState(isNormalState: true);

                if ((IsAudioPlaying && !Player.IsPlaying) || objFileImageSource.ToString().Contains("start"))
                    _showToast?.Invoke(Localization.Resources.Dictionary
                        .PleaseStopAnotherVoiceRecordBeforePlayingThisRecord);
                else if (!Player.IsPlaying && !IsAudioPlaying && ChatPageSupport.CurrentContact() != null)
                {
                    if (_composerStateProvider(RequiredComposerState.AudioRecord, -1))
                    {
                        var ms = new MemoryStream(audio);
                        Player.Load(ms);
                        startPlayer();
                    }
                    else if (Player.Duration >= 1 && !_composerStateProvider(RequiredComposerState.AudioRecord, -1))
                        _showToast?.Invoke(Localization.Resources.Dictionary.CannotStartPlayingWhileAudioIsRecording);
                }
                else
                    stopPlayer();
            }

            void startPlayer()
            {
                if (message != null)
                    SelectedMessageHashCode = message.GetHashCode();
                start_stop.Source = Icons.AudioPlayerState(isNormalState: false);
                Device.StartTimer(TimeSpan.FromSeconds(_AUDIO_SOUND_UPDATE_TIMER_DELAY), updatePosition);
                isStarted = true;
                Player.Seek(timer);
                Player.Play();
                IsAudioPlaying = true;
                if (ChatPageSupport.CurrentContact() != null &&
                    _composerStateProvider(RequiredComposerState.AudioSend, -1) == false)
                    _composerStateProvider(RequiredComposerState.AudioSend, 1);
                listenProximitySensorEvent?.Invoke(Player);
            }

            void audioPlayerPlaybackEndedListener(object s, EventArgs e)
            {
                if (timerLabel.Text != Utils.FormatTime(0))
                {
                    if (Device.RuntimePlatform == Device.UWP)
                        Device.BeginInvokeOnMainThread(() => resetPlayer());
                    else resetPlayer();
                }
                if (Device.RuntimePlatform == Device.UWP)
                    Device.BeginInvokeOnMainThread(() => timerLabel.Text = Utils.FormatTime(0));
                else timerLabel.Text = Utils.FormatTime(0);
            }

            bool updatePosition()
            {
                if (isStarted)
                {
                    timer += _AUDIO_SOUND_UPDATE_TIMER_DELAY;
                    slider.Value = timer;
                    timerLabel.Text = Utils.FormatTime(Convert.ToInt32(timer));
                }

                if (ChatPageSupport.CurrentContact() == null || _isAppInSleepMode)
                    stopPlayer();
                return isStarted;
            }

            void stopPlayer()
            {
                SelectedMessageHashCode = 0;
                isStarted = false;
                Player.Stop();
                IsAudioPlaying = false;
                DisableProximitySensorEvent?.Invoke();
                start_stop.Source = Icons.AudioPlayerState(isNormalState: true);
                baseview.IsEnabled = true && baseview.IsSwipeEnabled;
            }

            void resetPlayer()
            {
                SelectedMessageHashCode = 0;
                //Player.Stop();
                timer = 0;
                slider.Value = 0;
                stopPlayer();
            }

            void addViews()
            {
                audioMessageHolder.Children.Add(start_stop);
                sliderLayout.Children.Add(slider);
                sliderLayout.Children.Add(timerLabel);

                var stackLayout = new StackLayout
                { Orientation = StackOrientation.Vertical, Margin = 0, Padding = 0 };

                audioMessageHolder.Children.Add(sliderLayout);
                stackLayout.Children.Add(rec_icon);
                stackLayout.Children.Add(durationLabel);
                audioMessageHolder.Children.Add(stackLayout);
            }

            return audioMessageHolder;
        }

        //Text 
        // SetTextLayout(isMyMessage, message, baseView.MessageFrameContent, out textLabel);

        private static Label GenerateShowOriginalLabel()
        {
            var showOriginalLabel = new Label
            {
                Text = "Show Original",
                FontFamily = "PoppinsSemiBold",
                TextColor = Color.DeepSkyBlue,
                TextDecorations = TextDecorations.Underline,
                HorizontalTextAlignment = TextAlignment.End
            };
            showOriginalLabel.SetFontSizeBinding(14);
            return showOriginalLabel;
        }

        private void SetTextLayout(bool isMyMessage, Message message, StackLayout box, out CustomLinkLabel textLabel)
        {
            var contact = message.Contact;
            var label = new CustomLinkLabel
            {
                FontFamily = "PoppinsSemiBold",
                TextColor = Palette.MainTextColor(isOutgoing: isMyMessage),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.FillAndExpand,
            };

            label.SetFontSizeBinding(14);

            //it is for multi selection.
            textLabel = isMyMessage ? null : label;

            label.HorizontalTextAlignment = TextAlignment.Start;
            label.SetTag("original");

            var textMessage = Encoding.Unicode.GetString(message.GetData());
            var displayText = textMessage;
            var showOriginalLabel = GenerateShowOriginalLabel();
            showOriginalLabel.Click((sender, eventArgs) =>
            {
                var originalText = textMessage;
                box.Children.Remove(showOriginalLabel);
                if (originalText.Length > 700 && label.Text.EndsWith(". . ."))
                {
                    originalText = originalText.Substring(0, 700) + " . . .";
                }

                label.SetTag("original");
                label.Text = originalText;
            });

            var isLastMessage = contact.LastMessageTime == message.ReceptionTime;
            if (!isMyMessage && contact.TranslationOfMessages)
            {
                var translation = message.Translation ?? "";
                if (!translation.Equals("translation_failed"))
                    if (!string.IsNullOrWhiteSpace(translation))
                    {
                        displayText = translation;
                        label.SetTag("translated");
                        box.Children.Add(showOriginalLabel);
                    }
                    else if (isLastMessage)
                    {
                        TranslateText();
                    }
            }


            box.Children.Add(label);

            if (displayText.Length > 700)
            {
                displayText = displayText.Substring(0, 700) + " . . .";
                var readMoreLabel = new Label
                {
                    Text = Localization.Resources.Dictionary.ReadMore,
                    FontFamily = "PoppinsSemiBold",
                    TextColor = Color.DeepSkyBlue,
                    TextDecorations = TextDecorations.Underline,
                    HorizontalTextAlignment = TextAlignment.End
                };
                readMoreLabel.SetTag("read_more");
                readMoreLabel.SetFontSizeBinding(14);
                readMoreLabel.Click((sender, eventArgs) =>
                {
                    label.Text = displayText =
                        label.GetTag().Equals("original")
                            ? textMessage
                            : message.Translation;
                    box.Children.Remove(readMoreLabel);
                });
                box.Children.Add(readMoreLabel);
            }
            else if (displayText.Length < 3)
            {
                var containsDigitOrLetter = displayText.ToCharArray().Any(char.IsLetterOrDigit);
                if (!containsDigitOrLetter)
                    label.FontSize = 50;
            }

            label.Text = displayText;


            void TranslateText()
            {
                _translateService?.Invoke(message, () => UpdateDisplayedText(message, label));
            }
        }

        public static void UpdateDisplayedText(Message message, Label textLabel)
        {
            if (message == null || textLabel == null || message.GetData() == null) return;
            var originalText = Encoding.Unicode.GetString(message.GetData());
            var translatedText = message.Translation;
            var displayText = originalText;
            if (!string.IsNullOrWhiteSpace(translatedText))
            {
                displayText = translatedText;
                textLabel.SetTag("translated");
                var showOriginalLabel = GenerateShowOriginalLabel();
                var box = textLabel.Parent as StackLayout;
                var index = box?.Children.IndexOf(textLabel);
                box?.Children.Insert(index.Value, showOriginalLabel);

                showOriginalLabel.Click((sender, args) =>
                {
                    box?.Children.Remove(showOriginalLabel);
                    if (originalText.Length > 700 && textLabel.Text.EndsWith(". . ."))
                    {
                        originalText = originalText.Substring(0, 700) + " . . .";
                    }

                    textLabel.SetTag("original");
                    textLabel.Text = originalText;
                });
            }


            if (displayText.Length > 700 && textLabel.Text.EndsWith(". . ."))
            {
                displayText = displayText.Substring(0, 700) + " . . .";
            }

            textLabel.Text = displayText;
        }


        private static Image AddImageLayout(ImageSource imageSource, BaseView baseView)
        {
            var imageView = new Image
            {
                HeightRequest = 190,
                WidthRequest = 300,
                Aspect = Aspect.AspectFill,
                Margin = new Thickness(0, 0, 0, 0),
                Source = imageSource
            };
            baseView.MessageFrame.HorizontalOptions =
                baseView.IsMyMessage ? LayoutOptions.End : LayoutOptions.Start;
            baseView.MessageFrameContent.Padding = new Thickness(0);
            baseView.MessageFrameContent.Children.Add(imageView);
            return imageView;
        }


        private void OnReadmoreLabelClicked(StackLayout _box, string _text)
        {
        }

        private Tuple<StackLayout, CircularProgressBar, Image, Image> GetVideoLayout()
        {
            var stackLayout = new StackLayout()
            { Orientation = StackOrientation.Vertical, HeightRequest = 275 };
            var imageView = new Image()
            {
                HeightRequest = 300,
                WidthRequest = 300,
                Aspect = Aspect.AspectFill,
                Margin = new Thickness(0, 0, 0, -25),
            };
            var progressBar = new CircularProgressBar()
            {
                Margin = new Thickness(0, -150, 0, 0),
                Padding = 0,
                Easing = true,
                HeightRequest = 36,
                WidthRequest = 36,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Color = Palette.Colors.ThemeColor,
                Progress = 0
            };
            var playIcon = new Image()
            {
                HeightRequest = 36,
                WidthRequest = 36,
                Aspect = Aspect.AspectFill,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Margin = new Thickness(0, -150, 0, 0),
                Source = Icons.AudioPlayerState(true),
            };
            stackLayout.Children.Add(imageView);
            stackLayout.Children.Add(progressBar);
            stackLayout.Children.Add(playIcon);

            return Tuple.Create(stackLayout, progressBar, imageView, playIcon);
        }

        public static void SetAppSleepStatus(bool isAppInSleepMode)
        {
            _isAppInSleepMode = isAppInSleepMode;
        }

        private delegate void DownloadStatus(CircularProgressBar progressBar, double percentage, Image playIcon);

        private static async Task<string> DownloadVideo(byte[] _privateKey, CircularProgressBar progressBar,
            DownloadStatus downloadStatus, Image playIcon)
        {
            Debug.WriteLine("[Downloading Video]: File download started");
            var current = Xamarin.Essentials.Connectivity.NetworkAccess;
            // Check internet connectivity
            if (current != Xamarin.Essentials.NetworkAccess.Internet)
            {
                Debug.WriteLine("[Downloading Video]: Error - No internet connection available");
                return string.Empty;
            }

            // Generate File name from key
            // Filename is the hash of the private key
            var sha256 = SHA256.Create();
            var fileName = ComputePsuedoHash.ToHex(sha256.ComputeHash(_privateKey));

            // Get list of file chunks
            var fileList = await FileService.GetFileChunks(fileName).ConfigureAwait(false);
            if (fileList.Count > 0)
            {
                var _fileNameWithType = fileList[0].Split('-').LastOrDefault();
                //Saving video file in phone storage
                var folder = Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "downloads");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                var videoFile = Path.Combine(folder, _fileNameWithType);

                if (File.Exists(videoFile))
                {
                    File.Delete(videoFile);
                }

                using (var outputStream = File.Create(videoFile))
                {
                    for (var i = 0; i < fileList.Count; i++)
                    {
                        current = Xamarin.Essentials.Connectivity.NetworkAccess;
                        // Check internet connectivity
                        if (current != Xamarin.Essentials.NetworkAccess.Internet)
                        {
                            Debug.WriteLine("[Downloading Video]: Error - No internet connection available");
                            return string.Empty;
                        }

                        // Show download progress
                        var progressPercentAge = Math.Ceiling((i / Convert.ToDouble(fileList.Count)) * 100);
                        downloadStatus(progressBar, progressPercentAge, playIcon);
                        Debug.WriteLine(string.Format("{0} %", progressPercentAge));

                        var fileBlock = await FileService.DownloadFileChunk(fileList[i], _privateKey)
                            .ConfigureAwait(false);
                        if (fileBlock == null)
                        {
                            return string.Empty;
                        }

                        await outputStream.WriteAsync(fileBlock, 0, fileBlock.Length).ConfigureAwait(false);
                        Debug.WriteLine(string.Format("[Downloading Video]: File Uploaded - {0}", fileList[i]));
                    }
                }

                Debug.WriteLine(string.Format("[Downloading Video]: File Uploaded - {0}", videoFile));
                return videoFile;
            }
            else
            {
                return string.Empty;
            }
        }

        private byte[] GetThumbnail(byte[] privateKey, out string imageName)
        {
            imageName = null;
            try
            {
                var key = ComputePsuedoHash.ToHex(SHA256.Create().ComputeHash(privateKey));
                if (key != null)
                {
                    imageName = key += ".png";
                    return XamarinShared.Setup.Context.SecureStorage.DataStorage.LoadData(key);
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Debugger.Break();
                return null;
            }
        }

        private void ProgressChange(CircularProgressBar progressBar, double percentage, Image playIcon)
        {
            if (progressBar == null)
                return;
            Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
            {
                if (percentage == 100.0)
                {
                    progressBar.IsVisible = false;
                    playIcon.IsVisible = true;
                    playIcon.Source = Icons.AudioPlayerState(true); // should be play icon
                }
                else
                {
                    playIcon.IsVisible = false;
                    progressBar.IsVisible = true;
                }

                progressBar.Progress = percentage;
            });
        }

        private async Task SetThumbnail(byte[] privateKey, Image thumbnailImage)
        {
            var thumbNail = GetThumbnail(privateKey, out var thumbNailName);
            if (thumbNail == null)
            {
                await DownloadThumbnail(thumbNailName, thumbnailImage, privateKey).ConfigureAwait(false);
            }
            else
            {
                Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() => thumbnailImage.SetSource(thumbNail));
            }
        }

        private async Task DownloadThumbnail(string thumbNailName, Image image, byte[] privateKey)
        {
            var thumbNail = await FileService.DownloadFileThumbnail(thumbNailName, privateKey).ConfigureAwait(false);
            if (thumbNail == null)
            {
                image.BackgroundColor = Color.Black;
                return;
            }

            XamarinShared.Setup.Context.SecureStorage.DataStorage.SaveData(thumbNail, thumbNailName);
            Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() => image.SetSource(thumbNail));
        }

        public static Task BeginInvokeOnMainThreadAsync(Action function)
        {
            var tcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    function();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }
    }
}