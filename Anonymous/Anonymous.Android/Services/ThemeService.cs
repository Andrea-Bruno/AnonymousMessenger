using Telegraph.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(Telegraph.Droid.Services.ThemeService))]
namespace Telegraph.Droid.Services
{
    public class ThemeService : IThemeService
    {
        public ThemeService() { }

        public void SetTheme(bool isDarkMode) { }
    }
}