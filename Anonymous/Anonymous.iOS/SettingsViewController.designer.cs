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
    [Register ("SettingsViewController")]
    partial class SettingsViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel MaxBitrateValue { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel MaxFrameRateValue { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ProfileNameValue { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIPickerView ProfilePicker { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ResolutionValue { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISwitch UseSettingsSwitch { get; set; }

        [Action ("UseSettingsChanged:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void UseSettingsChanged (UIKit.UISwitch sender);

        void ReleaseDesignerOutlets ()
        {
            if (MaxBitrateValue != null) {
                MaxBitrateValue.Dispose ();
                MaxBitrateValue = null;
            }

            if (MaxFrameRateValue != null) {
                MaxFrameRateValue.Dispose ();
                MaxFrameRateValue = null;
            }

            if (ProfileNameValue != null) {
                ProfileNameValue.Dispose ();
                ProfileNameValue = null;
            }

            if (ProfilePicker != null) {
                ProfilePicker.Dispose ();
                ProfilePicker = null;
            }

            if (ResolutionValue != null) {
                ResolutionValue.Dispose ();
                ResolutionValue = null;
            }

            if (UseSettingsSwitch != null) {
                UseSettingsSwitch.Dispose ();
                UseSettingsSwitch = null;
            }
        }
    }
}