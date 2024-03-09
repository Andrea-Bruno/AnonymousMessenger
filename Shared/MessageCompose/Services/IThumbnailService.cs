using System;
using System.IO;
using Xamarin.Forms;

namespace MessageCompose.Services
{
    public interface IThumbnailService
    {
        byte[] GenerateThumbImage(string url, long usecond);
    }
}
