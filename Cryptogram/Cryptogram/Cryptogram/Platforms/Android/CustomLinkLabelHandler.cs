using Android.Content.Res;
using Android.Text;
using Android.Text.Method;
using Android.Widget;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using XamarinShared;

namespace Cryptogram
{
    // MAUI replacement for the legacy Xamarin.Android CustomLinkLabelRenderer:
    // enables native auto-link detection (URLs, emails, phone) with a clickable link color.
    // Applied via mapper chaining so it does not depend on the handler's exact platform-view type.
    public static class CustomLinkLabelHandler
    {
        public static void Register()
        {
            LabelHandler.Mapper.AppendToMapping("Cryptogram.CustomLinkLabel", (handler, view) =>
            {
                if (view is not CustomLinkLabel)
                    return;
                if (handler.PlatformView is not TextView tv)
                    return;
                tv.AutoLinkMask = Android.Text.Util.MatchOptions.All;
                tv.SetLinkTextColor(ColorStateList.ValueOf(Android.Graphics.Color.DeepSkyBlue));
                tv.LinksClickable = true;
                tv.MovementMethod = LinkMovementMethod.Instance;
            });
        }
    }
}
