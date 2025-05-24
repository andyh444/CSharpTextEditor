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

        public static (string, IReadOnlyList<int> cumulativeLineLengths) ToText(this IEnumerable<string> lines)
        {
            List<int> lengths = new List<int>();
            int previous = 0;
            int newLineLength = Environment.NewLine.Length;
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (string line in lines)
            {
                if (!first)
                {
                    sb.AppendLine();
                }
                first = false;
                sb.Append(line);
                int cumulativeSum = previous + line.Length + newLineLength;
                lengths.Add(cumulativeSum);
                previous = cumulativeSum;
            }
            return (sb.ToString(), lengths);
        }
    }
}
