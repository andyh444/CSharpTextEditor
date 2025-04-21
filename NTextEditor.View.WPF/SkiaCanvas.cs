using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

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

        public void DrawLine(Color lineColour, PointF point1, PointF point2)
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
            using var paint = new SKPaint
            {
                Color = colour.ToSkiaColour(),
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };
            List<SKPoint> points = new List<SKPoint>();
            using var path = new SKPath();
            int ySign = 1;
            float increment = Font.Size / 3;
            float halfIncrement = increment / 2;
            for (float x = startX; x < endX; x += increment)
            {
                points.Add(new SKPoint(x, y + halfIncrement * ySign));
                ySign = -ySign;
            }
            if (points.Last().X != endX)
            {
                points.Add(new SKPoint(endX, y));
            }
            path.AddPoly(points.ToArray(), false);
            Canvas.DrawPath(path, paint);
        }

        public void DrawText(string text, Color colour, Point location, bool rightAlign)
        {
            using var paint = new SKPaint
            {
                Color = colour.ToSkiaColour(),
                StrokeWidth = 0,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
            };

            SKTextBlobBuilder builder = new SKTextBlobBuilder();
            SKTextBlob.Create(new ReadOnlySpan<char>(text.ToCharArray()), Font);

            SKTextAlign textAlign = SKTextAlign.Left; //rightAlign ? SKTextAlign.Right : SKTextAlign.Left;

            Canvas.DrawText(text,
                (float)Math.Floor(location.X + 0.5f),
                location.Y - Font.Metrics.Ascent,
                textAlign,
                Font,
                paint);
        }

        public Size DrawText(string text, List<ColourTextSpan> colourSpans, Point location, bool rightAlign)
        {
            var textSpan = text.AsSpan();
            float thisX = 0;
            float y = location.Y - Font.Metrics.Ascent;

            using var paint = new SKPaint
            {
                StrokeWidth = 0,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
            };

            foreach (var span in colourSpans)
            {
                if (span.Count == 0)
                {
                    continue;
                }
                SKTextBlobBuilder builder = new SKTextBlobBuilder();

                var subString = textSpan.Slice(span.Start, span.Count);

                ushort[] glyphs = Font.GetGlyphs(subString);
                var glyphWidths = Font.GetGlyphWidths(glyphs);
                SKPositionedRunBuffer run = builder.AllocatePositionedRun(Font, glyphs.Length);

                int i = 0;
                foreach (var glyph in glyphs)
                {
                    run.Glyphs[i] = glyph;
                    run.Positions[i] = new SKPoint(thisX, 0);

                    thisX += glyphWidths[i];

                    i++;
                }
                paint.Color = span.Colour.ToSkiaColour();

                var blob = builder.Build();

                Canvas.DrawText(blob, location.X, location.Y - Font.Metrics.Ascent, paint);
            }
            return new Size((int)(thisX - location.X), 0);
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

        public void FillRectangle(Color fillColour, RectangleF rectangle)
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

        public SizeF GetTextSize(string text, bool bold)
        {
            return new SizeF(
                Font.GetGlyphWidths(text).Sum(),
                Math.Abs(Font.Metrics.Descent - Font.Metrics.Ascent));

            // TODO: Bold
            /*var width = Font.MeasureText(text, out var bounds);

            // don't use the width from bounds as this doesn't work for white space
            return new Size((int)Math.Round(width), (int)Math.Abs(Font.Metrics.Descent - Font.Metrics.Ascent));*/
        }
    }
}
