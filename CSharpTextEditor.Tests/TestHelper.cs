using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.Tests
{
    public static class TestHelper
    {
        public static void GetBracketPositionsAndRemove(string lineOfText, out string lineWithRemovedMarkup, out int startIndex, out int endIndex)
        {
            lineWithRemovedMarkup = lineOfText;
            startIndex = lineWithRemovedMarkup.IndexOf('[');
            lineWithRemovedMarkup = lineWithRemovedMarkup.Replace("[", string.Empty);
            endIndex = lineWithRemovedMarkup.IndexOf("]");
            lineWithRemovedMarkup = lineWithRemovedMarkup.Replace("]", string.Empty);
        }
    }
}
