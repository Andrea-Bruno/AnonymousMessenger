using System;
using System.IO;
using System.Threading.Tasks;

namespace Anonymous.Droid.Services
{
    public interface IPhotoPickerService
    {
        Task<Stream> GetImageStreamAsync();
    }
}
