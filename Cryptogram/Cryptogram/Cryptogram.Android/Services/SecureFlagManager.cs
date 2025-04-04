using Cryptogram.Droid.Services;
using Cryptogram.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(SecureFlagManager))]
namespace Cryptogram.Droid.Services
{
    public class SecureFlagManager : ISecurityFlag
    {
        public void DisableSecureFlag() => MainActivity.EnableSecureFlag(false);
        public void EnableSecureFlag() => MainActivity.EnableSecureFlag(true);
    }
}