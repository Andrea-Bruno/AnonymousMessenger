using System;
using Foundation;
using MoEngageXamarin.iOS;
using UIKit;
using UserNotifications;

namespace TelegraphServiceExtension
{
    [Register("NotificationService")]
    public class NotificationService : UNNotificationServiceExtension
    {
        Action<UNNotificationContent> ContentHandler { get; set; }
        UNMutableNotificationContent BestAttemptContent { get; set; }

        protected NotificationService(IntPtr handle) : base(handle)
        { }

        public override void DidReceiveNotificationRequest(UNNotificationRequest request, Action<UNNotificationContent> contentHandler)
        {
            ContentHandler = contentHandler;
            MORichNotification.SetAppGroupID("group.com.dtsocialize.uup.UupTest.MoEngage");
            MORichNotification.HandleRichNotificationRequest(request, contentHandler);
        }

        public override void TimeWillExpire()
        {
            ContentHandler(BestAttemptContent);
        }
    }
}
