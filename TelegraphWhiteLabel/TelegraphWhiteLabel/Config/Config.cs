using System.IO;
using System.Net;
using Xamarin.Forms;

namespace AnonymousWhiteLabel.Config
{
	internal struct Connection
	{

#if DEBUG
		public static string NetworkName = "mainnet";
		public static string EntryPoint = GetEntryPoint();
#else
		public static string NetworkName = "mainnet";
		public static string EntryPoint = GetEntryPoint();
#endif

		private static string GetEntryPoint()
		{
			string entryPoint = null;
			entryPoint = "test.cloudservices.agency"; //Main Server
			return entryPoint;
		}


	}

	internal struct ChatUI
	{
		public static bool NewMessageOnTop = false; //Is true, The messages will be in chronological order from the most recent to the oldest, from top to bottom 
		public static bool MultipleChatModes = true;
	}

	//internal struct Template
	//{
	//	public static Color BackgroundMessage = Color.FromRgb(0xb7, 0xcb, 0xf2);
	//	public static Color BackgroundMyMessage = Color.FromRgb(0xe2, 0xe8, 0xf3);
	//}
}


