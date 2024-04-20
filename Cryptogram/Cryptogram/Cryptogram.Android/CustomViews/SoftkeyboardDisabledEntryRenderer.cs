using Android.Content;
using Android.Graphics.Drawables;
using Android.Views.InputMethods;
using Anonymous.Droid.CustomViews;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Views;
using CustomViewElements;

[assembly: ExportRenderer(typeof(SoftkeyboardDisabledEntry), typeof(SoftkeyboardDisabledEntryRenderer))]
namespace Anonymous.Droid.CustomViews
{
    public class SoftkeyboardDisabledEntryRenderer : EntryRenderer
    {
        public SoftkeyboardDisabledEntryRenderer(Context context) : base(context)
        {

        }
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                ((CustomViewElements.SoftkeyboardDisabledEntry)e.NewElement).PropertyChanging += OnPropertyChanging;
            }

            if (e.OldElement != null)
            {
                ((CustomViewElements.SoftkeyboardDisabledEntry)e.OldElement).PropertyChanging -= OnPropertyChanging;
            }
            if (Control != null)
            {
                var gd = new GradientDrawable();
                gd.SetColor(Android.Graphics.Color.Transparent);
                Control.SetBackground(gd);
                // Disable the Keyboard on Focus
                Control.ShowSoftInputOnFocus = false;
                Control.CustomSelectionActionModeCallback = new Callback();
                Control.LongClickable = false;
            }

        }

        private void OnPropertyChanging(object sender, PropertyChangingEventArgs propertyChangingEventArgs)
        {
            // Check if the view is about to get Focus
            if (propertyChangingEventArgs.PropertyName == VisualElement.IsFocusedProperty.PropertyName)
            {
                // incase if the focus was moved from another Entry
                // Forcefully dismiss the Keyboard 
                InputMethodManager imm = (InputMethodManager)Context.GetSystemService(Context.InputMethodService);
                imm.HideSoftInputFromWindow(this.Control.WindowToken, 0);
            }
        }

    }
    public class Callback : Java.Lang.Object, ActionMode.ICallback
    {
        public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            return false;
        }
        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            return false;
        }
        public void OnDestroyActionMode(ActionMode mode) { }
        public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            return false;
        }
    }
}