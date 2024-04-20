using System;
using Android.App;
using Anonymous.Droid.Services;
using Anonymous.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(CloseApplicationManager))]
namespace Anonymous.Droid.Services
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
