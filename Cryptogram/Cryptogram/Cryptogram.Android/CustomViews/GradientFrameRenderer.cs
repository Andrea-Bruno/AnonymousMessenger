using System;
using Android.Content;
using Android.Graphics.Drawables;
using CustomViewElements;
using Anonymous.Droid.CustomViews;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(GradientFrame), typeof(GradientFrameRenderer))]
namespace Anonymous.Droid.CustomViews
{
    public class GradientFrameRenderer : VisualElementRenderer<Frame>
    {
        public GradientFrameRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);
            try
            {
                this.SetBackground(DrawGradient(e));
            }
            catch (Exception)
            {
            }
        }

        private GradientDrawable DrawGradient(ElementChangedEventArgs<Frame> e)
        {
            var button = e.NewElement as CustomViewElements.GradientFrame;
            var orientation = button.GradientColorOrientation == CustomViewElements.GradientFrame.GradientOrientation.Horizontal ?
                GradientDrawable.Orientation.LeftRight : GradientDrawable.Orientation.TopBottom;

            var _gradient = new GradientDrawable(orientation, new[] {
                button.StartColor.ToAndroid().ToArgb(),
                button.EndColor.ToAndroid().ToArgb(),
            });

            _gradient.SetCornerRadius(button.CornerRadius * 10);
            _gradient.SetStroke(0, button.StartColor.ToAndroid());

            return _gradient;
        }
    }
}