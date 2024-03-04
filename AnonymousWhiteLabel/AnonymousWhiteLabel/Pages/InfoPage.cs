using EncryptedMessaging;
using EncryptedMessaging.Resources;
using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Dict = Localization.Resources.Dictionary;

namespace AnonymousWhiteLabel.Pages
{
    public class InfoPage : ContentPage
    {
        public TapGestureRecognizer openLink = new TapGestureRecognizer();
        private List<Suggestion> suggestions = new List<Suggestion>()
        {
            new Suggestion{LabelText= Dict.DuckDuckGo, Text=Dict.OpenWebsite, Link="https://duckduckgo.com/" },
            new Suggestion{LabelText=Dict.TorBrowser, Text=Dict.OpenWebsite, Link="https://www.torproject.org/" },
            new Suggestion{LabelText=Dict.KaliLinux, Text=Dict.OpenWebsite, Link="https://www.kali.org/" },
            new Suggestion{LabelText=Dict.LineageOS, Text=Dict.OpenWebsite, Link="https://lineageos.org/" },
        };

        public InfoPage()
        {
            _grid = new Grid();
            _grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            _grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _grid.ColumnDefinitions.Add(new ColumnDefinition());

            var upload = new Button { Text = Dict.Upload, IsVisible = true };
            var avatar = new Image
            {

                // Aspect = Aspect.AspectFill,
                WidthRequest = 200,
                HeightRequest = 200,
                MinimumHeightRequest = 200,
                MinimumWidthRequest = 200,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Source = ImageSource.FromStream(() => new MemoryStream(App.Context.My.GetAvatar()))
            };

            upload.Clicked += async (sender, e) =>
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = Dict.PleaseSelectImageFiles,
                    FileTypes = FilePickerFileType.Images,
                });
                if (result == null) return;
                avatar.Source = ImageSource.FromStream(() => result.OpenReadAsync().Result);
                App.Context.My.SetAvatar(StreamToByteArray(result.OpenReadAsync().Result));
            };

            var labelPublicKey = new Label { Text = Dictionary.PublicKey + ":" };
            var publicKey = new Editor { Text = App.Context.My.Contact.GetQrCode(), IsReadOnly = true };

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(App.Context.My.Contact.GetQrCode(), QRCodeGenerator.ECCLevel.Q);
            //QRCode qrCode = new QRCode(qrCodeData);
            //var qrBitmap = qrCode.(20);

            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeAsPngByteArray = qrCode.GetGraphic(20);

            //byte[] qrArray;
            //using (var stream = new MemoryStream())
            //{
            //    qrBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            //    qrArray = stream.ToArray();
            //}

            var qrImage = new Image
            {

                // Aspect = Aspect.AspectFill,
                WidthRequest = 200,
                HeightRequest = 200,
                MinimumHeightRequest = 200,
                MinimumWidthRequest = 200,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Source = ImageSource.FromStream(() => new MemoryStream(qrCodeAsPngByteArray))
            };


            var nameLabel = new Label { Text = Dictionary.Name + ":" };
            var name = new Editor { Text = App.Context.My.Contact.GetRealName() };
            App.Context.My.OnNameChanged += (newName) => name.Text = newName;
            App.Context.My.OnAvatarChanged += (imageData) => avatar.Source = ImageSource.FromStream(() => new MemoryStream(imageData));
            name.TextChanged += (sender, e) => App.Context.My.SetName(name.Text);
            var sharePublicKeyButton = new Button { Text = Dictionary.Share };
            sharePublicKeyButton.Command = new Command(() => Share.RequestAsync(publicKey.Text));
            var showPassphrase = new Button { Text = Dictionary.ShowPassphrase };
#if DEBUG
            showPassphrase.Clicked += (o, e) =>
            {
                if (!App.Context.Modality.HasFlag(Modality.RemoveUnusedContacts))
                {
                    App.Context.My.SetAvatar(Convert.FromBase64String("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"));
                }
                labelPublicKey.Focus();
            };
