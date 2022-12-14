namespace Saradomin.Infrastructure.Messaging
{
    public class SettingsModifiedMessage
    {
        public string SettingName { get; }

        public SettingsModifiedMessage(string settingName)
        {
            SettingName = settingName;
        }
    }
}