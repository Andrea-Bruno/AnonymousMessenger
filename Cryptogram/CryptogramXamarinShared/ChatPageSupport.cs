using Xamarin.Forms;
using EncryptedMessaging;
using static EncryptedMessaging.MessageFormat;
using System.Collections.Generic;
using System;
using System.Text;

namespace TelegraphXamarinShared
{
	public static class ChatPageSupport
	{
		/// <summary>
		/// Initialize
		/// </summary>
		/// <param name="multipleChatModes"></param>
		/// <param name="newMessageOnTop"></param>
		/// <param name="messageScrollView"></param>
		/// <param name="onNewMessageAddedToView"></param>
		internal static void Initialize(bool multipleChatModes, bool newMessageOnTop, ScrollView messageScrollView, OnNewMessageAddedToView onNewMessageAddedToView)
		{
			Config.ChatUI.MultipleChatModes = multipleChatModes;
			Config.ChatUI.NewMessageOnTop = newMessageOnTop;
			_scrollMessageContainer = messageScrollView;
			_scrollMessageContainer.VerticalScrollBarVisibility = ScrollBarVisibility.Never;
			_scrollMessageContainer.VerticalOptions = LayoutOptions.FillAndExpand;
			_onNewMessageAddedToView = onNewMessageAddedToView;
		}

		public static void Hibernate() => Setup.Context.Contacts.HibernatedContactsUI();

		private struct Config
		{
			public struct ChatUI
			{
				public static bool MultipleChatModes;
				public static bool NewMessageOnTop;
			}
		}

		private static StackLayout CurrentMessageContainer
		{
			get => _scrollMessageContainer.Content as StackLayout;
			set => _scrollMessageContainer.Content = value;
		}

		public static void SetCurrentContact(Contact contact)
		{
			CurrentMessageContainer = GetMessageContainerOfContact(contact) ?? InstantiateNewMessageContainer();
			//CurrentContact = contact; //Don't moving from this position
			//_scrollMessageContainer.Content = CurrentMessageContainer;
			Setup.Context.Messaging.CurrentChatRoom = contact;
		}

		public static Contact CurrentContact ()=> Setup.Context.Messaging.CurrentChatRoom;

		//public static Contact CurrentContact { get => Messaging.CurrentChatRoom; private set => Messaging.CurrentChatRoom = value; }
		private static readonly Dictionary<ulong, DateTime> _lastMessageTime = new Dictionary<ulong, DateTime>();
		internal static void ViewMessage(Message message, bool isMyMessage)
		{
			//Device.BeginInvokeOnMainThread(() =>
			//{
			Contact contact = message.Contact;
			if (contact == null)
				return; //We received a massage from a stranger (a contact not in the address book)
			StackLayout messageContainer = GetMessageContainerOfContact(contact);
			if (messageContainer == null)
				return;

			Setup.OnViewMessage.Invoke(message, isMyMessage, out View frame);
			if (frame != null)
			{
				lock (messageContainer)
				{
					var addInTop = false; //Old messages are added at the top and new ones at the bottom of the view
					lock (_lastMessageTime)
					{
						var chatId = contact.ChatId;
						if (_lastMessageTime.TryGetValue(chatId, out DateTime lastMessageTime))
						{
							addInTop = message.ReceptionTime < lastMessageTime;
							_lastMessageTime.Remove(chatId);
						}
						_lastMessageTime.Add(chatId, message.ReceptionTime);
					}

					Device.BeginInvokeOnMainThread(() =>
					{
						if ((message.Type != MessageType.Contact || (message.Type == MessageType.Contact && messageContainer.Children.Count == 0)))
						{
							if (message.Type != MessageType.Contact && contact.UnreadMessages > 0 && CheckContainterContainsUnreadedMessageView(messageContainer.Children) == null)
							{
								if (messageContainer.Children.Count == contact.UnreadMessages)
									messageContainer.Children.Insert(0, GetUnreadedMessagesFrame());
								else if (messageContainer.Children.Count > contact.UnreadMessages)
									messageContainer.Children.Add(GetUnreadedMessagesFrame());
							}

							if (addInTop)
								messageContainer.Children.Insert(0, frame);
							else
							{
								messageContainer.Children.Insert(messageContainer.Children.Count, frame);
							}
							if (Setup.Context.Messaging.CurrentChatRoom != null && message.Contact != null && Setup.Context.Messaging.CurrentChatRoom == message.Contact)
							{
								NewMessageReceived();
							}
						}
					});
				}
			}
			//});
		}

		private static void NewMessageReceived()
		{
			_onNewMessageAddedToView.Invoke();
		}

