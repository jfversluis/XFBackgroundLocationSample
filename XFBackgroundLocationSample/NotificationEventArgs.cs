using System;
namespace XFBackgroundLocationSample
{
    public class NotificationEventArgs : EventArgs
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }
}
