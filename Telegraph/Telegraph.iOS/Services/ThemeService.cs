using System;
using Telegraph.iOS.CustomViews;
using Telegraph.iOS.Services;
using Telegraph.Services;
using Telegraph.Styles;
using UIKit;
using Xamarin.Forms.Platform.iOS;


[assembly: Xamarin.Forms.Dependency(typeof(ThemeService))]
namespace Telegraph.iOS.Services
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
