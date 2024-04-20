using Anonymous.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(Anonymous.Droid.Services.ThemeService))]
namespace Anonymous.Droid.Services
{
    public class ThemeService : IThemeService
    {
        public ThemeService() { }

        public void SetTheme(bool isDarkMode) { }
    }
}