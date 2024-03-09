using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EncryptedMessaging;
using Xamarin.Forms;
using static EncryptedMessaging.MessageFormat;

namespace XamarinShared
{
	public class MessageReadStatus
	{
		public MessageStatus AddReadStatusFlag(Message message, bool isMyMessage, Func<Contact, MessageType, Task> SendNotificationToChannel, out StackLayout flagAndTime)
		{
			var contact = message.Contact;
			MessageStatus messageStatus = null;


			var timeLabel = new Label() { FontSize = 13, FontFamily = "PoppinsRegular", TextColor= isMyMessage ? Color.Black : Color.White  };
			//var differentDay = Time.CurrentTimeGMT.ToLocalTime().DayOfYear == message.Creation.ToLocalTime().DayOfYear;
			timeLabel.Text = message.Creation.ToLocalTime().ToShortTimeString();
				

			flagAndTime = new StackLayout() { Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.EndAndExpand, Margin = new Thickness(0,0, isMyMessage ? 4: 12, 0) };
			if (message.Type != MessageType.Contact)
				flagAndTime.Children.Add(timeLabel);

			// Add the flag that the recipient has read it
			if (isMyMessage && message.Type != MessageType.Contact)
			{
				lock (_statusContacts)
				{
					if (!_statusContacts.TryGetValue(contact.ChatId, out List<MessageStatus> flags))
					{
						flags = new List<MessageStatus>();
						_statusContacts.Add(contact.ChatId, flags);
					}
					async void DeliveryNotification()
					{
						if (message.Type != MessageType.AudioCall && message.Type != MessageType.VideoCall && !message.Contact.IsMuted)
						{
							try
							{
								SendNotificationToChannel?.Invoke(contact, message.Type);
							}
							catch (Exception e)
							{

							}
						}
					}

					messageStatus = new MessageStatus(contact.LastMessageDelivered, message.Creation) { TotalParticipants = contact.Participants.Count, ExecuteOnMessageDelivered = DeliveryNotification };
					flags.Add(messageStatus);
					flagAndTime.Children.Add(messageStatus.StatusLabel);
				}
			}
			return messageStatus;
		}

		public enum Status
		{
			Pending,
			Delivered,
			Readed,
		}
		private  readonly Dictionary<ulong, List<MessageStatus>> _statusContacts = new Dictionary<ulong, List<MessageStatus>>();

		public void OnLastReadedTimeChange(Contact contact, ulong idParticipant, DateTime time)
		{
			lock (_statusContacts)
			{
				if (_statusContacts.TryGetValue(contact.ChatId, out List<MessageStatus> flags))
					foreach (MessageStatus messageStatus in flags)
						if (messageStatus.Creation <= time)
							if (messageStatus.Status != Status.Readed)
								if (!messageStatus.ParticipantsReaded.Contains(idParticipant))
								{
									messageStatus.ParticipantsReaded.Add(idParticipant);
									if (messageStatus.ParticipantsReaded.Count == messageStatus.TotalParticipants - 1)
										messageStatus.Status = Status.Readed;
								}
			}
		}

		/// <summary>
		/// This function is called whenever a message is successfully sent to the server, or when a message is loaded when populating the list of messages in the group
		/// </summary>
		/// <param name="contact">The group in which the message is located</param>
		/// <param name="messageCreation">When the message was created. This parameter helps us to understand which are the previous messages already sent and which is the message related to the notification</param>
		/// <param name="isLoading">If true, then the message has not been delivered now, but is simply loading the message list</param>
		public void OnMessageDelivered(Contact contact, DateTime messageCreation, bool isLoading)
		{
			lock (_statusContacts)
			{
				if (_statusContacts.TryGetValue(contact.ChatId, out List<MessageStatus> flags))
					foreach (MessageStatus messageStatus in flags)
						if (messageStatus.Creation <= messageCreation)
						{
							if (messageStatus.Status == Status.Pending)
							{
								if (!isLoading && messageStatus.Creation == messageCreation)
									messageStatus?.ExecuteOnMessageDelivered();
								messageStatus.ExecuteOnMessageDelivered = null;
								//if (!isLoading)
								//	Thread.Sleep(3000);
								Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() => messageStatus.Status = Status.Delivered);
							}
						}
			}
		}

		public class MessageStatus
		{
			public MessageStatus(Contact.MessageInfo lastMessageDelivered, DateTime creation)
			{
				Status = lastMessageDelivered != null
									? creation.Ticks <= lastMessageDelivered.Creation ? Status.Delivered : Status.Pending
									: Status.Pending;
				Creation = creation;
			}
			public Action ExecuteOnMessageDelivered = null;
			public Label StatusLabel = new Label() { Padding = 0, FontSize = 9 };
			public DateTime Creation;
			public List<ulong> ParticipantsReaded = new List<ulong>();
			public int TotalParticipants;
			private Status _status;
			public Status Status
			{
				get => _status; set
				{
					_status = value;
					if (value == Status.Pending)
					{ StatusLabel.Text = "🕑"; StatusLabel.TextColor = Color.Black; }
					else if (value == Status.Delivered)
					{ StatusLabel.Text = "✓"; StatusLabel.TextColor = Color.Black; }
					else if (value == Status.Readed)
					{ StatusLabel.Text = "✓✓"; StatusLabel.TextColor = Color.Green; }
				}
			}
		};

	}
}
