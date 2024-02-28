﻿using Android.Content;
using CustomViewElements;
using Telegraph.Droid.CustomViews;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomSearchBar), typeof(CustomSearchBarRenderer))]
namespace Telegraph.Droid.CustomViews 
{
    public class CustomSearchBarRenderer : SearchBarRenderer
    {
        public CustomSearchBarRenderer(Context context) : base(context)
        {
        }
        protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                var plateId = Resources.GetIdentifier("android:id/search_plate", null, null);
                var plate = Control.FindViewById(plateId);
                plate.SetBackgroundColor(Android.Graphics.Color.Transparent);
            }
        }
    }
}
