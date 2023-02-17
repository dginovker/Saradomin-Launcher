using Glitonea.Mvvm.Messaging;

namespace Saradomin.Infrastructure.Messaging
{
    public class SettingsModifiedMessage : Message
    {
        public string SettingName { get; }

        public SettingsModifiedMessage(string settingName)
        {
            SettingName = settingName;
        }
    }
}