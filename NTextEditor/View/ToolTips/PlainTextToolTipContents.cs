using System.Collections.Generic;
using System.Drawing;

namespace NTextEditor.View.ToolTips
{
    public class PlainTextToolTipContents : IToolTipContents
    {
        private readonly string _text;

        public PlainTextToolTipContents(string text)
        {
            _text = text;
        }

        public bool Cycle(int sign)
        {
            return false;
        }

        public Size Draw(ICanvas canvas, IIconCache iconCache, SyntaxPalette palette)
        {
            Size s = canvas.DrawText(_text, [new ColourTextSpan(0, _text.Length, palette.DefaultTextColour, false)], new Point(0, 0), false);
            return Size.Truncate(s);
        }

        public bool Equals(IToolTipContents? other)
        {
            return other is PlainTextToolTipContents otherPlainText && _text == otherPlainText._text;
        }

        public IEnumerable<IToolTipElement> GetElements(IIconCache iconCache, SyntaxPalette palette)
        {
            return [new ToolTipTextElement(_text, new ColourTextSpan(0, _text.Length, palette.DefaultTextColour, false))];
        }
    }
}
