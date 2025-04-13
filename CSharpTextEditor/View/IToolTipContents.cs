using System;
using System.Drawing;

namespace CSharpTextEditor.View
{
    public interface IToolTipContents : IEquatable<IToolTipContents>
    {
        bool Cycle(int sign);

        Size Draw(Graphics g, Font font, SyntaxPalette palette);
    }
}
