using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    public class SyntaxPalette
    {
        public Color PurpleKeywordColour { get; set; }

        public Color BlueKeywordColour { get; set; }

        public Color TypeColour { get; set; }

        public Color LocalVariableColour { get; set; }

        public Color MethodColour { get; set; }

        public Color StringLiteralColour { get; set; }

        public Color CommentColour { get; set; }

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
