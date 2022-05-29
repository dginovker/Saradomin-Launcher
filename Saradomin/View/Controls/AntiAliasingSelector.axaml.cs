using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace Saradomin.View.Controls
{
    [DoNotNotify]
    public class AntiAliasingSelector : UserControl
    {
        private readonly RadioButton _aa0;
        private readonly RadioButton _aa2;
        private readonly RadioButton _aa4;
        private readonly RadioButton _aa8;
        private readonly RadioButton _aa16;

        public static readonly StyledProperty<byte> AntiAliasingLevelProperty = new(
            nameof(AntiAliasingLevel),
            typeof(AntiAliasingSelector),
            new(0)
        );

        public byte AntiAliasingLevel
        {
            get => GetValue(AntiAliasingLevelProperty);
            set => SetValue(AntiAliasingLevelProperty, value);
        }

        public AntiAliasingSelector()
        {
            InitializeComponent();
            
            _aa0 = this.FindControl<RadioButton>("AA_0");
            _aa2 = this.FindControl<RadioButton>("AA_2");
            _aa4 = this.FindControl<RadioButton>("AA_4");
            _aa8 = this.FindControl<RadioButton>("AA_8");
            _aa16 = this.FindControl<RadioButton>("AA_16");
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            if (change.Property == AntiAliasingLevelProperty)
            {
                switch (AntiAliasingLevel)
                {
                    case 0:
                        _aa0.IsChecked = true;
                        break;
                    
                    case 2:
                        _aa2.IsChecked = true;
                        break;
                    
                    case 4:
                        _aa4.IsChecked = true;
                        break;
                    
                    case 8:
                        _aa8.IsChecked = true;
                        break;
                    
                    case 16:
                        _aa16.IsChecked = true;
                        break;
                }
            }
        }

        public void UpdateAntiAliasingLevel(byte level)
        {
            AntiAliasingLevel = level;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}