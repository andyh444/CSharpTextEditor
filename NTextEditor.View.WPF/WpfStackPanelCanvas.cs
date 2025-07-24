using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NTextEditor.View.WPF
{
    internal class WpfStackPanelCanvas : ICanvas
    {
        private readonly StackPanel stackPanel;

        public Size Size => Size.Empty;

        public WpfStackPanelCanvas(StackPanel stackPanel)
        {
            this.stackPanel = stackPanel;
        }

        public void Clear(Color backColour)
        {
            stackPanel.Children.Clear();
            stackPanel.Background = new System.Windows.Media.SolidColorBrush(backColour.ToWpfColour());
        }

        public void DrawImage(ICanvasImage bitmap, Point point)
        {
            throw new NotImplementedException();
        }

        public void DrawLine(Color lineColour, PointF point1, PointF point2)
        {
            // Ignore
        }

        public void DrawSquigglyLine(Color colour, int startX, int endX, int y)
        {
            // Ignore
        }

        public Size DrawText(string text, List<ColourTextSpan> colourSpans, Point location, bool rightAlign)
        {
            throw new NotImplementedException();
        }

        public void DrawText(string text, Color colour, Rectangle rectangle, bool rightAlign)
        {
            // Ignore
        }

        public void FillRectangle(Color fillColour, RectangleF rectangle)
        {
            // Ignore
        }

        public SizeF GetTextSize(string text, bool bold)
        {
            throw new NotImplementedException();
        }
    }
}
