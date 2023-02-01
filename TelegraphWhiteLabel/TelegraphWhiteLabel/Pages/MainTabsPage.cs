using EncryptedMessaging;
using EncryptedMessaging.Resources;
using Xamarin.Forms;

namespace AnonymousWhiteLabel.Pages
{
    internal class MainTabsPage : TabbedPage
    {
        public MainTabsPage(Context context)
        {
            //Children.Add(new NavigationPage(new ContactsPage()) { Title = Dictionary.Contacts });
            //         Children.Add(new NavigationPage(new InfoPage()) { Title = Dictionary.About });
            Children.Add(new ContactsPage() { Title = Dictionary.Contacts });
            Children.Add(new InfoPage() { Title = Dictionary.About });
            //NotificationManager = DependencyService.Get<INotificationManager>();
            //NotificationManager.NotificationReceived += (sender, eventArgs) =>
            //{
            //	var evtData = (NotificationEventArgs)eventArgs;
            //	ShowNotification(evtData.Title, evtData.Message);
            //};
        }
        public void toHide()
        {
            Children.Clear();
        }
        protected override void OnAppearing()
        {
            App.IgnoreBatteryOptimizations?.Invoke();
        }

        //=============================== notification
        //https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/local-notifications




    }


}
