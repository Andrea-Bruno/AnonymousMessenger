using System;
using System.Threading;
using Cryptogram.iOS.Services;
using Cryptogram.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(CloseApplicationManager))]
namespace Cryptogram.iOS.Services
{
    public class CloseApplicationManager : ICloseApplication
    {
        public CloseApplicationManager()
        {
        }

        public void CloseApplication() => Thread.CurrentThread.Abort();

    }
}
