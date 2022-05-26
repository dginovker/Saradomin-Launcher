using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace Saradomin.Views.Controls
{
    [DoNotNotify]
    public class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}