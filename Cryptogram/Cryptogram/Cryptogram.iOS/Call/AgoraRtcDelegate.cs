using System;
using System.Diagnostics;
using DT.Xamarin.Agora;

namespace Cryptogram.iOS.Call
{
    public class AgoraRtcDelegate : AgoraRtcEngineDelegate
    {
        private CallRoomViewController _controller;

        public AgoraRtcDelegate(CallRoomViewController controller) : base()
        {
            _controller = controller;
        }

        public override void DidJoinedOfUid(AgoraRtcEngineKit engine, nuint uid, nint elapsed)
        {
            Debug.WriteLine($"DidJoinedOfUid {uid}");
            _controller.OnUserJoined(Convert.ToUInt32(uid), 0);
        }

        public override void FirstRemoteVideoDecodedOfUid(AgoraRtcEngineKit engine, nuint uid, CoreGraphics.CGSize size, nint elapsed)
        {
            Debug.WriteLine($"FirstRemoteVideoDecodedOfUid {uid}");
            _controller.FirstRemoteVideoDecodedOfUid(engine, uid, size, elapsed);
        }

        public override void DidOfflineOfUid(AgoraRtcEngineKit engine, nuint uid, UserOfflineReason reason)
        {
            Debug.WriteLine($"DidOfflineOfUid {uid}");
            _controller.DidOfflineOfUid(engine, uid, reason);
        }

        public override void DidVideoMuted(AgoraRtcEngineKit engine, bool muted, nuint uid)
        {
            _controller.DidVideoMuted(engine, muted, uid);
        }

        public override void FirstLocalVideoFrameWithSize(AgoraRtcEngineKit engine, CoreGraphics.CGSize size, nint elapsed)
        {
            _controller.FirstLocalVideoFrameWithSize(engine, size, elapsed);
        }
        
        public override void ActiveSpeaker(AgoraRtcEngineKit engine, nuint speakerUid)
        {
            _controller.FirstRemoteVideoDecodedOfUid(engine, speakerUid, new CoreGraphics.CGSize(), 0);

        }
    }
}
