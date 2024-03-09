using AnonymousWhiteLabel;
using Xamarin.Forms;

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
