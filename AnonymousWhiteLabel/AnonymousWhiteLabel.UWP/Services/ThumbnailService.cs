using AnonymousWhiteLabel.UWP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.System;
using Xamarin.Forms;
using XamarinShared.ViewCreator;

[assembly: Dependency(typeof(IThumbnailService))]
namespace AnonymousWhiteLabel.UWP.Services
{
    public class ThumbnailService : IThumbnailService
    {
        public byte[] GenerateThumbImage(string url, long usecond)
        {
            return new byte[0];
        }
    }
}
