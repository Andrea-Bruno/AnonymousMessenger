using System;
using System.Threading.Tasks;
using EncryptedMessaging;
using Xamarin.Forms;
using static EncryptedMessaging.MessageFormat;

namespace XamarinShared.ViewCreator.Views.Outgoing
{
    public partial class OutgoingBaseView : BaseView
    {
        

        public OutgoingBaseView(Message message, MessageReadStatus readStatus, Func<Contact, MessageType, Task> sendNotification )
            :base()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                InitializeComponent(); // Some bugs on xamarin forms load view
            }
            Message = message;
            PancakeView.Content = new MR.Gestures.StackLayout { Padding = 12, Spacing = 0 };
            MessageFrameContent = PancakeView.Content as MR.Gestures.StackLayout;
            MessageFrame = PancakeView;
            SelectionCheckBox = CheckBox;
            IsMyMessage = true;
            Init( readStatus, sendNotification);
            CheckBox.BindingContext = ChatPageSupport.GetContactViewItems(message.Contact);
            PancakeView.BindingContext = ChatPageSupport.GetContactViewItems(message.Contact);

            CheckBox.Color = Palette.CheckBoxColor;
            PancakeView.BackgroundColor = Palette.FrameBackgroundColor(true);
            DropShadow.Color = Palette.FrameShadowColor;
        }


    }
}
