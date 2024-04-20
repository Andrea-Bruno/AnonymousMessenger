using System;
using System.Collections.Generic;
using System.Text;

namespace Banking
{
	public static class Communication
	{
		public enum Command : short // 2 byte : This series of enumerated commands must be identical to the one on the "Bank" cloud project, class "Communication"
		{
			CreateErc20Token,
			CreateTokenCompleted
		}

		public enum Feedback : byte // Negative values are used to insert feedback into messages when something goes wrong
		{
			Successful = 0,
			GenericError = 1,
		}

		private static readonly byte _version = 0;

		// =======================================================
		// =========    Messages for the cloud server    =========
		// =======================================================

		/// <summary>
		/// Prepare the data package for the cloud, and send it
		/// </summary>
		/// <param name="onReceivingReply">Action that will be performed when confirmation is received. If the action will be called without parameters (with parameters set to null), then it means that the server did not respond and the timeout was triggered</param>
		/// <param name="command">Command for the cloud</param>
		/// <param name="circuitId">The client can use this parameter to tell the cloud to route commands to a specific circuit</param>
		/// <param name="values">Parameters to sent</param>
		public static void JoinAndSend(Action<List<byte[]>> onReceivingReply, Command command, ulong circuitId = 0, params byte[][] values)
		{
			lock (actionsEventsForAnswer)
			{
				var listValues = new List<byte[]>(values);
				listValues.Insert(0, BitConverter.GetBytes(progressiveRequestId));
				listValues.Insert(0, BitConverter.GetBytes((short)command));
				listValues.Insert(0, BitConverter.GetBytes(circuitId));
				listValues.Insert(0, new byte[] { _version });
				var data = EncryptedMessaging.Functions.JoinData(false, listValues.ToArray());
				var cmd = Enum.GetName(typeof(Command), command);
				SendMessage(data);
				if (onReceivingReply != null)
				{
					actionsEventsForAnswer.Add(progressiveRequestId, onReceivingReply);
					var timeout = new System.Timers.Timer { Enabled = true, Interval = 10000 };
					timeout.Elapsed += (e, y) =>
					{
						lock (actionsEventsForAnswer)
						{
							if (!actionsEventsForAnswer.TryGetValue(progressiveRequestId, out var action)) return;
							actionsEventsForAnswer.Remove(progressiveRequestId);
							action.Invoke(null);
						}
					};
					timeout.Start();
				}
				progressiveRequestId++;
			}
		}
		private static readonly Dictionary<ushort, Action<List<byte[]>>> actionsEventsForAnswer = new Dictionary<ushort, Action<List<byte[]>>>();
		private static ushort progressiveRequestId;

		public static void CreateErc20Token(Action<List<byte[]>> onReceivingReply, string name, string symbol, ulong decimalUnits, ulong initialSupply)
		{
			JoinAndSend(onReceivingReply, Command.CreateErc20Token, default, Encoding.Unicode.GetBytes(name), Encoding.Unicode.GetBytes(symbol), BitConverter.GetBytes(decimalUnits), BitConverter.GetBytes(initialSupply));
		}

		// =======================================================
		// =========    Answers from the cloud server    =========
		// =======================================================

		/// <summary>
		/// Send a message to the cloud without spooling it: This means that if the message will reach its destination if both the client and the cloud are connected. If the connection is missing, the message will be lost and the cloud will not receive any command.
		/// </summary>
		/// <param name="data">Data to be sent</param>
		public static void SendMessage(byte[] data)
		{
			BankCloud.Context.Messaging.SendBinary(BankCloud.CloudContact, data, true);
		}
		public static void ProcessIncomingMessage(EncryptedMessaging.Message message)
		{
			System.Diagnostics.Debug.WriteLine(message.ChatId);

			var parts = EncryptedMessaging.Functions.SplitData(message.GetData(), false);
			var answer = (Command)BitConverter.ToUInt16(parts[0], 0);
			parts.RemoveAt(0);
			var replyToRequestId = BitConverter.ToUInt16(parts[0], 0);
			parts.RemoveAt(0);
			lock (actionsEventsForAnswer)
			{
				if (actionsEventsForAnswer.TryGetValue(replyToRequestId, out var action))
				{
					actionsEventsForAnswer.Remove(replyToRequestId);
					action.Invoke(parts);
				}
			}
			if (answer == Command.CreateTokenCompleted)
			{

			}
		}

	}
}
