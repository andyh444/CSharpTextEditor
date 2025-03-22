using System.Drawing;

namespace CSharpTextEditor.View.Winforms
{
    public interface IToolTipContents
    {
        bool Cycle(int sign);

        Size Draw(Graphics g, Font font, SyntaxPalette palette);
    }
}
