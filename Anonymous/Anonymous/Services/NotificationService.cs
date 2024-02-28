using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Telegraph.Models;
using Telegraph.Views;
using EncryptedMessaging;
using static EncryptedMessaging.MessageFormat;

namespace Telegraph.Services
{

	public class NotificationService
	{
		private static readonly string FCM_API = "https://fcm.googleapis.com";
		private static readonly string serverKey = "key=AAAAYXDykOk:APA91bG_GSApHmIdZc5IKy6zNgQdRXNnCQtslUkmDgPbH2JIO7mF3pBgxIuIzArXrt44VaC_jP9lnRqjODPeIXqv4rKDboWl7LqGs_DxVc_O8yZX0GRCz-ljFScjB9AThgQNBKGbLuyp";
		private static readonly string token = GetToken();

		private static string GetToken()
		{
			var id = Xamarin.Essentials.Preferences.Get("AppId", 0ul);
			return id == 0ul ? null : id.ToString(CultureInfo.InvariantCulture);
		}

		private static List<string> topics = new List<string>();

		public static void SendNotificationByMessageType(Contact contact, MessageType messageType)
		{
			if (contact == null)
				return;
			if (contact.IsGroup)
			{
				List<string> titleBody = GenerateGroupCreateNotificationBody(contact);
				SendNotification(titleBody, contact, null);
			}
			else
			{
				var topic = (messageType == MessageType.Contact && !string.IsNullOrEmpty(contact.FirebaseToken)) ? contact.FirebaseToken : "/topics/" + contact.ChatId + "";
				List<string> titleBody = GenerateNotificationBody(contact, messageType);
				SendNotification(titleBody, contact, topic);
			}
		}

		public static void SendCallNotification(Contact contact, CallTypes callType, bool _isVideoCall)
		{
			if (contact == null)
				return;
			List<string> titleBody = GenerateCallNotificationBody(contact, callType, _isVideoCall);
			SendCallNotification(contact, titleBody, callType);
		}


		// this is for cancel the call
		public static void SendCancelCallNotification(string topic, bool isGroupCall)
		{
			NotificationCall message = new NotificationCall("/topics/" + topic,
						 new NotificationCall.MessageData("Cancel call", "Cancel call", token, topic, "high", (int)CallTypes.MissingCall, isGroupCall), "high"); // in cancel call, title and body is not considered.
			var json = Newtonsoft.Json.JsonConvert.SerializeObject(message);
			_ = SendNotification(json, null);
		}


		private static async Task SendNotification(string json, string chatId)
		{
			try
			{
				var client = new HttpClient();
				client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", serverKey);
				client.BaseAddress = new Uri(FCM_API);
				var content = new StringContent(json, Encoding.UTF8, "application/json");
				HttpResponseMessage response = await client.PostAsync("/fcm/send", content);

				var result = await response.Content.ReadAsStringAsync();
				Console.WriteLine("-- >> Notification Response " + result);
				if (chatId != null && topics.Contains(chatId))
				{
					topics.Remove(chatId);
					NavigationTappedPage.Context.Storage.SaveObject(topics, "Topics");
				}
			}
			catch (Exception)
			{
				if (chatId != null && !topics.Contains(chatId))
				{
					topics.Add(chatId);
					NavigationTappedPage.Context.Storage.SaveObject(topics, "Topics");

				}
			}
		}

		private static string GenerateCallNotificationContent(Contact contact, string title, string body, CallTypes callType)
		{
			if (!contact.IsGroup)
			{
				string topic = "/topics/" + contact.ChatId;
				NotificationCall message = new NotificationCall(topic,
							new NotificationCall.MessageData(title, body, token, topic, "high", (int)callType, false), "high"); // in cancel call, title and body is not considered.
				return Newtonsoft.Json.JsonConvert.SerializeObject(message);
			}
			else
			{
				GroupNotificationCall message = new GroupNotificationCall(GetGroupParticipantsRegistrationIds(contact),
						new NotificationCall.MessageData(title, body, token, null, "high", (int)callType, true), "high"); // in cancel call, title and body is not considered.
				return Newtonsoft.Json.JsonConvert.SerializeObject(message);
			}
		}

