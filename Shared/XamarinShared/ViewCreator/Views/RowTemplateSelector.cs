using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EncryptedMessaging;
using Microsoft.Maui.Controls;
using XamarinShared.ViewCreator.ViewHolder;
using XamarinShared.ViewCreator.Views.Incoming;

namespace XamarinShared.ViewCreator.Views
{
    public class RowTemplateSelector : DataTemplateSelector
    {
        private MessageReadStatus _readStatus;
        private Func<Contact, MessageFormat.MessageType, Task> _sendNotification;

        public RowTemplateSelector(MessageReadStatus readStatus, Func<Contact, MessageFormat.MessageType, Task> sendNotification)
        {
            _readStatus = readStatus;
            _sendNotification = sendNotification;
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is TextRow textRow)
            {
                return new DataTemplate(() =>
                {
                    var c = new TextIncomingView(_readStatus, _sendNotification);
                    c.Setup(textRow);
                    return c;
                });
            }

            throw new ArgumentException("Element not supported", nameof(item));
        }
    }
}