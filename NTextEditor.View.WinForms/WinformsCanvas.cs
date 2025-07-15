using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NTextEditor.View.Winforms
{
    public class WinformsCanvas : ICanvas
    {
        public Graphics Graphics { get; }

        public Font Font { get; }

        public Size Size { get; }

        public WinformsCanvas(Graphics graphics, Size size, Font font)
        {
            Graphics = graphics;
            Size = size;
            Font = font;
        }

        public void Clear(Color backColour)
        {
            Graphics.Clear(backColour);
        }

        public void FillRectangle(Color fillColour, RectangleF rectangle)
        {
            using (Brush brush = new SolidBrush(fillColour))
            {
                Graphics.FillRectangle(brush, rectangle);
            }
        }

        public void DrawLine(Color lineColour, PointF point1, PointF point2)
        {
            using (Pen pen = new Pen(lineColour))
            {
                Graphics.DrawLine(pen, point1, point2);
            }
        }

        private TextFormatFlags GetTextFormatFlags(bool rightAlign)
        {
            if (rightAlign)
            {
                return TextFormatFlags.Right;
            }
            return TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
        }

        public Size DrawText(string text, List<ColourTextSpan> colourSpans, Point location, bool rightAlign)
        {
            TextFormatFlags flags = GetTextFormatFlags(rightAlign);

            int thisX = location.X;
            int height = 0;

            Font? boldFont = null;

            foreach (var span in colourSpans)
            {
                if (span.Count <= 0)
                {
                    continue;
                }

                if (span.Start < 0 || span.Start + span.Count > text.Length)
                {
                    continue; // Invalid span, skip it
                }

                bool isBold = span.Bold;
                Font thisFont;
                if (isBold)
                {
                    boldFont ??= new Font(Font, FontStyle.Bold);
                    thisFont = boldFont;
                }
                else
                {
                    thisFont = Font;
                }

                Point point = new Point(thisX, location.Y);
                string substring = text.Substring(span.Start, span.Count);

                TextRenderer.DrawText(
                    Graphics,
                    substring,
                    thisFont,
                    point,
                    span.Colour,
                    flags);

                Size subStringSize = TextRenderer.MeasureText(Graphics, substring, thisFont, new Size(), TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                height = Math.Max(height, subStringSize.Height);
                if (rightAlign)
                {
                    thisX -= subStringSize.Height;
                }
                else
                {
                    thisX += subStringSize.Width;
                }
                
                //}
            }
            boldFont?.Dispose();
            return new Size(thisX - location.X, height);
        }

        public void DrawText(string text, Color colour, Rectangle rectangle, bool rightAlign)
        {
            TextFormatFlags flags = GetTextFormatFlags(rightAlign);

            TextRenderer.DrawText(
                Graphics,
                text,
                Font,
                rectangle,
                colour,
                flags);
        }

        public void DrawSquigglyLine(Color colour, int startX, int endX, int y)
        {
            using (Pen p = new Pen(colour))
            {
                p.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                List<PointF> points = new List<PointF>();
                int ySign = 1;
                float increment = Font.Size / 3;
                float halfIncrement = increment / 2;
                for (float x = startX; x < endX; x += increment)
                {
                    points.Add(new PointF(x, y + halfIncrement * ySign));
                    ySign = -ySign;
                }
                if (points.Last().X != endX)
                {
                    points.Add(new PointF(endX, y));
                }
                Graphics.DrawLines(p, points.ToArray());
            }
        }

        public SizeF GetTextSize(string text, bool bold)
        {
            if (bold)
            {
                using (Font boldFont = new Font(Font, FontStyle.Bold))
                {
                    return TextRenderer.MeasureText(Graphics, text, boldFont, new Size(), TextFormatFlags.NoPadding);
                }
            }
            return TextRenderer.MeasureText(Graphics, text, Font, new Size(), TextFormatFlags.NoPadding);
        }

        public void DrawImage(ICanvasImage image, Point point)
        {
            image.DrawToCanvas(this, point);
        }
    }
}