		private static string GenerateNotificationContent(Contact contact, string title, string body, string topic)
		{
			string json = "";
			try
			{
				if (contact.Os == Contact.RuntimePlatform.iOS)
				{
					NotificationMessage message = new NotificationMessage(topic,
							new NotificationMessage.MessageData(title, body, token, contact.ChatId , "high"),
							new NotificationMessage.NotificationData(title, body, "high", "default", Convert.ToInt32(contact.RemoteUnreaded)), "high");

					json = Newtonsoft.Json.JsonConvert.SerializeObject(message);
				}
				else
				{
					AndroidNotificationMessage message = new AndroidNotificationMessage(topic,
							new AndroidNotificationMessage.MessageData(title, body, token, contact.ChatId, "high"), "high");

					json = Newtonsoft.Json.JsonConvert.SerializeObject(message);
				}
			}
			catch (Exception)
			{
			}
			return json;
		}


		private static void SendNotification(List<string> titleBody, Contact contact, string token)
		{
			var title = titleBody[0];
			var body = titleBody[1];
			string notificationContent = GenerateNotificationContent(contact, title, body, token);
			if (token == null && contact.IsGroup)
				notificationContent = GenerateGroupCreateNotificationContent(contact, title, body);
			_ = SendNotification(notificationContent, contact.ChatId + "");
		}

		private static void SendCallNotification(Contact contact, List<string> titleBody, CallTypes callType)
		{
			var title = titleBody[0];
			var body = titleBody[1];
			string notificationContent;
			if (callType == CallTypes.AudioCall || callType == CallTypes.VideoCall)
				notificationContent = GenerateCallNotificationContent(contact, title, body, callType);
			else
				notificationContent = GenerateNotificationContent(contact, title, body, "/topics/" + contact.ChatId + "");
			_ = SendNotification(notificationContent, contact.ChatId + "");
		}

		private static List<string> GenerateNotificationBody(Contact contact, MessageType messageType)
		{
			List<string> titleBody = new List<string>();
			var customCulture = new CultureInfo("en-US");
			if (contact.Language != null && contact.Language == "it")
				customCulture = new CultureInfo("it-IT");
			string myName = contact.MyRemoteName ?? NavigationTappedPage.Context.My.Name;
			string body;
			switch (messageType)
			{
				case MessageType.Contact:
					body = myName + Localization.Resources.Dictionary.ResourceManager.GetString("SomeoneAddedYourContact", customCulture);
					break;
				case MessageType.Text:
					body = Localization.Resources.Dictionary.ResourceManager.GetString("YouHaveReceivedNewMessageFrom", customCulture) + myName;
					break;
				case MessageType.Image:
					body = Localization.Resources.Dictionary.ResourceManager.GetString("YouHaveReceivedNewImageFrom", customCulture) + myName;
					break;
				case MessageType.Audio:
					body = Localization.Resources.Dictionary.ResourceManager.GetString("YouHaveReceivedNewAudioFrom", customCulture) + myName;
					break;
				default:
					body = Localization.Resources.Dictionary.ResourceManager.GetString("YouHaveReceivedNewNotificationFrom", customCulture) + myName;
					break;

			}
			if (!contact.IsGroup)
				titleBody.Add(Localization.Resources.Dictionary.ResourceManager.GetString("NewNotification", customCulture));
			else
				titleBody.Add(Localization.Resources.Dictionary.ResourceManager.GetString("NewNotification", customCulture) + " @ " + contact.Name);
			titleBody.Add(body);
			return titleBody;
		}

