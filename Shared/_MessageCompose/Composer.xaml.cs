using System;
using System.IO;
using System.Threading.Tasks;
using CustomViewElements;
using EncryptedMessaging;
using MessageCompose.Services;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using VideoFileCryptographyLibrary;
using System.Linq;
using Utils;
using static EncryptedMessaging.MessageFormat;
using VideoFileCryptographyLibrary.Models;
using System.Threading;
// using XamarinShared.ViewCreator;

namespace MessageCompose
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Composer : BaseContentView
    {
        public event Action OnSendClick;
        public event Action OnReplyClick;
        public event Action OnForwardClick;
        public static bool IsAudioRecordCancelled = true;
        public static bool IsAudioSendCancelled = true;
        public ulong? ReplyToPostId = null;
        public bool IsVideoUploading = false;

        public delegate void SendEvent(ulong? replyToPostId = null, string text = null, byte[] image = null, byte[] audio = null, Tuple<double, double> coordinate = null, byte[] pdf = null, byte[] phoneContact = null, byte[] video = null, string videoType = null);
        private SendEvent _onSend;

        public delegate void MediaFileSelectEvent(byte[] data, ulong? replyToPostId = null);
        private MediaFileSelectEvent _onMediaFileSelected;

        public delegate void VideoFileSelectEvent(FileResult data, bool showVideoPreview, ulong? replyToPostId = null);
        private VideoFileSelectEvent _onVideoFileSelected;

        public Action AudioPlayerEvent;

        public Action AudioOutputDeleteEvent;

        private byte[] audioOutput;
        private int _timer = 0;
        private Frame replyFrame;

        private int _uiState = 1;
        private int UIState
        {
            set
            {
                if (value == -1)  //-1 for reset
                    SwitchViewState(_uiState);
                else if (_uiState != value)
                {
                    SwitchViewState(value);
                    if (value != 4) //because we will back  to old viewstate;
                        _uiState = value;
                    else
                        TextMessage.IsEnabled = false;

                }
                TextMessage.IsEnabled = _uiState == 1;
            }
        }


        public Composer()
        {
            InitializeComponent();
            AttachmentIcon.Source = Utils.Icons.IconProvider?.Invoke("ic_new_attachment");
            CameraIcon.Source = Utils.Icons.IconProvider?.Invoke("ic_new_camera");
            RecIcon.Source = Utils.Icons.IconProvider?.Invoke("ic_new_record");
            SendIcon.Source = Utils.Icons.IconProvider?.Invoke("ic_new_send");
            Cancel.Source = Utils.Icons.IconProvider?.Invoke("ic_new_cancel");
            FinishRecordIcon.Source = Utils.Icons.IconProvider?.Invoke("ic_new_finish_recording");
            Cancel.Source = Utils.Icons.IconProvider?.Invoke("ic_new_cancel");
            Play.Source = Utils.Icons.IconProvider?.Invoke("ic_new_play");
            Delete.Source = Utils.Icons.IconProvider?.Invoke("ic_new_delete");
            AudioSend.Source = Utils.Icons.IconProvider?.Invoke("ic_new_audio_send");
        }

        public override void OnAppearing()
        {
            XamarinShared.ViewCreator.MessageViewCreator.Instance.OnMessageSwiped(LoadReplyMessage);
        }

        public override void OnDisappearing()
        {
            XamarinShared.ViewCreator.MessageViewCreator.Instance.OnMessageSwiped(null);
        }

        public void LoadReplyMessage(Message m)
        {
            RemoveReplyState();
            if (m == null) return;

            replyFrame = XamarinShared.ViewCreator.MessageViewCreator.GenerateReplyLayout(m);
            replyFrame.Margin = new Thickness(12, 0);
            var stack = replyFrame.Content as StackLayout;

            var button = new Button
            {
                Text = "X",
                BackgroundColor = Color.Transparent,
                VerticalOptions = LayoutOptions.Start,
                WidthRequest = 35,
                HeightRequest = 35,
                MinimumHeightRequest = 35,
                MinimumWidthRequest = 35
            };

            button.Clicked += (sender, e) => RemoveReplyState();

            stack?.Children.Add(button);
            ReplyToPostId = m.PostId;


            MainLayout.Children.Insert(0, replyFrame);
        }

        public void RemoveReplyState()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ReplyToPostId = null;
                if (replyFrame != null)
                {
                    MainLayout.Children.Remove(replyFrame);
                    replyFrame = null;
                }
            });
        }

        public void SetLastUnsentMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            TextMessage.Text = message;
        }

        public string GetUnsentMessage()
        {
            return TextMessage.Text;
        }

        public void Init(SendEvent onSend, MediaFileSelectEvent onMediaFileSelected, VideoFileSelectEvent videoFileSelectEvent)
        {
            _onSend = onSend;
            _onMediaFileSelected = onMediaFileSelected;
            _onVideoFileSelected = videoFileSelectEvent;
            AudioTime.Text = Utils.Utils.FormatTime(0);
        }

        private void Attachment_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick(600);
            var attachmentPopup = new AttachmentPopupPage();
            attachmentPopup.IsVideoUploading = IsVideoUploading;
            attachmentPopup.AttachPicture += (data) => _onMediaFileSelected?.Invoke(data, ReplyToPostId);
            attachmentPopup.AttachAudio += (data) => _onSend?.Invoke(ReplyToPostId, audio: data);
            attachmentPopup.AttachLocation += (latitude, longitude) => _onSend?.Invoke(ReplyToPostId, coordinate: new Tuple<double, double>(latitude, longitude));
            attachmentPopup.AttachPdfDocument += (data) => _onSend?.Invoke(ReplyToPostId, pdf: data);
            attachmentPopup.AttachPhoneContact += (data) => _onSend?.Invoke(ReplyToPostId, phoneContact: data);
            attachmentPopup.AttachVideo += (data) =>
            {
                if (Device.RuntimePlatform == Device.iOS)
                {
                    _onVideoFileSelected?.Invoke(data, false, replyToPostId: ReplyToPostId);
                }
                else
                {
                    _onVideoFileSelected?.Invoke(data, true, replyToPostId: ReplyToPostId);
                }
            };
            PopupNavigation.Instance.PushAsync(attachmentPopup, true);
        }

        private void Cancel_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            ResetAudioRecorderView();
        }

        //when you pause the recording
        private void Finish_Recording_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            DependencyService.Get<XamarinShared.ViewCreator.IAudioRecorder>()?.StopRecording();
            audioOutput = DependencyService.Get<XamarinShared.ViewCreator.IAudioRecorder>().GetOutput();
            UIState = 3;
        }

        // 1 ==  Normal State , 2 == Record state , 3 == Send state , 4 == SelectionState
        private void SwitchViewState(int state)
        {

            NormalState.IsVisible = state == 1;
            RecordingState.IsVisible = state == 2;
            SendState.IsVisible = state == 3;
            SelectionState.IsVisible = state == 4;
            if (state == 4)
            {
                RemoveReplyState();
            }

        }

        public void UpdateSelectionState(System.Collections.ObjectModel.ObservableCollection<Tuple<Message, Label>> selectedMessages, int messagesCount)
        {
            switch (messagesCount)
            {
                case 1:

                    UIState = 4;
                    if (IsMessageReplyable(selectedMessages[0].Item1.Type))
                        ReplyLayout.IsVisible = true;
                    else
                        ReplyLayout.IsVisible = false;

                    break;
                case 0:
                    ReplyLayout.IsVisible = true;
                    UIState = -1; //reseting
                    break;
                default:
                    ReplyLayout.IsVisible = false;
                    break;
            }
        }

        private bool IsMessageReplyable(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Text:
                case MessageType.Audio:
                case MessageType.Image:
                case MessageType.Location:
                case MessageType.PdfDocument:
                case MessageType.ShareEncryptedContent:
                case MessageType.PhoneContact:
                    return true;
                default: return false;
            }
        }

        private void AudioPlay_Clicked(object sender, EventArgs e)
        {
                sender.HandleButtonSingleClick();
                AudioPlayerEvent?.Invoke();
        }

        private void AudioSend_ClickedAsync(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            SendAudioUpdateView();
            AudioOutputDeleteEvent?.Invoke();
        }

        private void Delete_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            ResetAudioRecorderView();
            AudioOutputDeleteEvent?.Invoke();
        }

        public void ResetAudioRecorderView()
        {
            DependencyService.Get<XamarinShared.ViewCreator.IAudioRecorder>().StopRecording();
            StopAudio();
            audioOutput = null;
            UIState = 1;
            IsAudioRecordCancelled = true;
            _timer = 0;
            IsAudioSendCancelled = true;
            AudioTime.Text = Utils.Utils.FormatTime(0);
            SetSendButtonSource();
        }

        private async void Send_ClickedAsync(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick(500);
            if (!string.IsNullOrWhiteSpace(TextMessage.Text))
            {
                TextMessage.Focus();
                _onSend?.Invoke(ReplyToPostId, text: TextMessage.Text);
                //_onSend(text: TextMessage.Text);
                TextMessage.Text = "";
                OnSendClick?.Invoke();
            }
            else
            {
                if (!await PermissionManager.CheckMicrophonePermission().ConfigureAwait(true)
                                                || !await PermissionManager.CheckStoragePermission().ConfigureAwait(true))
                {
                    return;
                }
                try
                {
                    DependencyService.Get<XamarinShared.ViewCreator.IAudioRecorder>()?.StartRecording();
                }
                catch
                {
                    return;
                }
                TextMessage.Unfocus();
                UpdateTimer();
                IsAudioRecordCancelled = false;
                UIState = 2;
                void UpdateTimer()
                {
                    Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                    {
                        if (!IsAudioRecordCancelled)
                            AudioTime.Text = Utils.Utils.FormatTime(++_timer);
                        return !IsAudioRecordCancelled;

                    });
                }
                TextMessage.Unfocus();

            }
        }

        private void SendAudioUpdateView()
        {
            if (_timer > 1 && audioOutput != null)
            {
                _onSend?.Invoke(ReplyToPostId, audio: audioOutput);
            }
            ResetAudioRecorderView();
            AudioOutputDeleteEvent?.Invoke();

        }

        private void MessageText_Changed(object sender, TextChangedEventArgs _) => SetSendButtonSource();

        private void SetSendButtonSource()
        {
            if (Camera.IsVisible && !string.IsNullOrWhiteSpace(TextMessage.Text))
            {
                Camera.IsVisible = false;
                Rec.IsVisible = false;
                Send.IsVisible = true;

            }
            else if (!Camera.IsVisible && string.IsNullOrWhiteSpace(TextMessage.Text))
            {
                Camera.IsVisible = true;
                Rec.IsVisible = true;
                Send.IsVisible = false;
            }
        }

        private async void Camera_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (await PermissionManager.CheckCameraPermission() && await PermissionManager.CheckStoragePermission())
                {
                    ShowProgressDialog();
                    var photo = await MediaPicker.CapturePhotoAsync();
                    await OpenEditImagePageAsync(photo);


                }
            }catch(Exception )
            {
            }
        }

        private async Task OpenEditImagePageAsync(FileResult file)
        {
            if (file != null)
            {
                var stream = await file.OpenReadAsync();
                _onMediaFileSelected(Utils.Utils.StreamToByteArray(stream), ReplyToPostId);
            }
            else HideProgressDialog();

            if (PopupNavigation.Instance.PopupStack.Count > 0)
                await PopupNavigation.Instance.PopAsync(true).ConfigureAwait(true);
        }


        public void PlayAudio()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                PlayTxt.Text = Localization.Resources.Dictionary.Pause;
                Play.Source = Icons.IconProvider?.Invoke("ic_audio_pause");
            });

        }

        public void StopAudio()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Play.Source = Icons.IconProvider?.Invoke("ic_new_play");
                PlayTxt.Text = Localization.Resources.Dictionary.Play;
            });
        }

        private void Forward_Clicked(object sender, EventArgs e)
        {
            OnForwardClick?.Invoke();
        }

        private void Reply_Clicked(object sender, EventArgs e)
        {
            OnReplyClick?.Invoke();
        }
    }
}

