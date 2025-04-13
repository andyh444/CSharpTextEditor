﻿
using System.Drawing;

namespace CSharpTextEditor.View
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
            Size s = canvas.GetTextSize(_text);
            canvas.DrawText(_text, palette.DefaultTextColour, new Point(0, 0), false);
            return Size.Truncate(s);
        }

        public bool Equals(IToolTipContents? other)
        {
            return other is PlainTextToolTipContents otherPlainText && _text == otherPlainText._text;
        }
    }
}