		private static List<string> GenerateCallNotificationBody(Contact contact, CallTypes callType, bool _isVideoCall = false)
		{
			List<string> titleBody = new List<string>();
			var customCulture = new CultureInfo("en-US");
			if (contact.Language != null && contact.Language == "it")
				customCulture = new CultureInfo("it-IT");
			string name = contact.MyRemoteName ?? NavigationTappedPage.Context.My.Name;
			string body = "", title = "";
			switch (callType)
			{
				case CallTypes.MissingCall:

					title = _isVideoCall ?
							Localization.Resources.Dictionary.ResourceManager.GetString("MissedVideoCall", customCulture) :
							Localization.Resources.Dictionary.ResourceManager.GetString("MissedAudioCall", customCulture);

					body = contact.IsGroup ?
									_isVideoCall ?
									Localization.Resources.Dictionary.ResourceManager.GetString("MissedVideoCallFromGroup", customCulture) + " " + contact.Name :
									Localization.Resources.Dictionary.ResourceManager.GetString("MissedAudioCallFromGroup", customCulture) + " " + contact.Name
							:
							 _isVideoCall ?
									Localization.Resources.Dictionary.ResourceManager.GetString("MissedVideoCallFrom", customCulture) + name :
									Localization.Resources.Dictionary.ResourceManager.GetString("MissedAudioCallFrom", customCulture) + name;
					break;
				case CallTypes.AudioCall:
					//title = Localization.Resources.Dictionary.ResourceManager.GetString("NewAudioCall", customCulture);
					title = "New audio call";
					body = !contact.IsGroup ?
							"You receive call from " + name :
							"You receive call from group " + contact.Name;
					//Localization.Resources.Dictionary.ResourceManager.GetString("YouReceiveCallFrom", customCulture) + name :
					//Localization.Resources.Dictionary.ResourceManager.GetString("YouReceiveCallFromGroup", customCulture) + " " + contact.Name;
					break;
				case CallTypes.VideoCall:
					title = "New video call";
					body = !contact.IsGroup ?
							"You receive call from " + name :
							"You receive call from group " + contact.Name;
					//title = Localization.Resources.Dictionary.ResourceManager.GetString("NewVideoCall", customCulture);
					//body = !contact.IsGroup ?
					//    Localization.Resources.Dictionary.ResourceManager.GetString("YouReceiveCallFrom", customCulture) + name :
					//    Localization.Resources.Dictionary.ResourceManager.GetString("YouReceiveCallFromGroup", customCulture) + " " + contact.Name;
					break;
			}
			titleBody.Add(title);
			titleBody.Add(body);
			return titleBody;
		}

		private static List<string> GenerateGroupCreateNotificationBody(Contact contact)
		{
			List<string> titleBody = new List<string>();
			var customCulture = new CultureInfo("en-US");
			if (contact.Language != null && contact.Language == "it")
				customCulture = new CultureInfo("it-IT");
			titleBody.Add(Localization.Resources.Dictionary.ResourceManager.GetString("NewGroupCreated", customCulture));
			titleBody.Add(Localization.Resources.Dictionary.ResourceManager.GetString("YouHaveBeenAddedToTheGroup", customCulture) + " " + contact.Name);
			return titleBody;
		}

		private static string GenerateGroupCreateNotificationContent(Contact contact, string title, string body)
		{
			string json = "";
			GroupNotificationMessage message = new GroupNotificationMessage(GetGroupParticipantsRegistrationIds(contact),
					new NotificationMessage.MessageData(title, body, token, contact.ChatId, "high"),
					new NotificationMessage.NotificationData(title, body, "high", "default", Convert.ToInt32(contact.RemoteUnreaded)), "high");

			json = Newtonsoft.Json.JsonConvert.SerializeObject(message);
			return json;
		}


		private static List<string> GetGroupParticipantsRegistrationIds(Contact contact)
		{

			List<string> registration_ids = new List<string>();
			foreach (var key in contact.Participants)
			{
				if (key != null && Convert.ToBase64String(key) != NavigationTappedPage.Context.My.GetPublicKey())
				{
					Contact participant = NavigationTappedPage.Context.Contacts.GetParticipant(key);
					if (participant != null && !string.IsNullOrWhiteSpace(participant.FirebaseToken))
						registration_ids.Add(participant.FirebaseToken);
				}
			}
			return registration_ids;
		}

		public static void SendUnsendedNotification()
		{
			foreach (string topic in topics)
				SendNotificationByMessageType(NavigationTappedPage.Context.Contacts.GetContact(Convert.ToUInt64(topic)), MessageType.Data);
		}

		public static void LoadUnsendedNotifications()
		{
			var unsendedTopics = (List<string>)NavigationTappedPage.Context.Storage.LoadObject(typeof(List<string>), "Topics");
			topics = unsendedTopics ?? new List<string>();
		}

	}
}