using System.IO;
using System.Threading.Tasks;

namespace AnonymousWhiteLabel
{
	public interface IPhotoPickerService
	{
		Task<Stream> GetImageStreamAsync();
	}
}
