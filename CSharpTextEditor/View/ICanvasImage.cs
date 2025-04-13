using System.Drawing;

namespace CSharpTextEditor.View
{
    public interface ICanvasImage
    {
        int Width { get; }

        int Height { get; }

        void DrawToCanvas(ICanvas canvas, Point point);
    }
}
