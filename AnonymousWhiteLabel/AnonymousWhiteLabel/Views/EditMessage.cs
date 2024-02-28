using System;
using System.IO;
using AnonymousWhiteLabel.Pages;
using MessageCompose;
using Xamarin.Forms;

namespace AnonymousWhiteLabel.Views
{
    public class EditMessage : Grid
    {

        private Composer _composer;
        public Composer Composer { get => _composer; set => _composer = value; }

        public EditMessage(ScrollView MessageContainer)
        {
            //Grid _grid = new Grid();
            RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); //Message container
            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); //input text
                                                                                //if (_secureKeyboard)
                                                                                //	_grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); //secure keyboard
            ColumnDefinitions.Add(new ColumnDefinition());
            Children.Add(MessageContainer, 0, 0);
            //_sendButton.Clicked += (sender, e) => { onSendClick(EncryptedMessaging.MessageFormat.MessageType.Text, _input.Text); _input.Text = ""; };
            //_pickingPhotoButton.Clicked += async (sender, e) =>
            //{
            //    (sender as Button).IsEnabled = false;
            //    Stream stream = await DependencyService.Get<IPhotoPickerService>().GetImageStreamAsync().ConfigureAwait(false);
            //    if (stream != null)
            //    {
            //        byte[] data = null;
            //        using (var ms = new MemoryStream())
            //        {
            //            stream.CopyTo(ms);
            //            data = ms.ToArray();
            //        }
            //        stream.Dispose();
            //        var alert = new SendingAlert(data, onSendClick);
            //        Device.BeginInvokeOnMainThread(() =>
            //                                        //PushAsync outside MainThread don't work!
            //                                        _ = Navigation.PushAsync(alert));
            //    }
            //    Device.BeginInvokeOnMainThread(() => (sender as Button).IsEnabled = true);
            //};
            _composer = new Composer() { };

            //var input = new StackLayout  { Orientation = StackOrientation.Horizontal, Children = { _input, _sendButton } };
            //var buttons = new StackLayout { Orientation = StackOrientation.Horizontal, Children = { _pickingPhotoButton } };
            //View bottomBar = new StackLayout { Orientation = StackOrientation.Vertical, Children = { input, buttons } };
            var bottomBar = _composer;
#if audio
            var mp3FileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { ".mp3" } }, // or general UTType values
					{ DevicePlatform.Android, new[] { ".mp3" } },
                    { DevicePlatform.UWP, new[] { ".mp3" } },
                });
            var options = new PickOptions
            {
                PickerTitle = "Please select a audio file",
                FileTypes = mp3FileType,
            };

            _pickingAudioButton.Clicked += async (sender, e) =>
            {
                (sender as Button).IsEnabled = false;
                var result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    var stream = await result.OpenReadAsync();
                    byte[] data = null;
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        data = ms.ToArray();
                    }
                    stream.Dispose();
                    onSendClick(EncryptedMessaging.MessageFormat.MessageType.Audio, data);
                    //var alert = new SendingAlert(data, onSendClick);
                    //Device.BeginInvokeOnMainThread(() =>
                    //                                //PushAsync outside MainThread don't work!
                    //                                _ = Navigation.PushAsync(alert));
                    Device.BeginInvokeOnMainThread(() => (sender as Button).IsEnabled = true);
                }
            };
#endif


            Children.Add(bottomBar, 0, 1);

            //		Content = new StackLayout
            //		{
            //			Children = {
            //_grid
            //			}
            //		};
        }


        //public static readonly ScrollView MessageContainer = new ScrollView();
        //private readonly Button _pickingPhotoButton = new Button { Text = "🖼️", FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)), HorizontalOptions = LayoutOptions.Start };
        //private readonly Button _sendButton = new Button { Text = "➤", FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)), HorizontalOptions = LayoutOptions.End }; // Arrows Unicode: http://xahlee.info/comp/unicode_arrows.html
        //private readonly Editor _input = new Editor { AutoSize = EditorAutoSizeOption.TextChanges, HorizontalOptions = LayoutOptions.FillAndExpand, Placeholder = EncryptedMessaging.Resources.Dictionary.StrictlyConfidentialMessage };

    }
}