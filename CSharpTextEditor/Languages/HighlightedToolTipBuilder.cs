using CSharpTextEditor.Source;
using CSharpTextEditor.View;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.Languages
{
    internal class HighlightedToolTipBuilder : IToolTipSource
    {
        private readonly List<(string text, Color colour, int parameterIndex)> _values;
        private readonly SyntaxPalette _palette;
        private string? _cachedToolTip;
        private List<SyntaxHighlighting>? _cachedHighlightings;

        public HighlightedToolTipBuilder(SyntaxPalette palette)
        {
            _values = new List<(string text, Color colour, int parameterIndex)>();
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

        public HighlightedToolTipBuilder AddDefault(string text, int parameterIndex = -1) => Add(text, _palette.DefaultTextColour, parameterIndex);

        public HighlightedToolTipBuilder AddType(ITypeSymbol type, int parameterIndex = -1, bool fullyQualified = false)
        {
            // TODO: Use ToMinimalDisplayParts
            foreach (var part in type.ToDisplayParts(fullyQualified ? null : SymbolDisplayFormat.MinimallyQualifiedFormat))
            {
                Color colour = part.Kind switch
                {
                    SymbolDisplayPartKind.ClassName
                        or SymbolDisplayPartKind.DelegateName
                        or SymbolDisplayPartKind.EnumName
                        or SymbolDisplayPartKind.StructName
                        or SymbolDisplayPartKind.InterfaceName => _palette.TypeColour,

                    SymbolDisplayPartKind.Keyword => _palette.BlueKeywordColour,
                    _ => _palette.DefaultTextColour
                };       
                Add(part.ToString(), colour, parameterIndex);
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
