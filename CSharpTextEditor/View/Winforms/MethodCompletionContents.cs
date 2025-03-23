using CSharpTextEditor.Languages;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CSharpTextEditor.View.Winforms
{
    public class MethodCompletionContents : IToolTipContents
    {
        public IReadOnlyList<CodeCompletionSuggestion> Suggestions { get; }
        public int ActiveSuggestion { get; private set; }
        public int ActiveParameterIndex { get; }

        public MethodCompletionContents(IReadOnlyList<CodeCompletionSuggestion> suggestions, int activeSuggestion, int activeParameterIndex)
        {
            Suggestions = suggestions;
            ActiveSuggestion = activeSuggestion;
            ActiveParameterIndex = activeParameterIndex;
        }

        public bool Cycle(int sign)
        {
            if (sign > 0)
            {
                ActiveSuggestion = (ActiveSuggestion + 1) % Suggestions.Count;
            }
            else if (sign < 0)
            {
                ActiveSuggestion = (ActiveSuggestion - 1 + Suggestions.Count) % Suggestions.Count;
            }
            return true;
        }

        public Size Draw(Graphics g, Font font, SyntaxPalette palette)
        {
            CodeCompletionSuggestion suggestion = Suggestions[ActiveSuggestion];

            ICanvas canvas = new WinformsCanvas(g, new Size(), font);
            Bitmap? icon = IconCache.GetIcon(suggestion.SymbolType);
            int x = 0;
            int height = 0;
            if (Suggestions.Count > 1)
            {
                string indexText = $"🡹{ActiveSuggestion + 1} of {Suggestions.Count}🡻";
                Size indexSize = canvas.GetTextSize(indexText);
                canvas.DrawText(indexText, palette.DefaultTextColour, new Point(x, 0), false);
                x += indexSize.Width;
                height = Math.Max(height, indexSize.Height);
            }
            if (icon != null)
            {
                g.DrawImage(icon, x, 0);
                x += icon.Width;
                height = Math.Max(height, icon.Height);
            }
            (string toolTip, List<SyntaxHighlighting> highlightings) = suggestion.ToolTipSource.GetToolTip();
            Size textSize = DrawingHelper.DrawTextLine(canvas, 0, toolTip, x, 0, highlightings, palette, ActiveParameterIndex);
            height = Math.Max(height, textSize.Height);
            x += textSize.Width;

            return new Size(x + 3, height);
        }
    }
}
