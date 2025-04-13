using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.Utility
{
    internal static class StringExtensions
    {
        internal static IEnumerable<string> SplitIntoLines(this string text)
        {
            // do it like this as StringReader.ReadLine doesn't do what is expected if there is a line break at the end of the string
            return text.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
        }
    }
}
