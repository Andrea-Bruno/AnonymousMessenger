using CommunityToolkit.Maui;
using Cryptogram.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Mopups;
using Mopups.Hosting;
using MPowerKit.Lottie;
using ZXing.Net.Maui.Controls;

namespace Cryptogram
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement(false)
                .ConfigureMopups()
                .UseMPowerKitLottie()
                .UseBarcodeReader();

            RegisterHandlers(builder);
            RegisterServices(builder);

            return builder.Build();
        }

        private static void RegisterHandlers(MauiAppBuilder builder)
        {
            // Custom renderers from the legacy Xamarin.Android head are registered here as handlers.
            CustomLinkLabelHandler.Register();
            CustomEntryHandler.Register();
        }

        private static void RegisterServices(MauiAppBuilder builder)
        {
            // Neutralized legacy DependencyService contracts (original platform
            // impls were in the removed Cryptogram.Android head). Registered as
            // singletons before the App is constructed so the startup path finds
            // a non-null instance instead of dereferencing null.
            DependencyService.RegisterSingleton<IStatusBarColor>(new StatusBarColorStub());
            DependencyService.RegisterSingleton<IEndCall>(new EndCallStub());
            DependencyService.RegisterSingleton<ISharedPreference>(new SharedPreferenceStub());
            DependencyService.RegisterSingleton<IAudioPlayerSpeakerService>(new AudioPlayerSpeakerStub());
            DependencyService.RegisterSingleton<IProgressInterface>(new ProgressStub());
            DependencyService.RegisterSingleton<ISecurityFlag>(new SecurityFlagStub());
            DependencyService.RegisterSingleton<CustomViewElements.Services.IProgressInterface>(new ProgressImpl());
            DependencyService.RegisterSingleton<IImageCompressionService>(new ImageCompressionStub());
            DependencyService.RegisterSingleton<ICloseApplication>(new CloseApplicationStub());
            DependencyService.RegisterSingleton<CustomViewElements.Services.IKeyboardRegistrationService>(new KeyboardRegistrationImpl());
            DependencyService.RegisterSingleton<MessageCompose.Services.IThumbnailService>(new ThumbnailImpl());
            DependencyService.RegisterSingleton<MessageCompose.Services.IAudioRecorder>(new AudioRecorderImpl());
            DependencyService.RegisterSingleton<MessageCompose.Services.IGpsService>(new GpsStub());
            DependencyService.RegisterSingleton<Cryptogram.Backup.IDriveService>(new DriveServiceStub());
            DependencyService.RegisterSingleton<XamarinShared.ViewCreator.IAudioPlayer>(new AudioPlayerImpl());
        }
    }
}
