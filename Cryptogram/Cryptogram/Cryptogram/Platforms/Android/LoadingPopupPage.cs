#if ANDROID
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Mopups.Pages;

namespace Cryptogram.Services
{
    // Minimal code-only Mopups loading HUD (replaces the AndHUD spinner from the original
    // ProgressLoader). A centered spinner + title over a dimmed, non-dismissible backdrop.
    public sealed class LoadingPopupPage : PopupPage
    {
        public LoadingPopupPage(string title)
        {
            BackgroundColor = Color.FromArgb("#80000000");

            var spinner = new ActivityIndicator
            {
                IsRunning = true,
                Color = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var label = new Label
            {
                Text = title ?? string.Empty,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var stack = new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(28, 22),
                Spacing = 12,
                BackgroundColor = Color.FromArgb("#CC000000")
            };
            stack.Children.Add(spinner);
            stack.Children.Add(label);
            Content = stack;
        }
    }
}
#endif
