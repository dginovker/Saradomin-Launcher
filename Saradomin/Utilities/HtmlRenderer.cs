using System;
using System.Collections.Generic;
using System.Web;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using HtmlAgilityPack;

namespace Saradomin.Utilities
{
    public class HtmlRenderer
    {
        private int _listDescent = 0;

        private Stack<StackPanel> _panels = new();
        private StackPanel _container;
        private HtmlNode _rootNode;

        public StackPanel CurrentLine => _panels.Peek();

        public HtmlRenderer(StackPanel container, HtmlNode rootNode)
        {
            _container = container;
            _rootNode = rootNode;
        }

        public void RenderToContainer()
        {
            _panels.Push(_container);
            Visit(_rootNode);
            _panels.Pop();
        }

        private void InNewBlockDo(Action<StackPanel> action)
        {
            var line = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _panels.Push(line);
            action(line);

            _container.Children.Add(_panels.Pop());
        }

        private void CreateNewInlineTextElement(
            string text,
            FontWeight weight = FontWeight.Normal,
            double fontSize = 12,
            TextWrapping wrapping = TextWrapping.Wrap)
        {
            CurrentLine.Children.Add(new TextBlock
            {
                Foreground = new SolidColorBrush(Colors.Black),
                FontSize = fontSize,
                FontWeight = weight,
                TextWrapping = wrapping,
                Text = text,
                Margin = new Thickness(_listDescent * 8, 0, 0, 0)
            });
        }

        private void VisitBold(HtmlNode htmlNode)
        {
            CreateNewInlineTextElement(
                HttpUtility.HtmlDecode(
                    htmlNode.GetDirectInnerText()
                ),
                FontWeight.Bold
            );
        }

        private void VisitList(HtmlNode htmlNode)
        {
            _listDescent++;
            {
                InNewBlockDo((_) =>
                {
                    foreach (var child in htmlNode.ChildNodes)
                    {
                        Visit(child);
                    }
                });
            }
            _listDescent--;
        }

        private void VisitListEntry(HtmlNode htmlNode)
        {
            foreach (var child in htmlNode.ChildNodes)
            {
                Visit(child);
            }
        }

        private void VisitParagraph(HtmlNode htmlNode)
        {
            InNewBlockDo((_) =>
            {
                foreach (var child in htmlNode.ChildNodes)
                {
                    Visit(child);
                }
            });
        }

        private void Visit(HtmlNode htmlNode)
        {
            switch (htmlNode.Name)
            {
                case "#text":
                    var text = HttpUtility.HtmlDecode(
                        htmlNode.GetDirectInnerText().Trim(' ').Trim('\n')
                    );
                    
                    if (htmlNode.ParentNode.Name == "li")
                    {
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            text = $"• {text}";
                        }
                    }
                    
                    CreateNewInlineTextElement(text);
                    break;

                case "b":
                    VisitBold(htmlNode);
                    break;

                case "ul":
                    VisitList(htmlNode);
                    break;

                case "li":
                    VisitListEntry(htmlNode);
                    break;

                case "p":
                    VisitParagraph(htmlNode);
                    break;

                default:
                    foreach (var child in htmlNode.ChildNodes)
                    {
                        Visit(child);
                    }
                    break;
            }
        }
    }
}