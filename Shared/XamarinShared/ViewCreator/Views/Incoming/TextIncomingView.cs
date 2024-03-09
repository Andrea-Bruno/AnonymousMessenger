using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EncryptedMessaging;
using Xamarin.Forms;
using XamarinShared.ViewCreator.ViewHolder;

namespace XamarinShared.ViewCreator.Views.Incoming
{
    public class TextIncomingView : ViewCell
    {
        private MessageReadStatus readStatus;
        private Func<Contact, MessageFormat.MessageType, Task> sendNotification;
        
        

        public TextIncomingView( MessageReadStatus readStatus,
            Func<Contact, MessageFormat.MessageType, Task> sendNotification)
        {
            this.readStatus = readStatus;
            this.sendNotification = sendNotification;
        }


        public void Setup(TextRow textRow)
        {
            var message = textRow.message;
            var baseView = new IncomingBaseView(message, readStatus, sendNotification);
            var box = baseView.MessageFrameContent;
            var isMyMessage = false;
            var contact = message.Contact;
            var textMessage = Encoding.Unicode.GetString(message.GetData());

            /// Defining message label
            var label = new CustomLinkLabel
            {
                FontFamily = "PoppinsSemiBold",
                TextColor = Palette.MainTextColor(isOutgoing: isMyMessage),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalTextAlignment = TextAlignment.Start
            };
            label.SetFontSizeBinding(14);
            /// it is for multi selection
            // textLabel = isMyMessage ? null : label;
            label.SetTag("original");


            /// -----------------
            /// Generating Labels
            /// -----------------

            var displayText = textMessage;
            /// Generate show original label
            var showOriginalLabel = GenerateLabel("Show Original");
            showOriginalLabel.Click((sender, eventArgs) =>
            {
                var originalText = textMessage;
                box.Children.Remove(showOriginalLabel);

                var index = box?.Children.IndexOf(label);
                var showFull = ((Label) box?.Children[index.Value + 1]).GetTag().Equals("read_less");

                label.SetTag("original");
                label.Text = !showFull ? displayLessMessage() : displayFullMessage();
            });

            /// Generate Read More/Less Labels
            string displayLessMessage()
            {
                displayText = label.GetTag().Equals("original")
                    ? textMessage.Substring(0, 700) + " . . ."
                    : message.Translation.Substring(0, 700) + " . . .";
                return displayText;
            }

            string displayFullMessage()
            {
                displayText = label.GetTag().Equals("original")
                    ? textMessage
                    : message.Translation;
                return displayText;
            }

            var readLessLabel = GenerateLabel(Localization.Resources.Dictionary.ReadLess, "read_less");
            var readMoreLabel = GenerateLabel(Localization.Resources.Dictionary.ReadMore, "read_more");
            readLessLabel.Click((sender, eventArgs) =>
            {
                label.Text = displayLessMessage();
                box.Children.Insert(box.Children.IndexOf(readLessLabel), readMoreLabel);
                box.Children.Remove(readLessLabel);
            });
            readMoreLabel.Click((sender, eventArgs) =>
            {
                label.Text = displayFullMessage();
                box.Children.Insert(box.Children.IndexOf(readMoreLabel), readLessLabel);
                box.Children.Remove(readMoreLabel);
            });

            /// -------------------
            /// Translating Message
            /// -------------------

            if (!isMyMessage && contact.TranslationOfMessages)
            {
                var translation = message.Translation ?? "";

                if (!string.IsNullOrWhiteSpace(translation))
                {
                    displayText = translation;
                    label.SetTag("translated");
                    box.Children.Add(showOriginalLabel);
                }
                else
                {
                    // _translateService?.Invoke(message, () => { UpdateDisplayedText(message, label); });
                }
            }

            box.Children.Add(label);

            if (displayText.Length > 700)
            {
                displayLessMessage();
                box.Children.Add(readMoreLabel);
            }
            else if (displayText.Length < 3)
            {
                var containsDigitOrLetter = displayText.ToCharArray().Any(char.IsLetterOrDigit);
                if (!containsDigitOrLetter)
                    label.FontSize = 50;
            }

            label.Text = displayText;
        }

        private static Label GenerateLabel(string labelText, string tag = "")
        {
            var label = new Label
            {
                Text = labelText,
                FontFamily = "PoppinsSemiBold",
                TextColor = Color.DeepSkyBlue,
                TextDecorations = TextDecorations.Underline,
                HorizontalTextAlignment = TextAlignment.End
            };
            if (!string.IsNullOrWhiteSpace(tag))
            {
                label.SetTag(tag);
            }

            label.SetFontSizeBinding(14);
            return label;
        }
    }
}