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

        public Color ClassColour { get; init; }

        public Color MethodColour { get; init; }

        public static SyntaxPalette GetLightModePalette() => new SyntaxPalette
        {
            PurpleKeywordColour = Color.Purple,
            BlueKeywordColour = Color.Blue,
            ClassColour = Color.SteelBlue,
            MethodColour = Color.FromArgb(136, 108, 64)
        };
    }
}
