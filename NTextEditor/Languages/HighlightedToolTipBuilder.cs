﻿using NTextEditor.Source;
using NTextEditor.View;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.Languages
{
    internal class HighlightedToolTipBuilder : IToolTipSource
    {
        private readonly List<(string text, Color colour, int parameterIndex)> _values;
        private string? _cachedToolTip;
        private List<SyntaxHighlighting>? _cachedHighlightings;

        public SyntaxPalette Palette { get; }

        public HighlightedToolTipBuilder(SyntaxPalette palette)
        {
            _values = new List<(string text, Color colour, int parameterIndex)>();
            Palette = palette;
        }

        (string toolTip, List<SyntaxHighlighting> highlightings) IToolTipSource.GetToolTip()
        {
            return ToToolTip();
        }

        public (string toolTipText, List<SyntaxHighlighting> highlightings) ToToolTip()
        {
            if (_cachedToolTip == null
                || _cachedHighlightings == null)
            {
                StringBuilder sb = new StringBuilder();
                List<SyntaxHighlighting> highlightings = new List<SyntaxHighlighting>();
                int current = 0;
                foreach ((string text, Color colour, int parameterIndex) in _values)
                {
                    sb.Append(text);
                    int next = current + text.Length;
                    highlightings.Add(new SyntaxHighlighting(new SourceCodePosition(0, current), new SourceCodePosition(0, next), colour, parameterIndex));
                    current = next;
                }
                _cachedToolTip = sb.ToString();
                _cachedHighlightings = highlightings;
            }
            return (_cachedToolTip, _cachedHighlightings);
        }

        public HighlightedToolTipBuilder Add(string text, Color colour, int parameterIndex = -1)
        {
            _values.Add((text, colour, parameterIndex));
            _cachedToolTip = null;
            _cachedHighlightings = null;
            return this;
        }

        public HighlightedToolTipBuilder AddDefault(string text, int parameterIndex = -1) => Add(text, Palette.DefaultTextColour, parameterIndex);
    }
}
