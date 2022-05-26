using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace Saradomin.Views.Controls
{
    [DoNotNotify]
    public class HeaderedTextBox : UserControl
    {
        public static readonly StyledProperty<string> HeaderProperty = new(
            nameof(Header),
            typeof(HeaderedTextBox),
            new StyledPropertyMetadata<string>("i_am_a_lazy_piece_of_shit")
        );
        
        public static readonly StyledProperty<string> TextProperty = new(
            nameof(Text),
            typeof(HeaderedTextBox),
            new StyledPropertyMetadata<string>(string.Empty)
        );

        public string Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }
        
        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
            
        public HeaderedTextBox()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}