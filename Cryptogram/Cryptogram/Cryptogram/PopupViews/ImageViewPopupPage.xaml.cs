using System.IO;
using CustomViewElements;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Cryptogram.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ImageViewPopupPage : BasePopupPage
	{

		public ImageViewPopupPage(ImageSource imageSource)
		{
			InitializeComponent();
			User_Photo.Source = imageSource;
		}


		public ImageViewPopupPage(byte[] image)
		{
			InitializeComponent();
			User_Photo.Source = ImageSource.FromStream(() => new MemoryStream(image));
		}

	}
}