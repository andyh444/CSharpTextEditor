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
        public static Size GetMonospaceCharacterSize(Font font, Graphics g) => GetStringSize("A", font, g);

        public static Size GetStringSize(string text, Font font, Graphics g)
        {
            Size size = TextRenderer.MeasureText(g, text, font, new Size(), TextFormatFlags.NoPadding);
            return new Size(size.Width, size.Height);
        }

        public static void DrawLine(ICanvas canvas, int lineIndex, string lineText, int y, IReadOnlyCollection<SyntaxHighlighting>? highlightings, Func<int, int> getXCoordinate, SyntaxPalette palette, int activeParameterIndex = -1)
        {
            if (highlightings == null
                || !TryGetStringsToDraw(lineText, lineIndex, highlightings.Where(x => x.IsOnLine(lineIndex)).Distinct(new SyntaxHighlightingEqualityComparer()).ToList(), palette, out var stringsToDraw))
            {
                canvas.DrawText(lineText, palette.DefaultTextColour, new Point(getXCoordinate(0), y), false);
            }
            else
            {
                foreach ((string text, int characterOffset, Color colour, int parameterIndex) in stringsToDraw)
                {
                    bool isBold = activeParameterIndex != -1 && activeParameterIndex == parameterIndex;
                    if (isBold)
                    {
                        canvas.DrawTextBold(text, colour, new Point(getXCoordinate(characterOffset), y), false);
                    }
                    else
                    {
                        canvas.DrawText(text, colour, new Point(getXCoordinate(characterOffset), y), false);
                    }
                }
            }
        }

        public static bool TryGetStringsToDraw(string originalLine, int lineIndex, IEnumerable<SyntaxHighlighting> highlightingsOnLine, SyntaxPalette palette, out List<(string text, int characterOffset, Color colour, int parameterIndex)> stringsToDraw)
        {
            int start = 0;
            int characterCount = 0;
            stringsToDraw = new List<(string text, int characterOffset, Color colour, int parameterIndex)>();
            foreach (SyntaxHighlighting highlighting in highlightingsOnLine)
            {
                if (highlighting.Start.LineNumber == highlighting.End.LineNumber)
                {
                    if (highlighting.Start.ColumnNumber - start < 0)
                    {
                        return false;
                    }
                    string before = originalLine.Substring(start, highlighting.Start.ColumnNumber - start);
                    stringsToDraw.Add((before, characterCount, palette.DefaultTextColour, highlighting.ParameterIndex));

                    characterCount += before.Length;

                    string highlightedText = originalLine.Substring(highlighting.Start.ColumnNumber, highlighting.End.ColumnNumber - highlighting.Start.ColumnNumber);
                    stringsToDraw.Add((highlightedText, characterCount, highlighting.Colour, highlighting.ParameterIndex));

                    characterCount += highlightedText.Length;

                    start = highlighting.End.ColumnNumber;
                }
                else if (highlighting.Start.LineNumber == lineIndex)
                {
                    string before = originalLine.Substring(characterCount, highlighting.Start.ColumnNumber - characterCount);
                    stringsToDraw.Add((before, characterCount, palette.DefaultTextColour, highlighting.ParameterIndex));

                    characterCount += before.Length;

                    string highlightedText = originalLine.Substring(highlighting.Start.ColumnNumber);
                    stringsToDraw.Add((highlightedText, characterCount, highlighting.Colour, highlighting.ParameterIndex));

                    characterCount += highlightedText.Length;

                    start = originalLine.Length;
                }
                else if (highlighting.End.LineNumber == lineIndex)
                {
                    string highlightedText = originalLine.Substring(0, highlighting.End.ColumnNumber);
                    stringsToDraw.Add((highlightedText, characterCount, highlighting.Colour, highlighting.ParameterIndex));

                    characterCount += highlightedText.Length;

                    start = highlighting.End.ColumnNumber;
                }
                else
                {
                    stringsToDraw.Add((originalLine, 0, highlighting.Colour, highlighting.ParameterIndex));
                    characterCount += originalLine.Length;
                    start = originalLine.Length;
                }
            }
            if (start != originalLine.Length)
            {
                stringsToDraw.Add((originalLine.Substring(start), characterCount, palette.DefaultTextColour, -1));
            }
            return true;
        }
    }
}
