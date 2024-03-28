using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Glitonea.Mvvm.Messaging;
using PropertyChanged;
using Saradomin.Infrastructure.Messaging;

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