using Anonymous.Droid.Services;
using Anonymous.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(PathService))]
namespace Anonymous.Droid.Services
{
    public class PathService : IPathService
    {
        public string InternalFolder => Android.App.Application.Context.FilesDir.AbsolutePath;

#pragma warning disable CS0618 // 'Environment.ExternalStorageDirectory' è obsoleto: 'deprecated'
        public string PublicExternalFolder => Android.OS.Environment.ExternalStorageDirectory.ToString();
#pragma warning restore CS0618 // 'Environment.ExternalStorageDirectory' è obsoleto: 'deprecated'

        public string PrivateExternalFolder => System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

        public string AssetsPathUrl => "file:///android_asset/";
    }
}