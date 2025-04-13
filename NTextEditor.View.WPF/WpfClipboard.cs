using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.View.WPF
{
    internal class WpfClipboard : IClipboard
    {
        public string GetText()
        {
            if (System.Windows.Clipboard.ContainsText())
            {
                return System.Windows.Clipboard.GetText();
            }
            return string.Empty;
        }

        public void SetText(string text)
        {
            System.Windows.Clipboard.SetText(text);
        }
    }
}
