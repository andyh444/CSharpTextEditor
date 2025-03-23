using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.View
{
    public interface IClipboard
    {
        string GetText();

        void SetText(string text);
    }
}
