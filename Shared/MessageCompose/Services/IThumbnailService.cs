using System;
using System.IO;
using Microsoft.Maui.Controls;

namespace MessageCompose.Services
{
    public interface IThumbnailService
    {
        byte[] GenerateThumbImage(string url, long usecond);
    }
}
