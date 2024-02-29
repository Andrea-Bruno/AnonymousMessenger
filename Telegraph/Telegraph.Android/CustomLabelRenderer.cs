using telegraph;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using CustomRenderer.Android;
using Android.Graphics.Drawables;
using Android.Content.Res;
using Android.Text;
using System;
using Android.Runtime;
using Android.Widget;
using Android.Graphics;

[assembly: ExportRenderer(typeof(CustomLabel), typeof(CustomLabelRenderer))]
namespace CustomRenderer.Android
{
    public class CustomLabelRenderer : LabelRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);
            var text = (TextView)Control; // for example
            Typeface font = Typeface.CreateFromAsset(Forms.Context.Assets, "Product Sans Regular.ttf");  // font name specified here
            text.Typeface = font;

        }
    }
}