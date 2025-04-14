using System.Drawing;

namespace NTextEditor.View
{
    public class ColourTextSpan(int start, int count, Color colour, bool bold)
    {
        public int Start { get; } = start;
        public int Count { get; } = count;
        public Color Colour { get; } = colour;
        public bool Bold { get; } = bold;
    }
}
