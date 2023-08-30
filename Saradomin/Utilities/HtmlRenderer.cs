using System;
using System.Collections.Generic;
using System.Web;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using HtmlAgilityPack;

namespace Saradomin.Utilities
{
    public class HtmlRenderer
    {
        private int _listDescent = 0;
        private string _prefix;
        private InlineCollection _inlines;
        private TextBlock _textBlock;
        private HtmlNode _rootNode;

        public HtmlRenderer(TextBlock textBlock, HtmlNode rootNode)
        {
            _textBlock = textBlock;

            _textBlock.Inlines = _textBlock.Inlines ?? (_inlines = new());
            _inlines = _textBlock.Inlines;

            _rootNode = rootNode;
        }

        public void RenderToContainer()
        {
            Visit(_rootNode);
        }

        private void InNewBlockDo(Action action)
        {
            action();
            _inlines.Add(new LineBreak());
        }

        private void CreateNewInlineTextElement(
            string text,
            FontWeight weight = FontWeight.Normal,
            double fontSize = 12
        )
        {
            _inlines.Add(new Run(text)
            {
                FontWeight = weight,
                FontSize = fontSize,
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
                InNewBlockDo(() =>
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
            _inlines.Add(new Run("• "));

            for (var i = 0; i < htmlNode.ChildNodes.Count; i++)
            {
                var child = htmlNode.ChildNodes[i];

                if (i != 0)
                {
                    _prefix = "  ";
                }
                
                Visit(child);
                
                _prefix = null;
            }

            _prefix = null;
            _inlines.Add(new LineBreak());
        }

        private void VisitParagraph(HtmlNode htmlNode)
        {
            InNewBlockDo(() =>
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

                    CreateNewInlineTextElement($"{_prefix}{text}");
                    break;

                case "br":
                    _inlines.Add(new LineBreak());
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