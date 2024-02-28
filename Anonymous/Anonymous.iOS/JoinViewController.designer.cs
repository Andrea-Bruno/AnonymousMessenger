// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace Telegraph.iOS
{
    [Register ("JoinViewController")]
    partial class JoinViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel AgoraVersionLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField ChannelNameEdit { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ConnectingLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView ConnectionImage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField EncryptionKeyEdit { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton JoinButton { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (AgoraVersionLabel != null) {
                AgoraVersionLabel.Dispose ();
                AgoraVersionLabel = null;
            }

            if (ChannelNameEdit != null) {
                ChannelNameEdit.Dispose ();
                ChannelNameEdit = null;
            }

            if (ConnectingLabel != null) {
                ConnectingLabel.Dispose ();
                ConnectingLabel = null;
            }

            if (ConnectionImage != null) {
                ConnectionImage.Dispose ();
                ConnectionImage = null;
            }

            if (EncryptionKeyEdit != null) {
                EncryptionKeyEdit.Dispose ();
                EncryptionKeyEdit = null;
            }

            if (JoinButton != null) {
                JoinButton.Dispose ();
                JoinButton = null;
            }
        }
    }
}