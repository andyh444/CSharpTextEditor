using NTextEditor.View;
using NTextEditor.View.ToolTips;
using System.Drawing;

namespace NTextEditor.Source
{
    public interface ISourceCodeListener
    {
        void TextChanged();

        void CursorsChanged();

        void ShowMethodToolTip(SyntaxPalette palette, IToolTipContents toolTipContents, Point point);

        void HideMethodToolTip();

        void ShowHoverToolTip(SyntaxPalette palette, IToolTipContents toolTipContents, Point point);

        void HideHoverToolTip();
    }
}
