using System;
using Android.App;
using Cryptogram.Droid.Services;
using Cryptogram.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(CloseApplicationManager))]
namespace Cryptogram.Droid.Services
{
    public class CloseApplicationManager: ICloseApplication
    {
        public void CloseApplication()
        {
            var activity = (Activity)Forms.Context;
            if(activity!=null)
                activity.FinishAffinity();
        }
    }
}
