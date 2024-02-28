using Android.Content;
using Android.Graphics.Drawables;
using CustomViewElements.ViewElements;
using Telegraph.Droid.CustomViews;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomDashedFrame), typeof(CustomDashedFrameRenderer))]
namespace Telegraph.Droid.CustomViews
{
    public class CustomDashedFrameRenderer : VisualElementRenderer<Frame>
    {
        public CustomDashedFrameRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);
           // CustomDashedFrame customFrame = Element as CustomDashedFrame;
           //float r = customFrame.CornerRadius;
            GradientDrawable shape = new GradientDrawable();
            //shape.SetCornerRadii(new float[] { r, r, r, r, r, r, r, r });
            //shape.SetColor(Android.Graphics.Color.Transparent);
           //shape.SetStroke(2, customFrame.BorderColor.ToAndroid(), 5f, 2f);
            this.SetBackground(shape);
        }
    }
}