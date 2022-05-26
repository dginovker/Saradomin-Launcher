using System;
using System.Net.Http;
using Glitonea.Mvvm;
using HtmlAgilityPack;
using Saradomin.Messaging;
using Saradomin.Utilities;

namespace Saradomin.ViewModel.Windows
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Title { get; set; } = "2009scape launcher";

        public MainWindowViewModel()
        {
            App.Messenger.Register<MainViewLoadedMessage>(this, MainViewLoaded);
        }

        public void ExitApplication()
        {
            Environment.Exit(0);
        }

        public async void MainViewLoaded(MainViewLoadedMessage msg)
        {
            msg.HtmlView.BaseStylesheet = "* { font-size: 18px; }";
            
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync("https://2009scape.org/services/m=news/archives/latest.html");
                var doc = new HtmlDocument();
                doc.Load(await response.Content.ReadAsStreamAsync());
                var node = doc.DocumentNode.SelectSingleNode("//div[@class='msgcontents']");
                msg.HtmlView.Text = node.InnerHtml;
            }
        }

        public void LaunchPage(string parameter)
        {
            var url = parameter switch
            {
                "news" => "https://2009scape.org/services/m=news/archives/latest.html",
                "issues" => "https://gitlab.com/2009scape/2009scape/-/issues",
                "hiscores" => "https://2009scape.org/services/m=hiscore/hiscores.html?world=2",
                "discord" => "https://discord.gg/YY7WSttN7H",
                _ => throw new ArgumentException($"{parameter} is not a valid page parameter.")
            };

            CrossPlatform.LaunchURL(url);
        }
    }
}