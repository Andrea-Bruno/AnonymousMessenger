using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AnonymousWhiteLabel.DesignHandler
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
            return DesignResourceManager.GetImageSource(Source);
        }
    }
}
//Source = "{local:ImageResourceExtension menu.settings.png}"