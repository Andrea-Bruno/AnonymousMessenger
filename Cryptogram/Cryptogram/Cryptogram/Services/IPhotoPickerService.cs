using System;
using System.IO;
using System.Threading.Tasks;

namespace Cryptogram.Droid.Services
{
    public interface IPhotoPickerService
    {
        Task<Stream> GetImageStreamAsync();
    }
}
