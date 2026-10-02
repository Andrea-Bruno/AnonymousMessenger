using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
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
        }

        private static void RegisterServices(MauiAppBuilder builder)
        {
            // Legacy DependencyService implementations are registered here.
        }
    }
}
