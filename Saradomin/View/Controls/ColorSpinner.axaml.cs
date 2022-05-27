using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using PropertyChanged;
using Saradomin.Model;

namespace Saradomin.View.Controls
{
    [DoNotNotify]
    public class ColorSpinner : UserControl
    {
        public static readonly StyledProperty<MenuColor> TargetColorProperty = new(
            nameof(TargetColor),
            typeof(ColorSpinner),
            new(Optional<MenuColor>.Empty)
        );
        
        public static readonly StyledProperty<bool> ShowAlphaSpinnerProperty = new(
            nameof(ShowAlphaSpinner),
            typeof(ColorSpinner),
            new(true)
        );
        
        public static readonly StyledProperty<string> HeaderProperty = new(
            nameof(Header),
            typeof(string),
            new(string.Empty)
        );

        public MenuColor TargetColor
        {
            get => GetValue(TargetColorProperty);
            set => SetValue(TargetColorProperty, value);
        }

        public bool ShowAlphaSpinner
        {
            get => GetValue(ShowAlphaSpinnerProperty);
            set => SetValue(ShowAlphaSpinnerProperty, value);
        }

        public string Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public ColorSpinner()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}