using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.CS
{
    internal class HighlightedToolTipBuilder : IToolTipSource
    {
        private readonly List<(string text, Color colour)> _values;
        private readonly SyntaxPalette _palette;
        private string _cachedToolTip;
        private List<SyntaxHighlighting> _cachedHighlightings;

        public HighlightedToolTipBuilder(SyntaxPalette palette)
        {
            _values = new List<(string text, Color colour)>();
            _palette = palette;
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
                foreach ((string text, Color colour) in _values)
                {
                    sb.Append(text);
                    int next = current + text.Length;
                    highlightings.Add(new SyntaxHighlighting(new SourceCodePosition(0, current), new SourceCodePosition(0, next), colour));
                    current = next;
                }
                _cachedToolTip = sb.ToString();
                _cachedHighlightings = highlightings;
            }
            return (_cachedToolTip, _cachedHighlightings);
        }

        public HighlightedToolTipBuilder Add(string text, Color colour)
        {
            _values.Add((text, colour));
            _cachedToolTip = null;
            _cachedHighlightings = null;
            return this;
        }

        public HighlightedToolTipBuilder AddDefault(string text) => Add(text, _palette.DefaultTextColour);

        public HighlightedToolTipBuilder AddType(ITypeSymbol type)
        {
            // TODO: Use ToMinimalDisplayParts
            foreach (var part in type.ToDisplayParts(SymbolDisplayFormat.MinimallyQualifiedFormat))
            {
                string partName = part.ToString();
                if (partName == "<"
                    || partName == ">"
                    || partName == ","
                    || partName == "."
                    || partName == "?"
                    || partName == "["
                    || partName == "]"
                    || partName == "("
                    || partName == ")")
                {
                    Add(partName, _palette.DefaultTextColour);
                }
                else if (IsPredefinedType(partName))
                {
                    Add(partName, _palette.BlueKeywordColour);
                }
                else
                {
                    Add(partName, _palette.TypeColour);
                }
            }
            return this;
        }

        private bool IsPredefinedType(string name)
        {
            switch (name)
            {
                case "bool":
                case "byte":
                case "sbyte":
                case "char":
                case "decimal":
                case "double":
                case "float":
                case "int":
                case "uint":
                case "nint":
                case "nuint":
                case "long":
                case "ulong":
                case "short":
                case "ushort":
                case "void":
                case "object":
                case "string":
                case "dynamic":
                    return true;
            }
            return false;
        }
    }
}
