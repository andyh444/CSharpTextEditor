using NTextEditor.Languages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.View
{

    internal static class DrawingHelper
    {
        public static SizeF GetMonospaceCharacterSize(ICanvas canvas) => canvas.GetTextSize("A", false);

        public static Size DrawTextLine(ICanvas canvas, int lineIndex, string lineText, int x, int y, IReadOnlyCollection<SyntaxHighlighting>? highlightings, SyntaxPalette palette, int activeParameterIndex = -1)
        {
            if (highlightings == null
                || !TryGetStringsToDraw(lineText, lineIndex, activeParameterIndex, highlightings.Where(x => x.IsOnLine(lineIndex)).Distinct(new SyntaxHighlightingEqualityComparer()).ToList(), palette, out var stringsToDraw))
            {
                canvas.DrawText(lineText, [new ColourTextSpan(0, lineText.Length, palette.DefaultTextColour, false)], new Point(x, y), false);
                return new Size();
            }
            else
            {
                return canvas.DrawText(lineText, stringsToDraw, new Point(x, y), false);
            }
        }

        private static bool TryGetStringsToDraw(string originalLine, int lineIndex, int activeParameterIndex, IEnumerable<SyntaxHighlighting> highlightingsOnLine, SyntaxPalette palette, out List<ColourTextSpan> stringsToDraw)
        {
            int start = 0;
            int characterCount = 0;
            stringsToDraw = new List<ColourTextSpan>();
            foreach (SyntaxHighlighting highlighting in highlightingsOnLine)
            {
                bool bold = highlighting.ParameterIndex == activeParameterIndex;
                if (activeParameterIndex == -1)
                {
                    bold = false;
                }
                if (highlighting.Start.LineNumber == highlighting.End.LineNumber)
                {
                    if (highlighting.Start.ColumnNumber - start < 0)
                    {
                        return false;
                    }
                    stringsToDraw.Add(new ColourTextSpan(start, highlighting.Start.ColumnNumber - start, palette.DefaultTextColour, bold));
                    characterCount += stringsToDraw.Last().Count;

                    stringsToDraw.Add(new ColourTextSpan(highlighting.Start.ColumnNumber, highlighting.End.ColumnNumber - highlighting.Start.ColumnNumber, highlighting.Colour, bold));
                    characterCount += stringsToDraw.Last().Count;

                    start = highlighting.End.ColumnNumber;
                }
                else if (highlighting.Start.LineNumber == lineIndex)
                {
                    string before = originalLine.Substring(characterCount, highlighting.Start.ColumnNumber - characterCount);
                    stringsToDraw.Add(new ColourTextSpan(characterCount, highlighting.Start.ColumnNumber - characterCount, palette.DefaultTextColour, bold));

                    characterCount += stringsToDraw.Last().Count;

                    string highlightedText = originalLine.Substring(highlighting.Start.ColumnNumber);
                    stringsToDraw.Add(new ColourTextSpan(highlighting.Start.ColumnNumber, originalLine.Length - highlighting.Start.ColumnNumber, highlighting.Colour, bold));

                    characterCount += stringsToDraw.Last().Count;

                    start = originalLine.Length;
                }
                else if (highlighting.End.LineNumber == lineIndex)
                {
                    stringsToDraw.Add(new ColourTextSpan(0, highlighting.End.ColumnNumber, highlighting.Colour, bold));

                    characterCount += highlighting.End.ColumnNumber;

                    start = highlighting.End.ColumnNumber;
                }
                else
                {
                    stringsToDraw.Add(new ColourTextSpan(0, originalLine.Length, highlighting.Colour, bold));
                    characterCount += originalLine.Length;
                    start = originalLine.Length;
                }
            }
            if (start != originalLine.Length)
            {
                stringsToDraw.Add(new ColourTextSpan(start, originalLine.Length - start, palette.DefaultTextColour, false));
            }
            return true;
        }
    }
}
