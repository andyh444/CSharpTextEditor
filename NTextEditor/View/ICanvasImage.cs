using System.Drawing;

namespace NTextEditor.View
{
    public interface ICanvasImage
    {
        int Width { get; }

        int Height { get; }

        void DrawToCanvas(ICanvas canvas, Point point);
    }
}
