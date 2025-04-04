namespace Cryptogram.ViewModels
{
	internal class MessageListViewModel
	{
		public string Message { get; set; }
		public string Time { get; set; }
		public bool IsMyMessage { get; set; }

		public MessageListViewModel(string Message, string Time, bool IsMyMessage)
		{
			this.Message = Message;
			this.Time = Time;
			this.IsMyMessage = IsMyMessage;
		}

	}
}
