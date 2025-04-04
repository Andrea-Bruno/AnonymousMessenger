using System.Collections.Generic;
using System.ComponentModel;
using Cryptogram.iOS.CustomViews;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XamarinShared;

[assembly: ExportRenderer(typeof(CustomLinkLabel), typeof(CustomLinkLabelRenderer))]

namespace Cryptogram.iOS.CustomViews
{
    internal class CustomLinkLabelRenderer : ViewRenderer<CustomLinkLabel, UITextView>
    {
        private UITextView uiTextView;

        public CustomLinkLabelRenderer()
        {
            
        }

        protected override void OnElementChanged(ElementChangedEventArgs<CustomLinkLabel> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement == null) return;
            if (Control != null) return;
            uiTextView = new UITextView();

            uiTextView.DataDetectorTypes = UIDataDetectorType.All;
            uiTextView.Editable = false;
            uiTextView.ScrollEnabled = false;
            uiTextView.BackgroundColor = UIColor.Clear;
            uiTextView.Text = Element.Text;
            uiTextView.Font = UIFont.FromName("Poppins-SemiBold", (float) Element.FontSize);
            uiTextView.TextColor = Element.TextColor.ToUIColor();
            SetNativeControl(uiTextView);
        }
        
     
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Label.TextProperty.PropertyName && uiTextView!=null)
            {
                uiTextView.Text = Element.Text;
            }
            base.OnElementPropertyChanged(sender, e);
        }
    }
}