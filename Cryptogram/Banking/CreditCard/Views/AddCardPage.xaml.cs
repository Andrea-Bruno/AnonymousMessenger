using Banking.Models;
using Banking.Views;
using Plugin.Toast;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Banking.Views
{
    public partial class AddCardPage : BasePage
    {
        public AddCardPage()
        {
            InitializeComponent();
            CardNumber.Focus();
            CreditCardRange.CreateDefaults(new List<CreditCardType> { CreditCardType.AmEx, CreditCardType.MasterCard, CreditCardType.Visa, CreditCardType.Maestro });
        }


        public void Back_Clicked(object _, EventArgs e) => OnBackButtonPressed();

        protected override bool OnBackButtonPressed()
        {
            Application.Current.MainPage.Navigation.PopAsync(false);
            return true;
        }

        private async void Confirm_Clicked(object _, EventArgs e)
        {
            var month = Convert.ToInt32(Month.Text);
            var year = Convert.ToInt32(Year.Text);
            var Cvv_Number = CVV.Text;
            var Card_number = CardNumber.Text;
            CreditCardType cardValidity = CreditCardRange.ValidateCardNumber(CardNumber.Text);
            if (cardValidity == CreditCardType.Unknown)
            {
                CardImage.IsVisible = false;
                CardError.Text = Localization.Resources.Dictionary.Unknown;
                return;
            }
            CardImage.IsVisible = true;
            if (cardValidity == CreditCardType.AmEx && !string.IsNullOrEmpty(CVV.Text) && Cvv_Number.Length == 3 && (month >= 1 && month <= 12) && (year >= 2020 && year <= 2060))
            {
                CardImage.Source = "ic_american";
                await Application.Current.MainPage.Navigation.PushAsync(new PaymentHistory(), false);
                return;
            }
            else if (cardValidity == CreditCardType.AmEx && ((month < 1 || month > 12) || (year < 2020 || year > 2060)))
            {
                CardImage.Source = "ic_american";
                CardError.Text = Localization.Resources.Dictionary.InvalidMonthAndYear;
                return;
            }
            else if (cardValidity == CreditCardType.AmEx && (string.IsNullOrEmpty(CVV.Text) || Cvv_Number.Length != 3))
            {
                CardImage.Source = "ic_american";
                CardError.Text = Localization.Resources.Dictionary.InvalidCVV;
                return;
            }
            if (cardValidity == CreditCardType.MasterCard && !string.IsNullOrEmpty(CVV.Text) && Cvv_Number.Length == 3 && (month >= 1 && month <= 12) && (year >= 2020 && year <= 2060))
            {
                CardImage.Source = "ic_master";
                await Application.Current.MainPage.Navigation.PushAsync(new PaymentHistory(), false);
                return;
            }
            else if (cardValidity == CreditCardType.MasterCard && ((month < 1 || month > 12) || (year < 2020 || year > 2060)))
            {
                CardImage.Source = "ic_master";
                CardError.Text = Localization.Resources.Dictionary.InvalidMonthAndYear;
                return;
            }
            else if (cardValidity == CreditCardType.MasterCard && (string.IsNullOrEmpty(CVV.Text) || Cvv_Number.Length != 3))
            {
                CardImage.Source = "ic_master";
                CardError.Text = Localization.Resources.Dictionary.InvalidCVV;
                return;
            }
            if (cardValidity == CreditCardType.Visa && !string.IsNullOrEmpty(CVV.Text) && Cvv_Number.Length == 3 && (month >= 1 && month <= 12) && (year >= 2020 && year <= 2060))
            {
                CardImage.Source = "ic_visa";
                await Application.Current.MainPage.Navigation.PushAsync(new PaymentHistory(), false);
            }
            else if (cardValidity == CreditCardType.Visa && ((month < 1 || month > 12) || (year < 2020 || year > 2060)))
            {
                CardImage.Source = "ic_visa";
                CardError.Text = Localization.Resources.Dictionary.InvalidMonthAndYear;
                return;
            }
            else if (cardValidity == CreditCardType.Visa && (string.IsNullOrEmpty(CVV.Text) || Cvv_Number.Length != 3))
            {
                CardImage.Source = "ic_visa";
                CardError.Text = Localization.Resources.Dictionary.InvalidCVV;
                return;
            }
            if (cardValidity == CreditCardType.Maestro && !string.IsNullOrEmpty(CVV.Text) && Cvv_Number.Length == 3 && (month >= 1 && month <= 12) && (year >= 2020 && year <= 2060))
            {
                CardImage.Source = "ic_maestro";
                await Application.Current.MainPage.Navigation.PushAsync(new PaymentHistory(), false);
            }
            else if (cardValidity == CreditCardType.Maestro && ((month < 1 || month > 12) || (year < 2020 || year > 2060)))
            {
                CardImage.Source = "ic_maestro";
                CardError.Text = Localization.Resources.Dictionary.InvalidMonthAndYear;
                return;
            }
            else if (cardValidity == CreditCardType.Maestro && (string.IsNullOrEmpty(CVV.Text) || Cvv_Number.Length != 3))
            {
                CardImage.Source = "ic_maestro";
                CardError.Text = Localization.Resources.Dictionary.InvalidCVV;
                return;
            }
        }
    }
}