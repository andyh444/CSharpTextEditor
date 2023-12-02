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
        public Color BackColour { get; set; }

        public Color DefaultTextColour { get; set; }

        public Color PurpleKeywordColour { get; set; }

        public Color BlueKeywordColour { get; set; }

        public Color TypeColour { get; set; }

        public Color LocalVariableColour { get; set; }

        public Color MethodColour { get; set; }

        public Color StringLiteralColour { get; set; }

        public Color CommentColour { get; set; }

        public Color SelectionColour { get; set; }

        public Color DefocusedSelectionColour { get; set; }

        public static SyntaxPalette GetLightModePalette() => new SyntaxPalette
        {
            BackColour = Color.White,
            DefaultTextColour = Color.Black,
            PurpleKeywordColour = Color.Purple,
            BlueKeywordColour = Color.Blue,
            TypeColour = Color.SteelBlue,
            LocalVariableColour = Color.FromArgb(31, 55, 127),
            MethodColour = Color.FromArgb(116, 83, 31),
            StringLiteralColour = Color.DarkRed,
            CommentColour = Color.Green,
            SelectionColour = Color.LightBlue,
            DefocusedSelectionColour = Color.LightGray
        };

        public static SyntaxPalette GetDarkModePalette() => new SyntaxPalette
        {
            BackColour = Color.FromArgb(30, 30, 30),
            DefaultTextColour = Color.White,
            PurpleKeywordColour = Color.FromArgb(216, 160, 223),
            BlueKeywordColour = Color.FromArgb(86, 156, 214),
            TypeColour = Color.FromArgb(73, 182, 160),
            LocalVariableColour = Color.FromArgb(156, 220, 254),
            MethodColour = Color.FromArgb(218, 218, 168),
            StringLiteralColour = Color.FromArgb(214, 157, 133),
            CommentColour = Color.Green,
            SelectionColour = Color.FromArgb(38, 79, 120),
            DefocusedSelectionColour = Color.FromArgb(52, 52, 52)
        };
    }
}
