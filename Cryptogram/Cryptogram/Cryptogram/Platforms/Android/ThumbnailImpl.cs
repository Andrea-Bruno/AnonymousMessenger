#if ANDROID
using System;
using System.IO;
using Android.Graphics;
using Android.Media;
using MessageCompose.Services;

namespace Cryptogram.Services
{
    // Ported from Cryptogram.Droid.Services.ThumbNailManager (Xamarin.Android).
    // The original used ThumbnailUtils.CreateVideoThumbnailAsync + PNG(40) and ignored the
    // usecond offset. This port uses MediaMetadataRetriever to grab the frame at the given
    // microsecond offset and encodes it as JPEG bytes.
    public sealed class ThumbnailImpl : IThumbnailService
    {
        private const int JpegQuality = 75;

        public byte[] GenerateThumbImage(string url, long usecond)
        {
            if (string.IsNullOrEmpty(url))
                return Array.Empty<byte>();

            var retriever = new MediaMetadataRetriever();
            try
            {
                retriever.SetDataSource(url);
                Bitmap frame = retriever.GetFrameAtTime(usecond, Option.Closest);
                if (frame == null)
                    return Array.Empty<byte>();

                using var ms = new MemoryStream();
                frame.Compress(Bitmap.CompressFormat.Jpeg, JpegQuality, ms);
                return ms.ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine("ThumbnailImpl.GenerateThumbImage: " + e.Message);
                return Array.Empty<byte>();
            }
            finally
            {
                try { retriever.Release(); } catch { }
            }
        }
    }
}
#endif
