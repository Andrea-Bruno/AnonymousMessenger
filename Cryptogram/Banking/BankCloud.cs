using System;
using System.Collections.Generic;
using System.Text;

namespace Banking
{
	public static	class BankCloud
	{
		internal static EncryptedMessaging.Context Context;
		internal static EncryptedMessaging.Contact CloudContact;
		public	static void Initialize(EncryptedMessaging.Context context, string serverPublicKey = "A1JNVNQxFZf6lVpo2R+35qElmaJwVBAk/1o+Vz3y8WPv")
		{
			Context = context;

			CloudContact = Context.Contacts.AddContact(serverPublicKey, "BankCloud", true, EncryptedMessaging.Contacts.SendMyContact.None);

			System.Diagnostics.Debug.WriteLine("Cloud user ID " + CloudContact.UserId);
			System.Diagnostics.Debug.WriteLine("Cloud PubKet " + serverPublicKey);

			context.OnMessageBinaryCome.Add(CloudContact.ChatId, Communication.ProcessIncomingMessage); // Set the executor for incoming binary messages for this plug-in (Messages received from the cloud will be processed by this routine)
		}

	}
}
