using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.View.WPF
{
    internal class SkiaCanvas : ICanvas
    {
        public SKCanvas Canvas { get; }

        public Size Size { get; }

        public SKFont Font { get; }

        public SkiaCanvas(SKCanvas canvas, Size size, SKFont font)
        {
            Canvas = canvas;
            Size = size;
            Font = font;
        }

        public void Clear(Color backColour)
        {
            Canvas.Clear(backColour.ToSkiaColour());
        }

        public void DrawImage(ICanvasImage bitmap, Point point)
        {
            // TODO
        }

        public void DrawLine(Color lineColour, Point point1, Point point2)
        {
            using var paint = new SKPaint
            {
                Color = lineColour.ToSkiaColour(),
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };
            Canvas.DrawLine(
                point1.ToSkiaPoint(),
                point2.ToSkiaPoint(),
                paint);
        }

        public void DrawSquigglyLine(Color colour, int startX, int endX, int y)
        {
            // TODO
        }

        public void DrawText(string text, Color colour, Point location, bool rightAlign)
        {
            using var paint = new SKPaint
            {
                Color = colour.ToSkiaColour(),
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
            };
            
            SKTextAlign textAlign = rightAlign ? SKTextAlign.Right : SKTextAlign.Left;

            Canvas.DrawText(text,
                location.X,
                location.Y - Font.Metrics.Ascent,
                textAlign,
                Font,
                paint);
        }

        public void DrawText(string text, Color colour, Rectangle rectangle, bool rightAlign)
        {
            DrawText(text, colour, rectangle.Location, rightAlign);
        }

        public void DrawTextBold(string text, Color colour, Point location, bool rightAlign)
        {
            // TODO
            DrawText(text, colour, location, rightAlign);
        }

        public void FillRectangle(Color fillColour, Rectangle rectangle)
        {
            using var paint = new SKPaint
            {
                Color = fillColour.ToSkiaColour(),
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            Canvas.DrawRect(
                rectangle.ToSkiaRect(),
                paint);
        }

        public Size GetTextSize(string text)
        {
            var width = Font.MeasureText(text, out var bounds);

            // don't use the width from bounds as this doesn't work for white space
            return new Size((int)width, (int)Math.Abs(Font.Metrics.Descent - Font.Metrics.Ascent));
        }

        public Size GetTextSizeBold(string text)
        {
            // TODO
            return GetTextSize(text);
        }
    }
}
