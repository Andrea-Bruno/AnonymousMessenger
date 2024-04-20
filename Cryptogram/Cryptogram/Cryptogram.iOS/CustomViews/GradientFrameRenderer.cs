using CoreAnimation;
using CoreGraphics;
using Anonymous;
using Anonymous.iOS.CustomViews;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(CustomViewElements.GradientFrame), typeof(GradientFrameRenderer))]
namespace Anonymous.iOS.CustomViews
{
    public class GradientFrameRenderer : VisualElementRenderer<Frame>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);
        }
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SetNeedsDisplay();
            base.OnElementPropertyChanged(sender, e);
        }
        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            if (Element != null)
            {
                if (Element is CustomViewElements.GradientFrame)
                {

                    var obj = (CustomViewElements.GradientFrame)Element;
                    var StartColor = obj.StartColor.ToCGColor();
                    var EndColor = obj.EndColor.ToCGColor();

                    var gradientLayer = new CAGradientLayer
                    {
                        Frame = rect,
                        Colors = new CGColor[] { StartColor, EndColor }
                    };
                    //for horizontal gradient
                    //if (obj.GradientColorOrientation == GradientStackLayout.GradientOrientation.Horizontal)
                    {
                        gradientLayer.StartPoint = new CGPoint(0.0, 0.5);
                        gradientLayer.EndPoint = new CGPoint(1.0, 0.5);
                    }
                    gradientLayer.CornerRadius = obj.CornerRadius;
                    NativeView.Layer.InsertSublayer(gradientLayer, 0);
                }
            }
        }
    }
}