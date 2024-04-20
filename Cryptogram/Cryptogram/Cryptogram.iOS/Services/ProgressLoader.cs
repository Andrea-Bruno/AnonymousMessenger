using System;
using CustomViewElements.Services;
using Anonymous.iOS.CustomViews;
using Anonymous.iOS.Services;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(ProgressLoader))]
namespace Anonymous.iOS.Services
{
	class ProgressLoader : IProgressInterface
	{
		private ProgressDialogOveralay loadPop;

		public ProgressLoader()
		{
			try
			{
				var bounds = UIScreen.MainScreen.Bounds;
				loadPop = new ProgressDialogOveralay(bounds); // using field from step 2
			}
			catch (Exception)
			{

			}
			
		}

		public void Hide()
		{
			try
			{
				loadPop.Hide();
			}
			catch (Exception)
			{

			}
		}

		public void Show(string title = "Loading")
		{
			try
			{
				AppDelegate.Instance?.Window?.Add(loadPop);
			}
			catch(Exception)
            {

            }
		}
	}
}