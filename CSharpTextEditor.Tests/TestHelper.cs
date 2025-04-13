using NTextEditor.Source;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTextEditor;

namespace NTextEditor.Tests
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

        public static List<(SourceCodePosition?, SourceCodePosition)> GetPositionRanges(string textWithMarkup, out string textWithoutMarkup)
        {
            StringBuilder sourceTextBuilder = new StringBuilder();
            SourceCodePosition? openingBracket = null;

            SourceCodePosition currentPosition = new SourceCodePosition(0, 0);
            Queue<char> characters = new Queue<char>(textWithMarkup);
            List<(SourceCodePosition?, SourceCodePosition)> ranges = new List<(SourceCodePosition?, SourceCodePosition)>();
            while (characters.Count > 0)
            {
                char c = characters.Dequeue();
                switch (c)
                {
                    case '\r':
                        if (characters.Count > 0
                            && characters.Peek() == '\n')
                        {
                            characters.Dequeue();
                            currentPosition = new SourceCodePosition(currentPosition.LineNumber + 1, 0);
                            sourceTextBuilder.AppendLine();
                        }
                        else
                        {
                            throw new CSharpTextEditorException($"Unexpected \\r at {currentPosition}");
                        }
                        break;
                    case '\n':
                        currentPosition = new SourceCodePosition(currentPosition.LineNumber + 1, 0);
                        sourceTextBuilder.AppendLine();
                        break;
                    case '[':
                        if (openingBracket == null)
                        {
                            openingBracket = currentPosition;
                        }
                        else
                        {
                            // previous was just a caret; no selection
                            ranges.Add((null, openingBracket.Value));
                            openingBracket = currentPosition;
                        }
                        break;
                    case ']':
                        if (openingBracket != null)
                        {
                            ranges.Add((openingBracket, currentPosition));
                            openingBracket = null;
                        }
                        else
                        {
                            throw new CSharpTextEditorException($"Unexpected closing bracket at {currentPosition}");
                        }
                        break;
                    default:
                        sourceTextBuilder.Append(c);
                        currentPosition = new SourceCodePosition(currentPosition.LineNumber, currentPosition.ColumnNumber + 1);
                        break;
                }
            }
            if (openingBracket != null)
            {
                ranges.Add((null, openingBracket.Value));
            }
            textWithoutMarkup = sourceTextBuilder.ToString();
            return ranges;
        }

        private static string Replace(this string original, string replacement, int index)
        {
            char[] originalChars = original.ToCharArray();
            return new string(originalChars.Take(index).Concat(replacement).Concat(originalChars.Skip(index + 1)).ToArray());
        }
    }
}
