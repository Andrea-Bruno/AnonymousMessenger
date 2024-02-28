using System;
using System.IO;
using CustomViewElements;
using Syncfusion.SfImageEditor.XForms;
using Telegraph.DesignHandler;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Telegraph.Views.ChatRoom;

namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditImagePage : BasePage
    {
        public AttachPictureHandler AttachPicture;
        private byte[] selectedImage;

        public EditImagePage(byte[] image)
        {
            InitializeComponent();
            Toolbar.AddRightButton(0,DesignResourceManager.GetImageSource("ic_send_image.png"), SendImage_Clicked);
            Toolbar.AddRightButton(1,DesignResourceManager.GetImageSource("ic_edit.png"), EditImage_Clicked);

            if (image != null)
            {
                selectedImage = image;
                imageHolder.Source = ImageSource.FromStream(() => new MemoryStream(image));
            }
            HideProgressDialog();
        }

        protected override bool OnBackButtonPressed()
        {
            Application.Current.MainPage.Navigation.PopAsync(false);
            return true;
        }
     
        public async void SendImage_Clicked(object sender, EventArgs eventArgs)
        {
            if (AttachPicture != null)
            {
                AttachPicture(selectedImage);
                await Application.Current.MainPage.Navigation.PopAsync(false);
            }
            else
            {
                Console.WriteLine("Error sending file");
            }
        }

        public void EditImage_Clicked(object sender, EventArgs eventArgs)
        {
            LoadFromStream(imageHolder.Source);
        }

        private void LoadFromStream(ImageSource source)
        {
            SfImageEditorPage sfImageEditorPage = new SfImageEditorPage(source);
            sfImageEditorPage.OnImageSaving += OnImageSaving;

            if (Device.RuntimePlatform.ToLower() == "ios")
            {
                Application.Current.MainPage.Navigation.PushAsync(sfImageEditorPage, false);
            }
            else if (Device.RuntimePlatform.ToLower() == "uwp")
            {
                Navigation.PushAsync(sfImageEditorPage);
            }
            else
            {
                Navigation.PushModalAsync(sfImageEditorPage);
            }
        }

        private void OnImageSaving(Stream stream)
        {
            selectedImage = Utils.Utils.StreamToByteArray(stream);
            imageHolder.Source = ImageSource.FromStream(() => new MemoryStream(selectedImage));

        }

        public class SfImageEditorPage :  ContentPage
        {
            public delegate void ImageSavingHandler(Stream stream);

            public event ImageSavingHandler OnImageSaving;

            public SfImageEditorPage(ImageSource imagesource)
            {
                (Application.Current.MainPage as NavigationPage).BarBackgroundColor = DesignResourceManager.GetColorFromStyle("Color1");
                SfImageEditor editor = new SfImageEditor();
                editor.Source = imagesource;
                editor.RotatableElements = ImageEditorElements.Text;
                editor.ImageSaving += ImageSaving;
                Content = editor;
            }

            private void ImageSaving(object sender, ImageSavingEventArgs args)
            {
                args.Cancel = true; // To avoid the image saved into pictures library
                OnImageSaving(args.Stream);
                if (Device.RuntimePlatform == Device.Android)
                    OnBackButtonPressed();
                else
                    Application.Current.MainPage.Navigation.PopAsync(false);

            }
        }

        public void Back_Clicked(object sender, EventArgs eventArgs) => OnBackButtonPressed();
    }
}