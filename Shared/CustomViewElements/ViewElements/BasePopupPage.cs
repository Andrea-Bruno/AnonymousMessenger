using Mopups.Pages;
using CustomViewElements.Services;
using Microsoft.Maui.Controls;
using Mopups.Services;

namespace CustomViewElements
{
    public class BasePopupPage : PopupPage
    {
        
        public void ShowProgressDialog() => DependencyService.Get<IProgressInterface>()?.Show();

        public void HideProgressDialog() => DependencyService.Get<IProgressInterface>()?.Hide();

        protected override bool OnBackButtonPressed()
        {
            MopupService.Instance.PopAsync(true);
            return true;
        }

    }
}
