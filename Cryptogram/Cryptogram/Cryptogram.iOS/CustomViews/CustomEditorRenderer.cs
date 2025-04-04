using Cryptogram.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.ComponentModel;
using Foundation;
using CustomViewElements;

[assembly: ExportRenderer(typeof(CustomEditor), typeof(CustomEditorRenderer))]
namespace Cryptogram.iOS
{
    public class CustomEditorRenderer : EditorRenderer
    {

        UILabel _placeholderLabel;

        public CustomEditorRenderer()
        {
            //UIKeyboard.Notifications.ObserveWillShow((sender, args) => {

            //    if (Element != null)
            //    {
            //        Element.Margin = new Thickness(0, 0, 0, args.FrameEnd.Height); //push the entry up to keyboard height when keyboard is activated
            //    }
            //});

            //UIKeyboard.Notifications.ObserveWillHide((sender, args) => {

            //    if (Element != null)
            //    {
            //        Element.Margin = new Thickness(0); //set the margins to zero when keyboard is dismissed
            //    }

            //});
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            //if (Control != null)
            //{
            //    if (_placeholderLabel == null)
            //    {
            //        CreatePlaceholder();
            //    }

            //}

            if (e.NewElement != null)
            {
                //var customControl = (CustomViewElements.CustomEditor)e.NewElement;

                //if (customControl.IsExpandable)
                //    Control.ScrollEnabled = false;
                //else
                //    Control.ScrollEnabled = true;

                //if (customControl.HasRoundedCorner)
                //    Control.Layer.CornerRadius = 5;
                //else
                //    Control.Layer.CornerRadius = 0;
            }

           
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);


            //var customControl = (CustomViewElements.CustomEditor)Element;

            //if (e.PropertyName == Editor.TextProperty.PropertyName)
            //{
            //    _placeholderLabel.Hidden = !string.IsNullOrEmpty(Control.Text);
            //}
            //else if (CustomViewElements.CustomEditor.PlaceholderProperty.PropertyName == e.PropertyName)
            //{
            //    _placeholderLabel.Text = customControl.Placeholder;
            //}
            //else if (CustomViewElements.CustomEditor.PlaceholderColorProperty.PropertyName == e.PropertyName)
            //{
            //    _placeholderLabel.TextColor = customControl.PlaceholderColor.ToUIColor();
            //}
            //else if (CustomViewElements.CustomEditor.HasRoundedCornerProperty.PropertyName == e.PropertyName)
            //{
            //    if (customControl.HasRoundedCorner)
            //        Control.Layer.CornerRadius = 5;
            //    else
            //        Control.Layer.CornerRadius = 0;
            //}
            //else if (CustomViewElements.CustomEditor.IsExpandableProperty.PropertyName == e.PropertyName)
            ////{
            //    if (customControl.IsExpandable)
            //        Control.ScrollEnabled = false;
            //    else
            //        Control.ScrollEnabled = true;
            //}

            //base.OnElementPropertyChanged(sender, e);
        }
        public void CreatePlaceholder()
        {
            var element = Element as CustomViewElements.CustomEditor;

            _placeholderLabel = new UILabel
            {
                Text = element?.Placeholder,
                TextColor = element.PlaceholderColor.ToUIColor(),
                BackgroundColor = UIColor.Clear
            };

            var edgeInsets = Control.TextContainerInset;
            var lineFragmentPadding = Control.TextContainer.LineFragmentPadding;

            Control.AddSubview(_placeholderLabel);

            var vConstraints = NSLayoutConstraint.FromVisualFormat(
                "V:|-" + edgeInsets.Top + "-[PlaceholderLabel]-" + edgeInsets.Bottom + "-|", 0, new NSDictionary(),
                NSDictionary.FromObjectsAndKeys(
                    new NSObject[] { _placeholderLabel }, new NSObject[] { new NSString("PlaceholderLabel") })
            );

            var hConstraints = NSLayoutConstraint.FromVisualFormat(
                "H:|-" + lineFragmentPadding + "-[PlaceholderLabel]-" + lineFragmentPadding + "-|",
                0, new NSDictionary(),
                NSDictionary.FromObjectsAndKeys(
                    new NSObject[] { _placeholderLabel }, new NSObject[] { new NSString("PlaceholderLabel") })
            );

            _placeholderLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            Control.AddConstraints(hConstraints);
            Control.AddConstraints(vConstraints);
        }

    }
}
