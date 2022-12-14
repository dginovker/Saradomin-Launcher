using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Glitonea;
using Glitonea.Mvvm;
using PropertyChanged;
using Saradomin.View.Windows;

namespace Saradomin
{
    [DoNotNotify]
    public class App : Application
    {
        public static Messenger Messenger { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            GlitoneaCore.Initialize();
            Messenger = new Messenger();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}