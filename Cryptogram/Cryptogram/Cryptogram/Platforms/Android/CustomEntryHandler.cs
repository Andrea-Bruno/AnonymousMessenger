using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace Cryptogram
{
    // MAUI replacement for the legacy Xamarin.Android CustomEntryRenderer:
    // transparent entry background + gray hint text. Font comes from XAML (Poppins).
    public static class CustomEntryHandler
    {
        public static void Register()
        {
            EntryHandler.Mapper.AppendToMapping("Cryptogram.CustomEntry", (handler, view) =>
            {
                if (view is not CustomViewElements.CustomEntry)
                    return;
                if (handler.PlatformView is not EditText et)
                    return;
                var gd = new GradientDrawable();
                gd.SetColor(Android.Graphics.Color.Transparent);
                et.SetBackgroundDrawable(gd);
                et.SetHintTextColor(ColorStateList.ValueOf(Android.Graphics.Color.Gray));
            });
        }
    }
}
