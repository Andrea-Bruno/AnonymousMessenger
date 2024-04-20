using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CommunityClient.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Input : ContentPage
    {
        public Input(Action<string> OkAction, string inputPlaceholder = null)
        {
            InitializeComponent();
            InputText.Placeholder = inputPlaceholder;
            Ok.Text = Localization.Resources.Dictionary.Ok;
            Cancel.Text = Localization.Resources.Dictionary.Cancel;
            Ok.Command = new Command(() =>
            {
                OkAction?.Invoke(InputText.Text);
                _ = Navigation.PopAsync();
            });
            Cancel.Command = new Command(() => Navigation.PopAsync());
        }
    }
}