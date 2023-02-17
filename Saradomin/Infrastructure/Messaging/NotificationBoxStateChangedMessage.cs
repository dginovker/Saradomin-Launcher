using Glitonea.Mvvm.Messaging;

namespace Saradomin.Infrastructure.Messaging
{
    public class NotificationBoxStateChangedMessage : Message
    {
        public bool WasOpened { get; }

        public NotificationBoxStateChangedMessage(bool wasOpened)
        {
            WasOpened = wasOpened;
        }
    }
}