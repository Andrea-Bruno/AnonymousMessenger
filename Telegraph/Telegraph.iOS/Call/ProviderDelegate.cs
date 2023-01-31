using System;
using Foundation;
using CallKit;
using UIKit;
using Xamarin.Forms;
using Telegraph.CallHandler;
using Telegraph.Services;
using Telegraph.CallHandler.Helpers;

namespace Telegraph.iOS.Call
{
    public class ProviderDelegate : CXProviderDelegate
    {
        #region Computed Properties
        public ActiveCallManager CallManager { get; set; }
        public CXProviderConfiguration Configuration { get; set; }
        public CXProvider Provider { get; set; }
        #endregion

        #region Constructors
        public ProviderDelegate(ActiveCallManager callManager)
        {
            // Save connection to call manager
            CallManager = callManager;

            // Define handle types
            var handleTypes = new[] { (NSNumber)(int)CXHandleType.Generic };

            // Get Image Template
            var templateImage = UIImage.FromFile("group.png");

            // Setup the initial configurations
            Configuration = new CXProviderConfiguration("MonkeyCall")
            {
                MaximumCallsPerCallGroup = 1,
                SupportedHandleTypes = new NSSet<NSNumber>(handleTypes),
                SupportsVideo = true,
                //IconTemplateImageData = templateImage.AsPNG(),
                //RingtoneSound = "musicloop01.wav"
            };

            // Create a new provider
            Provider = new CXProvider(Configuration);

            // Attach this delegate
            Provider.SetDelegate(this, null);

        }
        #endregion

        #region Override Methods
        public override void DidReset(CXProvider provider)
        {
            // Remove all calls
            CallManager.Calls.Clear();
        }

        public override void PerformStartCallAction(CXProvider provider, CXStartCallAction action)
        {
            Console.WriteLine("PerformStartCallAction");
            // Create new call record
            /*
            // Monitor state changes
            activeCall.StartingConnectionChanged += (call) => {
                if (call.IsConnecting)
                {
                    // Inform system that the call is starting
                    Provider.ReportConnectingOutgoingCall(call.UUID, call.StartedConnectingOn.ToNSDate());
                }
            };

            activeCall.ConnectedChanged += (call) => {
                if (call.IsConnected)
                {
                    // Inform system that the call has connected
                    provider.ReportConnectedOutgoingCall(call.UUID, call.ConnectedOn.ToNSDate());
                }
            };

            // Start call
            activeCall.StartCall((successful) => {
                // Was the call able to be started?
                if (successful)
                {
                    // Yes, inform the system
                    action.Fulfill();

                    // Add call to manager
                    CallManager.Calls.Add(activeCall);
                }
                else
                {
                    // No, inform system
                    action.Fail();
                }
            });
            */
        }


        public override void PerformAnswerCallAction(CXProvider provider, CXAnswerCallAction action)
        {
            // Find requested call
            var call = CallManager.FindCall(action.CallUuid);

            // Found?
            if (call == null)
            {
                // No, inform system and exit
                action.Fail();
                return;
            }

            // Attempt to answer call
            call.AnswerCall((successful) => {
                // Was the call successfully answered?
                if (successful)
                {
                    string chatId = CallManager?.Calls?.Count > 1 ? CallManager?.Calls[1].ChatId : null; // Accept new call during the ot
                    // Yes, inform system
                    if (chatId!=null && chatId != call.ChatId)
                        DependencyService.Get<IEndCall>().FinishCall(CallManager?.Calls[0].ChatId);
                    if (chatId == null || chatId != call.ChatId)
                    {
                        DependencyService.Get<IAudioCallConnector>().Start(call.ChatId, call.Username, call.isVideoCall, false, false, null);
                        action.Fulfill();
                    }
                }
                else
                {
                    // No, inform system
                    action.Fail();
                }
            });
        }

        public override void PerformEndCallAction(CXProvider provider, CXEndCallAction action)
        {
            // Find requested call
            var call = CallManager.FindCall(action.CallUuid);

            // Found?
            if (call == null)
            {
                // No, inform system and exit
                action.Fail();
                return;
            }

            // Attempt to answer call
            call.EndCall((successful) => {
                // Was the call successfully answered?
                if (successful)
                {
                    // Remove call from manager's queue
                    CallManager.Calls.Remove(call);

                    // Yes, inform system
                    action.Fulfill();
                    if (CallRoomViewController.Instance != null && AgoraSettings.Current.RoomName == call.ChatId)
                        CallRoomViewController.Instance.FinishCall(true);
                    else
                        DependencyService.Get<ICallNotificationService>().DeclineCall(call.ChatId, true);
                }
                else
                {
                    // No, inform system
                    action.Fail();
                }
            });
        }

        public override void PerformSetHeldCallAction(CXProvider provider, CXSetHeldCallAction action)
        {
            // Find requested call
            var call = CallManager.FindCall(action.CallUuid);

            // Found?
            if (call == null)
            {
                // No, inform system and exit
                action.Fail();
                return;
            }
            PerformEndCallAction(provider, new CXEndCallAction(action.CallUuid));
        }

        public override void TimedOutPerformingAction(CXProvider provider, CXAction action)
        {
            Console.WriteLine("TimedOutPerformingAction");

            // Inform user that the action has timed out
        }

        public override void DidActivateAudioSession(CXProvider provider, AVFoundation.AVAudioSession audioSession)
        {
            Console.WriteLine("DidActivateAudioSession");
            // Start the calls audio session here
        }

        public override void DidDeactivateAudioSession(CXProvider provider, AVFoundation.AVAudioSession audioSession)
        {
            Console.WriteLine("DidDeactivateAudioSession");

            // End the calls audio session and restart any non-call
            // related audio
        }
        #endregion

        #region Public Methods
        public void ReportIncomingCall(NSUuid uuid, string handle, string chatId, bool isVideoCall, bool cancelCall)
        {
            // Create update to describe the incoming call and caller
            var update = new CXCallUpdate();
            update.RemoteHandle = new CXHandle(CXHandleType.Generic, handle);
            update.HasVideo = isVideoCall;
            // Report incoming call to system
            if (cancelCall)
            {
                Provider.ReportNewIncomingCallAsync(uuid, update);
                CallManager?.EndCall(new ActiveCall(uuid));
            }
            else
            {
                Provider.ReportNewIncomingCall(uuid, update, (error) => {
                    if (error == null && !cancelCall)
                    {
                        // Yes, report to call manager
                        CallManager.Calls.Add(new ActiveCall(uuid, handle, false, chatId, handle, isVideoCall));
                    }
                    else
                    {
                        // Report error to user here
                        Console.WriteLine("Error: {0}", error);
                    }
                });
            }
        }
        #endregion
    }
}