﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpTextEditor.Winforms
{
    internal static class DrawingHelper
    {
        public static Size GetMonospaceCharacterSize(Font font, Graphics g) => GetStringSize("A", font, g);

        public static Size GetStringSize(string text, Font font, Graphics g)
        {
            Size size = TextRenderer.MeasureText(g, text, font, new Size(), TextFormatFlags.NoPadding);
            return new Size(size.Width, size.Height);
        }

        public static void DrawLine(Graphics g, int lineIndex, string lineText, int y, Font font, IReadOnlyCollection<SyntaxHighlighting> highlightings, Func<int, int> getXCoordinate, SyntaxPalette palette)
        {
            if (highlightings == null
                || !DrawingHelper.TryGetStringsToDraw(lineText, lineIndex, highlightings.Where(x => x.IsOnLine(lineIndex)).Distinct(new SyntaxHighlightingEqualityComparer()).ToList(), palette, out var stringsToDraw))
            {
                using (Brush brush = new SolidBrush(palette.DefaultTextColour))
                {
                    TextRenderer.DrawText(g, lineText, font, new Point(getXCoordinate(0), y), Color.Black, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                }
            }
            else
            {
                foreach ((string text, int characterOffset, Color colour) in stringsToDraw)
                {
                    using (Brush brush = new SolidBrush(colour))
                    {
                        TextRenderer.DrawText(g, text, font, new Point(getXCoordinate(characterOffset), y), colour, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    }
                }
            }
        }

        public static bool TryGetStringsToDraw(string originalLine, int lineIndex, IEnumerable<SyntaxHighlighting> highlightingsOnLine, SyntaxPalette palette, out List<(string text, int characterOffset, Color colour)> stringsToDraw)
        {
            int start = 0;
            int characterCount = 0;
            stringsToDraw = new List<(string text, int characterOffset, Color colour)>();
            foreach (SyntaxHighlighting highlighting in highlightingsOnLine)
            {
                if (highlighting.Start.LineNumber == highlighting.End.LineNumber)
                {
                    if (highlighting.Start.ColumnNumber - start < 0)
                    {
                        return false;
                    }
                    string before = originalLine.Substring(start, highlighting.Start.ColumnNumber - start);
                    stringsToDraw.Add((before, characterCount, palette.DefaultTextColour));

                    characterCount += before.Length;

                    string highlightedText = originalLine.Substring(highlighting.Start.ColumnNumber, highlighting.End.ColumnNumber - highlighting.Start.ColumnNumber);
                    stringsToDraw.Add((highlightedText, characterCount, highlighting.Colour));

                    characterCount += highlightedText.Length;

                    start = highlighting.End.ColumnNumber;
                }
                else if (highlighting.Start.LineNumber == lineIndex)
                {
                    string before = originalLine.Substring(characterCount, highlighting.Start.ColumnNumber - characterCount);
                    stringsToDraw.Add((before, characterCount, palette.DefaultTextColour));

                    characterCount += before.Length;

                    string highlightedText = originalLine.Substring(highlighting.Start.ColumnNumber);
                    stringsToDraw.Add((highlightedText, characterCount, highlighting.Colour));

                    characterCount += highlightedText.Length;

                    start = originalLine.Length;
                }
                else if (highlighting.End.LineNumber == lineIndex)
                {
                    string highlightedText = originalLine.Substring(0, highlighting.End.ColumnNumber);
                    stringsToDraw.Add((highlightedText, characterCount, highlighting.Colour));

                    characterCount += highlightedText.Length;

                    start = highlighting.End.ColumnNumber;
                }
                else
                {
                    stringsToDraw.Add((originalLine, 0, highlighting.Colour));
                    characterCount += originalLine.Length;
                    start = originalLine.Length;
                }
            }
            if (start != originalLine.Length)
            {
                stringsToDraw.Add((originalLine.Substring(start), characterCount, palette.DefaultTextColour));
            }
            return true;
        }
    }
}
