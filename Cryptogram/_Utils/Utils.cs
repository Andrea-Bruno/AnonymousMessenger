using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Xamarin.Forms;

namespace Utils
{
    public static class Utils
    {
        public static string AudioPlayerName = "audio.mp3";
        private static string[] colors = { "#E51C23", "#E91E63", "#9C27B0", "#673AB7", "#3F51B5", "#5677FC", "#03A9F4", "#00BCD4", "#009688", "#259B24", "#8BC34A", "#AFB42B", "#FF9800", "#FF5722", "#795548", "#607D8B" };
        public static long ConvertToUnixTimestamp()
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = DateTime.Now.ToUniversalTime() - origin;
            return (long)Math.Floor(diff.TotalSeconds);
        }

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
        public static string GetAddressPreviewImageUrl(double lat, double lng)
        {
            string _latLng = lng.ToString(CultureInfo.InvariantCulture) + "," + lat.ToString(CultureInfo.InvariantCulture);
            return Defaults.MAPBOX_BASE_URL + "styles/v1/mapbox/streets-v11/static/" + _latLng + "," + Defaults.ZOOM_LEVEL + "/400x600@2x?access_token=" + Defaults.MAPBOX_ACCESS_TOKEN;
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

        public static byte[] ObjectToByteArray(object obj)
        {

            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            items.ToList().ForEach(collection.Add);
        }
        public static object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            object obj = (object)binForm.Deserialize(memStream);

            return obj;
        }
        public static string GetTag(this View view)
        {
            return ViewTag.GetTag(view) ?? "";
        }
        public static void SetTag(this View  view,string tag)
        {
             ViewTag.SetTag(view,tag);
        }
        public static void SyncTag(this View view, View tag)
        {
            ViewTag.SetTag(view, tag.GetTag());
        }

        public static void HandleButtonSingleClick(this object sender, int duration = 1000)
        {
            try
            {
                if (sender == null || !((View)sender).IsEnabled) return;
                ((View)sender).IsEnabled = false;

                var timer = new System.Threading.Timer((object obj) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ((View)sender).IsEnabled = true;
                    });
                }, null, duration, System.Threading.Timeout.Infinite);
            }
            catch(Exception e)
            {

            }
        }
    }
}