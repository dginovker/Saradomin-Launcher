namespace Saradomin.Model.Settings.Launcher
{
    public class LauncherSettings : SettingsBase
    {
        public const string FileName = "saradomin_launcher.json";

        public bool PlaceCloseButtonOnLeft { get; set; } = false;
        public bool ExitAfterLaunchingClient { get; set; } = false;
        
        public bool CheckForClientUpdatesOnLaunch { get; set; } = true;
        public string UserFriendlySongName { get; set; } = "Scape Main";
    }
}