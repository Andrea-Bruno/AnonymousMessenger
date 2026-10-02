using CustomViewElements;
using System;
using System.Threading.Tasks;
using Cryptogram.DesignHandler;
using Utils;
using Xamarin.CommunityToolkit.Core;
using Xamarin.CommunityToolkit.UI.Views;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;
using Microsoft.Maui.Networking;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using static MessageCompose.AttachmentPopupPage;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Cryptogram.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoPreviewPage : BasePage
    {
        private MediaElement VideoPlayer;
        private FileResult selectedFile = null;
        private string selectedFilePath = null;
        public AttachVideoHandler AttachVideo;

        public VideoPreviewPage(string videoPath)
        {
            InitializeComponent();
            Task.Run(() => CreateVideoPlayerLayout()).Wait();
            if (!string.IsNullOrWhiteSpace(videoPath))
            {
                selectedFilePath = videoPath;
            }
        }

        public VideoPreviewPage(FileResult fileResult)
        {
            InitializeComponent();
            Task.Run(() => CreateVideoPlayerLayout()).Wait();
            if (fileResult != null)
            {
                selectedFile = fileResult;
            }
        }

        protected override void OnAppearing()
        {
            if (!string.IsNullOrWhiteSpace(selectedFilePath))
            {
                VideoPlayer.Source = new FileMediaSource
                {
                    File = selectedFilePath
                };
            }
            else if (selectedFile != null)
            {
                Toolbar.AddRightButton(0, DesignResourceManager.GetImageSource("ic_send_image.png"), SendVideo_Clicked);
                VideoPlayer.Source = new FileMediaSource
                {
                    File = selectedFile.FullPath
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

        private void CreateVideoPlayerLayout()
        {
            VideoPlayer = new MediaElement()
            {
                ShowsPlaybackControls = true,
                BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent,
                AutoPlay = false,
                KeepScreenOn = true,
                Aspect = Aspect.AspectFit,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            if (Device.RuntimePlatform == Device.iOS)
            {
                VideoPlayer.HeightRequest = 500;
            }

            VideoLayout.Children.Add(VideoPlayer);
        }
    }
}