using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    internal class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
            :base()
        {
            DoubleBuffered = true;
        }
    }
}
