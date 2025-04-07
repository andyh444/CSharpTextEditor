using System.Drawing;

namespace CSharpTextEditor.View.Winforms
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

        public Size Draw(Graphics g, Font font, SyntaxPalette palette)
        {
            SizeF s = g.MeasureString(_text, font);
            using (Brush b = new SolidBrush(palette.DefaultTextColour))
            {
                g.DrawString(_text, font, b, 0, 0);
            }
            return Size.Truncate(s);
        }

        public bool Equals(IToolTipContents? other)
        {
            return other is PlainTextToolTipContents otherPlainText && _text == otherPlainText._text;
        }
    }
}
