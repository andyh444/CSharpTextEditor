using NTextEditor.View.ToolTips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace NTextEditor.View.WPF
{
    internal class StackPanelToolTipDrawBuilder : IToolTipDrawBuilder
    {
        private StackPanel _stackPanel;
        private TextBlock? _currentTextBlock;

        public System.Drawing.Size Size => System.Drawing.Size.Empty;

        public StackPanelToolTipDrawBuilder(StackPanel stackPanel)
        {
            _stackPanel = stackPanel;
            _stackPanel.Orientation = Orientation.Horizontal;
        }

        public void AddImage(ICanvasImage image)
        {
            if (image is WpfCanvasImage wpfImage)
            {
                _currentTextBlock = null;
                _stackPanel.Children.Add(new Image() { Source = wpfImage.Image });
            }
        }

        public void AddText(string fullText, ColourTextSpan span, bool bold)
        {
            if (_currentTextBlock == null)
            {
                _currentTextBlock = new TextBlock();
                _stackPanel.Children.Add(_currentTextBlock);
            }
            _currentTextBlock.Inlines.Add(new Run(fullText.Substring(span.Start, span.Count)) { Foreground = new SolidColorBrush(span.Colour.ToWpfColour()) });
        }
    }
}
