using System;
using Mopups.Services;
using Utils;

namespace Cryptogram.PopupViews
{
    public partial class SuccessPopup : CustomViewElements.BasePopupPage
    {
        public Action BackButtonAction;
        public SuccessPopup(bool isRecovered = false)
        {
            InitializeComponent();
            if (isRecovered)
                InfoLabel.Text = Localization.Resources.Dictionary.YourAccountWasSuccessFullyRecovered;
        }
     
        void CloseButton_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            MopupService.Instance.PopAsync(false);
            BackButtonAction?.Invoke();
        }
    }
}
