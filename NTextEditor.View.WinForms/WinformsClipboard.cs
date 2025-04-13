using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NTextEditor.View.Winforms
{
    public class WinformsClipboard : IClipboard
    {
        public string GetText() => Clipboard.GetText();

        public void SetText(string text) => Clipboard.SetText(text);
    }
}
