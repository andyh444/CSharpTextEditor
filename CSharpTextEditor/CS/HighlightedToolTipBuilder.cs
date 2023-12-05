using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.CS
{
    internal class HighlightedToolTipBuilder
    {
        private readonly List<(string text, Color colour)> _values;
        private readonly SyntaxPalette _palette;

        public HighlightedToolTipBuilder(SyntaxPalette palette)
        {
            _values = new List<(string text, Color colour)>();
            _palette = palette;
        }

        public (string toolTipText, List<SyntaxHighlighting> highlightings) ToToolTip()
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
            return (sb.ToString(), highlightings);
        }

        public HighlightedToolTipBuilder Add(string text, Color colour)
        {
            _values.Add((text, colour));
            return this;
        }

        public HighlightedToolTipBuilder AddDefault(string text) => Add(text, _palette.DefaultTextColour);

        public HighlightedToolTipBuilder AddTypeInfo(ITypeSymbol type)
        {
            foreach (var part in type.ToDisplayParts())
            {
                string partName = part.ToString();
                if (partName == "<"
                    || partName == ">"
                    || partName == ","
                    || partName == "."
                    || partName == "?")
                {
                    _values.Add((partName, _palette.DefaultTextColour));
                }
                else if (IsPredefinedType(partName))
                {
                    _values.Add((partName, _palette.BlueKeywordColour));
                }
                else
                {
                    _values.Add((partName, _palette.TypeColour));
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
                    return true;
            }
            return false;
        }
    }
}
