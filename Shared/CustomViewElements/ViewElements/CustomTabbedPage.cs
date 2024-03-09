using Xamarin.Forms;

namespace CustomViewElements
{
    public class CustomTabbedPage : TabbedPage
    {
        public CustomTabbedPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
        }

        public void ShowProgressDialog() => DependencyService.Get<Services.IProgressInterface>().Show();

        public void HideProgressDialog() => DependencyService.Get<Services.IProgressInterface>().Hide();

    }
}
