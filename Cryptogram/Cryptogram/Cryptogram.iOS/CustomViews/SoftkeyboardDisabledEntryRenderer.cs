using Anonymous.iOS.CustomViews;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using ObjCRuntime;
using Foundation;
using CustomViewElements;

[assembly: ExportRenderer(typeof(SoftkeyboardDisabledEntry), typeof(SoftkeyboardDisabledEntryRenderer))]
namespace Anonymous.iOS.CustomViews
{
    public class SoftkeyboardDisabledEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            // Disabling the keyboard
            Control.InputView = new UIView();
        }

        public override bool CanPerform(Selector action, NSObject withSender)
        {
            NSOperationQueue.MainQueue.AddOperation(() => {
                UIMenuController.SharedMenuController.SetMenuVisible(false, false);
            });
            return base.CanPerform(action, withSender);
        }
    }
}

