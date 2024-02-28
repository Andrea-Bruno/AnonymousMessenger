using EncryptedMessaging;
using EncryptedMessaging.Resources;
using System;
using System.Linq;
using System.IO;
using Xamarin.Forms;


namespace AnonymousWhiteLabel.Pages
{
    internal class ContactPage : ContentPage
    {
        public static TapGestureRecognizer tapQRCode = new TapGestureRecognizer();
        readonly Grid qrGrid = new Grid
        {
            RowSpacing = 0,
            ColumnSpacing = 0,
            RowDefinitions = { new RowDefinition { Height = GridLength.Auto }, },
            //ColumnDefinitions = { new ColumnDefinition{ Width = new GridLength(50) },},
        };
        readonly Editor qrEditor;
        readonly Button scanQr;
        string newParticipant;
        public ContactPage(Contact contact = null)
        {
            Title = Dictionary.Contact;
            var avatar = new Image();
            var labelPubKey = new Label { Text = Dictionary.PublicKey + ":" };
            var labelName = new Label { Text = Dictionary.Name + ":" };
            var name = new Entry { Text = contact?.Name, IsSpellCheckEnabled = false };
            name.IsEnabled = !string.IsNullOrEmpty(name.Text);
            var labelChatId = new Label { Text = "ChatId = " + contact?.ChatId.ToString() };
            var labelUserId = new Label { Text = "UserId = " + contact?.UserId.ToString() };

            if (contact == null)
            {
                qrEditor = new Editor { Text = contact?.GetQrCode() };
                qrEditor.TextChanged += (o, e) =>
                {
                    try
                    {
                        var contactMessage = new ContactMessage(qrEditor.Text);
                        var participants = contactMessage.GetParticipantsKeys(App.Context);
                        name.Text = string.IsNullOrEmpty(qrEditor.Text) ? "" : App.Context.Contacts.Pseudonym(participants);
                        name.IsEnabled = !string.IsNullOrEmpty(name.Text);
                        newParticipant = qrEditor.Text;
                    }
                    catch (Exception) { }
                };
                scanQr = new Button { Text = Dictionary.ScanQRCode };
                scanQr.Clicked += async (sender, e) =>
                {
                    var scanner = new ZXing.Mobile.MobileBarcodeScanner();
                    var options = new ZXing.Mobile.MobileBarcodeScanningOptions
                    {
                        PossibleFormats = new[] { ZXing.BarcodeFormat.QR_CODE }.ToList()
                    };
                    var result = await scanner.Scan(options);
                    if (result != null)
                        qrEditor.Text = result.Text;
                };
            }
            else
            {
                if (contact.Avatar != null)
                    avatar = new Image()
                    {
                        Source = ImageSource.FromStream(() => new MemoryStream(contact.Avatar)),
                        WidthRequest = 200,
                        HeightRequest = 200,
                        MinimumHeightRequest = 200,
                        MinimumWidthRequest = 200,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                    };

                var qrCode = new Label { Text = contact?.GetQrCode() };
                qrGrid.Children.Add(qrCode);
                qrGrid.GestureRecognizers.Add(tapQRCode);

                tapQRCode.Tapped += async (object sender, EventArgs e) =>
                {
                    if (Device.RuntimePlatform == Device.iOS || Device.RuntimePlatform == Device.Android)
                        await Xamarin.Essentials.Share.RequestAsync(((sender as Grid).Children[0] as Label).Text);
                    else
                        await Xamarin.Essentials.Clipboard.SetTextAsync(((sender as Grid).Children[0] as Label).Text);

                    //var x = Xamarin.Essentials.Clipboard.GetTextAsync();
                };
            }

            //qrCode.IsReadOnly = true;

            var ok = new Button { Text = Dictionary.Ok };
            ok.Clicked += (o, e) =>
            {
                if (contact != null)
                {
                    contact.Name = name.Text;
                    ContactsPage.ChatPage.SetCurrentContact(contact);
                }
                else
                {
                    try
                    {
                        var contactMessage = new ContactMessage(newParticipant);
                        var newContact = App.Context.Contacts.AddContact(contactMessage);
                        if (newContact == null)
                            throw new Exception(Dictionary.InvalidKey);
                        if (!string.IsNullOrEmpty(name.Text))
                        {
                            newContact.Name = name.Text;
                        }
                    }
                    catch (Exception)
                    {
                        DisplayAlert(Dictionary.Alert, Dictionary.InvalidKey, Dictionary.Ok);
                        return;
                    }
                }
                Navigation.PopAsync();
            };
            var delete = new Button { Text = Dictionary.Delete, IsVisible = contact != null };
            delete.Clicked += (o, e) =>
            {
                App.Context.Contacts.RemoveContact(contact);
                ContactsPage.ChatPage.SetCurrentContact(null);
                Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);    //deleting contact page
                Navigation.PopAsync();
            };
            var cancel = new Button { Text = Dictionary.Cancel };
            cancel.Clicked += (o, e) => Navigation.PopAsync();

            StackLayout stack;
            if (contact == null)
                stack = new StackLayout { Orientation = StackOrientation.Vertical, Children = { labelPubKey, qrEditor, scanQr, labelName, name, ok, cancel } };
            else
                stack = new StackLayout { Orientation = StackOrientation.Vertical, Children = { avatar, labelPubKey, qrGrid, labelName, name, labelChatId, labelUserId, ok, delete, cancel } };
            Content = stack;
        }
    }
}
