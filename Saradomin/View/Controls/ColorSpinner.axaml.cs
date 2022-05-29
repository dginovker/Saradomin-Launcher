using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using PropertyChanged;
using Saradomin.Model;

namespace Saradomin.View.Controls
{
    [DoNotNotify]
    public class ColorSpinner : UserControl
    {
        // These are RGBA because nobody sane uses ARGB on the Internet.
        //
        private static readonly List<Regex> _decFormatRegexes = new()
        {
            // 255, 255,255,   255 and 255,255, 255
            new(@"^((?<R>\d{1,3}),)?(\s+)?(?<G>\d{1,3}),(\s+)?(?<B>\d{1,3}),(\s+)?(?<A>\d{1,3})$"),
            
            // 255 255 255 255 and 255 255 255
            new(@"^((?<R>\d{1,3}))?\s+(?<G>\d{1,3})\s+(?<B>\d{1,3})\s+(?<A>\d{1,3})$"),
        };

        private static readonly List<Regex> _hexFormatRegexes = new()
        {
            // #FFFFFFFF and #FFFFFF
            new(@"^#?(?<R>[A-Fa-f0-9]{2})(?<G>[A-Fa-f0-9]{2})(?<B>[A-Fa-f0-9]{2})(?<A>[A-Fa-f0-9]{2})?$")
        };
        
        public static readonly StyledProperty<MenuColor> TargetColorProperty = new(
            nameof(TargetColor),
            typeof(ColorSpinner),
            new(new MenuColor("#000000"))
        );
        
        public static readonly StyledProperty<bool> EnableAlphaSpinnerProperty = new(
            nameof(EnableAlphaSpinner),
            typeof(ColorSpinner),
            new(true)
        );
        
        public static readonly StyledProperty<string> HeaderProperty = new(
            nameof(Header),
            typeof(ColorSpinner),
            new(string.Empty)
        );
        
        public static readonly StyledProperty<bool> ShowClipboardPasteLinkProperty = new(
            nameof(ShowClipboardPasteLink),
            typeof(ColorSpinner),
            new(true)
        );
        
        public static readonly StyledProperty<string> ClipboardPasteLinkTextProperty = new(
            nameof(ClipboardPasteLinkText),
            typeof(ColorSpinner),
            new("paste")
        );

        public MenuColor TargetColor
        {
            get => GetValue(TargetColorProperty);
            set => SetValue(TargetColorProperty, value);
        }

        public bool EnableAlphaSpinner
        {
            get => GetValue(EnableAlphaSpinnerProperty);
            set => SetValue(EnableAlphaSpinnerProperty, value);
        }

        public string Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public bool ShowClipboardPasteLink
        {
            get => GetValue(ShowClipboardPasteLinkProperty);
            set => SetValue(ShowClipboardPasteLinkProperty, value);
        }

        public string ClipboardPasteLinkText
        {
            get => GetValue(ClipboardPasteLinkTextProperty);
            set => SetValue(ClipboardPasteLinkTextProperty, value);
        }

        public ColorSpinner()
        {
            InitializeComponent();
        }

        protected virtual async void PasteFromClipboard()
        {
            var content = await Application.Current!.Clipboard!.GetTextAsync();

            if (string.IsNullOrEmpty(content))
                return;

            try
            {
                foreach (var regex in _decFormatRegexes)
                {
                    var match = regex.Match(content);

                    if (!match.Success)
                        continue;
                    
                    TargetColor.R = byte.Parse(match.Groups["R"].Value);
                    TargetColor.G = byte.Parse(match.Groups["G"].Value);
                    TargetColor.B = byte.Parse(match.Groups["B"].Value);

                    if (EnableAlphaSpinner && match.Groups.ContainsKey("A"))
                    {
                        TargetColor.A = byte.Parse(match.Groups["A"].Value);
                    }

                    return;
                }

                foreach (var regex in _hexFormatRegexes)
                {
                    var match = regex.Match(content);

                    if (!match.Success)
                        continue;
                    
                    TargetColor.R = byte.Parse(match.Groups["R"].Value, NumberStyles.AllowHexSpecifier);
                    TargetColor.G = byte.Parse(match.Groups["G"].Value, NumberStyles.AllowHexSpecifier);
                    TargetColor.B = byte.Parse(match.Groups["B"].Value, NumberStyles.AllowHexSpecifier);

                    if (EnableAlphaSpinner && match.Groups.ContainsKey("A"))
                    {
                        TargetColor.A = byte.Parse(match.Groups["A"].Value, NumberStyles.AllowHexSpecifier);
                    }
                }
            }
            catch
            {
                // Silently ignore.
                // Likely reasons include invalid format, regex failures.
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}