using System;
using System.Globalization;
using System.IO;
using Microsoft.Maui.Controls;

namespace Utils
{
	public class ContactImageConverter : IValueConverter
	{
		private static ImageSource GroupImageSource = ImageSource.FromFile("ic_group_icon");
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
