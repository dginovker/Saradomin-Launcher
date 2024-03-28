using System;
using Avalonia.Controls;
using Avalonia.Input;
using PropertyChanged;
using Saradomin.Infrastructure;

namespace Saradomin.View.Windows
{
    [DoNotNotify]
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnOpened(EventArgs e)
        {
            new MainViewLoadedMessage()
                .Broadcast();
        }

        private void TitleBar_MouseDown(object _, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        }
    }
}