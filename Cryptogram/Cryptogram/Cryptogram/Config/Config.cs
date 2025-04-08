using System.IO;
using System.Net;
using Xamarin.Forms;

namespace Cryptogram.Config
{
	internal struct Connection
	{
		public static string NetworkName = "mainnet";
		public static string EntryPoint = "http://test.tc0.it";
	}

	internal struct ChatUI
	{
		public static bool NewMessageOnTop = false; //Is true, The messages will be in chronological order from the most recent to the oldest, from top to bottom 
		public static bool MultipleChatModes = true;
	}
}
