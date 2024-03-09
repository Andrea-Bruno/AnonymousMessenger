using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinShared.ViewCreator
{
   public interface IThumbnailService
    {
        byte[] GenerateThumbImage(string url, long usecond);
    }
}
