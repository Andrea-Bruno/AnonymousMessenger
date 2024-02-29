using System;
using Xamarin.Forms;
namespace Telegraph
{
    public abstract class BaseContentView : ContentView
    {
        public BaseContentView()
        {
        }
        public abstract void OnAppearing();
        public abstract void OnDisappearing();
    }
}