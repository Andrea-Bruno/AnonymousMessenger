using System;
using System.Collections.Generic;
using System.IO;
using Android.Graphics;
using Android.Media;
using Android.Provider;
using MessageCompose.Services;
using Cryptogram.Droid.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(ThumbNailManager))]
namespace Cryptogram.Droid.Services
{
    public class ThumbNailManager : IThumbnailService
    {
        public ThumbNailManager()
        {
        }
		public byte[] GenerateThumbImage(string url, long usecond)
		{
			Bitmap bitmap = Utils.AsyncUtil.RunSync(()=>ThumbnailUtils.CreateVideoThumbnailAsync(url, ThumbnailKind.FullScreenKind));

            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Compress(Bitmap.CompressFormat.Png, 40, ms);
                return ms.ToArray();
            }
        }
    }
}
