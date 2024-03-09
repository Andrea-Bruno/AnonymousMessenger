using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace XamarinShared
{
    public class Icons
    {
       
        private static readonly string BasePath = typeof(Icons).Namespace + ".Images.ic_message_";
        public static ImageSource NewGroup
        {
            get => GetImageSource("new_group");
        }

        public static ImageSource Pdf
        {
            get => GetImageSource("pdf");
        }

        public static ImageSource PhoneContact
        {
            get => GetImageSource("phonecontact");
        }

        public static ImageSource VideoDownload
        {
            get => GetImageSource("video_download");
        }

        public static ImageSource CallArrow(bool isAnswered,bool isOutgoing)
        {
            string iconPath = "call_arrow_"  + (isAnswered ? "green" : "red");
            return GetImageSource(iconPath,isOutgoing);
        }

        public static ImageSource CallIcon(bool isVideo, bool isOutgoing)
        {
            string iconPath = "call_"+ (isVideo ? "video" : "audio") ;
            return GetImageSource(iconPath,isOutgoing);
        }

        public static ImageSource AudioMic(bool isOutgoing)
        {
            string iconPath = "audio_mic";
            return GetImageSource(iconPath,isOutgoing);
        }

        public static ImageSource SliderThumb(bool isOutgoing)
        {
            string iconPath = "slider_thumb";
            return GetImageSource(iconPath, isOutgoing);
        }

        public static ImageSource AudioPlayerState(bool isNormalState)
        {
            string iconPath = "audio_" + (isNormalState ? "start" : "pause");
            return GetImageSource(iconPath);
        }

        private static ImageSource GetImageSource(string key,bool isOutgoing)
        {
            return GetImageSource(key + (isOutgoing ? "_outgoing" : "_incoming"));
        }


        private static ImageSource GetImageSource(string key)
        {
            return ImageSource.FromResource(BasePath+key+ ".png");
        }


    }
}
