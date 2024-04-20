using System;
using Foundation;
using Anonymous.iOS.Services;
using Anonymous.Services;
using Xamarin.Forms;
[assembly: Dependency(typeof(PathService))]
namespace Anonymous.iOS.Services
{
    public class PathService : IPathService
    {
        string IPathService.InternalFolder => throw new NotImplementedException();
        string IPathService.PublicExternalFolder => throw new NotImplementedException();
        string IPathService.PrivateExternalFolder => throw new NotImplementedException();
        string IPathService.AssetsPathUrl => NSBundle.MainBundle.BundlePath;
    }
}