using Glitonea.Mvvm.Messaging;

namespace Saradomin.Infrastructure.Messaging
{
    public sealed record MainViewLoadedMessage : Message;
    public sealed record NotificationBoxStateChangedMessage(bool WasOpened) : Message;
    public sealed record SettingsModifiedMessage(string SettingName) : Message;
}