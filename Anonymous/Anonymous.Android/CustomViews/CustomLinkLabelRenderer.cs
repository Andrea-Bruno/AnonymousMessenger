using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using CustomViewElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegraph.Droid.CustomViews;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomLinkLabel), typeof(CustomLinkLabelRenderer))]
namespace Telegraph.Droid.CustomViews
{
    internal class CustomLinkLabelRenderer : LabelRenderer
    {
        public CustomLinkLabelRenderer(Context context) : base(context)
        {

        }
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
             
                if (Control != null)
                {
                    TextView view = Control;
                    view.AutoLinkMask = Android.Text.Util.MatchOptions.All;
                    view.SetLinkTextColor(ColorStateList.ValueOf(Android.Graphics.Color.DeepSkyBlue));
                    view.LinksClickable = true;
                    view.MovementMethod = LinkMovementMethod.Instance;
                }
            }

        }
    }
}