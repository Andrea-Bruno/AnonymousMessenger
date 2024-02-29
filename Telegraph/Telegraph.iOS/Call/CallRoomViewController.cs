using System;
using DT.Xamarin.Agora;
using Telegraph.CallHandler;
using UIKit;
using Foundation;
using Telegraph.CallHandler.Helpers;
using Telegraph.iOS.Services;
using Xamarin.Forms;
using Telegraph.Services;
using AVFoundation;
using Telegraph.iOS.Call;
using Plugin.Toast;
using System.Threading.Tasks;
using Xamarin.Essentials;
using System.Drawing;
using System.Collections.Generic;
using Telegraph.DesignHandler;

[assembly: Dependency(typeof(CallRoomViewController))]
namespace Telegraph.iOS.Call
{
    public partial class CallRoomViewController : UIViewController, IAudioPlayerSpeakerService
    {
        public static CallRoomViewController Instance;

        public AgoraRtcDelegate AgoraDelegate;
        public AgoraRtcEngineKit AgoraKit;
        private int DurationInSecond;
        private NSTimer timer;
        private bool _isAnyoneJoined;
       
        private Dictionary<nuint, bool> participants;
        private AgoraRtcVideoCanvas videoCanvas;

        private NSLayoutConstraint viewWidth, viewHeight, viewMarginTopConstraint, viewMarginRightConstraint;
        public AVAudioPlayer player;
        private UIWindow window;

        private bool _audioMuted = false;
        private bool _videoMuted = false;
        private bool _isSpeakerEnable = false;
        private bool AudioMuted
        {
            get
            {
                return _audioMuted;
            }
            set
            {
                _audioMuted = value;
                AgoraKit?.MuteLocalAudioStream(value);
                UpdateMicrophoneBtnIcon();

            }
        }

        private bool VideoMuted
        {
            get
            {
                return _videoMuted;
            }
            set
            {
                _videoMuted = value;
                LocalView.Hidden = value;
                AgoraKit?.EnableLocalVideo(!value);
                AgoraKit?.MuteLocalVideoStream(value);
                UpdateCameraBtnIcon();
                if (value)
                    App.ListenProximitySensor();
                else
                    App.DisableProximitySensor();
            }
        }

        private bool SpeakerEnabled
        {
            get
            {
                return _isSpeakerEnable;
            }
            set
            {
                _isSpeakerEnable = value;
                AgoraKit?.SetEnableSpeakerphone(value);
                UpdateSpeakerBtnIcon();
            }
        }

        public CallRoomViewController() : base("CallRoomViewController", null)
        {
            Task.Delay(300).Wait(); // wait for init
            Forms.Init();
            participants = new Dictionary<nuint, bool>();

            InitAudioSession();
            UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(UIApplication.BackgroundFetchIntervalMinimum);
            ModalPresentationStyle = UIModalPresentationStyle.FullScreen;

        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Instance = this;
            if (!AskMicrophonePermission())
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.MicrophonePermissionIsNeeded);

