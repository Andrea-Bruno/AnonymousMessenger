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
    [Register ("RoomViewController")]
    partial class RoomViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITapGestureRecognizer BackgroundDoubleTap { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITapGestureRecognizer BackgroundTap { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView ContainerView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel DebugData { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton EndCallButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint LocalVideoHeight { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint LocalVideoWidth { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView LocalView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView MutedView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel RoomNameLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton SwitchCamButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ToggleAudioButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ToggleCamButton { get; set; }

        [Action ("DoBackDoubleTapped:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void DoBackDoubleTapped (UIKit.UITapGestureRecognizer sender);

        [Action ("DoBackTapped:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void DoBackTapped (UIKit.UITapGestureRecognizer sender);

        [Action ("EndCallClicked:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void EndCallClicked (UIKit.UIButton sender);

        [Action ("SwitchCamClicked:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void SwitchCamClicked (UIKit.UIButton sender);

        [Action ("ToggleAudioButtonClicked:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void ToggleAudioButtonClicked (UIKit.UIButton sender);

        [Action ("ToggleCamClicked:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void ToggleCamClicked (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (BackgroundDoubleTap != null) {
                BackgroundDoubleTap.Dispose ();
                BackgroundDoubleTap = null;
            }

            if (BackgroundTap != null) {
                BackgroundTap.Dispose ();
                BackgroundTap = null;
            }

            if (ContainerView != null) {
                ContainerView.Dispose ();
                ContainerView = null;
            }

            if (DebugData != null) {
                DebugData.Dispose ();
                DebugData = null;
            }

            if (EndCallButton != null) {
                EndCallButton.Dispose ();
                EndCallButton = null;
            }

            if (LocalVideoHeight != null) {
                LocalVideoHeight.Dispose ();
                LocalVideoHeight = null;
            }

            if (LocalVideoWidth != null) {
                LocalVideoWidth.Dispose ();
                LocalVideoWidth = null;
            }

            if (LocalView != null) {
                LocalView.Dispose ();
                LocalView = null;
            }

            if (MutedView != null) {
                MutedView.Dispose ();
                MutedView = null;
            }

            if (RoomNameLabel != null) {
                RoomNameLabel.Dispose ();
                RoomNameLabel = null;
            }

            if (SwitchCamButton != null) {
                SwitchCamButton.Dispose ();
                SwitchCamButton = null;
            }

            if (ToggleAudioButton != null) {
                ToggleAudioButton.Dispose ();
                ToggleAudioButton = null;
            }

            if (ToggleCamButton != null) {
                ToggleCamButton.Dispose ();
                ToggleCamButton = null;
            }
        }
    }
}