using System.ComponentModel;
using Avalonia.Media;

namespace Saradomin.Model
{
    public class MenuColor : INotifyPropertyChanged
    {
        public byte A { get; set; } = 255;
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public SolidColorBrush Brush
        {
            get => new(new Color(A, R, G, B));
            set
            {
                A = value.Color.A;
                R = value.Color.R;
                G = value.Color.G;
                B = value.Color.B;
            }
        }

        public string HexString => $"#{A:X2}{R:X2}{G:X2}{B:X2}";

        public event PropertyChangedEventHandler PropertyChanged;

        public MenuColor()
        {
            PropertyChanged += OnPropertyChanged;
        }

        public MenuColor(string hexColor)
            : this()
        {
            Brush = SolidColorBrush.Parse(hexColor);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(A):
                case nameof(R):
                case nameof(G):
                case nameof(B):
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Brush)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HexString)));
                    break;
            }
        }
    }
}