namespace Saradomin.Infrastructure.Messaging
{
    public class NotificationBoxStateChangedMessage
    {
        public bool WasOpened { get; }

        public NotificationBoxStateChangedMessage(bool wasOpened)
        {
            WasOpened = wasOpened;
        }
    }
}