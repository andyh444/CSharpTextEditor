using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.View.WPF
{
    internal static class SystemDrawingExtensions
    {
        public static SKColor ToSkiaColour(this Color colour)
        {
            return new SKColor(colour.R, colour.G, colour.B, colour.A);
        }

        public static SKPoint ToSkiaPoint(this Point point)
        {
            return new SKPoint(point.X, point.Y);
        }

        public static SKPoint ToSkiaPoint(this PointF point)
        {
            return new SKPoint(point.X, point.Y);
        }

        public static SKRect ToSkiaRect(this Rectangle rectangle)
        {
            return new SKRect(rectangle.X, rectangle.Y, rectangle.Right, rectangle.Bottom);
        }

        public static SKRect ToSkiaRect(this RectangleF rectangle)
        {
            return new SKRect(rectangle.X, rectangle.Y, rectangle.Right, rectangle.Bottom);
        }

        public static System.Windows.Media.Color ToWpfColour(this Color colour)
        {
            return System.Windows.Media.Color.FromArgb(
                colour.A,
                colour.R,
                colour.G,
                colour.B);
        }
    }
}
