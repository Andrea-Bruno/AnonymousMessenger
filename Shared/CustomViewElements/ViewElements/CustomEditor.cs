using System;
using Xamarin.Forms;

namespace CustomViewElements
{
	public class CustomEditor : Editor
	{

        public static readonly BindableProperty AutoSizeMinimumHeightProperty = BindableProperty.Create(nameof(AutoSizeMinimumHeight), typeof(double), typeof(Editor), (double)-1, propertyChanged: (bindable, oldValue, newValue)
            => ((CustomEditor)bindable)?.InvalidateMeasure());
        public static readonly BindableProperty AutoSizeMaximumHeightProperty = BindableProperty.Create(nameof(AutoSizeMaximumHeight), typeof(double), typeof(Editor), (double)-1, propertyChanged: (bindable, oldValue, newValue)
            => ((CustomEditor)bindable)?.InvalidateMeasure());

   
    
        public double AutoSizeMinimumHeight
        {
            get => (double)GetValue(AutoSizeMinimumHeightProperty);
            set => SetValue(AutoSizeMinimumHeightProperty, value);
        }

        public double AutoSizeMaximumHeight
        {
            get => (double)GetValue(AutoSizeMaximumHeightProperty);
            set => SetValue(AutoSizeMaximumHeightProperty, value);
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            SizeRequest sizeRequest = base.OnMeasure(widthConstraint, heightConstraint);
            if (AutoSize != EditorAutoSizeOption.TextChanges)
                return sizeRequest;
            double height = sizeRequest.Request.Height;
            if (AutoSizeMinimumHeight > 0)
                height = Math.Max(height, AutoSizeMinimumHeight);
            if (AutoSizeMaximumHeight > 0)
                height = Math.Min(height, AutoSizeMaximumHeight);
            return new SizeRequest(new Size(sizeRequest.Request.Width, height));
        }

    }
}