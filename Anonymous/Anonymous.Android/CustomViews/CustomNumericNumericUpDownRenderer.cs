using Android.Graphics.Drawables;
using Android.Widget;
using Syncfusion.SfNumericUpDown.XForms;
using Syncfusion.SfNumericUpDown.XForms.Droid;
using Telegraph.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomNumericUpDown),
    typeof(Telegraph.Droid.CustomViews.CustomNumericNumericUpDownRenderer))]

namespace Telegraph.Droid.CustomViews
{
    public class CustomNumericNumericUpDownRenderer : SfNumericUpDownRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Syncfusion.SfNumericUpDown.XForms.SfNumericUpDown> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                for (int i = 0; i < Control.ChildCount; i++)
                {
                    var child = Control.GetChildAt(i);
                    if (child is EditText)
                    {
                        var control = child as EditText;
                        control.Background = null;
                    }
                }
            }
        }
    }
}