using System;
using Cryptogram.iOS.CustomViews;
using Cryptogram.iOS.Services;
using Cryptogram.Services;
using Cryptogram.Styles;
using UIKit;
using Xamarin.Forms.Platform.iOS;


[assembly: Xamarin.Forms.Dependency(typeof(ThemeService))]
namespace Cryptogram.iOS.Services
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
