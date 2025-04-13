using CSharpTextEditor.View;
using System.Drawing;

namespace CSharpTextEditor.Source
{
    internal interface ISourceCodeListener
    {
        void TextChanged();

        void CursorsChanged();

        void ShowMethodToolTip(SyntaxPalette palette, IToolTipContents toolTipContents, Point point);

        void HideMethodToolTip();

        void ShowHoverToolTip(SyntaxPalette palette, IToolTipContents toolTipContents, Point point);

        void HideHoverToolTip();
    }
}
