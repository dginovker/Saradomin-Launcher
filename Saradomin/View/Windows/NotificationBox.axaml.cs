using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Glitonea;
using Glitonea.Extensions;
using PropertyChanged;
using Saradomin.Infrastructure.Messaging;

namespace Saradomin.View.Windows
{
    [DoNotNotify]
    public partial class NotificationBox : WindowEx
    {
        private static Queue<NotificationBox> _notificationQueue = new();
        
        public static NotificationBox Current { get; private set; }
        
        public static readonly StyledProperty<string> MessageProperty = AvaloniaProperty.Register<NotificationBox, string>(
            nameof(Message), 
            "This is supposed to be a message.\n" +
            "Someone messed up, though, so have this placeholder instead."
        );

        public string Message
        {
            get => GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }
        
        public NotificationBox()
        {
            InitializeComponent();
        }

        private NotificationBox(string title, string message)
            : this()
        {
            Title = title;
            Message = message;
        }

        public static void DisplayNotification(string message)
            => DisplayNotification("Notification", message);

        public static void DisplayNotification(string title, string message)
            => DisplayNotification(title, message, Application.Current.GetMainWindow());

        public static void DisplayNotification(string title, string message, Window owner)
        {
            var box = new NotificationBox(title, message) { Owner = owner };
            
            if (Current != null)
            {
                _notificationQueue.Enqueue(box);
                Current.Activate();
            }
            else
            {
                Current = box;
                Current.ShowDialog(owner);
                new NotificationBoxStateChangedMessage(true)
                    .Broadcast();
            }
        }

        public new void Close()
        {
            if (Current != null)
            {
                Current = null;
                
                base.Close();
                new NotificationBoxStateChangedMessage(false)
                    .Broadcast();
            }

            if (_notificationQueue.Any())
            {
                Current = _notificationQueue.Dequeue();
                Current.ShowDialog(Current.Owner as Window);
                new NotificationBoxStateChangedMessage(true)
                    .Broadcast();
            }
        }
    }
}