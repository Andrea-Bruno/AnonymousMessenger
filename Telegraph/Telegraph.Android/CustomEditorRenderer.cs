using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Widget;
using telegraph;
using telegraph.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomEditor), typeof(CustomEditorRenderer))]

namespace telegraph.Droid
{
	internal class CustomEditorRenderer : EditorRenderer
    {
        public CustomEditorRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                GradientDrawable gd = new GradientDrawable();
                gd.SetColor(Android.Graphics.Color.Transparent);
                this.Control.SetBackgroundDrawable(gd);
                Control.SetHintTextColor(ColorStateList.ValueOf(Android.Graphics.Color.Gray));
                var edittext = (EditText)Control; // for example
                Typeface font = Typeface.CreateFromAsset(Forms.Context.Assets, "Product Sans Regular.ttf");  // font name specified here
                edittext.Typeface = font;
            }
        }
    }
}