using System;
using Xamarin.Forms;

namespace Utils
{
    public class ViewTag
    {
        public static readonly BindableProperty TagProperty = BindableProperty.Create("Tag", typeof(string), typeof(ViewTag), null);

        public static string GetTag(BindableObject bindable)
        {
            return (string)bindable.GetValue(TagProperty);
        }

        public static void SetTag(BindableObject bindable, string value)
        {
            bindable.SetValue(TagProperty, value);
        }
    }
}
