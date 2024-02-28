using System;
using System.IO;
using Telegraph.Views;

namespace Telegraph.Utils
{
    public static class Utils
    {
        public static string AudioPlayerName = "audio.mp3";
        public static long ConvertToUnixTimestamp()
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = DateTime.Now.ToUniversalTime() - origin;
            return (long)Math.Floor(diff.TotalSeconds);
        }
        public static string GenerateBarCode()
        {
            return EncryptedMessaging.ContactMessage.GetMyQrCode(NavigationTappedPage.Context); ;
        }
        static string[] colors = { "#E51C23", "#E91E63", "#9C27B0", "#673AB7", "#3F51B5", "#5677FC", "#03A9F4", "#00BCD4", "#009688", "#259B24", "#8BC34A", "#AFB42B", "#FF9800", "#FF5722", "#795548", "#607D8B" };
        public static string StringToColour(int index)
        {
            if (index > -1)
                return colors[index];
            return colors[9];
        }
        public static double ConvertToUnixTimestampSecond(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }
        public static string ReplaceURLWithHTMLLinks(string text)
        {
            var urlRegex = "^((http?|ftp)://|(www|ftp)\\.)?[a-z0-9-]+(\\.[a-z0-9-]+)+([/?].*)?$";
            return text.Replace(urlRegex, "<a href='$1'>$1</a>");
        }
        public static string GetAddressPreviewImageUrl(string _LatLng)
        {
            return Defaults.MAPBOX_BASE_URL + "styles/v1/mapbox/streets-v11/static/" + _LatLng + "," + Defaults.ZOOM_LEVEL + "/400x600@2x?access_token=" + Defaults.MAPBOX_ACCESS_TOKEN;
        }
        public static byte[] StreamToByteArray(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
        public static string FormatTime(int time)
        {
            string sec, min;
            min = Convert.ToString(time / 60);
            sec = Convert.ToString(time % 60);
            if (min.Length == 1) min = "0" + min;
            if (sec.Length == 1) sec = "0" + sec;
            return min + ":" + sec;
        }
    }
}