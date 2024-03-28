using System.ComponentModel;
using Saradomin.Infrastructure;

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
            OnSettingsModified(e.PropertyName);

            new SettingsModifiedMessage(e.PropertyName)
                .Broadcast();
        }

        protected virtual void OnSettingsModified(string propertyName)
        {
        }
    }
}