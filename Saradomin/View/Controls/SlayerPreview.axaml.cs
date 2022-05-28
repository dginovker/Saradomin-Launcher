using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;
using Saradomin.Model;

namespace Saradomin.View.Controls
{
    [DoNotNotify]
    public class SlayerPreview : UserControl
    {
        public static readonly StyledProperty<MenuColor> BackgroundColorProperty = new(
            nameof(BackgroundColor),
            typeof(SlayerPreview),
            new()
        );

        public MenuColor BackgroundColor
        {
            get => GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }
        
        public SlayerPreview()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}