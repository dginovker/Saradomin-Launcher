using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace Saradomin.View.Controls
{
    [DoNotNotify]
    public class SingleplayerView : UserControl
    {
        public SingleplayerView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}