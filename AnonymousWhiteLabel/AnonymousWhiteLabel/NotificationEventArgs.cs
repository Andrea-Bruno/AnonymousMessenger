using System;

namespace AnonymousWhiteLabel
{
  public class NotificationEventArgs : EventArgs
  {
    public string Title { get; set; }
    public string Message { get; set; }
  }
}
