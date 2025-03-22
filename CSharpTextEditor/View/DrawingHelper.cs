using CSharpTextEditor.Languages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpTextEditor.View
{
    internal static class DrawingHelper
    {
        private class ColouredSubString(string subString, int characterOffset, Color colour, int? parameterIndex)
        {
            public string SubString { get; } = subString;
            public int CharacterOffset { get; } = characterOffset;
            public Color Colour { get; } = colour;
            public int? ParameterIndex { get; } = parameterIndex;
        }

        public static Size GetMonospaceCharacterSize(ICanvas canvas) => canvas.GetTextSize("A");

        public static void DrawTextLine(ICanvas canvas, int lineIndex, string lineText, int x, int y, IReadOnlyCollection<SyntaxHighlighting>? highlightings, SyntaxPalette palette, int activeParameterIndex = -1)
        {
            if (highlightings == null
                || !TryGetStringsToDraw(lineText, lineIndex, highlightings.Where(x => x.IsOnLine(lineIndex)).Distinct(new SyntaxHighlightingEqualityComparer()).ToList(), palette, out var stringsToDraw))
            {
                canvas.DrawText(lineText, palette.DefaultTextColour, new Point(x, y), false);
            }
            else
            {
                foreach (ColouredSubString substring in stringsToDraw)
                {
                    bool isBold = activeParameterIndex != -1 && activeParameterIndex == substring.ParameterIndex;
                    Point point = new Point(x, y);
                    if (isBold)
                    {
                        canvas.DrawTextBold(substring.SubString, substring.Colour, point, false);
                        x += canvas.GetTextSizeBold(substring.SubString).Width;
                    }
                    else
                    {
                        canvas.DrawText(substring.SubString, substring.Colour, point, false);
                        x += canvas.GetTextSize(substring.SubString).Width;
                    }
                }
            }
        }

        private static bool TryGetStringsToDraw(string originalLine, int lineIndex, IEnumerable<SyntaxHighlighting> highlightingsOnLine, SyntaxPalette palette, out List<ColouredSubString> stringsToDraw)
        {
            int start = 0;
            int characterCount = 0;
            stringsToDraw = new List<ColouredSubString>();
            foreach (SyntaxHighlighting highlighting in highlightingsOnLine)
            {
                if (highlighting.Start.LineNumber == highlighting.End.LineNumber)
                {
                    if (highlighting.Start.ColumnNumber - start < 0)
                    {
                        return false;
                    }
                    string before = originalLine.Substring(start, highlighting.Start.ColumnNumber - start);
                    stringsToDraw.Add(new ColouredSubString(before, characterCount, palette.DefaultTextColour, highlighting.ParameterIndex));

                    characterCount += before.Length;

                    string highlightedText = originalLine.Substring(highlighting.Start.ColumnNumber, highlighting.End.ColumnNumber - highlighting.Start.ColumnNumber);
                    stringsToDraw.Add(new ColouredSubString(highlightedText, characterCount, highlighting.Colour, highlighting.ParameterIndex));

                    characterCount += highlightedText.Length;

                    start = highlighting.End.ColumnNumber;
                }
                else if (highlighting.Start.LineNumber == lineIndex)
                {
                    string before = originalLine.Substring(characterCount, highlighting.Start.ColumnNumber - characterCount);
                    stringsToDraw.Add(new ColouredSubString(before, characterCount, palette.DefaultTextColour, highlighting.ParameterIndex));

                    characterCount += before.Length;

                    string highlightedText = originalLine.Substring(highlighting.Start.ColumnNumber);
                    stringsToDraw.Add(new ColouredSubString(highlightedText, characterCount, highlighting.Colour, highlighting.ParameterIndex));

                    characterCount += highlightedText.Length;

                    start = originalLine.Length;
                }
                else if (highlighting.End.LineNumber == lineIndex)
                {
                    string highlightedText = originalLine.Substring(0, highlighting.End.ColumnNumber);
                    stringsToDraw.Add(new ColouredSubString(highlightedText, characterCount, highlighting.Colour, highlighting.ParameterIndex));

                    characterCount += highlightedText.Length;

                    start = highlighting.End.ColumnNumber;
                }
                else
                {
                    stringsToDraw.Add(new ColouredSubString(originalLine, 0, highlighting.Colour, highlighting.ParameterIndex));
                    characterCount += originalLine.Length;
                    start = originalLine.Length;
                }
            }
            if (start != originalLine.Length)
            {
                stringsToDraw.Add(new ColouredSubString(originalLine.Substring(start), characterCount, palette.DefaultTextColour, -1));
            }
            return true;
        }
    }
}
