using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    public class SyntaxPalette
    {
        public Color PurpleKeywordColour { get; init; }

        public Color BlueKeywordColour { get; init; }

        public Color TypeColour { get; init; }

        public Color MethodColour { get; init; }

        public Color StringLiteralColour { get; init; }

        public Color CommentColour { get; init; }

        public static SyntaxPalette GetLightModePalette() => new SyntaxPalette
        {
            PurpleKeywordColour = Color.Purple,
            BlueKeywordColour = Color.Blue,
            TypeColour = Color.SteelBlue,
            MethodColour = Color.FromArgb(136, 108, 64),
            StringLiteralColour = Color.DarkRed,
            CommentColour = Color.Green,
        };
    }
}
