using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Controls;

// Global shim restoring the Xamarin.CommunityToolkit DisplayToastAsync extension,
// removed in CommunityToolkit.Maui 14 (replaced by Toast.Make(...).Show()).
public static class ToastCompat
{
    public static Task DisplayToastAsync(this VisualElement _, string message)
        => Toast.Make(message).Show();

    public static Task DisplayToastAsync(this VisualElement _, string message, ToastDuration duration)
        => Toast.Make(message, duration).Show();

    public static Task DisplayToastAsync(this VisualElement _, string message, ToastDuration duration, double textSize)
        => Toast.Make(message, duration, textSize).Show();
}
