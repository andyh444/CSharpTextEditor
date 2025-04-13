using CSharpTextEditor.Languages;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.Languages.CSharp
{
    internal static class HighlightedToolTipBuilderExtensions
    {
        public static HighlightedToolTipBuilder AddType(this HighlightedToolTipBuilder builder, ITypeSymbol type, int parameterIndex = -1, bool fullyQualified = false)
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
                        or SymbolDisplayPartKind.InterfaceName => builder.Palette.TypeColour,

                    SymbolDisplayPartKind.Keyword => builder.Palette.BlueKeywordColour,
                    _ => builder.Palette.DefaultTextColour
                };
                builder.Add(part.ToString(), colour, parameterIndex);
            }
            return builder;
        }
    }
}
