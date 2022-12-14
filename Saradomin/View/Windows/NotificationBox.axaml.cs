using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;
using Saradomin.Infrastructure.Messaging;

namespace Saradomin.View.Windows
{
    [DoNotNotify]
    public class NotificationBox : Window
    {
        public static NotificationBox Current { get; private set; }
        
        public static readonly StyledProperty<string> MessageProperty = new(
            nameof(Message), 
            typeof(NotificationBox),
            new StyledPropertyMetadata<string>(
                "This is supposed to be a message.\n" +
                "Someone messed up, though, so have this placeholder instead."
            )
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

        private NotificationBox(string message)
            : this("Notification", message)
        {
        }

        private NotificationBox(string title, string message)
            : this()
        {
            Title = title;
            Message = message;
        }

        public static void DisplayNotification(string title, string message, Window owner)
        {
            if (Current != null)
            {
                Current.Activate();
            }
            else
            {
                Current = new NotificationBox(title, message);
                Current.ShowDialog(owner);

                App.Messenger.Send(new NotificationBoxStateChangedMessage(true));
            }
        }

        public new void Close()
        {
            if (Current != null)
            {
                Current = null;
                base.Close();

                App.Messenger.Send(new NotificationBoxStateChangedMessage(false));
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}