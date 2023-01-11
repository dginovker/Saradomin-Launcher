using System.ComponentModel;
using System.Runtime.InteropServices;
using Saradomin.Utilities;

namespace Saradomin.Model.Settings.Launcher
{
    public class LauncherSettings : SettingsBase
    {
        public const string FileName = "saradomin_launcher.json";
        
        public bool PlaceCloseButtonOnLeft { get; set; } = false;
        public bool ExitAfterLaunchingClient { get; set; } = true;
        public bool AllowMultiboxing { get; set; } = false;

        public bool CheckForClientUpdatesOnLaunch { get; set; } = true;
        public bool CheckForServerProfilesOnLaunch { get; set; } = true;
        
        public string JavaExecutableLocation { get; set; }

        public string InstallationDirectory { get; set; } = CrossPlatform.LocateDefault2009scapeHome();

        protected override void OnSettingsModified(string propertyName)
        {
            if (propertyName == nameof(AllowMultiboxing) && AllowMultiboxing)
            {
                ExitAfterLaunchingClient = false;
            }
        }
    }
}