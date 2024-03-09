
namespace AnonymousWhiteLabel
{
    public interface IThumbnailService
    {
        byte[] GenerateThumbImage(string url, long usecond);
    }
}