using System.ComponentModel;
using Saradomin.Messaging;

namespace Saradomin.Model.Settings
{
    public class SettingsComponent : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsComponent()
        {
            PropertyChanged += RaiseSettingsModified;
        }

        protected void RaiseSettingsModified(object sender, PropertyChangedEventArgs e)
        {
            App.Messenger.Send(new SettingsModifiedMessage(e.PropertyName));
        }
    }
}