using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.View
{
    public interface IIconCache
    {
        ICanvasImage? GetIcon(SymbolType symbolType);
    }
}
