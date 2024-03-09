using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Xamarin.Forms;

namespace XamarinShared.ViewCreator
{
    public static class Utils
    {
        private static string[] colors = { "#E51C23", "#E91E63", "#9C27B0", "#673AB7", "#3F51B5", "#5677FC", "#03A9F4", "#00BCD4", "#009688", "#259B24", "#8BC34A", "#AFB42B", "#FF9800", "#FF5722", "#795548", "#607D8B" };
        internal static readonly string MAPBOX_BASE_URL = "https://api.mapbox.com/";
        internal static readonly string MAPBOX_ACCESS_TOKEN = "pk.eyJ1IjoibXVzYWJpciIsImEiOiJjazNubTZvYWswY3FmM3BxZXkxYmprdWdwIn0.h1LVVpfASIINFh3vtWp9Mg";
        internal static readonly string ZOOM_LEVEL = "14.2";
        private static FontSizeConverter fontSizeConverter = new FontSizeConverter();
        public static string StringToColour(int index)
        {
            if (index > -1)
                return colors[index];
            return colors[9];
        }


        public static void Click(this View view, EventHandler tapped)
        {
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += tapped;
            tapGestureRecognizer.NumberOfTapsRequired = 1;
            view.GestureRecognizers.Add(tapGestureRecognizer);
        }

        public static void SetFontSizeBinding(this View view, int size)
        {
            view.BindingContext = MessageViewCreator.Instance;
            view.SetBinding(Label.FontSizeProperty, new Binding(nameof(MessageViewCreator.TextSize), converter: fontSizeConverter, converterParameter: 14));
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

        public static string ReplaceURLWithHTMLLinks(string text)
        {
            var urlRegex = "^((http?|ftp)://|(www|ftp)\\.)?[a-z0-9-]+(\\.[a-z0-9-]+)+([/?].*)?$";
            return text.Replace(urlRegex, "<a href='$1'>$1</a>");
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


        public static string GetAddressPreviewImageUrl(double lat, double lng)
        {
            string _latLng = lng.ToString(CultureInfo.InvariantCulture) + "," + lat.ToString(CultureInfo.InvariantCulture);
            return MAPBOX_BASE_URL + "styles/v1/mapbox/streets-v11/static/pin-l-embassy+f74e4e("+ _latLng+")/" + _latLng + "," + ZOOM_LEVEL + "/400x600@2x?access_token=" + MAPBOX_ACCESS_TOKEN;
        }

        public static readonly BindableProperty TagProperty = BindableProperty.Create("Tag", typeof(string), typeof(Utils), null);

        public static string GetTag(this BindableObject bindable)
        {
            return (string)bindable.GetValue(TagProperty) ?? string.Empty;
        }

        public static void SetTag(this BindableObject bindable, string value)
        {
            bindable.SetValue(TagProperty, value);
        }

        public static void SetSource(this Image image, byte[] data)
        {
            if (data == null || image == null) return;
            Device.BeginInvokeOnMainThread(() =>
            {
                image.Source = ImageSource.FromStream(() => new MemoryStream(data));
            });
        }

        public static int GetTotalDays(this DateTime dateTime)
        {
            return (dateTime - DateTime.MinValue).Days;
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
            catch (Exception e)
            {

            }
        }
    }
}
