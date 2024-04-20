using Anonymous.Droid.Services;
using Anonymous.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(SecureFlagManager))]
namespace Anonymous.Droid.Services
{
    public class SecureFlagManager : ISecurityFlag
    {
        public void DisableSecureFlag() => MainActivity.EnableSecureFlag(false);
        public void EnableSecureFlag() => MainActivity.EnableSecureFlag(true);
    }
}