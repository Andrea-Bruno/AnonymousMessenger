using System;
using System.Threading.Tasks;
using EncryptedMessaging;
using Microsoft.Maui.Controls;
using static EncryptedMessaging.MessageFormat;
using Microsoft.Maui.Graphics;

namespace XamarinShared.ViewCreator.Views.Incoming
{
    public partial class IncomingBaseView : BaseView
    {
        public IncomingBaseView(Message message, MessageReadStatus readStatus, Func<Contact, MessageType, Task> sendNotification)
            :base()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception)
            {
                InitializeComponent(); // Some bugs on xamarin forms load view
            }
            Message = message;
            PancakeView.Content = new StackLayout { Padding = 12, Spacing = 0 };
            MessageFrameContent = PancakeView.Content as StackLayout;
            MessageFrame = PancakeView;
            SelectionCheckBox = CheckBox;
            Init(readStatus, sendNotification);
            CheckBox.BindingContext = ChatPageSupport.GetContactViewItems(message.Contact);
            PancakeView.BindingContext = ChatPageSupport.GetContactViewItems(message.Contact);
            CheckBox.Color = Palette.CheckBoxColor;
            PancakeView.BackgroundColor = Palette.FrameBackgroundColor(false);
            DropShadow.Brush = new SolidColorBrush(Palette.FrameShadowColor);
        }
    }
}
