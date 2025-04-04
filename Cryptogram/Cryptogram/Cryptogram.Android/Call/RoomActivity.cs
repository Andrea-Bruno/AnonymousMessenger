using System;
using System.Collections.Generic;
using System.Timers;
using Android;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using DT.Xamarin.Agora;
using DT.Xamarin.Agora.Video;
using Cryptogram.CallHandler;
using Cryptogram.CallHandler.Helpers;
using Cryptogram.Droid.Call;
using Cryptogram.Droid.CustomViews;
using Cryptogram.Droid.Services;
using Cryptogram.Services;
//using Xamarin.Essentials;
using static Android.Animation.ValueAnimator;

[assembly: Xamarin.Forms.Dependency(typeof(RoomActivity))]
namespace Cryptogram.Droid.Call
{
    [Activity(Label = "Cryptogram", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/DT.Theme.Room", LaunchMode = LaunchMode.SingleTop)]
    public class RoomActivity : Activity, IAudioPlayerSpeakerService
    {
        protected RtcEngine AgoraEngine;
        protected AgoraRtcHandler AgoraHandler;
        private SurfaceView localSurfaceView;
        private VideoCanvas remoteVideoCanvas;
        private int _localId = 0;
        private static bool isAnyoneJoined;
        private static Dictionary<int, bool> participants;
        public static RoomActivity Instance;
        private MediaPlayer dialingPlayer;
        private int durationInSecond;
        private Timer timer;
        private TextView username, duration;
        private RelativeLayout localView, bottomBar, mainContainer, activityVideoChatView;
        private LinearLayout defButtonLayout;
        private ImageView speakerButton, cameraButton, endButton, microphoneButton, profilePicture;
        private FrameLayout localViewContainer, remoteViewContainer;
        private DisplayMetrics displayMetrics;
        private float dX, dY;

        private bool isLocalVideoContainerMinimized = false;
        private bool isLabelsMoved = false;

        private bool _audioMuted = false;
        private bool _videoMuted = false;
        private bool _isSpeakerEnable = false;
        private SurfaceView remoteSurfaceView;

        private bool AudioMuted
        {
            get
            {
                return _audioMuted;
            }
            set
            {
                _audioMuted = value;
                AgoraEngine?.MuteLocalAudioStream(value);
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

                localView.Visibility = value ? ViewStates.Gone : ViewStates.Visible;
                AgoraEngine?.EnableLocalVideo(!value);
                AgoraEngine?.MuteLocalVideoStream(value);
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
                AgoraEngine?.SetEnableSpeakerphone(value);
                UpdateSpeakerBtnIcon();
            }
        }



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Room);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Instance = this;
            participants = new Dictionary<int, bool>();
            displayMetrics = Resources.DisplayMetrics;
            InitViews();
            InitBottomBar();
            SetLabels();
            SetAvatar();
            AskRequiredPermissions();
            DisableStandby();
            AndroidNotificationManager.GetInstance().ScheduleOnGoingCallNotification("Call is going on", AgoraSettings.Current.RoomName, AgoraSettings.Current.Username);
            StartService(new Intent(this, typeof(TaskKilledService)));
        }



        private void InitViews()
        {
            mainContainer = FindViewById<RelativeLayout>(Resource.Id.main_container);
            activityVideoChatView = FindViewById<RelativeLayout>(Resource.Id.activity_video_chat_view);
            username = FindViewById<TextView>(Resource.Id.username);
            duration = FindViewById<TextView>(Resource.Id.state);
            profilePicture = FindViewById<ImageView>(Resource.Id.avatar);

            defButtonLayout = FindViewById<LinearLayout>(Resource.Id.bottomButtonsly);
            bottomBar = FindViewById<RelativeLayout>(Resource.Id.bottomBar);
            cameraButton = FindViewById<ImageView>(Resource.Id.mute_video_button);
            speakerButton = FindViewById<ImageView>(Resource.Id.mute_speaker);
            endButton = FindViewById<ImageView>(Resource.Id.end_call_button);
            microphoneButton = FindViewById<ImageView>(Resource.Id.mute_audio_button);

            localView = FindViewById<RelativeLayout>(Resource.Id.local_video_container);
            localViewContainer = FindViewById<FrameLayout>(Resource.Id.local_video_view_container);
            remoteViewContainer = FindViewById<FrameLayout>(Resource.Id.remote_video_view_container);

            localView.Touch += LocalVideoTouch;
        }

        private void SetLabels()
        {
            username.Text = AgoraSettings.Current.Username;
            username.Post(() =>
            {
                username.SetX(displayMetrics.WidthPixels / 2 - username.Width / 2);

            });
            if (!AgoraSettings.Current.IsCallingByMe)
            {

                InitTimer();
                RunOnUiThread(() =>
                {
                    if (!isLabelsMoved)
                        username.Post(() =>
                        {
                            AnimateLabels();
                        });
                });
                isAnyoneJoined = true;
            }
            else
            {
                ChangeStateLabelText(Localization.Resources.Dictionary.Calling);
            }
        }

        private void SetAvatar()
        {
            if (AgoraSettings.Current?.Avatar != null)
                SetBitmapAvatar(AgoraSettings.Current?.Avatar);
            else
            {
                if (AgoraSettings.Current?.IsGroupCall == true)
                    profilePicture.SetImageResource(Resource.Drawable.ic_new_addGroup_contact);
                else
                    profilePicture.SetImageResource(Resource.Drawable.ic_call_profile);
            }

        }

        private void SetBitmapAvatar(byte[] array)
        {
            try
            {
                Bitmap bmp = BitmapFactory.DecodeByteArray(array, 0, array.Length);
                profilePicture.SetImageBitmap(bmp);
                profilePicture.SetScaleType(ImageView.ScaleType.CenterCrop);
            }
            catch (Exception e)
            {
                profilePicture.SetImageResource(Resource.Drawable.ic_call_profile);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            AndroidNotificationManager.GetInstance().CancelCallNotification(AgoraSettings.Current.RoomName);
            AndroidNotificationManager.GetInstance().DisableVibratorRinging();
            if (VideoMuted)
                App.ListenProximitySensor();
            Instance = this;
        }


        protected override void OnPause()
        {
            base.OnPause();
            App.DisableProximitySensor();
        }

        private void InitBottomBar()
        {
            bottomBar.Post(() =>
            {

                int screenWitdh = bottomBar.Width;
                int marginright = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 16, displayMetrics);
                int bottomBarHeight = (int)(screenWitdh / 2.81f);
                int defbtnHeight = (int)(bottomBarHeight / 3.05f);
                int endBtnHeight = (int)(bottomBarHeight / 2.57f);
                int defBtnLyBtmMrgn = (defbtnHeight / 5) * 3;
                int mainContainerBtmMrgn = (int)(bottomBarHeight / 2.09f);
                int localVideoContainerBtmMrgn = bottomBarHeight + marginright;

                RelativeLayout.LayoutParams bottomBarParams = (RelativeLayout.LayoutParams)bottomBar.LayoutParameters;
                bottomBarParams.Height = bottomBarHeight;
                bottomBar.LayoutParameters = bottomBarParams;

                RelativeLayout.LayoutParams defButtonLyLp = (RelativeLayout.LayoutParams)defButtonLayout.LayoutParameters;
                defButtonLyLp.BottomMargin = defBtnLyBtmMrgn;
                defButtonLyLp.Height = defbtnHeight;
                defButtonLayout.LayoutParameters = defButtonLyLp;

                ViewGroup.LayoutParams endbtnparams = endButton.LayoutParameters;
                endbtnparams.Height = endBtnHeight;
                endbtnparams.Width = endBtnHeight;
                endButton.LayoutParameters = endbtnparams;

                RelativeLayout.LayoutParams mainContainerLp = (RelativeLayout.LayoutParams)mainContainer.LayoutParameters;
                mainContainerLp.BottomMargin = mainContainerBtmMrgn;
                mainContainer.LayoutParameters = mainContainerLp;

            });
        }

        private void AskRequiredPermissions()
        {
            ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.RecordAudio, Manifest.Permission.Camera, Manifest.Permission.WriteExternalStorage }, 1);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            switch (requestCode)
            {
                case 1:
                    {
                        if (grantResults.Length > 0 && grantResults[0] == (int)Permission.Granted && grantResults[1] == (int)Permission.Granted)
                        {
                            InitAgoraEngineAndJoinChannel();
                        }
                        else
                        {
                            Toast.MakeText(this, Localization.Resources.Dictionary.RequestedPermissionIsNeeded, ToastLength.Short).Show();
                            InitAgoraEngineAndJoinChannel();
                        }
                    }
                    break;
            }
        }

        public void OnFirstRemoteVideoDecoded(int uid, int width, int height, int elapsed)
        {
            RunOnUiThread(() =>
            {
                if (AgoraHandler == null)
                    InitAgoraEngineAndJoinChannel();
                SetupRemoteVideo(uid);
                if (!isLocalVideoContainerMinimized)
                    MinimizeLocalVideoContainer(localView);
            });
        }

        public void OnUserOffline(int uid, int reason)
        {
            RunOnUiThread(() =>
            {
                OnRemoteUserLeft(uid);
            });
        }

        public void OnUserJoined(int uid, int reason)
        {
            participants.Add(uid, false);
            DisableRinging();
            isAnyoneJoined = true;
            if (AgoraSettings.Current.IsCallingByMe && timer == null)
            {

                InitTimer();
                RunOnUiThread(() =>
                {
                    if (!isLabelsMoved)
                        username.Post(() =>
                        {
                            AnimateLabels();
                        });
                });
            }
        }

        public void OnJoinChannelSuccess(string channel, int uid, int elapsed)
        {
            if (Instance == null) return;
            if (_localId != uid && participants.Count == 0 && AgoraSettings.Current.IsCallingByMe && Instance.dialingPlayer == null)
            {
                _localId = uid; // prevent to go this if statement twice
                SetRinging();
            }
            _localId = uid;
        }

        public void OnFirstLocalVideoFrame(float height, float width, int p2)
        {

        }

        private void InitDefaultValues()
        {
            VideoMuted = AgoraSettings.Current.IsAudioCall;
            AudioMuted = false;
            SpeakerEnabled = !AgoraSettings.Current.IsAudioCall;
        }

        private void InitAgoraEngineAndJoinChannel()
        {
            InitializeAgoraEngine();
            InitRemoteVideoSurfaceView();
            SetupVideoProfile();
            JoinChannel();
            SetupLocalVideo();
            InitDefaultValues();

        }

        private void InitializeAgoraEngine()
        {
            AgoraHandler = AgoraRtcHandler.GetInstance(this);
            AgoraEngine = RtcEngine.Create(BaseContext, AgoraTestConstants.AgoraAPI, AgoraHandler);
            AgoraEngine.EnableAudioVolumeIndication(1200, 3, false);
            AgoraEngine.SetDefaultAudioRoutetoSpeakerphone(true);
        }

        private void JoinChannel()
        {
            AgoraEngine.JoinChannel(AgoraTestConstants.Token, AgoraSettings.Current.RoomName, string.Empty, 0); // if you do not specify the uid, we will generate the uid for you
        }

        private void SetupLocalVideo()
        {
            localSurfaceView = RtcEngine.CreateRendererView(BaseContext);
            localSurfaceView.SetZOrderMediaOverlay(true);

            localSurfaceView.SetMinimumHeight(localViewContainer.Height);
            localSurfaceView.SetMinimumWidth(localViewContainer.Width);
            localViewContainer.AddView(localSurfaceView);
            VideoCanvas videoCanvas = new VideoCanvas(localSurfaceView, VideoCanvas.RenderModeHidden, 0);
            AgoraEngine.SetupLocalVideo(videoCanvas);
            if (!string.IsNullOrEmpty(AgoraSettings.Current.EncryptionPhrase))
            {
                AgoraEngine.SetEncryptionMode(AgoraSettings.Current.EncryptionType.GetModeString());
                AgoraEngine.SetEncryptionSecret(AgoraSettings.Current.EncryptionPhrase);
            }
            AgoraEngine.StartPreview();
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            AndroidNotificationManager.GetInstance().CancelOnGoingCallNotification();
            if (Instance != null)
                EndCall(true);
            App.DisableProximitySensor();
        }

        [Java.Interop.Export("OnLocalVideoMuteClicked")]
        public void OnLocalVideoMuteClicked(View view)
        {
            VideoMuted = !VideoMuted;
        }

        private void UpdateCameraBtnIcon()
        {
            if (VideoMuted)
                cameraButton.SetImageResource(Resource.Drawable.ic_turn_on_camera);
            else
                cameraButton.SetImageResource(Resource.Drawable.ic_turn_off_camera);

        }

        [Java.Interop.Export("OnLocalSpeakerClicked")]
        public void OnLocalSpeakerClicked(View view)
        {
            SpeakerEnabled = !SpeakerEnabled;
        }

        private void UpdateSpeakerBtnIcon()
        {
            if (SpeakerEnabled)
                speakerButton.SetImageResource(Resource.Drawable.ic_speaker_off);
            else
                speakerButton.SetImageResource(Resource.Drawable.ic_speaker_on);
        }

        [Java.Interop.Export("OnLocalAudioMuteClicked")]
        public void OnLocalAudioMuteClicked(View view)
        {
            AudioMuted = !AudioMuted;
        }

        private void UpdateMicrophoneBtnIcon()
        {
            if (AudioMuted)
                microphoneButton.SetImageResource(Resource.Drawable.ic_turn_on_mic);
            else
                microphoneButton.SetImageResource(Resource.Drawable.ic_turn_off_mic);
        }

        [Java.Interop.Export("OnSwitchCameraClicked")]
        public void OnSwitchCameraClicked(View view)
        {
            if (!VideoMuted)
                AgoraEngine?.SwitchCamera();
        }

        [Java.Interop.Export("OnEndCallClicked")]
        public void OnEndCallClicked(View view)
        {
            EndCall(true);
        }

        public void EndCall(bool sendNotification)
        {
            App.DisableProximitySensor();
            DisableRinging();
            DisposeAgora();
            if (Instance != null && sendNotification)
            {
                Instance.SendEndCallNotification();
            }
            DisposeTimer();
            FinishPage();
            Instance = null;
            AndroidNotificationManager.GetInstance().CancelOnGoingCallNotification();

        }

        private void SendEndCallNotification()
        {
            if (!isAnyoneJoined
                || (participants.Count <= 1 && AgoraSettings.Current.IsCallingByMe && !AgoraSettings.Current.IsGroupCall)
                || (participants.Count == 0 && AgoraSettings.Current.IsGroupCall))
                Xamarin.Forms.DependencyService.Get<ICallNotificationService>().FinishCall(AgoraSettings.Current.RoomName, !AgoraSettings.Current.IsAudioCall, AgoraSettings.Current.IsCallingByMe, Instance.durationInSecond);

        }

        private void DisposeTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
        }

        private void DisposeAgora()
        {
            if (AgoraEngine != null)
            {
                try
                {
                    AgoraEngine.DisableVideo();
                    AgoraEngine.StopPreview();
                    AgoraEngine.SetupLocalVideo(null);
                    AgoraEngine.LeaveChannel();
                    AgoraEngine.Dispose();
                }
                catch (Exception)
                {
                }
                AgoraEngine = null;
            }
        }

        private void FinishPage()
        {
            if (IsTaskRoot)
                FinishAndRemoveTask();
            else
                Finish();
        }

        private void OnRemoteUserLeft(int uid)
        {
            participants.Remove(uid);
            if (participants.Count == 0)
                EndCall(true);
            else
                SetupRemoteVideo(GetAnotherUserId());
        }

        private void SetupVideoProfile()
        {
            if (AgoraEngine != null)
            {
                AgoraEngine.EnableVideo();
                AgoraEngine.SetVideoProfile(AgoraSettings.Current.UseMySettings ? AgoraSettings.Current.Profile : Constants.VideoProfile720p, true);
            }
        }

        private void InitRemoteVideoSurfaceView()
        {
            remoteSurfaceView = remoteViewContainer.ChildCount == 0 ? RtcEngine.CreateRendererView(BaseContext) : remoteViewContainer.GetChildAt(0) as SurfaceView;
            remoteSurfaceView.SetMinimumHeight(remoteViewContainer.Height);
            remoteSurfaceView.SetMinimumWidth(remoteViewContainer.Width);
            if (remoteViewContainer.ChildCount == 0)
                remoteViewContainer.AddView(remoteSurfaceView);
        }

        private void SetupRemoteVideo(int uid)
        {
            RunOnUiThread(() =>
            {
                if (uid == 0)
                {
                    remoteViewContainer.Visibility = ViewStates.Gone;
                    return;
                }
                else
                    remoteViewContainer.Visibility = ViewStates.Visible;

                if (AgoraEngine != null)
                {
                    try
                    {
                        remoteVideoCanvas = new VideoCanvas(remoteSurfaceView, VideoCanvas.RenderModeHidden, uid);
                        AgoraEngine.SetupRemoteVideo(remoteVideoCanvas);
                    }
                    catch (Exception)
                    {

                    }
                }
            });
        }

        public void OnUserMuteVideo(int uid, bool muted)
        {
            participants[uid] = muted;
            if (!muted)
                SetupRemoteVideo(uid);
            else if (remoteVideoCanvas == null || remoteVideoCanvas.Uid == uid)
                SetupRemoteVideo(GetAnotherUserId());
        }


        private int GetAnotherUserId()
        {
            if (participants.Count == 0) return 0;
            foreach (int id in participants.Keys)
                if (participants[id] == false)
                    return id;
            return 0;
        }

        public void UpdateRingingStatus()
        {
            if (Instance != null)
                RunOnUiThread(() =>
                {
                    try
                    {
                        Instance.duration.Text = Localization.Resources.Dictionary.Ringing;
                    }
                    catch (Exception) { }

                });
        }

        private void DisableStandby()
        {
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
        }

        private void SetRinging()
        {
            if (Instance == null) return;
            try
            {
                Instance.dialingPlayer = new Android.Media.MediaPlayer();
                Android.Net.Uri uri = Android.Net.Uri.Parse("android.resource://" + PackageName + "/" + Resource.Raw.dialing);
                Instance.dialingPlayer.SetDataSource(this, uri);
                Instance.dialingPlayer.Looping = true;
                Instance.dialingPlayer.SetAudioAttributes(
                                   new AudioAttributes
                                      .Builder()
                                      .SetContentType(AudioContentType.Speech)
                                      .SetUsage(AudioUsageKind.VoiceCommunication)
                                      .Build());
                Instance.dialingPlayer.Prepare();
                Instance.dialingPlayer.Start();
            }
            catch (Exception) { }

        }

        private void DisableRinging()
        {
            if (Instance == null) return;
            if (Instance.dialingPlayer != null)
            {
                Instance.dialingPlayer.Stop();
                Instance.dialingPlayer.Release();
                Instance.dialingPlayer = null;
            }
        }

        public override void OnBackPressed()
        {
            // base.OnBackPressed();
        }

        private void InitTimer()
        {
            Instance.durationInSecond = 0;
            if (timer == null)
            {
                timer = new Timer
                {
                    Interval = 1000
                };
                timer.Enabled = true;
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
            }

        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Instance.durationInSecond++;
            Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Instance != null)
                    ChangeStateLabelText(Utils.Utils.FormatTime(Instance.durationInSecond));
            });
        }

        public void ChangeSpeaker(bool isEarpieceEnabled)
        {
            if (Instance != null)
            {
                try
                {
                    if (isEarpieceEnabled)
                        AudioPlayerManager.WakeLock.Acquire();
                    else
                    {
                        if (AudioPlayerManager.WakeLock.IsHeld)
                            AudioPlayerManager.WakeLock.Release();
                    }
                }
                catch (Java.Lang.Throwable)
                {
                }
            }
        }

        private void MinimizeLocalVideoContainer(View v)
        {

            DisplayMetrics displayMetrics = Resources.DisplayMetrics;

            ValueAnimator valueAnimator = ValueAnimator.OfFloat(1, 0);
            valueAnimator.SetDuration(500L);

            int currentX = (int)v.GetX();
            int currentY = (int)v.GetY();

            int desiredWidth = CalculateViewSize(120);
            int desiredHeight = CalculateViewSize(172);

            int marginbtm = CalculateViewSize(16);
            int marginright = CalculateViewSize(16);

            int desiredX = displayMetrics.WidthPixels - desiredWidth - marginright;
            int desiredY = (int)bottomBar.GetY() - desiredHeight - marginbtm;

            int witdhDif = displayMetrics.WidthPixels - desiredWidth;
            int hDif = mainContainer.Height - desiredHeight;

            valueAnimator.Update += ValueAnimator_Update;
            valueAnimator.AnimationEnd += (sender, args) =>
            {
                isLocalVideoContainerMinimized = true;
            };


            valueAnimator.Start();
            void ValueAnimator_Update(object sender, AnimatorUpdateEventArgs e)
            {
                float value = (float)valueAnimator.AnimatedValue;
                ViewGroup.LayoutParams layoutParams = v.LayoutParameters;

                layoutParams.Height = desiredHeight + Math.Abs((int)(hDif * value));
                layoutParams.Width = desiredWidth + Math.Abs((int)(witdhDif * value));

                v.LayoutParameters = layoutParams;

                v.SetX((int)((1f - value) * desiredX));
                v.SetY((int)((1f - value) * desiredY));

            }
            Instance?.localView.SetBackgroundResource(Resource.Drawable.localview_round_corner);
            Instance?.localViewContainer.SetBackgroundResource(Resource.Drawable.localview_round_corner);

        }


        private void AnimateLabels()
        {
            profilePicture.Visibility = ViewStates.Gone;
            float desiredX = CalculateViewSize(16);
            float desiredy = desiredX;


            ObjectAnimator animX = ObjectAnimator.OfFloat(username, "x", desiredX);
            ObjectAnimator animy = ObjectAnimator.OfFloat(username, "y", desiredy);
            ObjectAnimator animX2 = ObjectAnimator.OfFloat(duration, "x", desiredX);
            ObjectAnimator animy2 = ObjectAnimator.OfFloat(duration, "y", desiredy + desiredX + username.Height);


            AnimatorSet animSetXY = new AnimatorSet();
            animSetXY.PlayTogether(animX, animy, animX2, animy2);
            animSetXY.SetDuration(500L);
            animSetXY.AnimationEnd += (p1, p2) =>
            {
                isLabelsMoved = true;
            };
            animSetXY.Start();
        }


        private int CalculateViewSize(float value)
        {
            if (displayMetrics == null)
                displayMetrics = Resources.DisplayMetrics;
            return (int)TypedValue.ApplyDimension(
                    ComplexUnitType.Dip,
                    value,
                    displayMetrics
            );
        }

        private void ChangeStateLabelText(string text)
        {
            duration.Text = text;
            if (!isLabelsMoved)
                duration.Post(() =>
                {
                    duration.SetX(displayMetrics.WidthPixels / 2 - duration.Width / 2);

                });

        }

        private void LocalVideoTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (!isLocalVideoContainerMinimized) return;

                MotionEvent motionEvent = e.Event;
                View view = sender as View;
                switch (motionEvent.Action)
                {
                    case MotionEventActions.Down:
                        dX = view.GetX() - motionEvent.GetRawX(0);
                        dY = view.GetY() - motionEvent.GetRawY(0);
                        break;
                    case MotionEventActions.Move:
                        float finalX = motionEvent.GetRawX(0) + dX;
                        float finalY = motionEvent.GetRawY(0) + dY;
                        float[] bounds = IsViewInBounds(view, finalX, finalY);
                        view.Animate()
                                            .X(bounds[0])
                                            .Y(bounds[1])
                                            .SetDuration(0)
                                            .Start();
                        break;
                    case MotionEventActions.Up:
                        ObjectAnimator animator;
                        if (view.GetX() + (float)view.Width / 2 >= (float)activityVideoChatView.Width / 2)
                        {
                            animator = ObjectAnimator.OfFloat(
                                localView,
                                "x",
                                displayMetrics.WidthPixels - CalculateViewSize(16) - localView.Width
                                );
                        }
                        else
                        {
                            animator = ObjectAnimator.OfFloat(
                             localView,
                             "x",
                             CalculateViewSize(16)
                             );
                        }

                        animator.Start();
                        break;

                }
            }
            catch(Exception )
            {

            }

        }
        private float[] IsViewInBounds(View view, float x, float y)
        {
            float defMargin = CalculateViewSize(16);
            float minXValue = defMargin;
            float maxXValue = displayMetrics.WidthPixels - view.Width - defMargin;
            float minYValue = defMargin;
            float maxYValue = bottomBar.GetY() - view.Height - defMargin;

            if (x < minXValue)
                x = minXValue;
            if (x > maxXValue)
                x = maxXValue;
            if (y < minYValue)
                y = minYValue;
            if (y > maxYValue)
                y = maxYValue;

            return new float[] { x, y };
        }


    }
}