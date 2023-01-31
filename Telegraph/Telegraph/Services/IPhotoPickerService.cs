using System;
using System.IO;
using System.Threading.Tasks;

namespace Telegraph.Droid.Services
{
    public interface IPhotoPickerService
    {
        Task<Stream> GetImageStreamAsync();
    }
}
