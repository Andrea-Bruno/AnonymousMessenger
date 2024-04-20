using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using Google.Android.Material.BottomNavigation;
using Anonymous.Droid.CustomViews;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppCompat;
using static Google.Android.Material.BottomNavigation.BottomNavigationView;
using View = Android.Views.View;

[assembly: ExportRenderer(typeof(TabbedPage), typeof(CustomTabbedPageRenderer))]
namespace Anonymous.Droid.CustomViews
{
    public class CustomTabbedPageRenderer : TabbedPageRenderer
    {
        public CustomTabbedPageRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<TabbedPage> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                IEnumerable<View> children = GetAllChildViews(ViewGroup);
                BottomNavigationView bottomNavBar = (BottomNavigationView)children.SingleOrDefault(view => view is BottomNavigationView);

                if (bottomNavBar != null)
                    bottomNavBar.LabelVisibilityMode = LabelVisibilityMode.LabelVisibilityUnlabeled;
            }
        }

        private IEnumerable<View> GetAllChildViews(View view)
        {
            if (!(view is ViewGroup group))
                return new List<View> { view };

            List<View> result = new List<View>();
            int childCount = group.ChildCount;

            for (int i = 0; i < childCount; i++)
            {
                View child = group.GetChildAt(i);
                List<View> childList = new List<View> { child };
                childList.AddRange(GetAllChildViews(child));
                result.AddRange(childList);
            }

            return result.Distinct();
        }
    }
}