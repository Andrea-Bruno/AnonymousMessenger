using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Telegraph.iOS.CustomViews;
using Telegraph.Styles;
using CustomViewElements;

[assembly: ExportRenderer(typeof(BasePage), typeof(BaseContentPage))]
namespace Telegraph.iOS.CustomViews
{
    public class BaseContentPage : PageRenderer
    {
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (ViewController.NavigationController != null)
            {
                ViewController.NavigationController.InteractivePopGestureRecognizer.Enabled = true;
                ViewController.NavigationController.InteractivePopGestureRecognizer.Delegate = new UIGestureRecognizerDelegate();
            }

        }
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null || Element == null)
                return;
           
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);
        }
        public void SetTheme()
        {
            if (TraitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Dark)
				Xamarin.Forms.Application.Current.Resources = new DarkTheme();
            else
				Xamarin.Forms.Application.Current.Resources = new LightTheme();
            
        }
    }
}
                 