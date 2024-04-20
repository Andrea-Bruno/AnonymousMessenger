using CustomViewElements;
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MessageCompose;
using Utils;

namespace Anonymous.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImagePreviewView : BasePage
    {
        public AttachmentPopupPage.AttachPictureHandler AttachPicture;
      
        public ImagePreviewView(byte[] image)
        {
            InitializeComponent();

            if (image != null)
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    imageHolder.WidthRequest = Application.Current.MainPage.Width;
                    imageHolder.HeightRequest = Application.Current.MainPage.Height - Toolbar.Height;
                }
                imageHolder.Source = ImageSource.FromStream(() => new MemoryStream(image));
            }
        }

        public void Back_Clicked(object sender, EventArgs eventArgs)
        {
            sender.HandleButtonSingleClick();
            PinchToZoomContainer.Content = null;
            imageHolder.Source = null;
            OnBackButtonPressed();
        }

    }
}