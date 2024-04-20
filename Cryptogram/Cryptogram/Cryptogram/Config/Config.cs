using System.IO;
using System.Net;
using Xamarin.Forms;

namespace Anonymous.Config
{
	internal struct Connection
	{
#if DEBUG
		public static string NetworkName = "mainnet";
		//public static string EntryPoint = "http://172.21.128.1/";
		public static string EntryPoint = GetEntryPoint();
#else
		public static string NetworkName = "mainnet";
		//public static string EntryPoint = "http://90.191.43.19/"; // in tallinn
			public static string EntryPoint = GetEntryPoint();
#endif

		private static string GetEntryPoint()
		{
			string entryPoint = null;
			//#if DEBUG
				//	entryPoint = "http://90.191.43.19/"; // Aruba 2 GB ram server
			entryPoint = "http://88.196.134.76/"; // Aruba 2 GB ram server

			//#else
			//try
			//{
			//	var client = new WebClient();
			//	Stream stream = client.OpenRead("http://88.196.53.119/Anonymous/entrypoint.txt");
			//	var reader = new StreamReader(stream);
			//	entryPoint = reader.ReadToEnd();
			//}
			//catch (System.Exception) { }
			string host = null;
			try
			{
				host = new System.Uri(entryPoint).Host;
			}
			catch (System.Exception) { }
			if (host == null)
				//entryPoint = "http://46.22.209.146/"; //Server a Tallinn
				entryPoint = "http://88.196.53.119/Anonymous/"; // Computer in office
																//#endif
			return entryPoint;
		}
	}

	internal struct ChatUI
	{
		public static bool NewMessageOnTop = false; //Is true, The messages will be in chronological order from the most recent to the oldest, from top to bottom 
		public static bool MultipleChatModes = true;
	}
}
