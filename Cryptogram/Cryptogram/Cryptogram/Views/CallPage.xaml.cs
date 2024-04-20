using CustomViewElements;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinShared;
using System.Globalization;
using static EncryptedMessaging.MessageFormat;
using System.Collections.Specialized;
using XamarinShared.ViewModels;
using Anonymous.DesignHandler;
using Utils;

namespace Anonymous.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CallPage : BasePage
    {
        private string _searchQuery;
        public static ImageSource AudioCallIcon = DesignResourceManager.GetImageSource("ic_new_call.png");
        public static ImageSource VideoCallIcon = DesignResourceManager.GetImageSource("ic_new_cam_forCallPage.png");
        public static ImageSource AudioCallIconSmall = DesignResourceManager.GetImageSource("ic_new_smallCall.png");
        public static ImageSource VideoCallIconSmall = DesignResourceManager.GetImageSource("ic_new_smallVideo.png");
        private bool isPlaceholderVisible
        {
            set
            {
                NoResultPage.IsVisible = false;
                NoItemPage.IsVisible = false;

                if (string.IsNullOrWhiteSpace(_searchQuery) && value)
                    NoItemPage.IsVisible = true;
                else
                    NoResultPage.IsVisible = value;
                ItemsListView.IsVisible = !value;
            }
        }

        private bool _isLostState;
        private bool isLostState
        {
            set
            {
                _isLostState = value;
                StateAllUnderline.IsVisible = !value;
                StateLostUnderline.IsVisible = value;
                StateLost.TextColor = !value ? DesignResourceManager.GetColorFromStyle("ToolbarStateUnselected") : DesignResourceManager.GetColorFromStyle("WhiteColor");
                StateAll.TextColor = value ? DesignResourceManager.GetColorFromStyle("ToolbarStateUnselected") : DesignResourceManager.GetColorFromStyle("WhiteColor");
                FilterContacts("");
            }
        }

        public CallPage()
        {
            InitializeComponent();
            ItemsListView.ItemsSource = Calls.GetInstance().AllCalls;
            Calls.GetInstance().AllCalls.CollectionChanged += AllCals_CollectionChanged;
            isPlaceholderVisible = Calls.GetInstance().AllCalls.Count == 0;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ItemsListView.ItemsSource = Calls.GetInstance().AllCalls;
        }

        private void SearchBtn_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick(500);
            ToolbarSearchLayout.IsVisible = true;
            SearchEntry.Text = "";
            toolbar.IsVisible = false;
            SearchEntry.Focus();
        }

        private void AllCals_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (ItemsListView?.DataSource != null && !string.IsNullOrWhiteSpace(_searchQuery))
                    isPlaceholderVisible = ItemsListView.DataSource.Items.Count == 0;
                else
                    isPlaceholderVisible = Calls.GetInstance().AllCalls.Count == 0;
            });
        }

        private void Search_TextChanged(object sender, EventArgs e)
        {
            FilterContacts(((CustomEntry)sender).Text);
        }

        private void Delete_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            try
            {
                CallViewModel callViewModel = ((sender as View).GestureRecognizers[0] as TapGestureRecognizer).CommandParameter as CallViewModel;
                if (callViewModel != null)
                {
                    Calls.GetInstance().Remove(callViewModel.MessageId);
                }
            }
            catch (Exception ex) { }
        }

        private void FilterContacts(string query)
        {
            _searchQuery = query;
            if (ItemsListView.DataSource != null)
            {
                ItemsListView.DataSource.Filter = FilterContacts;
                ItemsListView.DataSource.RefreshFilter();
                var filteredDataCount = ItemsListView.DataSource.Items.Count;
                isPlaceholderVisible = filteredDataCount == 0;
            }
        }

        private bool FilterContacts(object obj)
        {
            var callViewModel = obj as CallViewModel;

            if (callViewModel.Contact.Name.ToLower().Contains(_searchQuery.ToLower()))
            {
                return _isLostState ? callViewModel.CallType == CallType.LOST : true;
            }
            else
                return false;
        }

        private void StateAll_Clicked(object sender, EventArgs e) => isLostState = false;

        private void StateLost_Clicked(object sender, EventArgs e) => isLostState = true;

        private void SearchClearButton_Clicked(object sender, EventArgs e)
        {
            toolbar.IsVisible = true;
            ToolbarSearchLayout.IsVisible = false;
            SearchEntry.Text = "";
        }

        private void ListItemClicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            try
            {
                View v = sender as View;
                CallViewModel callViewModel = (v.GestureRecognizers[0] as TapGestureRecognizer).CommandParameter as CallViewModel;
                if (callViewModel != null)
                    ((App)Application.Current)?.CallManager.StartCall(callViewModel.Contact, callViewModel.MessageType == MessageType.VideoCall || callViewModel.MessageType == MessageType.StartVideoGroupCall );
            }
            catch (FormatException) { }
        }

    }
    public class MessageCallIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((value as MessageType?) == MessageType.VideoCall || (value as MessageType?) == MessageType.StartVideoGroupCall ) ? CallPage.VideoCallIcon : CallPage.AudioCallIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class MessageCallSmallIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((value as MessageType?) == MessageType.VideoCall || (value as MessageType?) == MessageType.StartVideoGroupCall) ? CallPage.VideoCallIconSmall : CallPage.AudioCallIconSmall;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}