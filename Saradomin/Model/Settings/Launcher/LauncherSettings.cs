using System.ComponentModel;
using System.Runtime.InteropServices;

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
        public bool AllowMultiboxing { get; set; } = false;

        public bool CheckForClientUpdatesOnLaunch { get; set; } = true;
        public bool CheckForServerProfilesOnLaunch { get; set; } = true;
        
        public string UserFriendlySongName { get; set; } = "Scape Main";
        public string JavaExecutableLocation { get; set; }

        public ClientReleaseProfile ClientProfile { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? ClientReleaseProfile.Experimental : ClientReleaseProfile.Legacy;

        protected override void OnSettingsModified(string propertyName)
        {
            if (propertyName == nameof(AllowMultiboxing) && AllowMultiboxing)
            {
                ExitAfterLaunchingClient = false;
            }
        }
    }
}