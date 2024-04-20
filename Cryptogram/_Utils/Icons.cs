using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Utils
{
    [ContentProperty(nameof(Source))]
    public class ImageResourceExtension : IMarkupExtension
    {

        public string Source { get; set; }
        public ImageResourceExtension()
        {
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source == null) return null;
            return Icons.IconProvider?.Invoke(Source);
        }
    }

    public delegate ImageSource IconProvider(string key);

    public class Icons
    {
        public static IconProvider IconProvider;
    }
}
