using System;
using System.Threading;
using Anonymous.iOS.Services;
using Anonymous.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(CloseApplicationManager))]
namespace Anonymous.iOS.Services
{
    public class CloseApplicationManager : ICloseApplication
    {
        public CloseApplicationManager()
        {
        }

        public void CloseApplication() => Thread.CurrentThread.Abort();

    }
}
