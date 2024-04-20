using System;
using Rg.Plugins.Popup.Services;
using Utils;

namespace Anonymous.PopupViews
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
            PopupNavigation.Instance.PopAsync(false);
            BackButtonAction?.Invoke();
        }
    }
}
