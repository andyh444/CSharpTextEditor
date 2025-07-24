using System;
using System.Collections.Generic;
using System.Drawing;

namespace NTextEditor.View.ToolTips
{
    public interface IToolTipContents : IEquatable<IToolTipContents>
    {
        bool Cycle(int sign);

        Size Draw(ICanvas canvas, IIconCache iconCache, SyntaxPalette palette);

        IEnumerable<IToolTipElement> GetElements(IIconCache iconCache, SyntaxPalette palette);
    }
}
