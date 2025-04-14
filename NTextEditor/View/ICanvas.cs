using System.Collections.Generic;
using System.Drawing;

namespace NTextEditor.View
{
    public interface ICanvas
    {
        Size Size { get; }

        void Clear(Color backColour);

        void FillRectangle(Color fillColour, RectangleF rectangle);

        void DrawLine(Color lineColour, PointF point1, PointF point2);

        SizeF GetTextSize(string text, bool bold);

        Size DrawText(string text, List<ColourTextSpan> colourSpans, Point location, bool rightAlign);

        void DrawText(string text, Color colour, Rectangle rectangle, bool rightAlign);

        void DrawSquigglyLine(Color colour, int startX, int endX, int y);

        void DrawImage(ICanvasImage bitmap, Point point);
    }
}
