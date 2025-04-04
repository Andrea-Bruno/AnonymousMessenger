using System;
using CustomViewElements;
using Cryptogram.DesignHandler;
using Cryptogram.iOS.CustomViews;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomTabbedPage), typeof(CustomTabbedPageRenderer))]
namespace Cryptogram.iOS.CustomViews
{
    public class CustomTabbedPageRenderer : TabbedRenderer
    {

        public CustomTabbedPageRenderer()
        {
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            var newHeight = TabBar.Frame.Height + 15;
            CoreGraphics.CGRect tabFrame = TabBar.Frame; //self.TabBar is IBOutlet of your TabBar
            tabFrame.Height = newHeight;
           // tabFrame.Y = View.Frame.Size.Height - newHeight;
            TabBar.Frame = tabFrame;
            TabBar.BarTintColor = DesignResourceManager.GetColorFromStyle("Color2").ToUIColor();
            TabBar.Alpha = 1;
            TabBar.Translucent = false;
        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                TabBar.Translucent = false;
            }
        }

    }
}
