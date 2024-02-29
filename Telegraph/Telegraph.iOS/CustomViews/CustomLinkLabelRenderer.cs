using CoreGraphics;
using CustomViewElements;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Telegraph.iOS.CustomViews;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomLinkLabel), typeof(CustomLinkLabelRenderer))]
namespace Telegraph.iOS.CustomViews
{
    internal class CustomLinkLabelRenderer : ViewRenderer<CustomLinkLabel,UITextView>
    {

        public CustomLinkLabelRenderer()
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<CustomLinkLabel> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    UITextView uiTextView = new UITextView();

                    uiTextView.DataDetectorTypes = UIDataDetectorType.All;
                    uiTextView.Editable = false;
                    uiTextView.ScrollEnabled = false;
                    uiTextView.BackgroundColor = UIColor.Clear;
                    uiTextView.Text = Element.Text;
                    uiTextView.Font = UIFont.FromName("Poppins-SemiBold",(float)Element.FontSize);
                    uiTextView.TextColor = Element.TextColor.ToUIColor();

                    SetNativeControl(uiTextView);
                }
                }
            }

        }
      
    }
