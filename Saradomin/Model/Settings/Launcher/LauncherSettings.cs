namespace Saradomin.Model.Settings.Launcher
{
    public class LauncherSettings : SettingsBase
    {
        public const string FileName = "saradomin_launcher.json";

        public bool CloseButtonOnLeft { get; set; } = false;
        public bool CheckForClientUpdates { get; set; } = true;
    }
}