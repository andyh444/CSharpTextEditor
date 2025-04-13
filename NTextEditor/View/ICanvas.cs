using System.Drawing;

namespace NTextEditor.View
{
    public interface ICanvas
    {
        Size Size { get; }

        void Clear(Color backColour);

        void FillRectangle(Color fillColour, Rectangle rectangle);

        void DrawLine(Color lineColour, Point point1, Point point2);

        Size GetTextSize(string text);

        Size GetTextSizeBold(string text);

        void DrawText(string text, Color colour, Point location, bool rightAlign);

        void DrawTextBold(string text, Color colour, Point location, bool rightAlign);

        void DrawText(string text, Color colour, Rectangle rectangle, bool rightAlign);

        void DrawSquigglyLine(Color colour, int startX, int endX, int y);

        void DrawImage(ICanvasImage bitmap, Point point);
    }
}
