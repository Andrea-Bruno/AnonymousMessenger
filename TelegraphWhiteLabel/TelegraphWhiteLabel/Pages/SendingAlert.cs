using System;
using System.Threading;
using EncryptedMessaging;
using EncryptedMessaging.Resources;
using Xamarin.Forms;
namespace AnonymousWhiteLabel.Pages
{
	internal class SendingAlert : ContentPage
	{
		public SendingAlert(byte[] dataImage, Action<MessageFormat.MessageType, object> onSendClick)
		{
			Title = Dictionary.Send;
			_dataImage = dataImage;
			_image = new Image { Source = ImageSource.FromStream(() => new System.IO.MemoryStream(dataImage)) };
			_onSendClick = onSendClick;
			_grid = new Grid();
			_grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			_grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			_grid.ColumnDefinitions.Add(new ColumnDefinition());
			_grid.Children.Add(_image, 0, 0);
			var okButton = new Button { Text = Dictionary.Ok };
			okButton.Clicked += OnOk;
			var cancelButton = new Button { Text = Dictionary.Cancel };
			cancelButton.Clicked += (o, e) => Navigation.PopAsync();
			var stack = new StackLayout() { Orientation = StackOrientation.Horizontal, Children = { okButton, cancelButton } };
			_grid.Children.Add(stack, 0, 1);
			Content = _grid;
		}
		private readonly Image _image;
		private readonly Grid _grid;
		private readonly Action<MessageFormat.MessageType, object> _onSendClick;
		private byte[] _dataImage;
		private byte[] ResizeImage(byte[] data, double oroginWidth, double orininHeight)
		{
			var x = (float)oroginWidth;
			var y = (float)orininHeight;
			var ratio = x / y;
			if (x > y)
			{
				x = 800; y = 800 / ratio;
			}
			else
			{
				y = 800; x = 800 * ratio;
			}
			var scaled = DependencyService.Get<IImageResizeService>().ResizeImage(data, x, y);//under UWP this function don't work inside the MainThread!
			return scaled;
		}
		public void OnOk(object o, EventArgs e)
		{
			var width = _image.Width;
			var height = _image.Height;
			var async = new Thread(() =>
			{
				_dataImage = ResizeImage(_dataImage, width, height); //under UWP this function don't work inside the MainThread!
				_onSendClick(MessageFormat.MessageType.Image, _dataImage);
			});
			async.Start();

			//Device.BeginInvokeOnMainThread(() => {
			//});
			Navigation.PopAsync();


		}
	}
}
