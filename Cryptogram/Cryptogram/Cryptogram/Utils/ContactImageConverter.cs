using System;
using System.Globalization;
using System.IO;
using Anonymous.DesignHandler;
using EncryptedMessaging;
using Xamarin.Forms;

namespace Anonymous
{
	public class ContactImageConverter : IValueConverter
	{
		private static ImageSource GroupImageSource = DesignResourceManager.GetImageSource("group.png");
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is byte[]? ImageSource.FromStream(() => new MemoryStream(value as byte[])) : GroupImageSource;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}

	}
}
