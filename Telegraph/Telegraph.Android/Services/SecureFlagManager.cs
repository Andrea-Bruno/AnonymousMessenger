using Telegraph.Droid.Services;
using Telegraph.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(SecureFlagManager))]
namespace Telegraph.Droid.Services
{
    public class SecureFlagManager : ISecurityFlag
    {
        public void DisableSecureFlag() => MainActivity.EnableSecureFlag(false);
        public void EnableSecureFlag() => MainActivity.EnableSecureFlag(true);
    }
}