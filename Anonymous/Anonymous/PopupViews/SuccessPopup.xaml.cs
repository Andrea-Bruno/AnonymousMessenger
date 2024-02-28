using System;
using Rg.Plugins.Popup.Services;

namespace Telegraph.PopupViews
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
            PopupNavigation.Instance.PopAsync(false);
            BackButtonAction?.Invoke();
        }
    }
}
