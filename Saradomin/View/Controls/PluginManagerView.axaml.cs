using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace Saradomin.View.Controls
{
    [DoNotNotify]
    public class PluginManagerView : UserControl
    {
        public PluginManagerView()
        {
            InitializeComponent();
        }
    
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}