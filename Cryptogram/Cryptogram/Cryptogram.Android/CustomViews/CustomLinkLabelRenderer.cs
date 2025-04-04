using Android.Content;
using Android.Content.Res;
using Android.Text.Method;
using Cryptogram.Droid.CustomViews;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XamarinShared;

[assembly: ExportRenderer(typeof(CustomLinkLabel), typeof(CustomLinkLabelRenderer))]
namespace Cryptogram.Droid.CustomViews
{
    internal class CustomLinkLabelRenderer : LabelRenderer
    {
        public CustomLinkLabelRenderer(Context context) : base(context)
        {

        }
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null) return;
            if (Control == null) return;
            var view = Control;
            view.AutoLinkMask = Android.Text.Util.MatchOptions.All;
            view.SetLinkTextColor(ColorStateList.ValueOf(Android.Graphics.Color.DeepSkyBlue));
            view.LinksClickable = true;
            view.MovementMethod = LinkMovementMethod.Instance;

        }
    }
}