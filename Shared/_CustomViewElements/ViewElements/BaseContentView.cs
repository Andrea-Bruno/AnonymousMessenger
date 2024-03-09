using System;
using Xamarin.Forms;
namespace CustomViewElements
{
    public abstract class BaseContentView : ContentView
    {

        public void ShowProgressDialog() => DependencyService.Get<Services.IProgressInterface>()?.Show();

        public void HideProgressDialog() => DependencyService.Get<Services.IProgressInterface>()?.Hide();

        public BaseContentView()
        {
        }
        public abstract void OnAppearing();
        public abstract void OnDisappearing();
    }
}