#endif


            var formattedString = new FormattedString();
            formattedString.Spans.Add(new Span { Text = Dict.WhatIsAnonymous, FontAttributes = FontAttributes.Bold | FontAttributes.Italic, FontSize = 16 });
            formattedString.Spans.Add(new Span { Text = " " + Dict.Anonymous + "\n", FontSize = 16 });
            formattedString.Spans.Add(new Span { Text = Dict.OurMissionIsToMaximizeAnonymityAndSecurity, FontSize = 16 });
            formattedString.Spans.Add(new Span { Text = " " + Dict.WeAreOpenSource + "\n\n", FontAttributes = FontAttributes.Bold | FontAttributes.Italic, FontSize = 16 });
            formattedString.Spans.Add(new Span { Text = Dict.SecureStorage + " ", FontAttributes = FontAttributes.Bold | FontAttributes.Italic, FontSize = 16 });
            formattedString.Spans.Add(new Span { Text = Dict.SecureStorageDescription, FontSize = 16 });
            var secureLabel = new Label()
            {
                FormattedText = formattedString,
            };
            formattedString = new FormattedString();
            formattedString.Spans.Add(new Span { Text = Dict.Crypto_Messenger + " ", FontAttributes = FontAttributes.Bold | FontAttributes.Italic, FontSize = 16 });
            formattedString.Spans.Add(new Span { Text = Dict.CryptoMessengerDescription, FontSize = 16 });
            var cryptoLabel = new Label()
            {
                FormattedText = formattedString,
            };

            var openSecure = new Button()
            {
                Text = Dict.OpenGithub,
                HorizontalOptions = LayoutOptions.Start,
                BackgroundColor = Color.Transparent,
                BorderColor = Color.Black,
                BorderWidth = 1,

            };
            openSecure.Clicked += async (sender, e) => await Browser.OpenAsync("https://github.com/Andrea-Bruno/SecureStorage", BrowserLaunchMode.SystemPreferred);

            var openCrypto = new Button()
            {
                Text = Dict.OpenGithub,
                HorizontalOptions = LayoutOptions.Start,
                BackgroundColor = Color.Transparent,
                BorderColor = Color.Black,
                BorderWidth = 1,
            };
            openCrypto.Clicked += async (sender, e) => await Browser.OpenAsync("https://github.com/Andrea-Bruno/EncryptedMessaging", BrowserLaunchMode.SystemPreferred);


            var suggestLabel = new Label()
            {
                Text = "\n" + Dict.SuggestedApplicationsForYourPrivacy + "\n",
                FontAttributes = FontAttributes.Bold | FontAttributes.Italic,
                FontSize = 18,
            };
            Grid suggestGrid = new Grid();
            suggestGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            foreach (Suggestion suggestion in suggestions)
            {
                suggestGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var suggestionLabel = new Label()
                {
                    Text = suggestion.LabelText,
                    FontAttributes = FontAttributes.Bold | FontAttributes.Italic,
                    FontSize = 16,
                };
                var openSuggestion = new Button()
                {
                    Text = suggestion.Text,
                    HorizontalOptions = LayoutOptions.Start,
                    BackgroundColor = Color.Transparent,
                    BorderColor = Color.Black,
                    BorderWidth = 1,
                };
                openSuggestion.Clicked += async (sender, e) => await Browser.OpenAsync(suggestion.Link, BrowserLaunchMode.SystemPreferred);
                suggestGrid.Children.Add(suggestionLabel, 0, suggestGrid.RowDefinitions.Count - 1);
                suggestGrid.Children.Add(openSuggestion, 1, suggestGrid.RowDefinitions.Count - 1);
            }

            showPassphrase.Command = new Command(async () => await DisplayAlert("Passphrase", App.Context.My.GetPassphrase(), Dictionary.Ok));
            ScrollView scrollView = new ScrollView
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                Content = new StackLayout { Margin = new Thickness(5), Orientation = StackOrientation.Vertical, Children = { avatar, upload, nameLabel, name, labelPublicKey, publicKey, qrImage, sharePublicKeyButton, showPassphrase, secureLabel, openSecure, cryptoLabel, openCrypto, suggestLabel, suggestGrid } },
            };
            _grid.Children.Add(scrollView, 0, 1);
            Content = _grid;
        }

        public static byte[] StreamToByteArray(Stream input)
        {
            MemoryStream ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }

        private readonly Grid _grid;
    }

    class Suggestion
    {
        public string LabelText;
        public string Text;
        public string Link;
    }

}
