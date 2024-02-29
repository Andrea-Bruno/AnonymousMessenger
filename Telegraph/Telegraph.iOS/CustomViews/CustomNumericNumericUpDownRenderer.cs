using Syncfusion.SfNumericUpDown.XForms.iOS;
using Telegraph.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomNumericUpDown),
    typeof(Telegraph.iOS.CustomViews.CustomNumericNumericUpDownRenderer))]

namespace Telegraph.iOS.CustomViews
{
    public class CustomNumericNumericUpDownRenderer : SfNumericUpDownRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Syncfusion.SfNumericUpDown.XForms.SfNumericUpDown> e)
        {
            base.OnElementChanged(e);
            if (this.Control != null)
            {
                this.Control.Layer.BorderWidth = 0f;
            }
        }
    }
}