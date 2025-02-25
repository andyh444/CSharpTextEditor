using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.Utility
{
    internal static class StringExtensions
    {
        internal static IEnumerable<string> SplitIntoLines(this string text)
        {
            using (StringReader sr = new StringReader(text))
            {
                string? current;
                do
                {
                    current = sr.ReadLine();
                    if (current != null)
                    {
                        yield return current;
                    }
                }
                while (current != null);
            }
        }
    }
}
