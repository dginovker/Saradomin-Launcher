using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using PropertyChanged;
using Saradomin.Messaging;

namespace Saradomin.View.Controls
{
    [DoNotNotify]
    public class MenuPreview : UserControl
    {
        public static readonly StyledProperty<SolidColorBrush> BackgroundColorProperty = new(
            nameof(BackgroundColor),
            typeof(MenuPreview),
            new StyledPropertyMetadata<SolidColorBrush>(
                new(SolidColorBrush.Parse("#5D5447"))
            )
        );

        public static readonly StyledProperty<SolidColorBrush> TitleBarColorProperty = new(
            nameof(TitleBarColor),
            typeof(MenuPreview),
            new(SolidColorBrush.Parse("#000000"))
        );

        public static readonly StyledProperty<SolidColorBrush> TitleFontColorProperty = new(
            nameof(TitleFontColor),
            typeof(MenuPreview),
            new(SolidColorBrush.Parse("#FFFFFF"))
        );

        public static readonly StyledProperty<SolidColorBrush> BorderColorProperty = new(
            nameof(BorderColor),
            typeof(MenuPreview),
            new(SolidColorBrush.Parse("#FFFFFF"))
        );

        public static readonly StyledProperty<bool> UseRs3BorderProperty = new(
            nameof(UseRs3Border),
            typeof(MenuPreview),
            new(false)
        );

        private static readonly StyledProperty<Thickness> OldStyleBorderThicknessProperty = new(
            nameof(OldStyleBorderThickness),
            typeof(MenuPreview),
            new(Thickness.Parse("1"))
        );

        private static readonly StyledProperty<Thickness> RuneScape3StyleBorderThicknessProperty = new(
            nameof(RuneScape3StyleBorderThickness),
            typeof(MenuPreview),
            new(Thickness.Parse("0"))
        );

        public SolidColorBrush BackgroundColor
        {
            get => GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        public SolidColorBrush TitleBarColor
        {
            get => GetValue(TitleBarColorProperty);
            set => SetValue(TitleBarColorProperty, value);
        }

        public SolidColorBrush TitleFontColor
        {
            get => GetValue(TitleFontColorProperty);
            set => SetValue(TitleFontColorProperty, value);
        }

        public SolidColorBrush BorderColor
        {
            get => GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        public bool UseRs3Border
        {
            get => GetValue(UseRs3BorderProperty);
            set => SetValue(UseRs3BorderProperty, value);
        }

        private Thickness OldStyleBorderThickness
        {
            get => GetValue(OldStyleBorderThicknessProperty);
            set => SetValue(OldStyleBorderThicknessProperty, value);
        }

        private Thickness RuneScape3StyleBorderThickness
        {
            get => GetValue(RuneScape3StyleBorderThicknessProperty);
            set => SetValue(RuneScape3StyleBorderThicknessProperty, value);
        }

        public MenuPreview()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == UseRs3BorderProperty)
            {
                if (UseRs3Border)
                {
                    OldStyleBorderThickness = new(0);
                    RuneScape3StyleBorderThickness = new(1);
                }
                else
                {
                    OldStyleBorderThickness = new(1);
                    RuneScape3StyleBorderThickness = new(0);
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}