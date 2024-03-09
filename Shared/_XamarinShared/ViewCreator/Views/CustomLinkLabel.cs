using Xamarin.Forms;

namespace XamarinShared
{
    public class CustomLinkLabel : Label
    {
        public FormattedString LinksText
        {
            set => SetValue(TextProperty, value);
        }
    }
}
