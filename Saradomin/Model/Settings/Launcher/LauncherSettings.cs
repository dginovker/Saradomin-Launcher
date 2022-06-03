using System.ComponentModel;

namespace Saradomin.Model.Settings.Launcher
{
    public class LauncherSettings : SettingsBase
    {
        public const string FileName = "saradomin_launcher.json";

        public enum ClientReleaseProfile
        {
            [Description("Legacy")]
            Legacy,
            
            [Description("Experimental")]
            Experimental
        }
        
        public bool PlaceCloseButtonOnLeft { get; set; } = false;
        public bool ExitAfterLaunchingClient { get; set; } = true;
        
        public bool CheckForClientUpdatesOnLaunch { get; set; } = true;
        public string UserFriendlySongName { get; set; } = "Scape Main";
        public string JavaExecutableLocation { get; set; }
        public ClientReleaseProfile ClientProfile { get; set; } = ClientReleaseProfile.Legacy;
    }
}