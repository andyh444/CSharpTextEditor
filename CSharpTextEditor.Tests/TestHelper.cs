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
            (startIndex, endIndex) = GetBracketPositionsAndRemove(lineOfText, string.Empty, out lineWithRemovedMarkup).First();
        }

        public static IEnumerable<(int, int)> GetBracketPositionsAndRemove(string lineOfText, string replacement, out string lineWithRemovedMarkup)
        {
            List<(int, int)> positions = new List<(int, int)>();
            lineWithRemovedMarkup = lineOfText;
            while (true)
            {
                int startIndex = lineWithRemovedMarkup.IndexOf('[');
                if (startIndex == -1)
                {
                    break;
                }
                lineWithRemovedMarkup = lineWithRemovedMarkup.Replace(replacement, startIndex);
                int endIndex = lineWithRemovedMarkup.IndexOf("]");
                if (endIndex == -1)
                {
                    positions.Add((startIndex, endIndex));
                    continue;
                }
                lineWithRemovedMarkup = lineWithRemovedMarkup.Replace(replacement, endIndex);
                positions.Add((startIndex, endIndex));
            }
            return positions;
        }

        private static string Replace(this string original, string replacement, int index)
        {
            char[] originalChars = original.ToCharArray();
            return new string(originalChars.Take(index).Concat(replacement).Concat(originalChars.Skip(index + 1)).ToArray());
        }
    }
}
