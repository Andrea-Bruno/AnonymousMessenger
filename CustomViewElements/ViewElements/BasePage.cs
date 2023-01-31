using System;
using Xamarin.Essentials;
using Xamarin.Forms;
namespace CustomViewElements
{

    public class BasePage : ContentPage
    {
        public void ShowProgressDialog() => DependencyService.Get<Services.IProgressInterface>().Show();

        public void HideProgressDialog() => DependencyService.Get<Services.IProgressInterface>().Hide();

        public BasePage() {
            try
            {
                NavigationPage.SetHasNavigationBar(this, false);
                BackgroundColor = Palette.CommonBackgroundColor;
            }catch(Exception e)
            {

            }
        }

        protected override bool OnBackButtonPressed()
        {
            MainThread.InvokeOnMainThreadAsync(async () => await Navigation.PopAsync(true));
            return true;
        }
    }
    

}