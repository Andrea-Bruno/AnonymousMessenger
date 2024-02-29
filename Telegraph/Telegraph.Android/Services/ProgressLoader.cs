using System;
using AndroidHUD;
using CustomViewElements.Services;
using Telegraph.Droid.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(ProgressLoader))]
namespace Telegraph.Droid.Services
{
	public class ProgressLoader : IProgressInterface
	{
		public ProgressLoader()
		{
		}

		public void Hide()
		{
			try
			{
				AndHUD.Shared.Dismiss();
			}
			catch (Exception)
            {
			}
		}

		public void Show(string title = "Loading")
		{
			title = Localization.Resources.Dictionary.Loading;
			try
			{
				AndHUD.Shared.Show(Forms.Context, status: title, maskType: MaskType.Clear);
			}catch (Exception e)
            {
				Console.WriteLine("---------->>  " + e.Message);
            }
		}
	}
}