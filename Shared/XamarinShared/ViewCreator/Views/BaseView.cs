using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EncryptedMessaging;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using XamarinShared.CustomViews;
using XamarinShared.ViewCreator.Views;
using static EncryptedMessaging.MessageFormat;

namespace XamarinShared.ViewCreator
{
    public abstract class BaseView : ReplySwipeLayout
    {
        public MR.Gestures.StackLayout MessageFrameContent;
        public CustomCheckBox SelectionCheckBox;
        public PancakeView MessageFrame;
        public Message Message;
        public StackLayout FlagAndTime;
        public bool IsMyMessage = false;

        private static readonly List<MessageType> MessageTypeList = new List<MessageType>
        {
            MessageType.PdfDocument, MessageType.StartAudioGroupCall, MessageType.StartVideoGroupCall,
            MessageType.EndCall, MessageType.AudioCall, MessageType.VideoCall, MessageType.DeclinedCall
        };

        protected BaseView()
        {
        }

        protected void Init(MessageReadStatus readStatus, Func<Contact, MessageType, Task> _sendNotification)
        {
            var messageStatus = readStatus.AddReadStatusFlag(Message, IsMyMessage, _sendNotification, out FlagAndTime);
            foreach (var view in FlagAndTime.Children)
            {
                var label = (Label) view;
                label.SetFontSizeBinding(12);
                //label.BindingContext = creator;
                label.FontFamily = "PoppinsRegular";
                //label.SetBinding(Label.FontSizeProperty, new Binding(nameof(creator.TextSize), converter: fontSizeConverter, converterParameter: 12));
            }

            MessageFrameContent.SizeChanged += FlagAndTime_SizeChanged;

            if (!Message.Contact.IsGroup || IsMyMessage) return;
            var authorIndex = Message.Contact.Participants.FindIndex(a => a == Message.GetAuthor());
            var author = new Label
            {
                HorizontalTextAlignment = TextAlignment.Start,
                Text = Message.AuthorName(),
                TextColor = Color.FromHex(Utils.StringToColour(authorIndex)),
                Margin = new Thickness(4, 0, 0, 0),
                FontFamily = "PoppinsBold"
            };
            author.SetFontSizeBinding(12);
            
            //author.BindingContext = creator;
            //author.SetBinding(Label.FontSizeProperty, new Binding(nameof(creator.TextSize), converter: fontSizeConverter, converterParameter: 12));
            MessageFrameContent.Children.Add(author);
        }
        
        
        private void FlagAndTime_SizeChanged(object sender, EventArgs e)
        {
            
            var isReplyLayoutLoaded = false;
            if (Message.ReplyToPostId != null)
            {
                isReplyLayoutLoaded = MessageFrameContent.Children[0].GetTag().Equals("ReplyLayout");
            }
            
            

            //if (_message == null) return;
            if (MessageTypeList.Contains(Message.Type))
            {
                FlagAndTime.Margin = new Thickness(0, -FlagAndTime.Height, 0, 0);

                for (int i = isReplyLayoutLoaded ? 1 : 0; i < MessageFrameContent.Children.Count - 1; i++)
                {
                    View view = MessageFrameContent.Children[i];
                    view.Margin = new Thickness(0, 0, FlagAndTime.Width + 8, 0);
                }
            }
            else if (Message.Type == MessageType.Text)
            {
                CustomLinkLabel customLinkLabel=null;
                foreach (var child in MessageFrameContent.Children)
                {
                    if (!(child is CustomLinkLabel label)) continue;
                    customLinkLabel = label;
                    break;
                }

                if (customLinkLabel==null)
                    return;

                if (MessageFrameContent.Width - customLinkLabel.Width - 24 - FlagAndTime.Width - 5 > 0)
                {
                    customLinkLabel.Margin = new Thickness(0, 0, FlagAndTime.Width + 8, 0);
                    FlagAndTime.Margin = new Thickness(0, -FlagAndTime.Height, 0, 0);
                }
            }
            else if (Message.Type == MessageType.Image || Message.Type == MessageType.Location)
            {
                var imageRowPosition = Message.Contact.IsGroup && !IsMyMessage ? 1 : 0;

                imageRowPosition += isReplyLayoutLoaded ? 1 : 0;

                var imageView = MessageFrameContent.Children[imageRowPosition];

                if (imageView is Image)
                {
                    imageView.Margin = new Thickness(0, 0, 0, -FlagAndTime.Height);
                }
            }
        }

        public void InitTimeLabel()
        {
            MessageFrameContent.Children.Add(FlagAndTime);
        }
    }
}