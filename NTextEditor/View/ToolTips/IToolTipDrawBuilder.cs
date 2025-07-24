using System;
using System.Collections.Generic;
using System.Drawing;

namespace NTextEditor.View.ToolTips
{
    public interface IToolTipDrawBuilder
    {
        Size Size { get; }

        void AddText(string fullText, ColourTextSpan span, bool bold);

        void AddImage(ICanvasImage image);
    }

    public static class ToolTipDrawBuilderExtensions
    {
        public static Size Add(this IToolTipDrawBuilder builder, IEnumerable<IToolTipElement> elements)
        {
            foreach (IToolTipElement element in elements)
            {
                element.AddToDrawBuilder(builder);
            }
            return builder.Size;
        }
    }

    public class CanvasToolTipDrawBuilder : IToolTipDrawBuilder
    {
        private int x;
        private int height;
        private ICanvas canvas;
        private readonly SyntaxPalette palette;

        public Size Size => new Size(x, height);

        public CanvasToolTipDrawBuilder(ICanvas canvas, SyntaxPalette palette)
        {
            this.canvas = canvas;
            this.palette = palette;
        }

        public void AddImage(ICanvasImage image)
        {
            canvas.DrawImage(image, new Point(x, 0));
            x += image.Width;
            height = Math.Max(height, image.Height);
        }

        public void AddText(string fullText, ColourTextSpan span, bool bold)
        {
            Size indexSize = canvas.DrawText(fullText, [new ColourTextSpan(0, fullText.Length, palette.DefaultTextColour, false)], new Point(x, 0), false);
            x += indexSize.Width;
            height = Math.Max(height, indexSize.Height);
        }
    }
}
