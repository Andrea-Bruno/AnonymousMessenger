using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Banking.Views
{
	public partial class PaymentsPage : BasePage
	{
		public PaymentsPage() => InitializeComponent();

		private void Back_Clicked(object sender, EventArgs e) => OnBackButtonPressed();

		protected override bool OnBackButtonPressed()
		{
			Application.Current.MainPage.Navigation.PopAsync(false);
			return true;
		}
	}
}