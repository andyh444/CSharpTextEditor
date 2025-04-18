using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.View.WPF
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        private static extern uint GetCaretBlinkTime();

        public static TimeSpan CaretBlinkTime => TimeSpan.FromMilliseconds(GetCaretBlinkTime());
    }
}
