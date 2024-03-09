using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using EncryptedMessaging;
using Xamarin.Forms;
using XamarinShared;

namespace CustomViewElements
{
    public class CustomAvatar : Frame
    {
        public static readonly BindableProperty ContactProperty = BindableProperty.Create<CustomAvatar, Contact>(w => w.Contact, null);
        public static ImageSource GroupImageSource = Utils.Icons.IconProvider?.Invoke("ic_new_addGroup_contact");
        private bool isViewRendered;
        private bool isContactPropertySet;

        public Contact Contact
        {
            get { return (Contact)GetValue(ContactProperty); }
            set
            {

                SetValue(ContactProperty, value);
            }
        }

        public CustomAvatar()
        {
            Padding = 0;
            Margin = 0;
            HasShadow = false;
            BorderColor = Color.Transparent;
            IsClippedToBounds = true;
        }

        private void Contact_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Contact.Avatar))
            {
                var contact = sender as Contact;
                if (contact == null) return;

                ChatPageSupport.GetContactViewItems(contact).Avatar = contact.Avatar;
                UpdateView();
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(HeightProperty))
                MinimumHeightRequest = HeightRequest;
            else if (propertyName == nameof(WidthRequest))
                MinimumWidthRequest = WidthRequest;
            else if (Contact != null && propertyName == ContactProperty.PropertyName)
            {
                if (!isContactPropertySet)
                {
                    isContactPropertySet = !isContactPropertySet;
                    Contact.PropertyChanged += Contact_PropertyChanged;
                }

                UpdateView();
            }
            else if (Contact != null && propertyName == "Renderer")
            {
                if (!isViewRendered)
                    isViewRendered = true;
                else
                Contact.PropertyChanged -= Contact_PropertyChanged;
            }
        }

        private void UpdateView()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Content = GetAvatar(Contact, HeightRequest / 2);
                BackgroundColor = Color.FromHex(Contact.LightColorAsHex);
                CornerRadius = (float)HeightRequest / 2;
            });

        }

        public static View GetAvatar(Contact contact, double fontSize)
        {
            ChatPageSupport.GetContactViewItems(contact, out var contactViewItems);

            if (contactViewItems.Avatar?.Length == 0)
            {
                contactViewItems.Avatar = contact.Avatar;
            }

            if (contact.IsGroup || contactViewItems.Avatar != null)
                return new Image
                {
                    Aspect = Aspect.AspectFill,
                    Source = contact.IsGroup ? GroupImageSource : ImageSource.FromStream(() => new MemoryStream(contact.Avatar)),
                };


            else
            {
                return new CustomLabel
                {
                    Text = (string)contact.NameFirstLetter,
                    FontSize = fontSize,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Color.FromHex(contact.DarkColorAsHex),
                };

            }
        }

    }
    public class ContactAvatarConverter : IValueConverter
    {
        private double _fontSize;

        public ContactAvatarConverter(double fontSize)
        {
            _fontSize = fontSize;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var contact = parameter as Contact;
            return CustomAvatar.GetAvatar(contact, _fontSize);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}