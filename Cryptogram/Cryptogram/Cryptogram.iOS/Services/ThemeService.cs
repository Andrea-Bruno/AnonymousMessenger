using System;
using Anonymous.iOS.CustomViews;
using Anonymous.iOS.Services;
using Anonymous.Services;
using Anonymous.Styles;
using UIKit;
using Xamarin.Forms.Platform.iOS;


[assembly: Xamarin.Forms.Dependency(typeof(ThemeService))]
namespace Anonymous.iOS.Services
{
    public class ThemeService : IThemeService
    {
        public ThemeService()
        {
        }

        public void SetTheme(bool isDarkTheme)
        {
           // baseContentPage.SetTheme();
            AppDelegate.ChangeTheme(isDarkTheme);
        }
    }
}
