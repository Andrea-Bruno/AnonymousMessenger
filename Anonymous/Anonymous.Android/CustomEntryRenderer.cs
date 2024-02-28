using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Widget;
using CustomRenderer.Android;
using System;
using telegraph;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomEntry), typeof(CustomEntryRenderer))]
namespace CustomRenderer.Android
{
	public class CustomEntryRenderer : EntryRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			if (Control != null)
			{
				GradientDrawable gd = new GradientDrawable();
				gd.SetColor(global::Android.Graphics.Color.Transparent);
				this.Control.SetBackgroundDrawable(gd);
				Control.SetHintTextColor(ColorStateList.ValueOf(global::Android.Graphics.Color.Gray));
				var edittext = (EditText)Control; // for example
				Typeface font = Typeface.CreateFromAsset(Forms.Context.Assets, "Product Sans Regular.ttf");  // font name specified here
				edittext.Typeface = font;
			}

			IntPtr IntPtrtextViewClass = JNIEnv.FindClass(typeof(TextView));
			IntPtr mCursorDrawableResProperty = JNIEnv.GetFieldID(IntPtrtextViewClass, "mCursorDrawableRes", "I");

			//// my_cursor is the xml file name which we defined above
			////JNIEnv.SetField(Control.Handle, mCursorDrawableResProperty, 0);
		}
	}
}