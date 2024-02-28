using System;
using Xamarin.Forms;

namespace TouchTracking
{
	public class TouchEffect : RoutingEffect
	{
		public event TouchActionEventHandler TouchAction;

		public TouchEffect() : base("XamarinDocs.TouchEffect")
		{
		}	

		public bool Capture { set; get; }

		public void OnTouchAction(Element element, TouchActionEventArgs args)
		{
			Console.WriteLine("Touch captured");
			TouchAction?.Invoke(element, args);
		}
	}
}
