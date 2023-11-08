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

        public Color LocalVariableColour { get; init; }

        public Color MethodColour { get; init; }

        public Color StringLiteralColour { get; init; }

        public Color CommentColour { get; init; }

        public static SyntaxPalette GetLightModePalette() => new SyntaxPalette
        {
            PurpleKeywordColour = Color.Purple,
            BlueKeywordColour = Color.Blue,
            TypeColour = Color.SteelBlue,
            LocalVariableColour = Color.FromArgb(31, 55, 127),
            MethodColour = Color.FromArgb(116, 83, 31),
            StringLiteralColour = Color.DarkRed,
            CommentColour = Color.Green,
        };
    }
}
