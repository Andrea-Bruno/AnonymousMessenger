namespace AnonymousWhiteLabel
{
	public interface IImageResizeService
	{
		byte[] ResizeImage(byte[] imageData, float width, float height);
	}
}
