using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SubAppInterface
{
	internal interface Interface
	{
		string SubAppName { get; }
		NavigationPage[] NavigationPages { get; }
		ImageButton[] Buttons { get; }
		ToolbarItem[] ToolbarItem { get; }
	}
}