            InitializeAgoraEngine();
            InitDefaultValues();
            JoinChannel();
            SetupLocalVideo();
            SetLabels();
            SetAvatar();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            if (VideoMuted == true)
                App.ListenProximitySensor();
        }
        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            App.DisableProximitySensor();
        }
        public void ListenProximitySensor()
        {
            if (Instance?.VideoMuted == true)
                App.ListenProximitySensor();
            if (Instance?._isAnyoneJoined ==  false)
            {
                StopDialing();
                PlayDialing();
            }
        }
        private void InitDefaultValues()
        {
            ContainerView.Hidden = true;
            VideoMuted = AgoraSettings.Current.IsAudioCall;
            AudioMuted = false;
            SpeakerEnabled = !AgoraSettings.Current.IsAudioCall;
        }

        private void InitAudioSession()
        {
            AVAudioSession.SharedInstance().Init();
            AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.PlayAndRecord);
            AVAudioSession.SharedInstance().SetMode(AVAudioSession.ModeVoiceChat, out NSError error);
            AVAudioSession.SharedInstance().SetActive(true);
        }

        private void SetLabels()
        {
            Username.Text = AgoraSettings.Current.Username;
            Username.Layer.CornerRadius = 20;
            SetFullScreenLocalViewPlace();

            if (!AgoraSettings.Current.IsCallingByMe)
            {
                InitTimer();
                AnimateUsernameLabel();

                _isAnyoneJoined = true;
            }
            else
                Duration.Text = Localization.Resources.Dictionary.Calling;

            Username.Font = UIFont.FromName("Poppins-Bold", 26);
            Duration.Font = UIFont.FromName("Poppins-Light", 22);
        }

        private void SetAvatar()
        {
            if (AgoraSettings.Current?.Avatar != null)
            {
                ProfilePicture.Image = UIImage.LoadFromData(NSData.FromArray(AgoraSettings.Current?.Avatar));
                ProfilePicture.Layer.CornerRadius = ProfilePicture.Frame.Size.Height / 2;
                ProfilePicture.Layer.MasksToBounds = true;
                ProfilePicture.ContentMode = UIViewContentMode.ScaleAspectFill;
            }
            else
            {
                if (AgoraSettings.Current?.IsGroupCall == true)
                    ProfilePicture.Image = UIImage.FromBundle("ic_group.png");
                else
                    ProfilePicture.Image = UIImage.FromBundle("ic_call_profile.png");
            }
        }

        private void InitializeAgoraEngine()
        {
            AgoraDelegate = new AgoraRtcDelegate(this);
            AgoraKit = AgoraRtcEngineKit.SharedEngineWithAppIdAndDelegate(AgoraTestConstants.AgoraAPI, AgoraDelegate);
            AgoraKit.EnableAudio();
            AgoraKit.EnableAudioVolumeIndication(1200, 3, true);
            AgoraKit.SetDefaultAudioRouteToSpeakerphone(true);
            AgoraKit.EnableVideo();
        }

        private void SetupLocalVideo()
        {
            AgoraRtcVideoCanvas localVideoCanvas = new AgoraRtcVideoCanvas();
            localVideoCanvas.Uid = 0;
            localVideoCanvas.View = LocalView;
            localVideoCanvas.View.BackgroundColor = LocalView.BackgroundColor;
            localVideoCanvas.RenderMode = VideoRenderMode.Hidden;
            LocalView.BackgroundColor = LocalView.BackgroundColor;
            AgoraKit.SetupLocalVideo(localVideoCanvas);
            if (!string.IsNullOrEmpty(AgoraSettings.Current.EncryptionPhrase))
            {
                AgoraKit.SetEncryptionMode(AgoraSettings.Current.EncryptionType.GetModeString());
                AgoraKit.SetEncryptionSecret(AgoraSettings.Current.EncryptionPhrase);
            }
            AgoraKit.StartPreview();

        }

        private void JoinChannel()
        {
            AgoraKit.JoinChannelByToken(AgoraTestConstants.Token, AgoraSettings.Current.RoomName, null, 0, JoiningCompleted);

        }

        private void LocalViewDragGestureRecognizer()
        {
            LocalView.AddGestureRecognizer(new UIPanGestureRecognizer(panGesture =>
            {
                if ((panGesture.State == UIGestureRecognizerState.Began || panGesture.State == UIGestureRecognizerState.Changed) && (panGesture.NumberOfTouches == 1))
                {
                    var p0 = panGesture.LocationInView(View);
                    var p1 = new PointF((float)p0.X, (float)p0.Y);
                    if (p1.X < 40)
                        p1.X = 40;
                    if (p1.X > View.Layer.Frame.Width - 40)
                        p1.X = (float)View.Layer.Frame.Width - 40;

                    if (p1.Y < LocalView.Layer.Frame.Height + 40)
                        p1.Y = (float)LocalView.Layer.Frame.Height + 40;
                    if (p1.Y > View.Layer.Frame.Height - LocalView.Layer.Frame.Height - 40)
                        p1.Y = (float)(View.Layer.Frame.Height - LocalView.Layer.Frame.Height - 40);
                        LocalView.Center = p1;
                }

                else if (panGesture.State == UIGestureRecognizerState.Ended)
                {
                    UIView.Animate(0.5, () =>
                    {
                        if (LocalView.Center.X >= View.Layer.Frame.Width / 2)
                            LocalView.Center = new PointF((float)View.Layer.Frame.Width - 70, (float)LocalView.Center.Y);
                        else
                            LocalView.Center = new PointF(70, (float)LocalView.Center.Y);

                    }, null);
                }
                CalculateLocalViewConstraints((float)LocalView.Center.X - (float)LocalView.Layer.Frame.Width / 2, (float)LocalView.Center.Y - (float)LocalView.Layer.Frame.Height / 2);

            }));

        }

        private void CalculateLocalViewConstraints(float x, float y)
        {
            NSLayoutConstraint.DeactivateConstraints(new NSLayoutConstraint[] {viewMarginTopConstraint , viewMarginRightConstraint });
            viewMarginTopConstraint = NSLayoutConstraint.Create(LocalView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, y);
            viewMarginRightConstraint = NSLayoutConstraint.Create(LocalView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, x);
            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[] { viewMarginTopConstraint, viewMarginRightConstraint });
        }

        private void JoiningCompleted(NSString channel, nuint uid, nint elapsed)
        {
            try
            {
                UIApplication.SharedApplication.IdleTimerDisabled = true;
            }
            catch (Exception)
            {
            }
            if (AgoraSettings.Current.IsCallingByMe && participants.Count == 0)
                PlayDialing();
        }

        public void FirstRemoteVideoDecodedOfUid(AgoraRtcEngineKit engine, nuint uid, CoreGraphics.CGSize size, nint elapsed)
        {
            InitRemoteVideoCanvas(uid);
        }

        private void InitRemoteVideoCanvas(nuint uid)
        {
            if (uid == 0)
            {
                ContainerView.Hidden = true;
                return;
            }
            videoCanvas = new AgoraRtcVideoCanvas();
            videoCanvas.Uid = uid;
            videoCanvas.View = ContainerView;
            videoCanvas.View.BackgroundColor = LocalView.BackgroundColor;
            videoCanvas.RenderMode = VideoRenderMode.Hidden;
            AgoraKit.SetupRemoteVideo(videoCanvas);
            if (ContainerView.Hidden)
            {
                ContainerView.Hidden = false;
            }

        }

        public void DidVideoMuted(AgoraRtcEngineKit engine, bool muted, nuint uid)
        {
            participants[uid] = muted;
            if (!muted)
                InitRemoteVideoCanvas(uid);
            else if(videoCanvas == null || videoCanvas.Uid == uid)
                InitRemoteVideoCanvas(GetAnotherUserId());
        }

        private nuint GetAnotherUserId()
        {
            if (participants.Count == 0) return 0;
            foreach (nuint id in participants.Keys)
                if (participants[id] == false)
                    return id;
            return 0;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
        }

        partial void MicrophoneBtn_Clicked(UIButton sender)
        {
            AudioMuted = !AudioMuted;
        }

        private void UpdateMicrophoneBtnIcon()
        {
            if (AudioMuted)
                MicrophoneBtn.SetImage(UIImage.FromBundle("ic_turn_on_mic.png"), UIControlState.Normal);
            else
                MicrophoneBtn.SetImage(UIImage.FromBundle("ic_turn_off_mic.png"), UIControlState.Normal);
        }

        partial void CameraSwitchBtn_Clicked(UIButton sender)
        {
            if(!VideoMuted)
                AgoraKit?.SwitchCamera();
        }

        partial void SpeakerBtn_Clicked(UIButton sender)
        {
            SpeakerEnabled = !SpeakerEnabled;
        }

        private void UpdateSpeakerBtnIcon()
        {
            if (SpeakerEnabled)
                SpeakerBtn.SetImage(UIImage.FromBundle("ic_turn_off_speaker.png"), UIControlState.Normal);
            else
                SpeakerBtn.SetImage(UIImage.FromBundle("ic_turn_on_speaker.png"), UIControlState.Normal);
        }

        partial void CameraStatusBtn_Clicked(UIButton sender)
        {
            VideoMuted = !VideoMuted; //VideoMuted: audio call
        }

        private void UpdateCameraBtnIcon()
        {
            if (VideoMuted)
                CameraStatusBtn.SetImage(UIImage.FromBundle("ic_turn_on_camera.png"), UIControlState.Normal);
            else 
                CameraStatusBtn.SetImage(UIImage.FromBundle("ic_turn_off_camera.png"), UIControlState.Normal);
        }

        partial void EndCallBtn_Clicked(UIButton sender)
        {
            FinishCall(true, false);
        }


        private void FinishPage()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                window = UIApplication.SharedApplication?.KeyWindow;
                if (window == null || AppDelegate.UIViewController ==null) return;
                if (window.Subviews?.Length > 0)
                    window.Subviews[0].RemoveFromSuperview();
                window.RootViewController = AppDelegate.UIViewController;
                DependencyService.Get<IStatusBarColor>().SetStatusbarColor(DesignResourceManager.GetColorFromStyle("Color1"));
            });
           
        }

        private void DisposeAgora()
        {

            if (AgoraKit != null)
            {
                try
                {
                    AgoraKit.LeaveChannel(null);
                    AgoraKit.DisableVideo();
                    AgoraKit.StopPreview();
                    AgoraKit.SetupLocalVideo(null);
                    AgoraKit.Dispose();
                }
                catch (Exception)
                {
                }
                AgoraKit = null;
            }
        }

        public void OnUserOffline(uint uid, int reason)
        {
            participants.Remove(uid);
        }

        public void OnUserJoined(uint uid, int reason)
        {
            _isAnyoneJoined = true;
            StopDialing();
            participants.Add(uid, false);
            if (Instance != null && AgoraSettings.Current.IsCallingByMe && timer == null)
            {
                InitTimer();
                AnimateUsernameLabel();
            }
        }

        private void AnimateUsernameLabel()
        {
            if (ProfilePicture.Hidden) return;
            ProfilePicture.Hidden = true;
            LocalView.Layer.CornerRadius = 9;
            LocalView.Layer.MasksToBounds = true;
            SetSmallLocalViewPlace();
            LocalViewDragGestureRecognizer();
            UIView.Animate(0.5, () =>
            {
                Username.Center = new PointF(70, 70);
                Duration.Center = new PointF(70, 120);
                LocalView.Center = new PointF((float)ContainerView.Layer.Frame.Width,(float)ContainerView.Layer.Frame.Height);

            }, null);

        }
     
        public void FinishCall(bool sendNotification, bool isCallKitDestroyed = true)
        {
            App.DisableProximitySensor();
            StopDialing();
            DisposeAgora();
            if (Instance != null && sendNotification)
            {
                SendEndCallNotification();
            }
            DisposeTimer();
            LeaveCall(isCallKitDestroyed);
            Instance = null;
        }

        private void DisposeTimer()
        {
            if (timer != null)
            {
                timer.Invalidate();
                timer.Dispose();
                timer = null;
            }
        }

        private void SendEndCallNotification()
        {
            if (!_isAnyoneJoined
                ||(participants.Count <= 1 && AgoraSettings.Current.IsCallingByMe && !AgoraSettings.Current.IsGroupCall)
                || (participants.Count == 0 && AgoraSettings.Current.IsGroupCall))
                DependencyService.Get<ICallNotificationService>().FinishCall(AgoraSettings.Current.RoomName, !AgoraSettings.Current.IsAudioCall, AgoraSettings.Current.IsCallingByMe, Instance.DurationInSecond);
        }


        private void LeaveCall(bool isCallKitDestroyed)
        {
           if (!isCallKitDestroyed && AppDelegate.Instance?.CallManager?.Calls?.Count > 0)
                    AppDelegate.Instance.CallManager.EndCall(AppDelegate.Instance.CallManager.Calls[0]);
            FinishPage();
        }

        public void DidOfflineOfUid(AgoraRtcEngineKit engine, nuint uid, UserOfflineReason reason)
        {
            ContainerView.Hidden = true;
            participants.Remove(Convert.ToUInt32(uid));
            if (participants.Count == 0)
                FinishCall(true, false);
        }

        public void FirstLocalVideoFrameWithSize(AgoraRtcEngineKit engine, CoreGraphics.CGSize size, nint elapsed)
        {
           
        }

        private void PlayDialing()
        {
            NSUrl songURL;
            songURL = new NSUrl("Sounds/dialing.mp3");
            NSError err;
            player = new AVAudioPlayer(songURL, "Song", out err);
            player.Volume = 0.5f;
            player.NumberOfLoops = 100;
            player.FinishedPlaying += delegate
            {
                player = null;
            };
            player.Play();
            AVAudioSession session = AVAudioSession.SharedInstance();
            session.SetCategory(AVAudioSessionCategory.PlayAndRecord);
            session.SetActive(true);
        }

        private void StopDialing()
        {
            if (player != null)
            {
                player.Stop();
                player = null;
            }
        }


        private void SetSmallLocalViewPlace()
        {
            try
            {
                NSLayoutConstraint.DeactivateConstraints(GetLocalViewConstraints());
                NSLayoutConstraint.DeactivateConstraints(new NSLayoutConstraint[] { viewWidth, viewHeight, viewMarginTopConstraint });
                viewWidth = NSLayoutConstraint.Create(LocalView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Width, 0.3f, 0f);
                viewHeight = NSLayoutConstraint.Create(LocalView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Height, 0.21f, 0f);
                viewMarginTopConstraint = NSLayoutConstraint.Create(LocalView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, -165f);
                viewMarginRightConstraint = NSLayoutConstraint.Create(LocalView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, -10f);

                var usernameMarginTopConstraint = NSLayoutConstraint.Create(Username, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, 70f);
                var ringingMarginTopConstraint = NSLayoutConstraint.Create(Duration, NSLayoutAttribute.Top, NSLayoutRelation.Equal, Username, NSLayoutAttribute.Bottom, 1f, 8f);
                var usernameMarginLeftConstraint = NSLayoutConstraint.Create(Username, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, 30f);
                var ringingMarginLeftConstraint = NSLayoutConstraint.Create(Duration, NSLayoutAttribute.Left, NSLayoutRelation.Equal, Username, NSLayoutAttribute.Left, 1f, 0f);

                NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[] { viewWidth, viewHeight, viewMarginTopConstraint,viewMarginRightConstraint,
                    usernameMarginTopConstraint, ringingMarginTopConstraint, usernameMarginLeftConstraint, ringingMarginLeftConstraint });
            }
            catch (Exception)
            {

            }
        }

        private NSLayoutConstraint[] GetLocalViewConstraints()
        {
            List<NSLayoutConstraint> constraints = new List<NSLayoutConstraint>();
            foreach (NSLayoutConstraint constraint in View.Constraints)
            {
                if (constraint.SecondItem == LocalView)
                    constraints.Add(constraint);
            }
            return constraints.ToArray();
        }


        private void SetFullScreenLocalViewPlace()
        {
            try
            {
                viewWidth = NSLayoutConstraint.Create(LocalView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Width, 1f, 0f);
                viewHeight = NSLayoutConstraint.Create(LocalView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Height, 1f, 0f);
                viewMarginTopConstraint = NSLayoutConstraint.Create(LocalView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, 0f);
                NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[] { viewWidth, viewHeight, viewMarginTopConstraint });
            }
            catch (Exception)
            {

            }
        }

        private bool AskMicrophonePermission()
        {
            bool response = false;
            AVCaptureDevice.RequestAccessForMediaType(AVMediaType.Audio, (bool isAccessGranted) =>
            {
                response = isAccessGranted;
            });
            return response;
        }
       
        public void UpdateRingingStatus()
        {
            if (Instance != null)
                Instance.Duration.Text = Localization.Resources.Dictionary.Ringing;
        }

        private void InitTimer()
        {
            Instance.DurationInSecond = 0;
            timer = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromSeconds(1.0), delegate {
            if (Instance != null)
                Duration.Text = Utils.Utils.FormatTime(++Instance.DurationInSecond);
            });
        }

        public void ChangeSpeaker(bool isEarpiece)
        {
            if (player != null && Instance !=null)
            {
                Instance.AgoraKit?.SetEnableSpeakerphone(!isEarpiece);
            }
        }
    }
}