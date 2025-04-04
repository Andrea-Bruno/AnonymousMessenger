using Cryptogram.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(Cryptogram.Droid.Services.ThemeService))]
namespace Cryptogram.Droid.Services
{
    public class ThemeService : IThemeService
    {
        public ThemeService() { }

        public void SetTheme(bool isDarkMode) { }
    }
}