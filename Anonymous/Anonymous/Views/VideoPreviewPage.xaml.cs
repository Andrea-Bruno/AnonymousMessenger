using CustomViewElements;
using System;
using Telegraph.DesignHandler;
using Utils;
using Xamarin.CommunityToolkit.Core;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static MessageCompose.AttachmentPopupPage;

namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoPreviewPage : BasePage
    {
        public AttachVideoHandler AttachVideo;
        private FileResult selectedFile;

        public VideoPreviewPage(string videoPath)
        {
            InitializeComponent();
            if (!string.IsNullOrWhiteSpace(videoPath))
            {
                VideoPlayer.Source = new FileMediaSource
                {
                    File = videoPath
                };
            }
        }

        public VideoPreviewPage(FileResult fileResult)
        {
            InitializeComponent();
            if (fileResult != null)
            {
                Toolbar.AddRightButton(0, DesignResourceManager.GetImageSource("ic_send_image.png"), SendVideo_Clicked);
                selectedFile = fileResult;
                VideoPlayer.Source = new FileMediaSource
                {
                    File = fileResult.FullPath
                };
            }
        }

        public void Back_Clicked(object sender, EventArgs eventArgs)
        {
            sender.HandleButtonSingleClick();
            OnBackButtonPressed();
        }

        public async void SendVideo_Clicked(object sender, EventArgs eventArgs)
        {
            sender.HandleButtonSingleClick();
            if (AttachVideo != null)
            {
                AttachVideo(selectedFile);
                await Application.Current.MainPage.Navigation.PopAsync(false);
            }
            else
            {
                Console.WriteLine("Error sending file");
            }
        }
    }
}