		public static void TransalateMessageDisplay(Message message, SwipeView view)
		{
			Frame frame = (view.Content as StackLayout).Children[0] as Frame;
			StackLayout st = frame.Content as StackLayout;
			int index = message.Contact.IsGroup ? 1 : 0;
			Label textLabel = st.Children[index] as Label;

			var originalText = Encoding.Unicode.GetString(message.GetData());

			var isTranslation = textLabel.Text != null && textLabel.Text != originalText;

			if (message.Translation == null || isTranslation)
				textLabel.Text = originalText;
			else
				textLabel.Text = Localization.Resources.Dictionary.Translated + ": " + message.Translation;
		}

		public static void RemoveMessage(Message message, bool alsoDeleteRemote = true)
		{
			if (Setup.Context.Messaging.CurrentChatRoom != null)
			{
				var counter = 0;
				var id = message.PostId.ToString();
				var container = Setup.Context.Messaging.CurrentChatRoom.MessageContainerUI as StackLayout;
				lock (container)
				{
					foreach (SwipeView view in container.Children)
					{
						counter++;
						if (view.AutomationId == id)
						{
							(Setup.Context.Messaging.CurrentChatRoom.MessageContainerUI as StackLayout).Children.Remove(view);
							message.Delete(alsoDeleteRemote);
							return;
						}
					}
				}
			}
		}

		internal static void OnContactEvent(Message message)
		{
			if (message.Type == MessageType.Delete)
				RemoveMessage(message.Contact, CommunicationChannel.Converter.BytesToUlong(message.GetData()));
			if (message.Type == MessageType.SmallData || message.Type == MessageType.Data)
			{
				var keyValue = Functions.SplitIncomingData(message.GetData(), message.Type == MessageType.SmallData);
			}
		}

		public static void RemoveMessage(Contact contact, ulong postId)
		{
			if (contact.MessageContainerUI != null)
			{
				var id = postId.ToString();
				var container = contact.MessageContainerUI as StackLayout;
				lock (container)
				{
					View toRemove = null;
					//toRemove	= (CurrentContact.MessageContainerUI as StackLayout).Children.First(x => x.AutomationId == id);
					foreach (View item in container.Children)
					{
						if (item.AutomationId == id)
						{
							toRemove = item;
							break;
						}
					}
					if (toRemove != null)
						container.Children.Remove(toRemove);
				}
			}
		}

		public static void RemoveUnreadedMessageView()
		{
			if (Setup.Context.Messaging.CurrentChatRoom != null)
			{
				var view = Setup.Context.Messaging.CurrentChatRoom.MessageContainerUI as StackLayout;
				View unreadedMessagesContainer = CheckContainterContainsUnreadedMessageView(view.Children);
				if (unreadedMessagesContainer != null)
					view.Children.Remove(unreadedMessagesContainer);
			}
		}

		public static View CheckContainterContainsUnreadedMessageView(IList<View> list)
		{
			lock (list)
			{
				foreach (View view in list)
				{
					if (view.ClassId == "UnreadedMessageContainer")
						return view;
				}
			}
			return null;
		}

		private static View GetUnreadedMessagesFrame()
		{
			SwipeView swipeView = new SwipeView { ClassId = "UnreadedMessageContainer", BackgroundColor = Color.FromHex("#FFD62C") };
			StackLayout stack = new StackLayout { BackgroundColor = Color.FromHex("#2d2d2d") };
			var undreadedMessagesFrame = new Frame { HasShadow = false, BackgroundColor = Color.FromHex("#FFD62C"), CornerRadius = 10, Margin = 10, Padding = new Thickness(5, 4, 5, 4), HorizontalOptions = LayoutOptions.CenterAndExpand, };
			Label time = new Label { HorizontalTextAlignment = TextAlignment.Center, Padding = new Thickness(10, 4, 10, 4), VerticalTextAlignment = TextAlignment.Center, Text = Localization.Resources.Dictionary.UnreadedMessages, FontSize = 14 };
			undreadedMessagesFrame.Content = time;
			stack.Children.Add(undreadedMessagesFrame);
			swipeView.Content = stack;
			return swipeView;
		}
		private static ScrollView _scrollMessageContainer;
		private static StackLayout InstantiateNewMessageContainer() => new StackLayout { };
		public static StackLayout GetMessageContainerOfContact(Contact contact)
		{
			if (contact != null)
				if (Config.ChatUI.MultipleChatModes)
				{
					if (contact.MessageContainerUI == null)
						contact.MessageContainerUI = InstantiateNewMessageContainer();
					return contact.MessageContainerUI as StackLayout;
				}
				else if (contact == Setup.Context.Messaging.CurrentChatRoom)
					return CurrentMessageContainer;
			return null;
		}
		public delegate void OnViewMessage<T1, T2, T3>(Message message, bool isMy, out View content);
		//private static OnViewMessage<Message, bool, View> _onViewMessage;

		public delegate void OnNewMessageAddedToView();
		private static OnNewMessageAddedToView _onNewMessageAddedToView;


	}
}