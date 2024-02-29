using System;
using Foundation;
using Telegraph.iOS.Services;
using Telegraph.Services;
using Xamarin.Forms;
[assembly: Dependency(typeof(PathService))]
namespace Telegraph.iOS.Services
{
    public class PathService : IPathService
    {
        string IPathService.InternalFolder => throw new NotImplementedException();
        string IPathService.PublicExternalFolder => throw new NotImplementedException();
        string IPathService.PrivateExternalFolder => throw new NotImplementedException();
        string IPathService.AssetsPathUrl => NSBundle.MainBundle.BundlePath;
    }
}