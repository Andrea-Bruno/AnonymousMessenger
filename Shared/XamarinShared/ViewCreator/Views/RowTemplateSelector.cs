using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EncryptedMessaging;
using Xamarin.Forms;
using XamarinShared.ViewCreator.ViewHolder;
using XamarinShared.ViewCreator.Views.Incoming;

namespace XamarinShared.ViewCreator.Views
{
    public class RowTemplateSelector : DataTemplateSelector
    {
        private MessageReadStatus _readStatus;
        private Func<Contact, MessageFormat.MessageType, Task> _sendNotification;
        private DataTemplate IncomingTextTemplate;
        private DataTemplate OutgoingTextTemplate;

        //public RowTemplateSelector(MessageReadStatus readStatus, Func<Contact, MessageFormat.MessageType, Task> sendNotification)
        //{
        //    _readStatus = readStatus;
        //    _sendNotification = sendNotification;
        //    IncomingTextTemplate = new DataTemplate(() =>
        //    {
        //        var c= new TextIncomingView(_readStatus,_sendNotification);
        //        c.Setup(item as TextRow);
        //        return c;
        //    });
        //}

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {          
            var test = new DataTemplate(() =>
            {
                var c= new TextIncomingView(_readStatus,_sendNotification);
                c.Setup(item as TextRow);
                return c;
             });
            
            return test;

            // if (!(item is BaseRowViewholder vh)) throw new ArgumentException("Cannot bind");
            //
            // switch (vh.RowType)
            // {
            //     case RowType.INCOMING_TEXT:
            //     case RowType.OUTGOING_TEXT:
            //     default:
            //         throw new ArgumentOutOfRangeException();
            // }
          
        }
    }
}