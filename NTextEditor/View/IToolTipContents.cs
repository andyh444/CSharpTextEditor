using System;
using System.Drawing;

namespace NTextEditor.View
{
    public interface IToolTipContents : IEquatable<IToolTipContents>
    {
        bool Cycle(int sign);

        Size Draw(ICanvas canvas, IIconCache iconCache, SyntaxPalette palette);
    }
}
