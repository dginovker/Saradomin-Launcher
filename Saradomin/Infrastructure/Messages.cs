using Glitonea.Mvvm.Messaging;

namespace Saradomin.Infrastructure
{
    public record MainViewLoadedMessage : Message;
    public record NotificationBoxStateChangedMessage(bool WasOpened) : Message;
    public record SettingsModifiedMessage(string SettingName) : Message;
}