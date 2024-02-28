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
		public static string LicenseOEM = "3z66WQrrQnlksDQEcqt7qxABMVBgqexgH/PuY8EmIT4=";
		private static string GetEntryPoint()
		{
			string entryPoint = null;
#if DEBUG
			//entryPoint = "http://90.191.43.19/"; // Office line 2 for debug
			//entryPoint = "http://88.196.53.119/"; // Main			
			//entryPoint = "http://88.196.134.76/"; //Server a Tallinn
			//entryPoint = AnonymousChannel.Utility.GetLocalIPAddress(); // Intranet
			entryPoint = "http://test.cloudservices.agency";
#else
			try
			{
				var client = new WebClient();
				Stream stream = client.OpenRead("http://88.196.53.119/Anonymous/entrypoint.txt");
				var reader = new StreamReader(stream);
				entryPoint = reader.ReadToEnd();
			}
			catch (System.Exception) { }
			string host = null;
			try
			{
				host = new System.Uri(entryPoint).Host;
			}
			catch (System.Exception) { }
			if (host == null)
				entryPoint = "http://88.196.134.76/"; //Server a Tallinn
				//entryPoint = "http://88.196.53.119/Anonymous/"; // Computer in office
#endif
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


