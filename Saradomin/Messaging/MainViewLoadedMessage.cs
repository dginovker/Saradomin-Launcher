using Avalonia.Controls.Html;

namespace Saradomin.Messaging
{
    public class MainViewLoadedMessage
    {
        public HtmlControl HtmlView { get; }

        public MainViewLoadedMessage(HtmlControl htmlView)
        {
            HtmlView = htmlView;
        }
    }
}