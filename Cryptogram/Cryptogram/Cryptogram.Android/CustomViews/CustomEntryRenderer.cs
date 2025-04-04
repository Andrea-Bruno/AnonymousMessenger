using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using CustomViewElements;
using System;
using Cryptogram.Droid.CustomViews;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomEntry), typeof(CustomEntryRenderer))]
namespace Cryptogram.Droid.CustomViews
{
    public class CustomEntryRenderer : EntryRenderer
    {

        public CustomEntryRenderer (Context context) : base(context)
        {

        }
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                try
                {
                    var gd = new GradientDrawable();
                    gd.SetColor(Android.Graphics.Color.Transparent);
                    Control.SetBackground(gd);
                    if (Build.VERSION.SdkInt <= BuildVersionCodes.P)
                    {
                        IntPtr IntPtrtextViewClass = JNIEnv.FindClass(typeof(TextView));
                        IntPtr mCursorDrawableResProperty = JNIEnv.GetFieldID(IntPtrtextViewClass, "mCursorDrawableRes", "I");
                        JNIEnv.SetField(Control.Handle, mCursorDrawableResProperty, 0);
                    }
                    //else
                    //    Control.SetTextCursorDrawable(Resource.Drawable.custom_cursor);
                }
                catch (Exception)
                {
                }
            }

        }
    }
}