using Rg.Plugins.Popup.Pages;
using CustomViewElements.Services;
using Xamarin.Forms;
using Rg.Plugins.Popup.Services;

namespace CustomViewElements
{
    public class BasePopupPage : PopupPage
    {
        
        public void ShowProgressDialog() => DependencyService.Get<IProgressInterface>()?.Show();

        public void HideProgressDialog() => DependencyService.Get<IProgressInterface>()?.Hide();

        protected override bool OnBackButtonPressed()
        {
            PopupNavigation.Instance.PopAsync(true);
            return true;
        }

    }
}
