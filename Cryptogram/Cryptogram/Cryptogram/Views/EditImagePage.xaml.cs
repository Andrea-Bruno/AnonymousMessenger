using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CustomViewElements;
using Syncfusion.SfImageEditor.XForms;
using Anonymous.DesignHandler;
using Utils;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Linq;
using static Anonymous.Views.ChatRoom;
using System.Threading;
using NativeMedia;
using Anonymous.Services;
using System.Threading.Tasks;

namespace Anonymous.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditImagePage : BasePage
    {
        #region Image Management
        private class ScrollablePaneImage
        {
            public int index { get; set; }
            public ImageSource imageSource { get; set; }
        }
        private class SelectedImageManagement
        {
            private List<byte[]> Images = new List<byte[]>();

            private byte[] CompressImage(byte[] image)
            {
                return DependencyService.Get<IImageCompressionService>().CompressImage(image, 25);
            }

            public int ImageCount()
            {
                return Images.Count;
            }

            public void AddImage(byte[] image)
            {
                Images.Add(image);
            }

            public void AddImage(IEnumerable<byte[]> images)
            {
                foreach (var image in images)
                {
                    Images.Add(image);
                }
            }

            public void InsertImage(int index, byte[] image)
            {
                Images.Insert(index, image);
            }

            public bool ReplaceImage(int index, byte[] image)
            {
                if (RemoveImage(index))
                {
                    InsertImage(index, image);
                    return true;
                }
                return false;
            }

            public bool RemoveImage(int index)
            {
                if (index < Images.Count && index >= 0)
                {
                    Images.RemoveAt(index);
                    return true;
                }
                return false;
            }

            public List<byte[]> GetSelectedImages(bool withCompression = true)
            {
                if (withCompression)
                {
                    var result = new List<byte[]>();
                    Parallel.ForEach(Images, image => { result.Add(CompressImage(image)); });
                    return result;
                }
                else
                {
                    return Images;
                }
            }

            public byte[] GetSelectedImage(int index, bool withCompression = true)
            {
                if (withCompression)
                {
                    return CompressImage(Images[index]);
                }
                else
                {
                    return Images[index];
                }

            }

            public bool Contains(byte[] image)
            {
                if (Images.Where(x => x.SequenceEqual(image)).Count() > 0)
                {
                    return true;
                }
                return false;
            }

            public List<ScrollablePaneImage> GetSelectedImageSources()
            {
                int index = 0;
                var selectedImages = new List<ScrollablePaneImage>();
                foreach (var item in Images)
                {
                    selectedImages.Add(new ScrollablePaneImage()
                    {
                        imageSource = ImageSource.FromStream(() => new MemoryStream(item)),
                        index = index
                    });
                    index++;
                }
                return selectedImages;
            }

            public ScrollablePaneImage GetSelectedImageSource(int index)
            {
                return new ScrollablePaneImage()
                {
                    imageSource = ImageSource.FromStream(() => new MemoryStream(Images[index])),
                    index = index
                };
            }
        }
        private SelectedImageManagement selectedImageManagement = new SelectedImageManagement();
        #endregion

        public AttachPictureHandler AttachPicture;
        private ScrollablePaneImage CurrentImage = new ScrollablePaneImage();

        public EditImagePage(List<byte[]> images)
        {
            InitializeComponent();
            Toolbar.AddRightButton(0, DesignResourceManager.GetImageSource("ic_multiple_image_add.png"), AddImage_Clicked);
            Toolbar.AddRightButton(1, DesignResourceManager.GetImageSource("ic_multiple_image_delete.png"), RemoveImage_Clicked);
            Toolbar.AddRightButton(2, DesignResourceManager.GetImageSource("ic_multiple_image_edit.png"), EditImage_Clicked);
            Toolbar.AddRightButton(3, DesignResourceManager.GetImageSource("ic_multiple_image_send.png"), SendImage_Clicked);

            if (images != null)
            {
                selectedImageManagement.AddImage(images);

                if (selectedImageManagement.ImageCount() > 1)
                    ScrollablePane.IsVisible = true;

                var selectedimages = selectedImageManagement.GetSelectedImageSources();
                SelectedImages.ItemsSource = selectedimages;
                SelectedImages.SelectedItem = selectedimages.FirstOrDefault();

                CurrentImage = selectedimages.FirstOrDefault();
                imageHolder.Source = selectedimages.FirstOrDefault().imageSource;
            }
            else
            {
                OnBackButtonPressed();
            }
            HideProgressDialog();
        }

        private void SelectedImages_ItemTapped(object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs e)
        {
            CurrentImage = e.ItemData as ScrollablePaneImage;
            imageHolder.Source = CurrentImage.imageSource;
        }

        #region Add Button
        private async void AddImage_Clicked(object sender, EventArgs eventArgs)
        {
            sender.HandleButtonSingleClick();
            if (selectedImageManagement.ImageCount() >= 30)
                return;

            ShowProgressDialog();
            var cts = new CancellationTokenSource();
            try
            {
                IMediaFile[] newImages = null;
                var request = new MediaPickRequest(30 - selectedImageManagement.ImageCount(), MediaFileType.Image)
                {
                    PresentationSourceBounds = System.Drawing.Rectangle.Empty,
                    UseCreateChooser = true,
                    Title = "Select"
                };
                cts.CancelAfter(TimeSpan.FromMinutes(5));
                var results = await MediaGallery.PickAsync(request, cts.Token);
                newImages = results?.Files?.ToArray();
                if (newImages.Count() > 0)
                {
                    foreach (var image in newImages)
                    {
                        if (image.Type == MediaFileType.Image)
                        {
                            using (var stream = await image.OpenReadAsync())
                            {
                                var imageBytes = Utils.Utils.StreamToByteArray(stream);
                                if (!selectedImageManagement.Contains(imageBytes))
                                    selectedImageManagement.AddImage(imageBytes);
                            }
                        }
                    }
                    var selectedimages = selectedImageManagement.GetSelectedImageSources();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (selectedImageManagement.ImageCount() > 1)
                            ScrollablePane.IsVisible = true;
                        SelectedImages.ItemsSource = null;
                        SelectedImages.ItemsSource = selectedimages;
                        CurrentImage = selectedimages[CurrentImage.index];
                        SelectedImages.SelectedItem = CurrentImage;
                    });
                }
            }
            catch (Exception) { }
            finally
            {
                HideProgressDialog();
                cts.Dispose();
            }
        }
        #endregion

        #region Remove Button
        private void RemoveImage_Clicked(object sender, EventArgs eventArgs)
        {
            if (selectedImageManagement.ImageCount() > 1)
            {
                if (selectedImageManagement.RemoveImage(CurrentImage.index))
                {
                    if (selectedImageManagement.ImageCount() <= 1)
                        ScrollablePane.IsVisible = false;
                    var selectedImages = selectedImageManagement.GetSelectedImageSources();
                    SelectedImages.ItemsSource = null;
                    SelectedImages.ItemsSource = selectedImages;
                    if (selectedImageManagement.ImageCount() == CurrentImage.index)
                        CurrentImage = selectedImages[(CurrentImage.index - 1)];
                    else
                        CurrentImage = selectedImages[(CurrentImage.index)];
                    SelectedImages.SelectedItem = CurrentImage;
                    imageHolder.Source = CurrentImage.imageSource;
                }
            }
        }
        #endregion

        #region Edit Image
        private void EditImage_Clicked(object sender, EventArgs eventArgs)
        {
            sender.HandleButtonSingleClick();

            SfImageEditorPage sfImageEditorPage = new SfImageEditorPage(CurrentImage.imageSource);
            sfImageEditorPage.OnImageSaving += OnImageSaving;

            if (Device.RuntimePlatform.ToLower() == "ios")
                Application.Current.MainPage.Navigation.PushAsync(sfImageEditorPage, false);
            else if (Device.RuntimePlatform.ToLower() == "uwp")
                Navigation.PushAsync(sfImageEditorPage);
            else
                Navigation.PushModalAsync(sfImageEditorPage);
        }

        private void OnImageSaving(Stream stream)
        {
            var editedImageBytes = Utils.Utils.StreamToByteArray(stream);

            if (selectedImageManagement.ReplaceImage(CurrentImage.index, editedImageBytes))
            {
                var selectedImages = selectedImageManagement.GetSelectedImageSources();
                CurrentImage = selectedImages[CurrentImage.index];

                imageHolder.Source = CurrentImage.imageSource;
                SelectedImages.ItemsSource = null;
                SelectedImages.ItemsSource = selectedImages;
                SelectedImages.SelectedItem = CurrentImage;
            }
        }

        private class SfImageEditorPage : ContentPage
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
        #endregion

        #region Send Button
        private async void SendImage_Clicked(object sender, EventArgs eventArgs)
        {
            sender.HandleButtonSingleClick();
            if (selectedImageManagement.ImageCount() > 0)
            {
                AttachPicture(selectedImageManagement.GetSelectedImages());
                await Application.Current.MainPage.Navigation.PopAsync(false);
            }
        }
        #endregion

        #region Back Button
        protected override bool OnBackButtonPressed()
        {
            Application.Current.MainPage.Navigation.PopAsync(false);
            return true;
        }

        public void Back_Clicked(object sender, EventArgs eventArgs)
        {
            sender.HandleButtonSingleClick();
            OnBackButtonPressed();
        }
        #endregion

    }
}