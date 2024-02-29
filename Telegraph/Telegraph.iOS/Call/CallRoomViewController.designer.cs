// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Telegraph.iOS.Call
{
	[Register ("CallRoomViewController")]
	partial class CallRoomViewController
	{
		[Outlet]
		UIKit.UIImageView BottomBar { get; set; }

		[Outlet]
		UIKit.UIButton CameraStatusBtn { get; set; }

		[Outlet]
		UIKit.UIButton CameraSwitchBtn { get; set; }

		[Outlet]
		UIKit.UIView ContainerView { get; set; }

		[Outlet]
		UIKit.UILabel Duration { get; set; }

		[Outlet]
		UIKit.UIButton EndCallBtn { get; set; }

		[Outlet]
		UIKit.UIView LocalView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint LocalViewWidthConstraint { get; set; }

		[Outlet]
		UIKit.UIButton MicrophoneBtn { get; set; }

		[Outlet]
		UIKit.UIImageView ProfilePicture { get; set; }

		[Outlet]
		UIKit.UIButton SpeakerBtn { get; set; }

		[Outlet]
		UIKit.UILabel Username { get; set; }

		[Action ("CameraStatusBtn_Clicked:")]
		partial void CameraStatusBtn_Clicked (UIKit.UIButton sender);

		[Action ("CameraSwitchBtn_Clicked:")]
		partial void CameraSwitchBtn_Clicked (UIKit.UIButton sender);

		[Action ("EndCallBtn_Clicked:")]
		partial void EndCallBtn_Clicked (UIKit.UIButton sender);

		[Action ("MicrophoneBtn_Clicked:")]
		partial void MicrophoneBtn_Clicked (UIKit.UIButton sender);

		[Action ("SpeakerBtn_Clicked:")]
		partial void SpeakerBtn_Clicked (UIKit.UIButton sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (CameraStatusBtn != null) {
				CameraStatusBtn.Dispose ();
				CameraStatusBtn = null;
			}

			if (BottomBar != null) {
				BottomBar.Dispose ();
				BottomBar = null;
			}

			if (LocalViewWidthConstraint != null) {
				LocalViewWidthConstraint.Dispose ();
				LocalViewWidthConstraint = null;
			}

			if (CameraSwitchBtn != null) {
				CameraSwitchBtn.Dispose ();
				CameraSwitchBtn = null;
			}

			if (ContainerView != null) {
				ContainerView.Dispose ();
				ContainerView = null;
			}

			if (Duration != null) {
				Duration.Dispose ();
				Duration = null;
			}

			if (EndCallBtn != null) {
				EndCallBtn.Dispose ();
				EndCallBtn = null;
			}

			if (LocalView != null) {
				LocalView.Dispose ();
				LocalView = null;
			}

			if (MicrophoneBtn != null) {
				MicrophoneBtn.Dispose ();
				MicrophoneBtn = null;
			}

			if (ProfilePicture != null) {
				ProfilePicture.Dispose ();
				ProfilePicture = null;
			}

			if (SpeakerBtn != null) {
				SpeakerBtn.Dispose ();
				SpeakerBtn = null;
			}

			if (Username != null) {
				Username.Dispose ();
				Username = null;
			}
		}
	}
}
