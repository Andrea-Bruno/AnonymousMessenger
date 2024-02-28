using DT.Xamarin.Agora;
using System.Diagnostics;

namespace Telegraph.Droid.Call
{
    public class AgoraRtcHandler : IRtcEngineEventHandler
    {
        private static RoomActivity _context;
        private static AgoraRtcHandler Instance;

        private AgoraRtcHandler()
        {
        }

        private AgoraRtcHandler(RoomActivity activity)
        {
            _context = activity;
        }

        public static AgoraRtcHandler GetInstance(RoomActivity activity)
        {
            _context = activity;

            if (Instance == null)
            {
                Instance = new AgoraRtcHandler(activity);
            }
            return Instance;
        }
        public override void OnJoinChannelSuccess(string p0, int p1, int p2)
        {
            _context.OnJoinChannelSuccess(p0, p1, p2);
        }

        public override void OnFirstRemoteVideoDecoded(int p0, int p1, int p2, int p3)
        {
            Debug.WriteLine($"DidOfflineOfUid {p0}");
            _context.OnFirstRemoteVideoDecoded(p0, p1, p2, p3);
        }

        public override void OnUserJoined(int p0, int p1)
        {
            _context.OnUserJoined(p0, p1);
            Debug.WriteLine($"OnUserJoined {p0}");
        }

        public override void OnUserOffline(int p0, int p1)
        {
            Debug.WriteLine($"DidOfflineOfUid {p0}");
            _context.OnUserOffline(p0, p1);
        }

        public override void OnUserMuteVideo(int p0, bool p1)
        {
            _context.OnUserMuteVideo(p0, p1);
        }

        public override void OnFirstLocalVideoFrame(int p0, int p1, int p2)
        {
            _context.OnFirstLocalVideoFrame(p0, p1, p2);
        }

        public override void OnActiveSpeaker(int p0)
        {
            _context.OnFirstRemoteVideoDecoded(p0, 0, 0, 0);
            base.OnActiveSpeaker(p0);
        }

        public override void OnAudioVolumeIndication(AudioVolumeInfo[] p0, int p1)
        {
            int vol = -1;
            int uid = -1;
            if(p0!=null)
            foreach(AudioVolumeInfo audioVolumeInfo in p0)
            {
                if(audioVolumeInfo.Volume>vol)
                 {
                    vol = audioVolumeInfo.Volume;
                    uid = audioVolumeInfo.Uid;

                }
            }
            if(uid!=-1 && p0!=null && p0.Length >1)
                _context.OnFirstRemoteVideoDecoded(uid, 0, 0, 0);

            base.OnAudioVolumeIndication(p0, p1);
        }
    }
}
